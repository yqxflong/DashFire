using UnityEngine;
using System.Collections;
using DashFire;
using System;

public class MonsterAnimationEvent : MonoBehaviour {
  public GameObject m_DeadEffect;
  public GameObject m_MeetEnemyEffect;
  public string m_MeetEnemyEffectBone = "ef_head";
  public string m_DeadEffectActor;
  public float m_DeadEffectDuration = 2.0f;

  public void AnimationEvent_OnDead() {
  }

  public void OnDead() {
    if (null != m_DeadEffect) {
      GameObject deadEffect = ResourceSystem.NewObject(m_DeadEffect, m_DeadEffectDuration) as GameObject;
      if(null != deadEffect){
        deadEffect.transform.position = this.transform.position + this.transform.up * 0.5f;
        deadEffect.transform.rotation = Quaternion.identity;
      }
    } else {
      Debug.LogError(gameObject.name + " don't have a dead effect");
    }
  }

  public void OnMeetEnemy() {
    if (null != m_MeetEnemyEffect) {
      GameObject meetEnemyEffect = ResourceSystem.NewObject(m_MeetEnemyEffect, 2.0f) as GameObject;
      if (null != meetEnemyEffect) {
        Transform parent = LogicSystem.FindChildRecursive(this.transform, m_MeetEnemyEffectBone);
        if (null != parent) {
          meetEnemyEffect.transform.parent = parent;
          meetEnemyEffect.transform.localPosition = Vector3.zero;
          meetEnemyEffect.transform.localRotation = Quaternion.identity;
        }
      }
    } else {
      Debug.LogError(gameObject.name + " don't have a meet enemy effect");
    }
  }

  public void PlaySound(AudioClip clip) {
    AudioSource soundPlayer = gameObject.GetComponent<AudioSource>();
    if (null != soundPlayer) {
      soundPlayer.clip = clip;
      soundPlayer.Play();
    }
  }
}
