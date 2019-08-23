using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DashFire;

/// Touch Manager
public class TouchManager : MonoBehaviour 
{
  /// ����ƽ̨�б�
  public static readonly RuntimePlatform[] TouchScreenPlatforms = { 
    RuntimePlatform.IPhonePlayer,
    RuntimePlatform.Android,
    RuntimePlatform.BB10Player,
    RuntimePlatform.WP8Player,
  };

  /// ��ָ����״̬
  public enum FingerPhase {
    None = 0,
    Begin,
    Moving,
    Stationary,
  }

  /// ȫ��delegate
  public static Gesture.EventHandler OnGestureHintEvent;
  public static Gesture.EventHandler OnGestureEvent;
  public static FingerEventDetector<FingerEvent>.FingerEventHandler OnFingerEvent;
  public delegate void EventHandler();
  public static EventHandler OnInputProviderChanged;

  public static void FireHintEvent(Gesture gesture)
  {
    if (!TouchEnable) return;
    if (!GestureEnable) return;

    if (OnGestureHintEvent != null)
      OnGestureHintEvent(gesture);
  }

  public static void FireEvent(Gesture gesture)
  {
    if (!TouchEnable) return;
    if (!GestureEnable) return;

    if (null != gesture.Recognizer && "OnEasyGesture" != gesture.Recognizer.EventMessageName) {
      GestureArgs e = ToGestureArgs(gesture);
      LogicSystem.FireGestureEvent(e);
    }

    if (OnGestureEvent != null)
      OnGestureEvent(gesture);

    //Debug.Log("...input SkillTags : " + gesture.SkillTags + ", event : " + gesture.Recognizer.EventMessageName + ", state : " + curTouchState.ToString());
  }

  public static void FireEvent(FingerEvent eventData)
  {
    if (!TouchEnable) return;

    GestureArgs e = ToGestureArgs(eventData);
    LogicSystem.FireGestureEvent(e);

    if (OnFingerEvent != null)
      OnFingerEvent(eventData);

    //Debug.LogWarning("...input event : " + eventData.Name + ", State : " + curTouchState.ToString());
  }

  public void GestureRecognizerSwitch(bool isOpen)
  {
    EasyRegognizer er = gameObject.GetComponent<EasyRegognizer>();
    if (isOpen) {
      if (null != er && false == er.enabled) {
        er.enabled = true;
      }
    } else {
      if (null != er && true == er.enabled) {
        er.enabled = false;
      }
    }
  }

  // �Ƿ�糡��
  public bool aross = false;
  // �Ƿ�����Զ��
  public bool unityremote = true;
  // Ĭ������
  public InputProvider mouseInputPrefab;
  public InputProvider touchInputPrefab;
  // ����״̬
  public static TouchType curTouchState = TouchType.Regognizer;
  private static bool gestureEnable = true;
  public static bool GestureEnable
  {
    get
    {
      return gestureEnable;
    }
    set
    {
      gestureEnable = value;
    }
  }
  public static bool TouchEnable {get;set;}
  /// instance
  public static TouchManager Instance
  {
    get
    {
      return TouchManager.instance;
    }
  }

  void Init()
  {
    InitInputProvider();
  }

  /// ����
  InputProvider inputProvider;
  public InputProvider InputProvider
  {
    get
    {
      return inputProvider;
    }
  }

  public class InputProviderEvent 
  {
    public InputProvider inputPrefab;
  }

  public static bool IsTouchScreenPlatform(RuntimePlatform platform)
  {
    for (int i = 0; i < TouchScreenPlatforms.Length; ++i) {
      if (platform == TouchScreenPlatforms[i])
        return true;
    }
    return false;
  }

  void InitInputProvider()
  {
    InputProviderEvent e = new InputProviderEvent();
    if (IsTouchScreenPlatform(Application.platform)) {
      e.inputPrefab = touchInputPrefab;
    } else {
      e.inputPrefab = mouseInputPrefab;
    }
    InstallInputProvider(e.inputPrefab);
  }

  public void InstallInputProvider(InputProvider inputPrefab)
  {
    if (!inputPrefab) {
      //Debug.LogError("Invalid InputProvider (null)");
      return;
    }

    //Debug.Log("TouchManager: using " + inputPrefab.name);

    if (inputProvider) {
      Destroy(inputProvider.gameObject);
    }

    inputProvider = ResourceSystem.NewObject(inputPrefab) as InputProvider;
    inputProvider.name = inputPrefab.name;
    inputProvider.transform.parent = this.transform;

    InitFingers(MaxFingers);

    if (OnInputProviderChanged != null) {
      OnInputProviderChanged();
    }
  }

