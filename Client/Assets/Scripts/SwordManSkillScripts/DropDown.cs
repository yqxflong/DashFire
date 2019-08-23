using UnityEngine;
using System.Collections;

public class DropDown : SkillScript {
  public AnimationClip m_DropingAnim;
  public AnimationClip m_GroundAnim;
  public float m_DropingAnimSpeed = 1;
  public float m_GroundAnimSpeed = 1;
  public float m_GroundHeight = 0;
  public string m_DropCurve;

  private bool m_IsGroundAnimPlayed = false;
	// Use this for initialization
	void Start () {
    base.Init();
    if (m_ObjAnimation != null) {
      if (m_DropingAnim != null) {
        m_ObjAnimation[m_DropingAnim.name].speed = m_DropingAnimSpeed;
      }
      if (m_GroundAnim != null) {
        m_ObjAnimation[m_GroundAnim.name].speed = m_GroundAnimSpeed;
      }
    }
	}

	// Update is called once per frame
	void Update () {
    base.UpdateSkill();
    if (!m_IsSkillActive) {
      return;
    }
    if (!m_IsGroundAnimPlayed) {
      if (m_SkillMovement.GetHeightWithGround() <= m_GroundHeight || m_SkillMovement.IsGrounded()) {
        m_ObjAnimation[m_GroundAnim.name].wrapMode = WrapMode.ClampForever;
        m_ObjAnimation[m_GroundAnim.name].speed = m_GroundAnimSpeed;
        m_ObjAnimation.CrossFade(m_GroundAnim.name, m_CrossFadeTime);
        m_IsGroundAnimPlayed = true;
      }
      if (m_SkillMovement.IsGrounded()) {
        m_SkillMovement.StopCurveMove();
      }
    }
	}

  public override void StopSkill() {
    base.StopSkill();
    if (CanStop()) {
      m_IsGroundAnimPlayed = false;
    }
  }

  public override bool StartSkill() {
    if (!base.StartSkill()) {
      return false;
    }
    m_ObjAnimation[m_DropingAnim.name].speed = m_DropingAnimSpeed;
    m_ObjAnimation[m_DropingAnim.name].weight = m_StartWeight;
    m_ObjAnimation[m_DropingAnim.name].wrapMode = WrapMode.Loop;
    m_ObjAnimation.CrossFade(m_DropingAnim.name, m_CrossFadeTime);
    m_SkillMovement.StartCurveMove(m_DropCurve);
    m_IsGroundAnimPlayed = false;
    return true;
  }

  public override AnimationClip GetCurAnimationClip() {
    if (m_IsGroundAnimPlayed) {
      return m_GroundAnim;
    } else {
      return m_DropingAnim;
    }
  }

  protected override bool CheckSkillOver() {
    if (m_Status != MyCharacterStatus.kSkilling) {
      return true;
    }
    if (m_ObjAnimation[m_GroundAnim.name].normalizedTime >= 1) {
      m_ClampedTime += Time.deltaTime;
      if (m_ClampedTime > m_ClampTime) {
        return true;
      }
    }
    return false;
  }
}
