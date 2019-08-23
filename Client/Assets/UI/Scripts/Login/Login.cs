using UnityEngine;
using System.Collections;
using DashFire;

public class Login : MonoBehaviour
{
  // Use this for initialization

  void Start()
  {
    m_StartLoginEvent = DashFire.LogicSystem.EventChannelForGfx.Subscribe<string, int>("ge_start_login", "lobby", this.StartLogin);
    this.SetPanelVisible("SelectModePanel", false);
    this.SetPanelVisible("HeroSelect", false);
  }  
  
  void Update()
  {
    if (!m_IsHintShow) {
      m_DelayForHint -= RealTime.deltaTime;
      if (m_DelayForHint <= 0) {
        m_IsHintShow = true;
        Transform tf = this.transform.Find("LoginBtn/hint");
        if (null != tf) {
          GameObject go = tf.gameObject;
          NGUITools.SetActive(go, true);
          TweenAlpha alpha = go.GetComponent<TweenAlpha>();
          if (null != alpha)
            alpha.enabled = true;
        }
        BoxCollider collider = this.GetComponent<BoxCollider>();
        if (null != collider)
          collider.enabled = true;
      }
    }
  }
  public void OnLoginButtonClick()
  {
    string password  = this.transform.Find("Input/Password").GetComponent<UIInput>().value;
    string username = this.transform.Find("Input/UserName").GetComponent<UIInput>().value;
    string nodeIp = this.transform.Find("IpAddr").GetComponent<UIInput>().value;
    int nodePort = int.Parse(this.transform.Find("Port").GetComponent<UIInput>().value);
    LogicSystem.PublishLogicEvent("ge_login_lobby", "lobby", "http://" + nodeIp + ":" + nodePort, username, password);
    UnityEngine.Debug.LogError(string.Format("publish ge_login_lobby {0} {1} {2}", "http://" + nodeIp + ":" + nodePort, username, password));
  }
  public void StartLogin(string account, int loginResult)
  {
    if (loginResult == 0) {
      
      /***********************************************
       * Demo适用
       * 在这里发送场景、英雄选择及开始游戏三个消息
       * 不再跳转到相应页面
       ************************************************/
      DashFire.LogicSystem.PublishLogicEvent("ge_select_scene", "lobby", m_SceneId);
      LogicSystem.PublishLogicEvent("ge_select_hero", "lobby", m_HeroId);
      LogicSystem.PublishLogicEvent("ge_start_game", "lobby");
      SetLoginPanelVisible(false);
      Unsubscribe();
      DestroyImmediate(this.gameObject);

    } else { 
      //TODO:登录失败，给出错误提示，不跳转页面
    }    
  }
  public void SetLoginPanelVisible(bool visible)
  {
    NGUITools.SetActive(gameObject, visible);
  }
  public void SetPanelVisible(string panelName, bool visible)
  {
    Transform trans = null;
    trans = this.transform.parent.Find(panelName);
    if (null != trans) {
      NGUITools.SetActive(trans.gameObject, visible);
    } else {
      LogicSystem.LogicLog("Debug:Can not find " + panelName);
    }
  }
  private void InitLoading()
  {
    GameObject go = DashFire.ResourceSystem.GetSharedResource("Loading/Loading2") as GameObject;
    if (go != null) {
      GameObject ld = NGUITools.AddChild(this.transform.parent.gameObject, go);
      if (ld != null) {
        ld.transform.localPosition = new Vector3(0, 0, 0);
        NGUITools.SetActive(ld, true);
      }
    }
  }

  public void OnTweenFinished()
  {
    Transform ts = this.transform.Find("LoginBtn/logo");
    if (null == ts)
      return;
    GameObject go = ts.gameObject;
    if (go != null)
      NGUITools.SetActive(go, true);
  }

  private void Unsubscribe()
  {
    LogicSystem.EventChannelForGfx.Unsubscribe(m_StartLoginEvent);
  }
  //config  demo临时适用
  public int m_SceneId = 5;
  private int m_HeroId = 1;
  private float m_DelayForHint = 2.2f;
  private bool m_IsHintShow = false;
  private object m_StartLoginEvent = null;
}