  /// Finger
  public class Finger 
  {
    public int Index
    {
      get
      {
        return index;
      }
    }
    public bool IsDown
    {
      get
      {
        return phase != FingerPhase.None;
      }
    }
    public FingerPhase Phase
    {
      get
      {
        return phase;
      }
    }
    /// ǰһ֡״̬
    public FingerPhase PreviousPhase
    {
      get
      {
        return prevPhase;
      }
    }

    public bool WasDown
    {
      get
      {
        return prevPhase != FingerPhase.None;
      }
    }

    public bool IsMoving
    {
      get
      {
        return phase == FingerPhase.Moving;
      }
    }

    public bool WasMoving
    {
      get
      {
        return prevPhase == FingerPhase.Moving;
      }
    }

    public bool IsStationary
    {
      get
      {
        return phase == FingerPhase.Stationary;
      }
    }

    public bool WasStationary
    {
      get
      {
        return prevPhase == FingerPhase.Stationary;
      }
    }

    public bool Moved
    {
      get
      {
        return moved;
      }
    }

    public float StarTime
    {
      get
      {
        return startTime;
      }
    }

    public Vector2 StartPosition
    {
      get
      {
        return startPos;
      }
    }

    public Vector2 Position
    {
      get
      {
        return pos;
      }
    }

    public Vector2 PreviousPosition
    {
      get
      {
        return prevPos;
      }
    }

    public Vector2 DeltaPosition
    {
      get
      {
        return deltaPos;
      }
    }

    public float DistanceFromStart
    {
      get
      {
        return distFromStart;
      }
    }

    public bool IsFiltered
    {
      get
      {
        return filteredOut;
      }
    }

    public float TimeStationary
    {
      get
      {
        return elapsedTimeStationary;
      }
    }

    public List<GestureRecognizer> GestureRecognizers
    {
      get
      {
        return gestureRecognizers;
      }
    }

    /// privete
    int index = 0;
    FingerPhase phase = FingerPhase.None;
    FingerPhase prevPhase = FingerPhase.None;
    Vector2 pos = Vector2.zero;
    Vector2 startPos = Vector2.zero;
    Vector2 prevPos = Vector2.zero;
    Vector2 deltaPos = Vector2.zero;
    float startTime = 0;
    float lastMoveTime = 0;
    float distFromStart = 0;
    bool moved = false;
    // ��ָ�Ƿ񱻹���
    bool filteredOut = true;
    Collider collider;
    Collider prevCollider;
    float elapsedTimeStationary = 0;
    List<GestureRecognizer> gestureRecognizers = new List<GestureRecognizer>();

    public Finger(int index)
    {
      this.index = index;
    }

    public override string ToString()
    {
      return "Finger" + index;
    }

    public static implicit operator bool(Finger finger)
    {
      return finger != null;
    }

    internal void Update(bool newDownState, Vector2 newPos)
    {
      if (filteredOut && !newDownState)
        filteredOut = false;

      // ����
      if (!IsDown && newDownState && !TouchManager.instance.ShouldProcessTouch(index, newPos)) {
        filteredOut = true;
        newDownState = false;
      }

      // ������
      prevPhase = phase;

      if (newDownState) {
        // �µĴ�����������ָ״̬
        if (!WasDown) {
          phase = FingerPhase.Begin;

          pos = newPos;
          startPos = pos;
          prevPos = pos;
          deltaPos = Vector2.zero;
          moved = false;
          lastMoveTime = 0;

          startTime = Time.time;
          elapsedTimeStationary = 0;
          distFromStart = 0;
        } else {
          prevPos = pos;
          pos = newPos;
          distFromStart = Vector3.Distance(startPos, pos);
          deltaPos = pos - prevPos;

          if (deltaPos.sqrMagnitude > 0) {
            lastMoveTime = Time.time;
            phase = FingerPhase.Moving;
          } else if (!IsMoving || ((Time.time - lastMoveTime) > 0.05f)) {
            // ֹͣ�ƶ�֮����һ�㻺��ʱ��
            phase = FingerPhase.Stationary;
          }

          if (IsMoving) {
            // ��ָ�����ƶ�һ��
            moved = true;
          } else {
            if (!WasStationary) {
              // ��ʼ�Ժ��µľ�ֹ״̬
              elapsedTimeStationary = 0;
            } else {
              elapsedTimeStationary += Time.deltaTime;
            }
          }
        }
      } else {
        phase = FingerPhase.None;
      }
    }
  }

