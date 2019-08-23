using UnityEngine;
using System.Collections;

public class AnimationManager : MonoBehaviour {

  public Animation m_Animation;
  private float m_AnimationSpeed = 1.0f;

  public float AnimationSpeed {
    get { return m_AnimationSpeed; }
    set {
      m_AnimationSpeed = value;
      SetAnimationSpeed(m_AnimationSpeed);
    }
  }

	void Start () {
    if (null == m_Animation) {
      Debug.LogError("There is no animation");
    } else {
    }
	}
	
	// Update is called once per frame
	void Update () {
    if (!m_Animation.isPlaying) {
    }
	}


  private void SetAnimationSpeed(float speed){
    foreach(AnimationState animState in m_Animation){
      animState.speed = speed;
    }
  }

  public void Play(string animationName) {
    m_Animation.Play(animationName);
  }

  public void Play(AnimationClip clip) {
    if (null != clip) {
      m_Animation.Play(clip.name);
    }
  }

  public void PlayQueue(string animationName) {
    m_Animation.PlayQueued(animationName);
  }

  public void PlayQueue(AnimationClip clip) {
    if (null != clip) {
      m_Animation.PlayQueued(clip.name);
    }
  }
  public void CrossFade(string animationName) {
    m_Animation.CrossFade(animationName);
  }

  public void CrossFade(AnimationClip clip) {
    if (null != clip) {
      m_Animation.Play(clip.name);
    }
  }

  public void CrossFade(string animationName, float time) {
    m_Animation.CrossFade(animationName, time);
  }

  public void CrossFade(AnimationClip clip, float time) {
    if (null != clip) {
      m_Animation.CrossFade(clip.name, time);
    }
  }

  public bool IsPlaying(string animationName) {
    return m_Animation.IsPlaying(animationName) && m_Animation[animationName].normalizedTime < 1.0f;
  }

  public bool IsPlaying(AnimationClip clip) {
    if (null != clip) {
      return (m_Animation.IsPlaying(clip.name) && m_Animation[clip.name].normalizedTime < 1.0f);
    }
    return false;
  }

  public void Pause() {
    foreach (AnimationState anim in animation)
    {
      anim.speed = 0;
    }
  }
  public void EndPause() {
    foreach (AnimationState anim in animation)
    {
      anim.speed = 1;
    }
  }

  public float AnimationLenth(string animationName) {
    return m_Animation[animationName].length / m_AnimationSpeed;
  }

  public float AnimationLenth(AnimationClip clip) {
    if (null != clip) {
      return m_Animation[clip.name].length / m_AnimationSpeed;
    }
    return 0.0f;
  }

  public void StopAnimation() {
    m_Animation.Stop();
  }

  public void StopAnimation(string animationName) {
    m_Animation.Stop(animationName);
  }

  public void StopAnimation(AnimationClip clip) {
    if (null != clip) {
      m_Animation.Stop(clip.name);
    }
  }

  public void SetAnimationState(string animName, int layer, float weight, float speed, AnimationBlendMode mode) {
    this.animation[animName].layer = layer;
    this.animation[animName].weight = weight;
    this.animation[animName].speed = speed;
    this.animation[animName].blendMode = mode;
  }

  public void SetAnimationEnable(string animName, bool enable) {
    this.animation[animName].enabled = enable;
  }

  public void SetAnimationWeight(string animName, float weight) {
    foreach (AnimationState anim in this.animation) {
      anim.weight = anim.weight * (1 - weight);
    }
    this.animation[animName].weight = animation[animName].weight * weight;
  }

  public void AddMixingTranform(string animName, Transform mix, bool recursive = true) {
    this.animation[animName].AddMixingTransform(mix, recursive);
  }

  public void StopAllAnim() {
    this.animation.Stop();
  }
}
