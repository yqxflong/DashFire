using UnityEngine;
using System.Collections;
using DashFire;

public class BaseImpact : MonoBehaviour {

  protected float m_StartTime;
  protected bool m_IsActive = false;
  protected ImpactInfo m_ImpactInfo;
  protected AnimationManager m_AnimationPlayer;
  protected CharacterController m_CharacterController;
  protected float m_MovingTime = -1;
  protected float m_CurMovingTime;
  public Material m_NormalMaterial;
  public Material m_HighLightMaterial;
  protected bool m_IsHitHighLight = false;
  protected float m_HitHilghtLightTime = 0;
  public  Renderer m_Renderer;

  protected Vector3 m_CurVelocity;
  private GameObject m_ForTransformObj = null;
  private float m_MaxRayCastDis = 10.0f;

  protected enum HitDirection {
    FRONT,
    BACK,
  }

  protected HitDirection m_HitDirection = HitDirection.FRONT;

  public bool IsAcitve {
    get { return m_IsActive; }
    private set{}
  }

  public virtual void StopImpact() {
  }
  void Start() {
    m_ForTransformObj = new GameObject("");
    m_ForTransformObj.transform.position = Vector3.zero;
    m_ForTransformObj.transform.rotation = Quaternion.identity;
    m_AnimationPlayer = gameObject.GetComponentInChildren<AnimationManager>();
    if (null == m_AnimationPlayer) {
      m_AnimationPlayer = gameObject.GetComponent<AnimationManager>();
      if (null == m_AnimationPlayer) {
        Debug.LogError("GameObject has no AnimationPlayer");
      }
    }
    m_CharacterController = gameObject.GetComponent<CharacterController>();
    if (null == m_CharacterController) {
      Debug.LogError(this.name + " don't have CharacterController");
    }
  }
  public virtual ImpactType GetImpactType()
  {
    return ImpactType.Stiffness;
  }

  protected void GeneralStartImpact(ImpactInfo impactInfo) {
    m_StartTime = Time.time;
    m_IsActive = true;
    m_ImpactInfo = impactInfo;
    m_ImpactInfo.ApplyOffset(m_ImpactInfo.m_Offset);
    m_IsHitHighLight = m_ImpactInfo.m_IsHitHighLight;
    m_HitHilghtLightTime = m_ImpactInfo.m_HitHighLightTime;
    ComputeDirectionAndSpeed();
    m_CurVelocity = m_ImpactInfo.m_Velocity;
    if (!m_ImpactInfo.m_Velocity.Equals(Vector3.zero)) {
      if (Vector3.Dot(m_ImpactInfo.m_Velocity, this.transform.forward) > 0) {
        m_HitDirection = HitDirection.BACK;
      } else {
        m_HitDirection = HitDirection.FRONT;
      }
    }
    if (m_IsHitHighLight && null != m_HighLightMaterial) {
      m_Renderer.material = m_HighLightMaterial;
    }
    InterruptSkill();
    SharedGameObjectInfo info = LogicSystem.GetSharedGameObjectInfo(gameObject);
    ImpactPlaySound();
    if(null != info){
      LogicSystem.PublishLogicEvent("ge_set_ai_enable", "ai", info.m_LogicObjectId, false);
    }
    LogicSystem.NotifyGfxAnimationStart(gameObject);
    LogicSystem.NotifyGfxMoveControlStart(gameObject);
  }