  /// ͬʱ֧�ֵ������ָ��
  public int MaxFingers
  {
    get
    {
      return inputProvider.MaxSimultaneousFingers;
    }
  }

  public static Finger GetFinger(int index)
  {
    return instance.fingers[index];
  }
  /// ��ǰ������Ļ����ָ�б�
  public static IFingerList Touches
  {
    get
    {
      return instance.touches;
    }
  }

  static List<GestureRecognizer> gestureRecognizers = new List<GestureRecognizer>();

  public static List<GestureRecognizer> RegisteredGestureRecognizers
  {
    get
    {
      return gestureRecognizers;
    }
  }

  public static void Register(GestureRecognizer recognizer)
  {
    if (gestureRecognizers.Contains(recognizer)) {
      return;
    }
    gestureRecognizers.Add(recognizer);
  }

  public static void Unregister(GestureRecognizer recognizer)
  {
    gestureRecognizers.Remove(recognizer);
  }

  private static GestureArgs ToGestureArgs(FingerEvent args)
  {
    if (null == args) {
      return null;
    }
    GestureArgs e = new GestureArgs();
    e.startPositionX = args.Position.x;
    e.startPositionY = args.Position.y;
    e.positionX = args.Position.x;
    e.positionY = args.Position.y;

    e.startTime = Time.time;
    e.elapsedTime = 0;
    Vector3 pos = args.GetTouchToWorldPoint();
    e.gamePosX = pos.x;
    e.gamePosY = pos.y;
    e.gamePosZ = pos.z;
    e.name = args.Name;

    GameObject go = DashFire.LogicSystem.PlayerSelf;
    Vector3 srcPos = pos;
    Vector3 destPos = pos;
    if (null != go) {
      destPos = go.transform.position;
    }
    e.towards = Geometry.GetYAngle(new ScriptRuntime.Vector2(destPos.x, destPos.z), new ScriptRuntime.Vector2(srcPos.x, srcPos.z));

    e.moveType = curTouchState;

    return e;
  }

  public static GestureArgs ToGestureArgs(Gesture args)
  {
    if (null == args) {
      return null;
    }

    GestureArgs e = new GestureArgs();
    e.startPositionX = args.StartPosition.x;
    e.startPositionY = args.StartPosition.y;
    e.positionX = args.Position.x;
    e.positionY = args.Position.y;

    e.startTime = args.StartTime;
    e.elapsedTime = args.ElapsedTime;
    Vector3 pos = args.GetTouchToWorldPoint();
    e.gamePosX = pos.x;
    e.gamePosY = pos.y;
    e.gamePosZ = pos.z;
    e.name = args.Recognizer.EventMessageName;

    GameObject go = DashFire.LogicSystem.PlayerSelf;
    Vector3 srcPos = args.GetStartTouchToWorldPoint();
    Vector3 destPos = pos;
    if (null != go) {
      destPos = go.transform.position;
    }
    e.towards = Geometry.GetYAngle(new ScriptRuntime.Vector2(destPos.x, destPos.z), new ScriptRuntime.Vector2(srcPos.x, srcPos.z));
    e.moveType = curTouchState;
    e.selectedObjID = args.SelectedID;
    e.sectionNum = args.SectionNum;

    Vector3 airWellPos = args.GetTouchToAirWallWorldPoint();
    e.airWelGamePosX = airWellPos.x;
    e.airWelGamePosY = airWellPos.y;
    e.airWelGamePosZ = airWellPos.z;

    e.startGamePosX = srcPos.x;
    e.startGamePosY = srcPos.y;
    e.startGamePosZ = srcPos.z;
    ///
    e.attackRange = 3f;
    SkillControllerInterface skill_contrl = GetControl();
    if (null != skill_contrl) {
      SkillInputData skill_input_data = skill_contrl.GetSkillInputData(args.SkillTags);
      if (null != skill_input_data) {
        e.attackRange = skill_input_data.castRange;
      }
    }

    return e;
  }

  private static SkillControllerInterface GetControl()
  {
    SkillControllerInterface SkillCtrl = null;
    GameObject go = DashFire.LogicSystem.PlayerSelf;
    if (null != go) {
      BaseSkillManager skill_Manager = go.GetComponent<BaseSkillManager>();
      if (null != skill_Manager) {
        SkillCtrl = skill_Manager.GetSkillController();
      }
    }
    return SkillCtrl;
  }

