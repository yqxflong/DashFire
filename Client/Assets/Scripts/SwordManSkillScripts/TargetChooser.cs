using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TargetChooser {
  public static List<GameObject> FindTargetInSector(Vector3 center,
                                                    float radius,
                                                    Vector3 direction,
                                                    Vector3 degreeCenter,
                                                    float degree) {
    List<GameObject> result = new List<GameObject>();
    Collider[] colliders = Physics.OverlapSphere(center, radius);
    direction.y = 0;
    foreach (Collider co in colliders) {
      GameObject obj = co.gameObject;
      Vector3 targetDir = obj.transform.position - degreeCenter;
      targetDir.y = 0;
      if (Mathf.Abs(Vector3.Angle(targetDir, direction)) <= degree) {
        result.Add(obj);
      }
    }
    return result;
  }

  public static GameObject GetNearestObj(Vector3 targetpos, List<GameObject> list) {
    GameObject target = null;
    float nearest_distance = 0;
    foreach (GameObject obj in list) {
      if (target == null) {
        nearest_distance = GetPowerDistance(targetpos, obj.transform.position);
        target = obj;
      } else {
        float powerdistance = GetPowerDistance(targetpos, obj.transform.position);
        if (nearest_distance > powerdistance) {
          nearest_distance = powerdistance;
          target = obj;
        }
      }
    }
    return target;
  }

  public static GameObject GetMostForwardObj(Vector3 targetpos,
                                             Vector3 direction,
                                             List<GameObject> list) {
    GameObject target = null;
    float min_degree = 0;
    foreach (GameObject obj in list) {
      Vector3 targetDir = obj.transform.position - targetpos;
      float angle = Vector3.Angle(targetDir, direction);
      if (target == null) {
        min_degree = angle;
        target = obj;
      } else {
        if (min_degree > angle) {
          min_degree = angle;
          target = obj;
        }
      }
    }
    return target;
  }

  private static float GetPowerDistance(Vector3 begin, Vector3 end) {
    Vector3 v = end - begin;
    return v.x * v.x + v.y * v.y + v.z * v.z;
  }
}