  protected void ComputeDirectionAndSpeed()
  {
    switch (m_ImpactInfo.DirectionType) {
      case ImpactDirectionType.SenderDir:
        if (null != m_ImpactInfo.Attacker) {
          Vector3 direction = m_ImpactInfo.Attacker.transform.forward;
          m_ImpactInfo.m_Velocity = direction * m_ImpactInfo.HorizontalSpeed + new Vector3(0, m_ImpactInfo.VerticalSpeed, 0);
          m_ImpactInfo.m_Acceleration = m_ImpactInfo.Attacker.transform.TransformPoint(m_ImpactInfo.m_Acceleration);
        }
        break;
      case ImpactDirectionType.SenderToTargetDir:
        if (null != m_ImpactInfo.Attacker) {
          Vector3 direction = (this.transform.position - m_ImpactInfo.Attacker.transform.position).normalized;
          direction.y = 0;
          m_ImpactInfo.m_Velocity = direction * m_ImpactInfo.HorizontalSpeed + new Vector3(0, m_ImpactInfo.VerticalSpeed, 0);
          m_ImpactInfo.m_Acceleration = m_ImpactInfo.Attacker.transform.TransformPoint(m_ImpactInfo.m_Acceleration);
        }
        break;
      case ImpactDirectionType.Converge:
        InitConvergeOrDiverge(true);
        break;
      case ImpactDirectionType.Diverge:
        InitConvergeOrDiverge(false);
        break;
    }
  }

  private void InitConvergeOrDiverge(bool isConverge)
  {
    GameObject sender = m_ImpactInfo.Attacker;
    if (sender == null) {
      return;
    }
    Vector3 direction_point = sender.transform.TransformPoint(m_ImpactInfo.m_DirectionPoint);
    Vector3 force_point = sender.transform.TransformPoint(m_ImpactInfo.m_ForcePoint);
    float forceDistance = (transform.position - force_point).magnitude;
    Vector3 direction;
    if (isConverge) {
      direction = direction_point - transform.position;
    } else {
      direction = transform.position - direction_point;
    }
    direction.y = 0;
    Vector3 localVel = (forceDistance * m_ImpactInfo.m_VelocityFactorA + m_ImpactInfo.m_VelocityFactorB);
    m_ForTransformObj.transform.forward = direction;
    m_ImpactInfo.m_Velocity = m_ForTransformObj.transform.TransformPoint(localVel);
    Vector3 localAccel = (forceDistance * m_ImpactInfo.m_AccelFactorA + m_ImpactInfo.m_AccelFactorB);
    m_ImpactInfo.m_Acceleration = m_ForTransformObj.transform.TransformPoint(localAccel);
  }

  protected void GeneralTickImpact(float delatTime) {
    TickEffect(delatTime);
    TickAnimation(delatTime);
    TickMovement(delatTime);
    if (m_IsHitHighLight && Time.time > m_StartTime + m_HitHilghtLightTime) {
      m_IsHitHighLight = false;
      m_Renderer.material = m_NormalMaterial;
    }
    LogicSystem.NotifyGfxUpdatePosition(gameObject, this.transform.position.x, this.transform.position.y, this.transform.position.z, 0, this.transform.rotation.eulerAngles.y * Mathf.PI / 180f, 0);
  }
  protected void GeneralStopImpact() {
    m_StartTime = 0.0f;
    m_IsActive = false;
    m_ImpactInfo = null;

    if (m_IsHitHighLight) {
      m_IsHitHighLight = false;
      m_Renderer.material = m_NormalMaterial;
    }

    SharedGameObjectInfo info = LogicSystem.GetSharedGameObjectInfo(gameObject);
    if(null != info){
      LogicSystem.PublishLogicEvent("ge_set_ai_enable", "ai", info.m_LogicObjectId, true);
    }
    LogicSystem.NotifyGfxMoveControlFinish(gameObject);
    LogicSystem.NotifyGfxAnimationFinish(gameObject);
  }
  protected virtual void TickEffect(float deltaTime) {
    for (int i = m_ImpactInfo.m_EffectDatas.Count - 1; i >= 0; i--){
      m_ImpactInfo.m_EffectDatas[i].m_DelayTime -= deltaTime;
      if (m_ImpactInfo.m_EffectDatas[i].m_DelayTime <= 0) {
        PlayImpactEffect(m_ImpactInfo.m_EffectDatas[i]);
        m_ImpactInfo.m_EffectDatas.RemoveAt(i);
      }
    }
  }

