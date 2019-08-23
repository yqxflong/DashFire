using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class SkillIcon : MonoBehaviour {

	// Use this for initialization
	void Start () {
    
	}
	
	// Update is called once per frame
  void Update()
  {
    if (m_CountDown > 0) {
      m_CountDown -= RealTime.deltaTime;
      if (m_CountDown <= 0) {
        Destroy(this.gameObject);
      }
    }
    if (collisonTime > 0) {
      collisonTime -= RealTime.deltaTime;
      if (collisonTime <= 0) {
        Destroy(this.gameObject);
      }
    }
  }

  public void OnClick()
  {
    GameObject go = this.gameObject;
      if (go != null) {
        SkillControllerInterface skillCtrl = GetControl();
        if (skillCtrl != null) {
          skillCtrl.PushSkill(m_SkillCat, Vector3.zero);
          collisonTime = 0.3f;
          SetCountDown(collisonTime);
          AddTweenScale();
        }
      }
      if (this.transform.parent != null) {
        for (int index = 0; index < this.transform.parent.childCount; ++index) {
          Transform trans = this.transform.parent.GetChild(index);
          if (null != trans) {
            go = trans.gameObject;
            if (go.name == "SkillIcon(Clone)") {
              if (go != null && go != this.gameObject)
                Destroy(go);
            }
          }
        }
    }
  }

  private void AddTweenScale()
  {
    TweenScale tScale = GetComponent<TweenScale>();
    if (tScale != null) {
      Destroy(tScale);
    }
    tScale = this.gameObject.AddComponent<TweenScale>();
    if (tScale != null) {
      
      tScale.from = new  Vector3(1f, 1f, 1f);
      tScale.to = new Vector3(1.2f, 1.2f, 1.2f);
      tScale.duration = duration;
      tScale.animationCurve = animationCurve;
      tScale.PlayForward();
    }
    TweenAlpha tAlpha = this.GetComponent<TweenAlpha>();
    if (tAlpha != null) {
      tAlpha.enabled = true;
      tAlpha.ResetToBeginning();
      tAlpha.PlayForward();
    }
  }

  public void SetState()
  {
    UIButton btn = GetComponent<UIButton>();
    if (null != btn) {
      btn.UpdateColor(true, true);
    }
  }
  public void SetSkillTargetPos(Vector3 targetPos)
  {
    
  }

  public void SetSkillId(SkillCategory id)
  {
    m_SkillCat = id;
    SetIcon();
  }

  public SkillCategory GetSkillId()
  {
    return m_SkillCat;
  }

  private SkillControllerInterface GetControl()
  {
    SkillControllerInterface SkillCtrl = null;
    GameObject go = GameObject.FindGameObjectWithTag("Player");
    if (null != go) {
      BaseSkillManager skill_Manager = go.GetComponent<BaseSkillManager>();
      if (null != skill_Manager) {
        SkillCtrl = skill_Manager.GetSkillController();
      }
    }
    return SkillCtrl;
  }
  private void SetIcon()
  {
    UISprite sp = GetComponent<UISprite>();
    if (sp == null)
      return;
    switch (m_SkillCat) {
      case SkillCategory.kSkillQ:
        sp.spriteName = "A";break;
      case SkillCategory.kSkillE:
      //case SkillCategory.kSkillC:
        sp.spriteName = "B"; break;
      default: sp.spriteName = "Skill2"; break;
    }
  }
  public void SetCountDown(float time)
  {
    m_CountDown = time;
  }

  public float duration = 0.2f;
  public AnimationCurve animationCurve = null;
  private float collisonTime = 0f;
  private float m_CountDown = 0f;
  private SkillCategory m_SkillCat = SkillCategory.kNone;
}
