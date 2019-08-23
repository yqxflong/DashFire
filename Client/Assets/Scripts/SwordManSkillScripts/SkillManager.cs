using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DashFire;

public class SkillManager : BaseSkillManager {
  public KeyCode m_AttackKey = KeyCode.J;
  public KeyCode m_SkillAKey = KeyCode.I;
  public KeyCode m_SkillBKey = KeyCode.O;
  public KeyCode m_SkillCKey = KeyCode.K;
  public KeyCode m_SkillDKey = KeyCode.L;
  public KeyCode m_SkillQKey = KeyCode.Q;
  public KeyCode m_SkillEKey = KeyCode.E;
  public KeyCode m_SkillEX = KeyCode.Y;
  public Vector3 m_AttackCenter;
  public Vector3 m_DegreeCenter;
  public float m_AttackRadius = 3;
  public float m_AttackDegree = 180;
  public string[] m_WeaponsName;
  public GameObject m_SkillTipObjPrefab;

  private List<SkillScript> m_Skills = new List<SkillScript>();
  private SkillScript m_CurPlaySkill = null;
  private SkillController m_NewSwordManController;
  private SkillMovement m_SkillMovement;
  private SkillColliderManager m_ColliderManager;
  private Transform[] m_Weapons;
  private Transform[] m_WeaponsParent;
  private GameObject m_SkillTipObj;
  DashFire.SharedGameObjectInfo m_SelfObjInfo;

  void Awake()
  {
    //Application.targetFrameRate = 30;
    //QualitySettings.vSyncCount = 2;
  }

  public GameObject GetOwner() {
    return gameObject;
  }

  void OnDestroy() {
    TouchManager.OnGestureEvent -= OnGestureEvent;
    if (m_CurPlaySkill != null && m_CurPlaySkill.IsActive()) {
      m_CurPlaySkill.ForceStopSkill();
    }
    m_NewSwordManController.BeginCurSkillCD();
  }

  public override SkillControllerInterface GetSkillController() {
    return m_NewSwordManController;
  }

  public override bool IsUsingSkill() {
    if (m_CurPlaySkill != null && m_CurPlaySkill.IsActive()) {
      return true;
    }
    return false;
  }

  public override bool IsControledMove() {
    if (m_CurPlaySkill == null) {
      return false;
    }
    if (m_CurPlaySkill.IsActive() && m_CurPlaySkill.m_IsControlMove) {
      return true;
    }
    return false;
  }

  public SkillMovement GetSkillMovement() {
    return m_SkillMovement;
  }

  // Use this for initialization
  void Start () {
    m_ColliderManager = gameObject.GetComponent<SkillColliderManager>();
    m_SkillMovement = gameObject.GetComponent<SkillMovement>();
    m_SelfObjInfo = DashFire.LogicSystem.GetSharedGameObjectInfo(gameObject);
    if (m_SkillTipObjPrefab != null) {
      m_SkillTipObj = Instantiate(m_SkillTipObjPrefab, Vector3.zero, Quaternion.identity) as GameObject;
      m_SkillTipObj.SetActive(false);
    }
    if (m_SkillTipObj == null) {
      Debug.Log("--create skill tip obj failed!");
    }
    int weapon_count = m_WeaponsName.Length;
    m_Weapons = new Transform[weapon_count];
    m_WeaponsParent = new Transform[weapon_count];
    for (int i =0; i < weapon_count; i++) {
      string name = m_WeaponsName[i];
      if (!string.IsNullOrEmpty(name)) {
        m_Weapons[i] = EffectManager.GetChildNodeByName(gameObject, name);
        if (m_Weapons[i] != null) {
          m_WeaponsParent[i] = m_Weapons[i].parent;
          continue;
        }
      }
      m_Weapons[i] = null;
    }
    SkillScript[] skills = gameObject.GetComponents<SkillScript>();
    m_Skills.AddRange(skills);
    m_NewSwordManController = new NewSwordManSkillController(this, m_Skills);
    m_NewSwordManController.Init();

    TouchManager.OnGestureEvent += OnGestureEvent;
  }

