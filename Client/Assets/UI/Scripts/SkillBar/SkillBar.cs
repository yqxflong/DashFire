using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DashFire;
public class SkillBar : MonoBehaviour
{
  public GameObject CommonSkillGo = null;
  // Use this for initialization
  void Start()
  {
    if(null!=CommonSkillGo)
      UIEventListener.Get(CommonSkillGo).onPress = OnButtonPress;
    m_CastSkillEvent = LogicSystem.EventChannelForGfx.Subscribe<string>("ge_cast_skill", "ui", CastSkill);
    m_CastCdEvent = LogicSystem.EventChannelForGfx.Subscribe<string,float>("ge_cast_skill_cd", "ui", CastCDByGroup);
    m_UnlockSkillEvent = LogicSystem.EventChannelForGfx.Subscribe<string>("ge_unlock_skill", "ui", this.UnlockSkill);
    for (int index = 0; index < 4; index++) {
      string path = "Skill" + index + "/skill0";
      GameObject go = GetGoByPath(path);
      if (null != go) {
        m_SkillsPos[index] = go.transform.localPosition;
        UISprite sp = go.GetComponent<UISprite>();
        if (null != sp)
          m_SpriteName[index] = sp.spriteName;
      }
    }

  }

  public void UnSubscribe()
  {
    if (m_CastCdEvent != null)
      LogicSystem.EventChannelForGfx.Unsubscribe(m_CastCdEvent);
    if (m_CastSkillEvent != null)
      LogicSystem.EventChannelForGfx.Unsubscribe(m_CastSkillEvent);
    if (m_UnlockSkillEvent != null)
      LogicSystem.EventChannelForGfx.Unsubscribe(m_UnlockSkillEvent);
  }

  //发送普通攻击消息
  //press == true 表示按下
  //press == false 表示抬起
  void OnButtonPress(GameObject ob, bool press)
  {
    Debug.Log("Common attack!");
    LogicSystem.EventChannelForGfx.Publish("ge_attack", "game", press);

  }
  // Update is called once per frame
  void Update()
  {
    if (time >0) {
      time -= RealTime.deltaTime;
      if (time <= 0) {
        foreach (BoxCollider collider in m_GoList) {
          if (collider != null) {
              collider.enabled = true;
          }
        }
        m_GoList.Clear();
      }
    }
    for (int index = 0; index < c_SkillNum; ++index) {
      if (remainCdTime[index] > 0) {
        string path = "Skill" + index.ToString() + "/skill0/CD";
        GameObject go = GetGoByPath(path);
        if (null != go) {
          UISprite sp = go.GetComponent<UISprite>();
          if (null != sp) {
            sp.fillAmount -= RealTime.deltaTime / GetCDTimeByIndex(index);
            remainCdTime[index] = sp.fillAmount;
            if (remainCdTime[index] <= 0) {
              IconFlashByIndex(index);
            }
          }
        }
      }
    }
  }

  private void IconFlashByIndex(int index)
  {
    //Debug.Log("IconFlashByIndex:"+ index);
    string path = "Skill" + index.ToString() + "/skill0/bright";
    GameObject go = GetGoByPath(path);
    if (go == null)
      return;
    NGUITools.SetActive(go, true);
    TweenAlpha alpha = go.GetComponent<TweenAlpha>();
    if (null != alpha)
      Destroy(alpha);
    TweenAlpha tweenAlpha = go.AddComponent<TweenAlpha>();
    if (null == tweenAlpha)
      return;
    tweenAlpha.from = 0;
    tweenAlpha.to = 1;
    tweenAlpha.duration = 0.4f;
    tweenAlpha.animationCurve = tweenAnimation;
  }

  public void SetSkillEnableByIndex(int index, int period, bool enable)
  {
    string childPath = "Skill" + index.ToString() + "/skill" + period.ToString() + "/CD";
    GameObject go = GetGoByPath(childPath);
    if (go == null)
      return;
    UISprite sp = go.GetComponent<UISprite>();
    if (null != sp) {
      if (enable == false)
        sp.fillAmount = 1f;
      else {
        sp.fillAmount = 0f;
      }

    }
  }

