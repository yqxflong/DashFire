using UnityEngine;
using System.Collections;
using DashFire;

public class Bluelf_SkillThrowSpear : MonsterBaseSkill{

  public AnimationClip m_ThrowAnim;
  public AnimationClip m_IdleAnim;
  public string m_SpearName;
  public GameObject m_SpearPrefab;
  private GameObject m_SpearInHand;
  private Vector3 m_TargetPos;
	// Use this for initialization
	void Start () {
    m_AnimationPlayer = gameObject.GetComponent<AnimationManager>();
    if (null == m_AnimationPlayer) {
      Debug.LogError("Skill must have animationPlayer");
    }
	}

  public override void StartSkill() {
    GeneralStartSkill();
    StartSpellBar(this.transform.position + new Vector3(0.0f, 1.8f, 0.0f), 0.6f, gameObject);
    GameObject player = GameObject.FindGameObjectWithTag("Player");
    if (null != player) {
      m_TargetPos = player.transform.position + new Vector3(0f, 0.4f, 0f);
    } else {
      Debug.LogError("Skill ThrowSpear can't find target.Stop skill.");
      StopSkill();
      return;
    }
    m_SpearInHand = GetWeaponObj(m_SpearName);
    if (null != m_ThrowAnim) {
      m_AnimationPlayer.Play(m_ThrowAnim.name);
    }
  }
	// Update is called once per frame
	void Update () {
	}

  public void AnimEvent_ThrowSpear() {
    if (null != m_SpearPrefab && null != m_SpearInHand) {
      GameObject spear = ResourceSystem.NewObject(m_SpearPrefab) as GameObject;
      if (null != spear) {
        spear.transform.position = m_SpearInHand.transform.position;
        spear.transform.rotation = Quaternion.LookRotation(m_TargetPos - spear.transform.position);
        spear.SendMessage("SetSender", gameObject);
      }
      m_SpearInHand.SetActive(false);
    }
  }

  public override void StopSkill() {
    if (null != m_SpearPrefab) {
      m_SpearInHand.SetActive(true);
    }
    StopSpell(gameObject);
    GeneralStopSkill();
    if (null != m_IdleAnim) {
      m_AnimationPlayer.CrossFade(m_IdleAnim.name);
    }
  }
  public void AnimEnvet_ReloadSpear() {
    StopSkill();
  }

  private GameObject GetWeaponObj(string weaponName) {
    Transform weaponTransform = LogicSystem.FindChildRecursive(this.transform, weaponName);
    if (null != weaponTransform) {
      return weaponTransform.gameObject;
    }
    return null;
  }
}