  public void ResetWeapons() {
    for (int i = 0; i < m_Weapons.Length; i++) {
      if (m_Weapons[i] != null) {
        m_Weapons[i].parent = m_WeaponsParent[i];
        m_Weapons[i].localPosition = Vector3.zero;
        m_Weapons[i].localRotation = Quaternion.identity;
      }
    }
  }

  private void OnGestureEvent(Gesture args)
  {
    if (null != args && null != args.Recognizer) {
      int skill_index = -1;
      string gesture_name = args.Recognizer.EventMessageName;
      if (GestureEvent.OnLine.ToString() == gesture_name) {}
      if (skill_index >= 0) {
      }
    }
  }

  // Update is called once per frame
  void Update()
  {
    if (Input.GetKeyDown(m_AttackKey)) {
      m_NewSwordManController.StartAttack();
    }
    if (Input.GetKeyUp(m_AttackKey)) {
      m_NewSwordManController.StopAttack();
    }
    if (Input.GetKeyUp(m_SkillAKey)) {
      m_NewSwordManController.PushSkill(SkillCategory.kSkillA, Vector3.zero);
    }
    if (Input.GetKeyUp(m_SkillBKey)) {
      m_NewSwordManController.PushSkill(SkillCategory.kSkillB, Vector3.zero);
    }
    if (Input.GetKeyUp(m_SkillCKey)) {
      m_NewSwordManController.PushSkill(SkillCategory.kSkillC, Vector3.zero);
    }
    if (Input.GetKeyUp(m_SkillDKey)) {
      m_NewSwordManController.PushSkill(SkillCategory.kSkillD, Vector3.zero);
    }
    if (Input.GetKeyUp(m_SkillQKey)) {
      m_NewSwordManController.PushSkill(SkillCategory.kSkillQ, Vector3.zero);
    }
    if (Input.GetKeyUp(m_SkillEKey)) {
      m_NewSwordManController.PushSkill(SkillCategory.kSkillE, Vector3.zero);
    }

    if (Input.GetKeyUp(m_SkillEX)) {
      m_NewSwordManController.PushSkill(SkillCategory.kEx, Vector3.zero);
    }
    m_NewSwordManController.OnTick();
    if (m_SelfObjInfo != null && m_SelfObjInfo.Blood <= 0) {
      if (m_CurPlaySkill != null && m_CurPlaySkill.IsActive()) {
        m_NewSwordManController.BeginCurSkillCD();
        m_CurPlaySkill.ForceStopSkill();
      }
    }
  }

  public SkillScript GetCurPlaySkill()
  {
    return m_CurPlaySkill;
  }

  public void ShowSkillTip(Vector3 pos)
  {
    if (!gameObject.activeSelf) {
      return;
    }
    if (m_SkillTipObj != null) {
      m_SkillTipObj.SetActive(true);
      Vector3 dir = pos - gameObject.transform.position;
      dir.y = 0;
      m_SkillTipObj.transform.position = pos;
      m_SkillTipObj.transform.forward = dir;
    }
  }

  public void HideSkillTip()
  {
    if (m_SkillTipObj != null) {
      m_SkillTipObj.SetActive(false);
    }
  }

  public void ShowSkillFailedTip()
  {
    /*DashFire.LogicSystem.EventChannelForGfx.Publish("ge_skill_false", "ui");
    if (m_SkillTipObj != null) {
      m_SkillTipObj.SetActive(false);
    }
    if (m_CancelTipObj != null) {
      m_CancelTipObj.SetActive(false);
      m_CancelTipObj.SetActive(true);
      m_CancelTipObj.transform.position = m_SkillTipObj.transform.position;
    }*/
  }

  public bool ForceStartSkillById(int skillid)
  {
    SkillScript ss = GetSkillById(skillid);
    if (ss == null) {
      return false;
    }
    if (m_CurPlaySkill == null || ss.CanStart()) {
      if (m_CurPlaySkill != null) {
        m_CurPlaySkill.ForceStopSkill();
      }
      if (ss.StartSkill()) {
        m_CurPlaySkill = ss;
        return true;
      }
    }
    return false;
  }