  public void CastCDByGroup(string group, float cdTime)
  {
    CastSkill(group, true);
    int index = GetIndexByGroup(group);
    if (index == -1 || index >= c_SkillNum)
      return;
    skillsCDTime[index] = cdTime;
    string path = "Skill" + index.ToString() + "/skill0/CD";
    GameObject go = GetGoByPath(path);
    if (null != go) {
      UISprite sp = go.GetComponent<UISprite>();
      if (null != sp) {
        sp.fillAmount = 1;
        remainCdTime[index] = sp.fillAmount;
      }
    }
  }
  private GameObject GetGoByPath(string path)
  {
    GameObject go = null;
    Transform trans = this.transform.Find(path);
    if (null != trans) {
      go = trans.gameObject;
    } else {
      Debug.Log("Can not find " + path);
    }
    return go;
  }
  private GameObject GetGoByIndexAndName(int index, string name)
  {
    GameObject go = null;
    string path = "Skill" + index.ToString() + "/" + name;
    Transform trans = this.transform.Find(path);
    if (trans != null) {
      go = trans.gameObject;
    }
    return go;
  }

  private UILabel GetSkillTextByIndex(int index)
  {

    UILabel label = null;
    GameObject go = GetGoByIndexAndName(index, "Text");
    if (null != go) {
      label = go.GetComponent<UILabel>();
    } else {
      Debug.LogError("::Error Can not find label");
    }
    return label;
  }

  public float GetCDTimeByIndex(int index)
  {
    float ret = 0;
    ret = skillsCDTime[index];
    return ret;
  }
  //
  public void CastSkill(string skillGroup)
  {
    int index, period;
    index = GetIndexByGroup(skillGroup);
    if (index >= 4 || index == -1)
      return;
    period =  m_PeriodIndex[index]++;
    string nowSkillPath = "Skill" + index + "/skill0";
    int next = period + 1;
   // string nextSkillPath = "Skill" + index + "/skill" + next;
    GameObject nowSkill = GetGoByPath(nowSkillPath);
    if (nowSkill == null)
      return;
    UISprite sp = nowSkill.GetComponent<UISprite>();
    if (sp != null) {
      sp.spriteName = index.ToString() + "-" + next.ToString() ;
      //Debug.Log("sp.spriteName:" + sp.spriteName);
    }
  }

  public void CastSkill(string skillGroup, bool isEnd)
  {
    if (isEnd) {
      int index, period;
      index = GetIndexByGroup(skillGroup);
      if (index >= 4 || index == -1)
        return;
      period = m_PeriodIndex[index];
      string nowSkillPath = "Skill" + index + "/skill" + 0;
      int next = 0;
      if (next == period)
        return;
      //string nextSkillPath = "Skill" + index + "/skill" + next;
      GameObject nowSkill = GetGoByPath(nowSkillPath);
      if (null == nowSkill)
        return;
      UISprite sp = nowSkill.GetComponent<UISprite>();
      if (sp != null) {
        sp.spriteName = m_SpriteName[index];
      }
      m_PeriodIndex[index] = 0;
    }
  }

  public void ChangeSkill(GameObject nowGo, GameObject nextGo, Vector3 nowPos)
  {
    if (nowGo == null || nextGo == null)
      return;
    Vector3 from = new Vector3(nowPos.x + 50, nowPos.y, nowPos.z);
    time =0.6f;
    AddTweenPos(nextGo, from, nowPos, 0.3f, 0.3f);
    AddTweenAlpha(nextGo, 0f, 1f, 0.3f);
    AddTweenAlpha(nowGo, 1f, 0f, 0.6f);
    AddTweenPos(nowGo, nowPos, from, 0.1f, 0.6f);
  }
  private void AddTweenAlpha(GameObject father, float from, float to, float duration)
  {

    if (null == father)
      return;
    TweenAlpha alpha = father.GetComponent<TweenAlpha>();
    if (null != alpha) { Destroy(alpha); alpha = null; }
    alpha = father.AddComponent<TweenAlpha>();
    if (null != alpha) {
      alpha.duration = duration;
      alpha.from = from;
      alpha.to = to;
    }

  }
  private void AddTweenPos(GameObject father, Vector3 from, Vector3 to, float duration, float delay)
  {
    if (null == father)
      return;
    TweenPosition tweenPos = father.GetComponent<TweenPosition>();
    if (null != tweenPos) { Destroy(tweenPos); tweenPos = null; }
    tweenPos = father.AddComponent<TweenPosition>();
    if (null != tweenPos) {
      tweenPos.duration = duration;
      tweenPos.from = from;
      tweenPos.to = to;
      tweenPos.delay = delay;

    }

  }

