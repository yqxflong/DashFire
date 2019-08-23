using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using DashFire;

public enum SceneObjectType {
  None = -1,
  LinearBullet,
  Count,
}

[System.Serializable]
public class SceneObject_BaseInfo {
  public int Id = 0;
  public GameObject Attacker;
  public SkillEffectInfo SceneObjInfo;
  public string SceneObjDesc = "No Description";
}

public interface ISceneObjectLogic {
  void Active(SceneObject_BaseInfo info);
  GameObject GetAttacker();
}

public interface ISceneObjectBulletLogic {
  // Move
  void StartMove();
  void OnStartMove();
  void OnEndMove();
  void OnMove();
}