  protected virtual void TickAnimation(float deltaTime) {
  }

  protected virtual void TickMovement(float deltaTime) {
    if (m_CurMovingTime >= 0 && Time.time < m_StartTime + m_CurMovingTime) {
      Vector3 motion = m_CurVelocity * deltaTime + m_ImpactInfo.m_Acceleration * deltaTime * deltaTime / 2;
      m_CharacterController.Move(motion);
      m_CurVelocity = m_CurVelocity + m_ImpactInfo.m_Acceleration * deltaTime;
      this.transform.position = GetOnLandPosition(this.transform.position);
    }
  }

  private Vector3 GetOnLandPosition(Vector3 pos) {
    if (null == Terrain.activeTerrain)
      return pos;
    return new Vector3(pos.x, Terrain.activeTerrain.SampleHeight(pos), pos.z);
  }
  protected void PlayImpactEffect(EffectData effectData) {
    if (EffectPositionType.Bone == effectData.m_PositionType) {
      PlayImpactEffect(effectData.m_EffectPrefab, effectData.m_Bone, effectData.m_LifeTime);
    } else if (EffectPositionType.Position == effectData.m_PositionType) {
      PlayImpactEffect(effectData.m_EffectPrefab, effectData.m_Position, effectData.m_LifeTime);
    } else if (EffectPositionType.BoneWithPosition == effectData.m_PositionType) {
      PlayImpactEffect(effectData.m_EffectPrefab, effectData.m_LifeTime, effectData.m_Position, effectData.m_Rotation, "Bone_Root");
    } else if (EffectPositionType.PositionWithDirection == effectData.m_PositionType) {
      PlayImpactEffect(effectData.m_EffectPrefab, effectData.m_LifeTime, effectData.m_Position, effectData.m_Rotation, m_ImpactInfo.m_Attacker);
    }
    else {
      Debug.LogWarning("BaseImpact::PlayEffect--Unkonwn EffectPositionType");
    }
  }
  protected void PlayImpactEffect(GameObject effectPrefab, Vector3 position, float playTime) {
    GameObject obj = ResourceSystem.NewObject(effectPrefab, playTime) as GameObject;
    if (null != obj) {
      obj.transform.position = this.transform.TransformPoint(position);
      obj.transform.rotation = Quaternion.identity;
    }
  }

  protected void PlayImpactEffect(GameObject effectPrefab, string bone, float playTime) {
    GameObject obj = ResourceSystem.NewObject(effectPrefab, playTime) as GameObject;
    Transform parent = FindChildRecursive(this.transform, bone);
    if (null != obj && null != parent) {
      obj.transform.parent = parent;
      obj.transform.localPosition = Vector3.zero;
    } else if (null != obj) {
      Debug.LogWarning("BaseImpact::PlayEffect -- Can't find bone {0} " + bone);
      GameObject.Destroy(obj);
    } else {
      Debug.LogWarning("BaseImpact::PlayEffect -- Create GameObject Failed.");
    }
  }

  
  protected void PlayImpactEffect(GameObject effectPrefab, float playTime, Vector3 postion, Vector3 rotation, string bone = "Bone_Root") {
    GameObject obj = ResourceSystem.NewObject(effectPrefab, playTime) as GameObject;
    Transform parent = FindChildRecursive(this.transform, bone);
    if (null != obj && null != parent) {
      obj.transform.parent = parent;
      obj.transform.localPosition = postion;
      obj.transform.localRotation = Quaternion.Euler(rotation);
    } else if (null != obj) {
      Debug.LogWarning("BaseImpact::PlayEffect -- Can't find bone " + bone);
      GameObject.Destroy(obj);
    } else {
      Debug.LogWarning("BaseImpact::PlayEffect -- Create GameObject Failed");
    }
  }