  public void OnButtonClick()
  {
    if (UICamera.currentTouch != null) {
      GameObject go = UICamera.currentTouch.current;
      if (go == null)
        return;
      Transform trans = go.transform.parent;
      if (trans == null)
        return;
      GameObject parentGo = trans.gameObject;
      string name = parentGo.name;
      string skillType = "";
      switch (name) {
        case "Skill0": skillType = "SkillD"; break;
        case "Skill1": skillType = "SkillB"; break;
        case "Skill2": skillType = "SkillC"; break;
        case "Skill3": skillType = "SkillA"; break;
        default: skillType = null; break;
      }
      if (skillType != null) {
        //向上层逻辑发送释放技能的消息
        Debug.Log("!! Skill type is "+ skillType);
        LogicSystem.EventChannelForGfx.Publish("ge_cast_skill", "game", skillType);
      }
    }
  }

  public void UnlockSkill(string group)
  {
    int index = GetIndexByGroup(group);
    string goPath = "Skill" + index.ToString();
    Transform ts = this.transform.Find(goPath);
    if (null != ts) {
      GameObject go = ts.gameObject;
      NGUITools.SetActive(go, true);
    } else {
      Debug.LogError("!!can not find " + goPath);
    }
//     ts = this.transform.Find("Grid");
//     if (null != ts) {
//       GameObject goGrid = ts.gameObject;
//       UIGrid grid = goGrid.GetComponent<UIGrid>();
//       if (null != grid)
//         grid.Reposition();
//     }
  }
  
  public void Test(string Name)
  {
    int index = GetIndexByGroup(Name);
    if (m_PeriodIndex[index] == 2) {
      CastCDByGroup(Name,2f);
    } else {
      CastSkill(Name);
    }
  }

  public void OnTestTest()
  {
    Test("SkillA");
    Test("SkillB");
    Test("SkillC");
    Test("SkillD");
  
  }

  public int GetIndexByGroup(string group)
  {
    int ret = -1;
    switch (group) {
      case "SkillA": ret = 3; break;
      case "SkillB": ret = 1; break;
      case "SkillC": ret = 2; break;
      case "SkillD": ret = 0; break;
      default: ret = -1; break;
    }
    return ret;
  }
  public void Hide()
  {
    Vector3 pos = this.transform.localPosition;
    m_SkillBarPos = new Vector3(pos.x, pos.y, pos.z); 
    TweenAlpha.Begin(this.gameObject, 0.2f, 0f);
    TweenPosition tween = TweenPosition.Begin(this.gameObject, 0.2f, new Vector3(pos.x + 200, pos.y, pos.z));
    EventDelegate.Add(tween.onFinished, this.OnHideFinished);
  }

  public void OnHideFinished()
  {
    NGUITools.SetActive(this.gameObject, false);
    this.transform.localPosition = m_SkillBarPos;
    TweenAlpha.Begin(this.gameObject, 2f, 1f);
    //重新进入
    for (int index = 0; index < c_SkillNum; ++index ) {
      string goPath = "Skill" + index.ToString();
      Transform ts = this.transform.Find(goPath);
      if (null != ts) {
        GameObject go = ts.gameObject;
        NGUITools.SetActive(go, false);
      } else {
        Debug.LogError("!!can not find " +goPath);
      }
    }
  }
  public void SetId(string id)
  {
    m_Id = id;
  }
  public string GetId()
  {
    return m_Id;
  }

  private string[] m_SpriteName = new string[4];
  private float time = 0.6f;
  private string m_Id = "first";
  private int[] m_PeriodIndex = new int[4] { 0, 0, 0, 0 };
  public AnimationCurve tweenAnimation = null;
  public const float c_DisableScale = 1.2f;
  private const int c_SkillNum = 4;
  private float[] remainCdTime = new float[c_SkillNum];
  private Dictionary<int, Vector3> m_OriginalPos;
  private float[] skillsCDTime = new float[c_SkillNum];
  private Vector3[] m_SkillsPos = new Vector3[4];
  private List<BoxCollider> m_GoList = new List<BoxCollider>();
  private Vector3 m_SkillBarPos;

  private object m_UnlockSkillEvent = null;
  private object m_CastSkillEvent = null;
  private object m_CastCdEvent = null;

}