  void Awake()
  {
    CheckInit();
  }

  void Start()
  {
    if (aross) {
      DontDestroyOnLoad(this.gameObject);
    }
  }

  void OnEnable()
  {
    CheckInit();
  }

  void CheckInit()
  {
    if (instance == null) {
      instance = this;
      Init();
      TouchEnable = true;
    } else if (instance != this) {
      // ����ֻ��һ��TouchManagerʵ��
      Destroy(this.gameObject);
      TouchEnable = false;
      return;
    }
  }

  void Update()
  {
#if UNITY_EDITOR
    if (unityremote && Input.touchCount > 0 && inputProvider.GetType() != touchInputPrefab.GetType()) {
      // ��鵽unityremote���л��������뷽ʽ
      InstallInputProvider(touchInputPrefab);
      unityremote = false;
      return;
    }
#endif
    if (inputProvider) {      
      UpdateFingers();        
    }
  }

  // singleton
  static TouchManager instance;

  /// ��ָ����
  Finger[] fingers;
  FingerList touches;

  void InitFingers(int count)
  {
    // ÿ����ָ����
    fingers = new Finger[count];
    for (int i = 0; i < count; ++i) {
      fingers[i] = new Finger(i);
    }
    touches = new FingerList();
  }

  void UpdateFingers()
  {
    touches.Clear();
    // ����������ָ
    for (int i = 0; i < fingers.Length; ++i) {
      Finger finger = fingers[i];
      Vector2 pos = Vector2.zero;
      bool down = false;
      // ˢ������״̬
      inputProvider.GetInputState(finger.Index, out down, out pos);
      finger.Update(down, pos);
      if (finger.IsDown) {       
          touches.Add(finger);
      }
    }
  }

  /// Global Input Filter
  /// <returns>true ��������, false ���</returns>
  public delegate bool GlobalTouchFilterDelegate(int fingerIndex, Vector2 position);
  GlobalTouchFilterDelegate globalTouchFilterFunc;

  /// ����ָ���ص�ѡ���ԵĹ���������Ϊ��������Ļ�Ĳ����������������¼�
  public static GlobalTouchFilterDelegate GlobalTouchFilter
  {
    get
    {
      return instance.globalTouchFilterFunc;
    }
    set
    {
      instance.globalTouchFilterFunc = value;
    }
  }

  protected bool ShouldProcessTouch(int fingerIndex, Vector2 position)
  {
    if (globalTouchFilterFunc != null) {
      return globalTouchFilterFunc(fingerIndex, position);
    }
    return true;
  }

  /// ȫ�� ������ָ������ʶ��
  Transform[] fingerNodes;
  Transform CreateNode(string name, Transform parent)
  {
    GameObject go = new GameObject(name);
    go.transform.parent = parent;
    return go.transform;
  }
  void InitNodes()
  {
    int fingerCount = fingers.Length;

    if (fingerNodes != null) {
      foreach (Transform fingerCompNode in fingerNodes) {
        Destroy(fingerCompNode.gameObject);
      }
    }
    fingerNodes = new Transform[fingerCount];
    for (int i = 0; i < fingerNodes.Length; ++i) {
      fingerNodes[i] = CreateNode("Finger" + i, this.transform);
    }
  }

  /// ��ָ�б�ӿ�
  public interface IFingerList : IEnumerable<Finger> 
  {
    Finger this[int index]
    {
      get;
    }
    int Count
    {
      get;
    }
    /// ���ƽ���Ŀ�ʼ��λ��
    Vector2 GetAverageStartPosition();
    /// ���ƽ��λ��
    Vector2 GetAveragePosition();
    /// ���ǰһ֡ƽ��λ��
    Vector2 GetAveragePreviousPosition();
    /// ��ȡ������ʼ��λ�ú�����λ�õ�ƽ������
    float GetAverageDistanceFromStart();
    /// ��������ָ��ʼʱ��
    Finger GetOldest();
    /// �Ƿ����еĴ�����ָ�����ƶ���
    bool AllMoving();
    /// �����Ƿ�����ͬ�����ƶ�
    /// <param name="tolerance">0->1 ӳ�� 0->90 ��</param>
    bool MovingInSameDirection(float tolerance);
  }