  protected void PlayImpactEffect(GameObject effectPrefab, float playTime, Vector3 position, Vector3 rotation, GameObject attacker) {
    if (null != attacker) {
      Vector3 direction = (attacker.transform.position - this.transform.position).normalized;
      direction = Quaternion.LookRotation(direction).eulerAngles;
      if(Vector3.zero != rotation)
      {
        direction = direction + rotation;
      }
      Vector3.RotateTowards(position, direction, Mathf.PI * 2, 20);
      GameObject obj = ResourceSystem.NewObject(effectPrefab, playTime) as GameObject;
      if (null != obj) {
        obj.transform.position = this.transform.TransformPoint(position);
        obj.transform.rotation = Quaternion.Euler(direction);
      }
    } else {
      Debug.LogWarning("Attacker is null");
    }
  }
  private void InterruptSkill() {
    MonsterBaseSkill[] skillLogics = gameObject.GetComponents<MonsterBaseSkill>();
    foreach (MonsterBaseSkill skillLogic in skillLogics) {
      if (null != skillLogic && skillLogic.IsActive) {
        skillLogic.OnInterrupt();
      }
    }
  }

  protected bool IsAlive() {
    SharedGameObjectInfo info = LogicSystem.GetSharedGameObjectInfo(gameObject);
    if(null != info && info.Blood > 0.0f){
      return true;
    }
    return false;
  }
  private Transform FindChildRecursive(Transform parent, string bonePath) {
    Transform t = parent.Find(bonePath);
    if (null != t) {
      return t;
    } else {
      int ct = parent.childCount;
      for (int i = 0; i < ct; ++i) {
        t = FindChildRecursive(parent.GetChild(i), bonePath);
        if (null != t) {
          return t;
        }
      }
    }
    return null;
  }

  private void ChangeMaterial(Material m) {
    if (null != m_Renderer) {
      Material[] newMaterials = new Material[1];
      newMaterials[0] = m;
      m_Renderer.materials = newMaterials;
      Debug.LogError("123");
    }
  }

  protected bool IsLogicDead() {
    SharedGameObjectInfo info = LogicSystem.GetSharedGameObjectInfo(gameObject);
    if (null != info) {
      return (info.Blood <= 0);
    }
    return true;
  }

  protected int GetLogicId(GameObject obj) {
    SharedGameObjectInfo info = LogicSystem.GetSharedGameObjectInfo(gameObject);
    if (null != info) {
      return info.m_LogicObjectId;
    }
    return -1;
  }

  protected void NotifyNpcDead(GameObject obj) {
    if (null != obj) {
      LogicSystem.PublishLogicEvent("ge_notify_npc_dead", "npc", GetLogicId(obj));
      //Debug.LogError("on npc dead" + GetLogicId(obj));
    }
  }

  protected float GetTerrainHeight(Vector3 pos) {
    if (Terrain.activeTerrain != null) {
      return Terrain.activeTerrain.SampleHeight(transform.position);
    } else {
      RaycastHit hit;
      pos.y += 2;
      if (Physics.Raycast(pos, -Vector3.up, out hit, m_MaxRayCastDis, 1 << LayerMask.NameToLayer("Terrains"))){
        return  hit.point.y;
      }
      return 0;
    }
  }

  protected void ImpactPlaySound() {
    AudioSource audioPlayer = gameObject.GetComponent<AudioSource>();
    if (null != audioPlayer) {
      System.Random ran = new System.Random();
      if (m_ImpactInfo.m_AudioClips.Count > 0) {
        audioPlayer.clip = m_ImpactInfo.m_AudioClips[ran.Next(0, m_ImpactInfo.m_AudioClips.Count)];
        audioPlayer.Play();
      }
    } else {
      Debug.LogWarning(gameObject + " don't have AudioSource");
    }
  }
  protected void SetEndure(GameObject obj, bool isEndure) {
    CharacterCamp cc = obj.GetComponent<CharacterCamp>();
    if (null != cc) {
      cc.SetEndure(isEndure);
    }
  }
}
