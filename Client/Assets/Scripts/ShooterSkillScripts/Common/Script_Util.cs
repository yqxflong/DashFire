using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using DashFire;

public enum TargetStateType {
  STAND,
  STIFFNESS,
  HITFLY,
  KNOKDOWN,
}

public static class Script_Util {
  private static string[] s_VecSplitString = new string[] { " " };
  private static string[] s_ListSplitString = new string[] { "," };
  public static Vector3 CaculateFlyPos(Vector3 srcPos, Vector3 dir, float speed, float delta) {
    return srcPos + dir.normalized * speed * delta;
  }
  public static Vector3 CaculateRelativePos(Vector3 srcPos, Vector3 dir, float distance) {
    return srcPos + dir.normalized * distance;
  }
  public static bool AttachGameObject(GameObject parent, GameObject obj, string path) {
    if (null != obj && null != obj.transform && null != parent && null != parent.transform) {
      Transform t = LogicSystem.FindChildRecursive(parent.transform, path);
      if (null != t) {
        obj.transform.parent = t;
        obj.transform.localPosition = Vector3.zero;
        obj.transform.localRotation = Quaternion.identity;
        return true;
      } else {
        LogicSystem.LogicLog("AttachGameObject obj{1} can't attach to parent{1} at bone {2}", obj.name, parent.name, path);
      }
    }
    return false;
  }
  public static void DetachGameObject(GameObject obj) {
    if (null != obj && null != obj.transform) {
      obj.transform.parent = null;
    }
  }
  public static bool AttachGameObject(Transform parent, Transform obj, string path) {
    if (null != obj && null != parent) {
      Transform t = LogicSystem.FindChildRecursive(parent, path);
      if (null != t) {
        obj.transform.parent = t;
        obj.transform.localPosition = Vector3.zero;
        obj.transform.localRotation = Quaternion.identity;
        return true;
      } else {
        LogicSystem.LogicLog("AttachGameObject obj{1} can't attach to parent{1} at bone {2}", obj.name, parent.name, path);
      }
    }
    return false;
  }
  public static bool AttachGameObjectDirect(Transform parent, Transform obj) {
    if (null != obj && null != parent) {
      obj.transform.parent = parent;
      obj.transform.localPosition = Vector3.zero;
      obj.transform.localRotation = Quaternion.identity;
      return true;
    }
    return false;
  }
  public static void DetachGameObject(Transform obj) {
    if (null != obj) {
      obj.parent = null;
    }
  }
  public static void SortRaycastHit(ref RaycastHit[] hitInfos) {
    List<RaycastHit> tHitArray = new List<RaycastHit>(hitInfos);
    tHitArray.Sort(delegate(RaycastHit first, RaycastHit second) {
      if (first.distance == second.distance) return 0;
      else if (first.distance > second.distance) return 1;
      else return -1;
    });
    hitInfos = tHitArray.ToArray();
  }