  /// ��ָ�б�
  public class FingerList : IFingerList 
  {
    List<Finger> list;

    public FingerList()
    {
      list = new List<Finger>();
    }

    public FingerList(List<Finger> list)
    {
      this.list = list;
    }

    public Finger this[int index]
    {
      get
      {
        return list[index];
      }
    }

    public int Count
    {
      get
      {
        return list.Count;
      }
    }

    public IEnumerator<Finger> GetEnumerator()
    {
      return list.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }

    public void Add(Finger touch)
    {
      list.Add(touch);
    }

    public bool Remove(Finger touch)
    {
      return list.Remove(touch);
    }

    public bool Contains(Finger touch)
    {
      return list.Contains(touch);
    }

    public void AddRange(IEnumerable<Finger> touches)
    {
      list.AddRange(touches);
    }

    public void Clear()
    {
      list.Clear();
    }

    public delegate T FingerPropertyGetterDelegate<T>(Finger finger);

    public Vector2 AverageVector(FingerPropertyGetterDelegate<Vector2> getProperty)
    {
      Vector2 avg = Vector2.zero;
      if (Count > 0) {
        for (int i = 0; i < list.Count; ++i) {
          avg += getProperty(list[i]);
        }
        avg /= Count;
      }
      return avg;
    }

    public float AverageFloat(FingerPropertyGetterDelegate<float> getProperty)
    {
      float avg = 0;
      if (Count > 0) {
        for (int i = 0; i < list.Count; ++i) {
          avg += getProperty(list[i]);
        }
        avg /= Count;
      }
      return avg;
    }

    static FingerPropertyGetterDelegate<Vector2> delGetFingerStartPosition = GetFingerStartPosition;
    static FingerPropertyGetterDelegate<Vector2> delGetFingerPosition = GetFingerPosition;
    static FingerPropertyGetterDelegate<Vector2> delGetFingerPreviousPosition = GetFingerPreviousPosition;
    static FingerPropertyGetterDelegate<float> delGetFingerDistanceFromStart = GetFingerDistanceFromStart;

    static Vector2 GetFingerStartPosition(Finger finger)
    {
      return finger.StartPosition;
    }
    static Vector2 GetFingerPosition(Finger finger)
    {
      return finger.Position;
    }
    static Vector2 GetFingerPreviousPosition(Finger finger)
    {
      return finger.PreviousPosition;
    }
    static float GetFingerDistanceFromStart(Finger finger)
    {
      return finger.DistanceFromStart;
    }

    public Vector2 GetAverageStartPosition()
    {
      return AverageVector(delGetFingerStartPosition);
    }

    public Vector2 GetAveragePosition()
    {
      return AverageVector(delGetFingerPosition);
    }

    public Vector2 GetAveragePreviousPosition()
    {
      return AverageVector(delGetFingerPreviousPosition);
    }

    public float GetAverageDistanceFromStart()
    {
      return AverageFloat(delGetFingerDistanceFromStart);
    }

    public Finger GetOldest()
    {
      Finger oldest = null;
      foreach (Finger finger in list) {
        if (oldest == null || (finger.StarTime < oldest.StarTime)) {
          oldest = finger;
        }
      }

      return oldest;
    }

    public bool MovingInSameDirection(float tolerance)
    {
      if (Count < 2) {
        return true;
      }
      float minDOT = Mathf.Max(0.1f, 1.0f - tolerance);
      Vector2 refDir = this[0].Position - this[0].StartPosition;
      refDir.Normalize();
      for (int i = 1; i < Count; ++i) {
        Vector2 dir = this[i].Position - this[i].StartPosition;
        dir.Normalize();
        if (Vector2.Dot(refDir, dir) < minDOT) {
          return false;
        }
      }
      return true;
    }

    public bool AllMoving()
    {
      if (Count == 0) {
        return false;
      }
      // ������ָ�������ƶ�
      for (int i = 0; i < list.Count; ++i) {
        if (!list[i].IsMoving) {
          return false;
        }
      }
      return true;
    }
  }

  ///
  const float DESKTOP_SCREEN_STANDARD_DPI = 96; // Ĭ�� win7 dpi
  const float INCHES_TO_CENTIMETERS = 2.54f; // 1 Ӣ�� = 2.54 cm
  const float CENTIMETERS_TO_INCHES = 1.0f / INCHES_TO_CENTIMETERS; // 1 cm = 0.3937... Ӣ��

