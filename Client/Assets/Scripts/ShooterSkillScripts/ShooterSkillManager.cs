using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DashFire;

public class ShooterSkillManager : BaseSkillManager {
  public KeyCode m_AttackKey = KeyCode.J;
  public KeyCode m_SkillAKey = KeyCode.I;
  public KeyCode m_SkillBKey = KeyCode.O;
  public KeyCode m_SkillCKey = KeyCode.K;
  public KeyCode m_SkillDKey = KeyCode.L;
  public KeyCode m_SkillHoldKey = KeyCode.P;
  public KeyCode m_SkillQKey = KeyCode.Q;
  public KeyCode m_SkillEKey = KeyCode.E;
  public GameObject m_SkillTipPrefab;
  public GameObject m_JoyStick = null;
  public Vector2 m_JoyStickPosPercent = new Vector2(0.5f, 0.5f);
  public SkillAnimInfo DefaultAnimInfo = null;

  private GameObject m_SkillTipObj;
  private List<IShooterSkill> m_Skills = new List<IShooterSkill>();
  private IShooterSkill m_CurPlaySkill = null;
  private SkillController m_ShooterSkillController;
  private SkillMovement m_SkillMovement;
  private DFMUiRoot m_UIRoot = null;
  private GameObject m_JoyStickObj = null;

  void Awake() {
    //Application.targetFrameRate = 30;
    //QualitySettings.vSyncCount = 2;
  }

  internal void OnDestroy() {
    TouchManager.OnGestureEvent -= OnGestureEvent;
    if (m_UIRoot != null) {
      m_UIRoot.ShowGunChangeButton(false);
    }
    GameObject.Destroy(m_JoyStickObj);
  }

  internal void OnEnable() {
    //ShowChangWeaponButton();
    HideJoyStick();
  }
  internal void OnDisable() {
    //HideChangWeaponButton();
    HideJoyStick();
  }

  public override SkillControllerInterface GetSkillController() {
    return m_ShooterSkillController;
  }

  public override bool IsUsingSkill() {
    if (m_CurPlaySkill != null && m_CurPlaySkill.IsExecuting()) {
      return true;
    }
    return false;
  }

  public SkillMovement GetSkillMovement() {
    return m_SkillMovement;
  }

  public GameObject GetOwner() {
    return this.gameObject;
  }

  // Use this for initialization
  void Start() {
    m_SkillMovement = gameObject.GetComponent<SkillMovement>();
    if (m_SkillTipPrefab != null) {
      m_SkillTipObj = Instantiate(m_SkillTipPrefab, Vector3.zero, Quaternion.identity) as GameObject;
      m_SkillTipObj.SetActive(false);
    } else {
      Debug.Log("---sn: can't create skill notice obj!!");
    }
    
    CollectSkillScrip(m_Skills);
    m_ShooterSkillController = new ShooterControlHandler(this, m_Skills);
    m_ShooterSkillController.Init();
    ///
    TouchManager.OnGestureEvent += OnGestureEvent;

    GameObject tUiRoot = GameObject.FindGameObjectWithTag("UI");
    if (tUiRoot != null) {
      m_UIRoot = tUiRoot.GetComponent<DFMUiRoot>();
    }
    if (m_JoyStick != null) {
      m_JoyStickObj = GameObject.Instantiate(m_JoyStick) as GameObject;
      if (m_JoyStickObj != null) {
        m_JoyStickObj.transform.position = Vector3.zero;
        EasyJoystick joyStickScript = m_JoyStickObj.GetComponentInChildren<EasyJoystick>();
        if (joyStickScript != null) {
          joyStickScript.JoystickPositionOffset = new Vector2(
            m_JoyStickPosPercent.x * Screen.width,
            m_JoyStickPosPercent.y * Screen.height);
        }
      }
    }
    //ShowChangWeaponButton();
    HideJoyStick();
  }

  private void OnGestureEvent(Gesture args) {
    if (null != args && null != args.Recognizer) {
      int skill_index = -1;
      string gesture_name = args.Recognizer.EventMessageName;
      if (GestureEvent.OnLine.ToString() == gesture_name) { }
      if (skill_index >= 0) {
      }
    }
  }