  public static void AdjustImpactEffectDataPos(GameObject target, GameObject attacker, ImpactInfo info, Vector3 hitPos, Vector3 hitDir) {
    foreach (EffectData data in info.m_EffectDatas) {
      if (data.m_PositionType == EffectPositionType.PositionWithDirection) {
        data.m_Position = hitPos - target.transform.position;
        info.m_Attacker = attacker;
      } else if (data.m_PositionType == EffectPositionType.BoneWithPosition) {
        data.m_Position = hitPos - target.transform.position;
        data.m_Rotation = Quaternion.LookRotation(hitDir).eulerAngles;
      }
    }
  }
  public static bool IsChildGameObject(GameObject target, int id) {
    if (target == null || target.transform == null) {
      return false;
    }
    if (target.GetInstanceID() == id) {
      return true;
    }

    int childCount = target.transform.childCount;
    for (int index = 0; index < childCount; index++) {
      Transform child = target.transform.GetChild(index);
      if (child.gameObject.GetInstanceID() == id) {
        return true;
      } else {
        if (IsChildGameObject(child.gameObject, id)) {
          return true;
        }
      }
    }
    return false;
  }
  public static GameObject CreateGameObjectByName(string name) {
    UnityEngine.Object effect_prefab = Resources.Load(name);
    if (effect_prefab == null) {
      LogicSystem.GfxLog("CreateGameObjectByName load resource failed:{0}", name);
      return null;
    }
    GameObject effect = GameObject.Instantiate(effect_prefab) as GameObject;
    if (effect == null) {
      LogicSystem.GfxLog("CreateGameObjectByName Instantiate resource failed:{0}", name);
      return null;
    }
    return effect;
  }
  public static GameObject CreateGameObjectByAsset(GameObject asset) {
    if (asset == null) {
      LogicSystem.GfxLog("CreateGameObjectByAsset load resource failed:{0}", asset.name);
      return null;
    }
    GameObject effect = GameObject.Instantiate(asset) as GameObject;
    if (effect == null) {
      LogicSystem.GfxLog("CreateGameObjectByName Instantiate resource failed:{0}", asset.name);
      return null;
    }
    return effect;
  }
  public static string[] SplitParam(string param, int forceParamNum) {
    string[] result = param.Split(s_ListSplitString, StringSplitOptions.None);
    if (result.Length < forceParamNum) {
      Debug.Log("SplitParam param:" + param + " ForceNum:" + forceParamNum);
      return null;
    }
    return result;
  }
  public static Vector3 ToVector3(string vec) {
    Vector3 path = Vector3.zero;
    string strPos = vec;
    try {
      string[] result = strPos.Split(s_VecSplitString, StringSplitOptions.None);
      if (result != null && result.Length == 3) {
        path = new Vector3(Convert.ToSingle(result[0]),
            Convert.ToSingle(result[1]),
            Convert.ToSingle(result[2]));
      }
    } catch (System.Exception ex) {
      LogicSystem.LogicLog("ConvertVector3 vec:{0} stacktrace:{1}",
        vec, ex.StackTrace);
    }
    return path;
  }
  public static float GetHeightWithGround(Transform transform, int rayDis = 10) {
    if (Terrain.activeTerrain != null) {
      return transform.position.y - Terrain.activeTerrain.SampleHeight(transform.position);
    } else {
      RaycastHit hit;
      if (Physics.Raycast(transform.position, -Vector3.up, out hit, rayDis, (int)SceneLayerType.SceneStatic)) {
        return transform.position.y - hit.point.y;
      }
      return 0;
    }
  }
  public static bool IsOnGround(GameObject target, float tweak = 0.2f) {
    CharacterController controller = target.GetComponent<CharacterController>();
    if (false && controller != null) {
      return controller.isGrounded;
    } else {
      return GetHeightWithGround(target.transform) <= tweak;
    }
  }
  public static float ForceNotifyEffectTime(float ts) {
    if (ts <= 0) {
      ts = 0;
    }
    return ts;
  }
  public static string ForceNotifyEffectBone(string bone) {
    if (string.IsNullOrEmpty(bone)) {
      bone = "Bip001";
    }
    return bone;
  }
  public static void DrawPhysicsSphereCastLine(Ray ray, float distance, float radius, Color color, float duration) {
    Vector3 rotForward = new Vector3(0, 90.0f, 0);
    Vector3 rotBackward = new Vector3(0, -90.0f, 0);
    Vector3 rotUp = new Vector3(90.0f, 0, 0);
    Vector3 rotDown = new Vector3(-90.0f, 0, 0);
    Vector3 forwardOff = Quaternion.Euler(rotForward) * ray.direction * radius;
    Vector3 backwardOff = Quaternion.Euler(rotBackward) * ray.direction * radius;
    Vector3 upOff = Quaternion.Euler(rotUp) * ray.direction * radius;
    Vector3 downOff = Quaternion.Euler(rotDown) * ray.direction * radius;
    Debug.DrawRay(ray.origin, ray.direction * distance, color, duration, true);
    Debug.DrawRay(ray.origin + forwardOff, ray.direction * distance, color, duration, true);
    Debug.DrawRay(ray.origin + backwardOff, ray.direction * distance, color, duration, true);
    Debug.DrawRay(ray.origin + upOff, ray.direction * distance, color, duration, true);
    Debug.DrawRay(ray.origin + downOff, ray.direction * distance, color, duration, true);
  }
  public static void DrawPhysicsRayLine(Ray ray, float distance, Color color, float duration) {
    Debug.DrawRay(ray.origin, ray.direction * distance, color, duration, true);
  }
  public static void DrawPosWithRayLine(Vector3 targetPos, Color color, float duration) {
    Ray ray = new Ray(targetPos, Vector3.up);
    DrawPhysicsRayLine(ray, 5.0f, color, duration);
  }
  public static Vector3 AdjustTargetPos(Transform roleTrans, Vector3 targetPos, Transform cameraTrans, float weaponHeight, Vector3 gunRotate) {
    DrawPosWithRayLine(targetPos, Color.blue, 10.0f);
    Vector3 retTargetPos = targetPos;

    Vector3 targetDis = targetPos - roleTrans.transform.position;
    Vector3 targetPosRot = Quaternion.Euler(gunRotate) * targetDis;
    retTargetPos = roleTrans.transform.position + targetPosRot;

    if (!IsTargetPosCharacter(targetPos)) {
      float cameraYRot = cameraTrans.rotation.eulerAngles.x;
      cameraYRot = Mathf.Abs(90 - Mathf.Abs(cameraYRot));
      float targetPosOff = weaponHeight * Mathf.Tan(cameraYRot * Mathf.PI / 180);

      Vector3 targetCameraDir = cameraTrans.forward * (-1);
      targetCameraDir.y = 0;
      retTargetPos += targetCameraDir.normalized * targetPosOff;
    }
    DrawPosWithRayLine(retTargetPos, Color.red, 10.0f);

    return retTargetPos;
  }
  public static bool IsTargetPosCharacter(Vector3 targetPos, float rayLength = 5.0f) {
    targetPos += new Vector3(0, -1.0f, 0); 
    Ray ray = new Ray(targetPos, Vector3.up);
    LayerMask mask = (int)(SceneLayerType.Character | SceneLayerType.Player);
    if (Physics.Raycast(ray, rayLength, mask.value)) {
      return true;
    }
    return false;
  }
  public static Vector3 GetRoleFaceTargetPos(GameObject role, float dis = 2.0f) {
    Vector3 posOff = new Vector3(0, 0, dis);
    Vector3 targetPos = role.transform.position + role.transform.rotation * posOff;
    return targetPos;
  }
}