  public void SetFacePos(Vector3 targetPos)
  {
    m_SkillMovement.SetFacePos(targetPos);
  }

  public void SetFaceDir(float direction)
  {
    Vector3 rotate = new Vector3(0, direction * 180 / Mathf.PI, 0);
    gameObject.transform.eulerAngles = rotate;
    DashFire.LogicSystem.NotifyGfxUpdatePosition(gameObject, transform.position.x, transform.position.y,
                                        transform.position.z, 0, direction, 0);
  }

  public bool StartSkillById(int skillid)
  {
    SkillScript ss = GetSkillById(skillid);
    if (ss == null) {
      return false;
    }
    if (m_CurPlaySkill == null || m_CurPlaySkill.CanStop() && ss.CanStart()) {
      if (m_CurPlaySkill != null && m_CurPlaySkill.IsActive()) {
        m_CurPlaySkill.StopSkill();
      }
      if (ss.StartSkill()) {
        m_CurPlaySkill = ss;
        return true;
      }
    }
    return false;
  }

  public SkillScript GetSkillById(int skillid)
  {
    foreach (SkillScript ss in m_Skills) {
      if (ss.SkillId == skillid) {
        return ss;
      }
    }
    return null;
  }

  public void PlaySound(AudioClip clip)
  {
    audio.PlayOneShot(clip);
  }

  public bool IsHaveEnimyInAttackRange() {
    Vector3 center = gameObject.transform.TransformPoint(m_AttackCenter);
    Vector3 degreeCenter = gameObject.transform.TransformPoint(m_DegreeCenter);
    List<GameObject> result = GetEnimyInSector(center, m_AttackRadius,
                                               gameObject.transform.forward,
                                               degreeCenter, m_AttackDegree);
    return result.Count > 0;
  }

  public List<GameObject> GetEnimyInSector(Vector3 center, float radius, Vector3 direction,
                                           Vector3 degreeCenter, float degree) {
    List<GameObject> all_target = TargetChooser.FindTargetInSector(center, radius,
                                                                   direction,
                                                                   degreeCenter, degree);
    List<GameObject> targets = m_ColliderManager.FiltEnimy(all_target);
    return targets;
  }

  public void MoveChildToNode(string str_param) {
    string[] param_list = str_param.Split(' ');
    if (param_list.Length < 2) {
      Debug.Log("--MoveChildToNode: param not enough");
      return;
    }
    string childname = param_list[0];
    string nodename = param_list[1];
    Transform child = EffectManager.GetChildNodeByName(gameObject, childname);
    if (child == null) {
      Debug.Log("--MoveChildToNode: not find child by name:" + childname);
      return;
    }
    Transform togglenode = EffectManager.GetChildNodeByName(gameObject, nodename);
    if (togglenode == null) {
      Debug.Log("--MoveChildToNode: not find node by name:" + nodename);
      return;
    }
    child.parent = togglenode;
    child.localRotation = Quaternion.identity;
    child.localPosition = Vector3.zero;
  }

  public static bool AttatchNodeToNode(GameObject source,
                                       string sourcenode,
                                       GameObject target,
                                       string targetnode) {
    Transform source_child = EffectManager.GetChildNodeByName(source, sourcenode);
    Transform target_child = EffectManager.GetChildNodeByName(target, targetnode);
    if (source_child == null || target_child == null) {
      return false;
    }
    Vector3 relative_motion = (target.transform.position - target_child.position);
    target.transform.parent = source_child;
    target.transform.localRotation = Quaternion.identity;
    target.transform.localPosition = Vector3.zero;
    Vector3 ss = source_child.localScale;
    Vector3 scale = new Vector3(1 / ss.x, 1 / ss.y, 1 / ss.z);
    target.transform.localPosition = relative_motion;
    target.transform.localPosition = Vector3.Scale(target.transform.localPosition, scale);
    return true;
  }
}
