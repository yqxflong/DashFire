using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DashFire;

[RequireComponent(typeof(CharacterController))]
public class MonsterBaseSkill : MonoBehaviour{

  public float m_CoolDownTime;
  public int m_SkillId = 10000;

  protected float m_LastTriggerTime = 0.0f;
  private bool m_IsActive = false;
  protected AnimationManager m_AnimationPlayer;
  protected List<GameObject> m_EffectList = new List<GameObject>();

  public bool IsActive {
    get { return m_IsActive; }
    private set { }
  }

  public virtual void StartSkill() {
  }

  public virtual void StopSkill() {

  }
  protected void GeneralStartSkill(){
    m_IsActive = true;
    m_LastTriggerTime = Time.time;
    m_EffectList.Clear();
    SharedGameObjectInfo info = LogicSystem.GetSharedGameObjectInfo(gameObject);
    if(null != info){
      LogicSystem.PublishLogicEvent("ge_set_ai_enable", "ai", info.m_LogicObjectId, false);
    }
    LogicSystem.NotifyGfxAnimationStart(gameObject);
    LogicSystem.NotifyGfxMoveControlStart(gameObject);
  }

  protected void GeneralStopSkill() {
    m_IsActive = false;
    m_AnimationPlayer.StopAllAnim();
    SharedGameObjectInfo info = LogicSystem.GetSharedGameObjectInfo(gameObject);
    foreach (GameObject obj in m_EffectList) {
      ResourceSystem.RecycleObject(obj);
    }
    m_EffectList.Clear();
    if(null != info){
      LogicSystem.PublishLogicEvent("ge_set_ai_enable", "ai", info.m_LogicObjectId, true);
    }
    LogicSystem.NotifyGfxMoveControlFinish(gameObject);
    LogicSystem.NotifyGfxAnimationFinish(gameObject);
  }

  public bool IsCoolingDown() {
    return (Time.time < m_LastTriggerTime + m_CoolDownTime);
  }

  public bool IsCastSkill() {
    return m_IsActive;
  }

  public virtual bool CanInterrupt() {
    return true;
  }
  public bool OnInterrupt() {
    if (CanInterrupt()) {
      StopSkill();
      return true;
    }
    return false;
  }
  protected List<GameObject> GetObjInSector(Vector3 center, float radius, float angle) {
    List<GameObject> result = new List<GameObject>();
    Collider[] cols = Physics.OverlapSphere(center, radius);
    if (cols.Length > 0) {
      foreach (Collider col in cols) {
        if (col.gameObject.tag == "Player") {
          Quaternion targetRot = Quaternion.LookRotation(col.transform.position - this.transform.position);
          if (Mathf.Abs(Quaternion.Angle(targetRot, this.transform.rotation)) < angle) {
            result.Add(col.gameObject);
            //Debug.Log("do damage to " + col.gameObject.name);
          }
        }
      }
    }
    return result;
  }

  protected void StartSpellBar(Vector3 pos, float duration, GameObject obj) {
    SharedGameObjectInfo info = LogicSystem.GetSharedGameObjectInfo(obj);
    GameObject uiRoot = GameObject.FindGameObjectWithTag("UI");
    if (null != uiRoot && null != info) {
      DFMUiRoot uiMgr = uiRoot.GetComponent<DFMUiRoot>();
      if (null != uiMgr) {
        uiMgr.ShowMonsterPrePower(pos.x, pos.y, pos.z, duration, info.m_LogicObjectId);
      }
    }
  }

  protected void StopSpell(GameObject obj)
  {
    SharedGameObjectInfo info = LogicSystem.GetSharedGameObjectInfo(obj);
    GameObject uiRoot = GameObject.FindGameObjectWithTag("UI");
    if (null != uiRoot && null != info) {
      DFMUiRoot uiMgr = uiRoot.GetComponent<DFMUiRoot>();
      if (null != uiMgr) {
        uiMgr.BreakPrePower(info.m_LogicObjectId);
      }
    }
  }

  protected void SetEndure(GameObject obj, bool isEndure) {
    CharacterCamp cc = obj.GetComponent<CharacterCamp>();
    if (null != cc) {
      cc.SetEndure(isEndure);
    }
  }

  protected bool IsLogicDead() {
    SharedGameObjectInfo info = LogicSystem.GetSharedGameObjectInfo(gameObject);
    if (null != info) {
      return (info.Blood <= 0);
    }
    return true;
  }

  protected void NotifyNpcDead(GameObject obj) {
    if (null != obj) {
      LogicSystem.PublishLogicEvent("ge_notify_npc_dead", "npc", GetLogicId(obj));
      //Debug.LogError("on npc dead" + GetLogicId(obj));
    }
  }

  protected int GetLogicId(GameObject obj) {
    if (null != obj) {
      SharedGameObjectInfo info = LogicSystem.GetSharedGameObjectInfo(obj);
      if (null != info) {
        return info.m_LogicObjectId;
      }
    }

    return -1;
  }
}