  // Update is called once per frame
  void Update() {
    if (Input.GetKeyDown(m_AttackKey)) {
      m_ShooterSkillController.StartAttack();
    }
    if (Input.GetKeyUp(m_AttackKey)) {
      m_ShooterSkillController.StopAttack();
    }
    if (Input.GetKeyUp(m_SkillAKey)) {
      m_ShooterSkillController.PushSkill(SkillCategory.kSkillA, Vector3.zero);
    }
    if (Input.GetKeyUp(m_SkillBKey)) {
      m_ShooterSkillController.PushSkill(SkillCategory.kSkillB, Vector3.zero);
      Vector3 new_pos = gameObject.transform.position;
      new_pos.z += 5;
      m_ShooterSkillController.ShowSkillTip(SkillCategory.kNone, new_pos);
    }
    if (Input.GetKeyUp(m_SkillCKey)) {
      m_ShooterSkillController.PushSkill(SkillCategory.kSkillC, Vector3.zero);
      m_ShooterSkillController.HideSkillTip(SkillCategory.kNone);
    }
    if (Input.GetKeyUp(m_SkillDKey)) {
      m_ShooterSkillController.PushSkill(SkillCategory.kSkillD, Vector3.zero);
    }
    if (Input.GetKeyUp(m_SkillHoldKey)) {
      m_ShooterSkillController.PushSkill(SkillCategory.kHold, Vector3.zero);
    }
    if (Input.GetKeyUp(m_SkillQKey)) {
      m_ShooterSkillController.PushSkill(SkillCategory.kSkillQ, Vector3.zero);
    }
    if (Input.GetKeyUp(m_SkillEKey)) {
      m_ShooterSkillController.PushSkill(SkillCategory.kSkillE, Vector3.zero);
    }
    m_ShooterSkillController.OnTick();
  }

  public IShooterSkill GetCurPlaySkill() {
    return m_CurPlaySkill;
  }

  public void ShowSkillTip(Vector3 pos) {
    if (m_SkillTipObj != null) {
      m_SkillTipObj.SetActive(true);
      Vector3 dir = pos - gameObject.transform.position;
      dir.y = 0;
      m_SkillTipObj.transform.position = pos;
      m_SkillTipObj.transform.forward = dir;
    }
  }

  public void CancelSkillTip() {
    if (m_SkillTipObj != null) {
      m_SkillTipObj.SetActive(false);
    }
  }

  //public void ShowChangWeaponButton() {
  //  if (m_UIRoot != null) {
  //    m_UIRoot.ShowGunChangeButton(true);
  //  }
  //}

  //public void HideChangWeaponButton() {
  //  if (m_UIRoot != null) {
  //    m_UIRoot.ShowGunChangeButton(false);
  //  }
  //}
  public void ShowJoyStick() {
    if (m_JoyStickObj != null) {
      m_JoyStickObj.SetActive(true);
    }
  }

  public void HideJoyStick() {
    if (m_JoyStickObj != null) {
      m_JoyStickObj.SetActive(false);
    }
  }

  public bool ForceStartSkillById(int skillid) {
    IShooterSkill ss = GetSkillById(skillid);
    if (ss == null) {
      return false;
    }
    if (m_CurPlaySkill == null || ss.CanStart()) {
      Vector3 targetPos = Vector3.zero;
      if (m_CurPlaySkill != null) {
        targetPos = m_CurPlaySkill.GetTargetPos();
        m_CurPlaySkill.StopSkill();
      }
      if (ss.StartSkill(targetPos)) {
        m_CurPlaySkill = ss;
        return true;
      }
    }
    return false;
  }
  public void SetFacePos(Vector3 targetPos) {
    SetFacePos(targetPos, new MathRange());
  }
  public void SetFacePos(Vector3 targetPos, MathRange YRotateRange) {
    //Debug.Log("---face: cur pos " + gameObject.transform.position + " target-pos:" + targetPos);
    Vector3 dir = targetPos - transform.position;
    //dir.y = 0;

    transform.forward = dir;
    Vector3 rotate = gameObject.transform.rotation.eulerAngles;
    rotate.x = YRotateRange.Clip(rotate.x);
    gameObject.transform.rotation = Quaternion.Euler(rotate);
    LogicSystem.NotifyGfxUpdatePosition(gameObject, transform.position.x, transform.position.y, transform.position.z,
      0, rotate.y * Mathf.PI / 180, 0);
  }

  public bool StartSkillById(int skillid, Vector3 targetPos) {
    IShooterSkill ss = GetSkillById(skillid);
    if (ss == null) {
      return false;
    }
    if (m_CurPlaySkill == null || m_CurPlaySkill.CanBreakBySkill() && ss.CanStart()) {
      if (m_CurPlaySkill != null) {
        m_CurPlaySkill.StopSkill();
      }
      if (ss.StartSkill(targetPos)) {
        m_CurPlaySkill = ss;
        return true;
      }
    }
    return false;
  }

  public IShooterSkill GetSkillById(int skillid) {
    foreach (IShooterSkill ss in m_Skills) {
      if (ss.GetSkillId() == skillid) {
        return ss;
      }
    }
    return null;
  }

  public void PlaySound(AudioClip clip) {
    audio.clip = clip;
    audio.Play();
  }
  private void CollectSkillScrip(List<IShooterSkill> skillList) {
    Component[] components = gameObject.GetComponents(typeof(IShooterSkill));
    if (components != null && components.Length > 0) {
      foreach (Component component in components) {
        if (component is IShooterSkill) {
          skillList.Add((IShooterSkill)component);
        }
      }
    }
  }
  public void ChangeSkillByCategory(SkillCategory category, int skillId) {
    ShooterControlHandler control = (ShooterControlHandler)m_ShooterSkillController;
    control.ChangeSkillByCategory(category, m_Skills, skillId);
  }
}
