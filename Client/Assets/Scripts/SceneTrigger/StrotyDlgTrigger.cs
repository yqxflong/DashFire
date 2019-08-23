using UnityEngine;
using System.Collections;
using StoryDlg;

public enum StoryDlgType
{
  Small,
  Big
}

public class StoryDlgArgs
{
  public int m_StoryId;
  public StoryDlgType m_StoryDlgType;
  public float m_IntervalTime;
}

public class StrotyDlgTrigger : MonoBehaviour {
  
  public int m_StoryId = 0;
  public StoryDlgType m_StoryDlgType = StoryDlgType.Small;
  public float m_IntervalTime = 5.0f;

  private bool IsTriggered = false;  
  //逻辑事件触发
  public void OnLogicTrigger()
  {    
    TriggerStoryDlgImpl();
  }
  //区域碰撞触发
  void OnTriggerEnter(Collider other)
  {
    //Debug.LogError(other.gameObject.name + "Enter !!!" + m_StoryId);     
    //只有玩家才能触发
    if (other != null && other.tag != "Player") {
      return;
    }
    TriggerStoryDlgImpl();    
  }
  //触发剧情对话框具体实现
  private void TriggerStoryDlgImpl()
  {
    if (m_StoryId < 1 || m_StoryId > StoryDlgManager.Instance.StoryInfos.Count) {
      return;
    }
    //剧情对话框只触发一次
    if (IsTriggered == true) {
      return;
    }
    IsTriggered = true;
    string storyDlgName = "StoryDlgSmall";
    if (m_StoryDlgType == StoryDlgType.Big) {
      storyDlgName = "StoryDlgBig";
    }
    GameObject parentGO = GameObject.FindGameObjectWithTag("UI");
    if (parentGO != null) {
      Transform parentTransform = parentGO.transform;
      Transform t = parentTransform.Find(storyDlgName);
      if (t == null) {
        //第一次加载
        GameObject go = DashFire.ResourceSystem.GetSharedResource(string.Format("UI/{0}", storyDlgName)) as GameObject;
        if (go != null) {
          GameObject dlgGO = NGUITools.AddChild(parentTransform.gameObject, go);
          dlgGO.name = storyDlgName;
          StoryDlgArgs arg = new StoryDlgArgs();
          arg.m_StoryId = m_StoryId;
          arg.m_StoryDlgType = m_StoryDlgType;
          arg.m_IntervalTime = m_IntervalTime;
          dlgGO.BroadcastMessage("OnTriggerStory", arg);
        }
      } else if (NGUITools.GetActive(t.gameObject) == false) {
        NGUITools.SetActive(t.gameObject, true);
        StoryDlgArgs arg = new StoryDlgArgs();
        arg.m_StoryId = m_StoryId;
        arg.m_StoryDlgType = m_StoryDlgType;
        arg.m_IntervalTime = m_IntervalTime;
        t.gameObject.BroadcastMessage("OnTriggerStory", arg);
      }
    }      
  }
}