  static float screenDPI = 0;
  /// ��ĻÿӢ��ĵ�
  public static float ScreenDPI
  {
    get
    {
      if (screenDPI <= 0) {
        screenDPI = Screen.dpi;
        if (screenDPI <= 0)
          screenDPI = DESKTOP_SCREEN_STANDARD_DPI;

#if UNITY_IPHONE
          // ���һЩ��֧�ֵ��豸
          if( iPhone.generation == iPhoneGeneration.Unknown ||
              iPhone.generation == iPhoneGeneration.iPadUnknown ||
              iPhone.generation == iPhoneGeneration.iPhoneUnknown ) {
              // ipad mini 2
              if( Screen.width == 2048 && Screen.height == 1536 && screenDPI == 260 )
                  screenDPI = 326;
          }
#endif
      }
      return TouchManager.screenDPI;
    }
    set
    {
      TouchManager.screenDPI = value;
    }
  }

  public static float Convert(float distance, DistanceUnit fromUnit, DistanceUnit toUnit)
  {
    float dpi = ScreenDPI;
    float pixelDistance;

    switch (fromUnit) {
      case DistanceUnit.Centimeters:
        pixelDistance = distance * CENTIMETERS_TO_INCHES * dpi; // cm -> in -> px
        break;
      case DistanceUnit.Inches:
        pixelDistance = distance * dpi; // in -> px
        break;
      case DistanceUnit.Pixels:
      default:
        pixelDistance = distance;
        break;
    }

    switch (toUnit) {
      case DistanceUnit.Inches:
        return pixelDistance / dpi; // px -> in
      case DistanceUnit.Centimeters:
        return (pixelDistance / dpi) * INCHES_TO_CENTIMETERS;  // px -> in -> cm
      case DistanceUnit.Pixels:
        return pixelDistance;
    }

    return pixelDistance;
  }

  // ��һ�����뵽��һ�������2D�ƶ�����
  public static Vector2 Convert(Vector2 v, DistanceUnit fromUnit, DistanceUnit toUnit)
  {
    return new Vector2(Convert(v.x, fromUnit, toUnit),
                        Convert(v.y, fromUnit, toUnit));
  }
}

public enum DistanceUnit 
{
  Pixels,
  Inches,
  Centimeters,
}

/// ����
public abstract class InputProvider : MonoBehaviour 
{
  /// ���ͬʱ��ָ����
  public abstract int MaxSimultaneousFingers
  {
    get;
  }
  /// ���ĳ����ָ������״̬
  public abstract void GetInputState(int fingerIndex, out bool down, out Vector2 position);
}

/// ��չ
public static class TouchExtensions 
{
  /// ����һ����д (cm, in, px)
  public static string Abreviation(this DistanceUnit unit)
  {
    switch (unit) {
      case DistanceUnit.Centimeters:
        return "cm";
      case DistanceUnit.Inches:
        return "in";
      case DistanceUnit.Pixels:
        return "px";
    }
    return unit.ToString();
  }

  /// ��λת��
  public static float Convert(this float value, DistanceUnit fromUnit, DistanceUnit toUnit)
  {
    return TouchManager.Convert(value, fromUnit, toUnit);
  }

  /// ����ת����Ҫ�ĵ�λ
  public static float In(this float valueInPixels, DistanceUnit toUnit)
  {
    return valueInPixels.Convert(DistanceUnit.Pixels, toUnit);
  }

  /// ����ת������
  public static float Centimeters(this float valueInPixels)
  {
    return valueInPixels.In(DistanceUnit.Centimeters);
  }

  /// ����ת��Ӣ��
  public static float Inches(this float valueInPixels)
  {
    return valueInPixels.In(DistanceUnit.Inches);
  }

  /// Vector2
  public static Vector2 Convert(this Vector2 v, DistanceUnit fromUnit, DistanceUnit toUnit)
  {
    return TouchManager.Convert(v, fromUnit, toUnit);
  }

  /// ��������ת������Ҫ��
  public static Vector2 In(this Vector2 vecInPixels, DistanceUnit toUnit)
  {
    return vecInPixels.Convert(DistanceUnit.Pixels, toUnit);
  }

  /// ��������������
  public static Vector2 Centimeters(this Vector2 vecInPixels)
  {
    return vecInPixels.In(DistanceUnit.Centimeters);
  }

  /// ����������Ӣ��
  public static Vector2 Inches(this Vector2 vecInPixels)
  {
    return vecInPixels.In(DistanceUnit.Inches);
  }
}
