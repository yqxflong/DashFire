using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EffectInfo
{
  public GameObject effect;
  public float remainTime;
}

public class EffectManager : MonoBehaviour {
  private SkillColliderManager skill_collide_manager_;
  private SkillManager skill_manager_;
	// Use this for initialization
	void Start () {
    skill_collide_manager_ = gameObject.GetComponent<SkillColliderManager>();
    skill_manager_ = gameObject.GetComponent<SkillManager>();
	}

	// Update is called once per frame
	void Update () {
	}

  public void PlayEffect(string effectinfo)
  {
    string[] effect_param = effectinfo.Split(' ');
    if (effect_param.Length < 2) {
      return;
    }
    string child_name = effect_param[0];
    string effect_name = effect_param[1];
    Transform node = GetChildNodeByName(gameObject, child_name);
    if (node == null) {
      Debug.Log("--attach effect: can't find node by name " + child_name);
      return;
    }
    float remaintime = 5;
    if (effect_param.Length >= 3) {
      remaintime = (float)System.Convert.ToDouble(effect_param[2]);
    }
    GameObject effect = DashFire.ResourceSystem.NewObject(effect_name, remaintime) as GameObject;
    if (effect == null) {
      Debug.Log("--attach effect: create effect by name " + effect_name + " failed!");
      return;
    }
    effect.transform.position = node.transform.position;
    effect.transform.rotation = node.transform.rotation;
    effect.transform.parent = node.transform;
    SkillScript ss = skill_manager_.GetCurPlaySkill();
    if (ss != null) {
      ss.AddSkillEffect(effect);
    }
  }

  public void PlayEffectAtPos(string effect_str)
  {
    string[] effect_params = effect_str.Split(' ');
    if (effect_params.Length < 5) {
      return;
    }
    bool is_need_collide = System.Convert.ToBoolean(effect_params[0]);
    if (is_need_collide) {
      if (skill_collide_manager_ == null || !skill_collide_manager_.IsCollided()) {
        return;
      }
    }
    Vector3 pos = new Vector3();
    pos.x = (float)System.Convert.ToDouble(effect_params[1]);
    pos.y = (float)System.Convert.ToDouble(effect_params[2]);
    pos.z = (float)System.Convert.ToDouble(effect_params[3]);
    string effect_name = effect_params[4];
    float remaintime = 5;
    if (effect_params.Length >= 6) {
       remaintime = (float)System.Convert.ToDouble(effect_params[5]);
    }
    Vector3 effect_pos = gameObject.transform.TransformPoint(pos);
    GameObject effect = DashFire.ResourceSystem.NewObject(effect_name, remaintime) as GameObject; 
    if (effect == null) {
      Debug.Log("--attach effect: create effect by name " + effect_name + " failed!");
      return;
    }
    effect.transform.position = effect_pos;
    effect.transform.rotation = gameObject.transform.rotation;
    SkillScript ss = skill_manager_.GetCurPlaySkill();
    if (ss != null) {
      ss.AddSkillEffect(effect);
    }
  }

  public static Transform GetChildNodeByName(GameObject gameobj, string name)
  {
    Transform[] ts = gameobj.transform.GetComponentsInChildren<Transform>();
    foreach (Transform t in ts) {
      if (t.name == name) {
        return t;
      }
    }
    return null;
  }
}
