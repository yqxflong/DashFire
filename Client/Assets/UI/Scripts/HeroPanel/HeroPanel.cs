using UnityEngine;
using System.Collections;
using DashFire;
public class HeroPanel : MonoBehaviour
{

  // Use this for initialization
  private GameObject m_Particle = null;
  private GameObject goRageBar = null;
  private GameObject goHeroPortrait = null;
  private GameObject goHealthBar = null;
  private GameObject player = null;
  private GameObject goRageBarEx = null;
  void Start()
  {

    m_Particle = DashFire.ResourceSystem.GetSharedResource("Hero_FX/1_UI_Ex") as GameObject;
    Transform trans = this.transform.FindChild("HeroPortrait");
    if (null != trans) {
      goHeroPortrait = trans.gameObject;
    } else {
      Debug.LogError("Can Not Find HeroPortrait");
    }
    trans = this.transform.FindChild("HealthBar");
    if (trans != null) {
      goHealthBar = trans.gameObject;
    } else {
      Debug.LogError("Can Not Find HealthBar");
    }
    trans = this.transform.FindChild("RageBarEx");
    if (trans != null) {
      goRageBarEx = trans.gameObject;
    } else {
      Debug.LogError("Can Not Find RageBarEx");
    }
    trans = this.transform.Find("RageBar");
    if (trans != null) {
      goRageBar = trans.gameObject;
      for (int index = 0; index < c_RageIconNum; ++index) {
        string spName = "Sprite" + index.ToString();
        Transform ts = goRageBar.transform.Find(spName);
        if (null != ts && index < m_RageIconGo.Length) {
          m_RageIconGo[index] = ts.gameObject;
        } else {
          Debug.LogError("!!can not find " + spName);
        }
      }
    } else {
      Debug.LogError("!!can not find RageBar");
    }
  }

  // Update is called once per frame
  void Update()
  {
    //实时更新Ex图标特效位置
    if (null != m_Particle && m_Effect != null) {
      Vector3 curPos = UICamera.mainCamera.WorldToScreenPoint(goHeroPortrait.transform.position);
      curPos = Camera.main.ScreenToWorldPoint(new Vector3(curPos.x, curPos.y, 5));
      if (null != m_Effect)
        m_Effect.transform.position = curPos;
    }

    if (null == player) {
      player = GameObject.FindGameObjectWithTag("Player");
      if (player != null) {
        SharedGameObjectInfo info = DashFire.LogicSystem.GetSharedGameObjectInfo(player);
        if (null != info) {
          UpdateHealthBar((int)info.Blood, (int)info.MaxBlood);
          UpdateRageValue((int)info.Rage, (int)info.MaxRage);
        }
      }
    } else {
      SharedGameObjectInfo info = DashFire.LogicSystem.GetSharedGameObjectInfo(player);
      if (null != info) {
        UpdateHealthBar((int)info.Blood, (int)info.MaxBlood);
        UpdateRageValue((int)info.Rage, (int)info.MaxRage);
      }
    }
    CastAnimation(goHealthBar);
  }
  //设置玩家头像
  string SetHeroPortrait(string portrait)
  {
    string name = null;
    UISprite sp = null;
    sp = goHeroPortrait.GetComponent<UISprite>();
    if (null != sp) {
      //记下更换之前的SpriteName
      name = sp.spriteName;
      sp.spriteName = portrait;
    }
    return name;

  }
  //设置玩家等级
  void SetHeroLevel(int level)
  {
    Transform trans = this.transform.Find("Level");
    if (trans == null)
      return;
    GameObject go = trans.gameObject;
    if (null == go)
      return;
    UILabel label = go.GetComponent<UILabel>();
    if (null == label)
      return;
    label.text = level.ToString();
  }
  //设置玩家昵称
  void SetHeroNickName(string nickName)
  {
    Transform trans = this.transform.Find("NickName");
    if (trans == null)
      return;
    GameObject go = trans.gameObject;
    if (null != go) {
      UILabel label = go.GetComponent<UILabel>();
      if (null != label)
        label.text = nickName;
    }
  }
  //更新血条
  void UpdateHealthBar(int curValue, int maxValue)
  {
    if (maxValue <= 0 || curValue < 0)
      return;
    float value = curValue / (float)maxValue;
    UIProgressBar progressBar = null;
    progressBar = goHealthBar.GetComponent<UIProgressBar>();
    if (null != progressBar) {
      progressBar.value = value;
    }
    GameObject go = null;
    Transform trans = null;
    if (null != goHealthBar) {
      trans = goHealthBar.transform.Find("label");
      if (trans != null)
        go = trans.gameObject;
    }
    UILabel label = null;
    if (go != null)
      label = go.GetComponent<UILabel>();
    if (null != label)
      label.text = curValue + "/" + maxValue;
  }
  //更新怒气条
  private void UpdateRageValue(int current, int max)
  {
    //美术给的怒气条资源是6个点，这里将怒气六等分
    if (current >= max) {
      if (!m_IsEx) {
        m_IsEx = true;
        foreach (GameObject go in m_RageIconGo) {
          if (go != null)
            NGUITools.SetActive(go, true);
        }
        NGUITools.SetActive(goHeroPortrait,false);
        if (null != goRageBarEx) {
          NGUITools.SetActive(goRageBarEx, true);
          TweenAlpha.Begin(goRageBarEx, 0.5f, 1f);
          if (null != m_Particle && m_Effect == null) {
            Vector3 curPos = UICamera.mainCamera.WorldToScreenPoint(goHeroPortrait.transform.position);
            curPos = Camera.main.ScreenToWorldPoint(new Vector3(curPos.x, curPos.y, 5));
            m_Effect = GameObject.Instantiate(m_Particle, curPos, Quaternion.identity) as GameObject;
            if (null != m_Effect)
              m_Effect.transform.position = curPos;
          }
        }
      }
      return;
    } else {
      if (null != goRageBarEx && m_IsEx) {
        m_IsEx = false;
        NGUITools.SetActive(goRageBarEx, false);
        NGUITools.SetActive(goHeroPortrait, true);
        if (m_Effect != null) {
          Destroy(m_Effect);
          m_Effect = null;
        }
      }
    }
    int average = (int)(max / (float)c_RageIconNum);
    int number = current / average;
    for (int index = 0; index < number; ++index) {
      if (index < m_RageIconGo.Length && m_RageIconGo[index] != null) {
        UISprite sp = m_RageIconGo[index].GetComponent<UISprite>();
        if (sp != null)
          sp.fillAmount = 1;
        NGUITools.SetActive(m_RageIconGo[index], true);
      }
    }
    if (number < m_RageIconGo.Length && m_RageIconGo[number] != null) {
      UISprite sp = m_RageIconGo[number].GetComponent<UISprite>();
      if (null != sp) {
        sp.fillAmount = (current % average) / (float)average;
        NGUITools.SetActive(m_RageIconGo[number], true);
      }
    }
    for (int index = number + 1; index < c_RageIconNum; ++index) {
      if (index < m_RageIconGo.Length && m_RageIconGo[index] != null) {
        NGUITools.SetActive(m_RageIconGo[index], false);
      }
    }
  }
  void CastAnimation(GameObject father)
  {
    if (null == father)
      return;
    GameObject goBack = null;
    UIProgressBar progressBar = null;
    Transform trans = father.transform.Find("Sprite(red)");
    if (trans != null)
      goBack = trans.gameObject;
    progressBar = father.GetComponent<UIProgressBar>();

    UISprite spBack = null;
    if (null != goBack) {
      spBack = goBack.GetComponent<UISprite>();
    }

    if (null != spBack && null != progressBar) {
      if (spBack.fillAmount <= progressBar.value) {
        spBack.fillAmount = progressBar.value;
      } else {
        spBack.fillAmount -= RealTime.deltaTime * 0.5f;
      }
    }
  }

  public void Hide()
  {
    Vector3 pos = this.transform.localPosition;
    m_OriginalPos = new Vector3(pos.x, pos.y, pos.z);
    TweenAlpha.Begin(this.gameObject, 0.2f, 0f);
    TweenPosition tween = TweenPosition.Begin(this.gameObject, 0.2f, new Vector3(pos.x, pos.y - 200, pos.z));
    EventDelegate.Add(tween.onFinished, this.OnHideFinished);
  }

  public void OnHideFinished()
  {
    NGUITools.SetActive(this.gameObject, false);
    this.transform.localPosition = m_OriginalPos;
    TweenAlpha.Begin(this.gameObject, 2f, 1f);
    for (int index = 0; index < c_RageIconNum; ++index) {
      if (m_RageIconGo[index] != null) {
        NGUITools.SetActive(m_RageIconGo[index], false);
      }
    }
    //重新开始时，设置为剑士头像
    NGUITools.SetActive(goHeroPortrait, true);
    NGUITools.SetActive(goRageBarEx, false);
  }

  public void OnPortraitClick()
  {
      if (m_Effect != null)
      {
          Destroy(m_Effect);
          m_Effect = null;
      }
    LogicSystem.EventChannelForGfx.Publish("ge_cast_skill", "game", "SkillEx");
  }

  public void TestButton()
  {
    if (goHealthBar != null) {
      UpdateHealthBar(100, 500);
    }
  }
  public void SetId(string id)
  {
    m_ID = id;
  }

  public string GetId()
  {
    return m_ID;
  }

  //记录怒气条的6个怒气点
  private const int c_RageIconNum = 6;
  private GameObject[] m_RageIconGo = new GameObject[c_RageIconNum];
  private Vector3 m_OriginalPos;
  private string m_ID = "first";
  //记录Ex图标显示之前sprite的spritename;
  private bool m_IsEx = false;

  private GameObject m_Effect = null;
}
