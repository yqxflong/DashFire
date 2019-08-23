using System;
using StoryDlg;
using UnityEngine;

public class StoryDlgPanel : MonoBehaviour
{
  public delegate void StoryDlgOverEventHandler(object sender, int sceneId);
  public StoryDlgOverEventHandler StoryDlgOver;  

  private GameObject m_StoryDlgGO = null;
  private StoryDlgInfo m_StoryInfo = null;
  private int m_Count = 0;
  private int m_StepNumber = 0;
  private bool m_IsStoryDlgActive = false;
  private int m_StoryId = 0;
  private StoryDlgType m_StoryDlgType = StoryDlgType.Small;
  private float m_IntervalTime = 5.0f;

  public void OnTriggerStory(StoryDlgArgs arg)
  {    
    if(m_IsStoryDlgActive == false){           
      m_StoryInfo = StoryDlgManager.Instance.GetStoryInfoByID(arg.m_StoryId);
      if (m_StoryInfo != null) {
        //Debug.LogError("===== Trigger A New Story !!! " + arg.m_IntervalTime);
        m_StoryId = arg.m_StoryId;
        m_StoryDlgType = arg.m_StoryDlgType;
        m_IntervalTime = arg.m_IntervalTime;
        m_IsStoryDlgActive = true;
        m_StoryDlgGO = this.gameObject;
        m_Count = 0;    //剧情对话计数器，触发一个新的剧情时重置为0         
        m_StepNumber = m_StoryInfo.StoryItems.Count;
        StoryDlgItem item = m_StoryInfo.StoryItems[m_Count];
        UpdateStoryDlg(m_StoryDlgGO.transform, item);
        NGUITools.SetActive(m_StoryDlgGO, true);
        if (m_StoryDlgType == StoryDlgType.Big) {
          TouchManager.TouchEnable = false;
          JoyStickInputProvider.JoyStickEnable = false;
          GameObject dfmUIRootGO = this.transform.parent.gameObject;
          if (dfmUIRootGO != null) {
            dfmUIRootGO.GetComponent<DFMUiRoot>().SetUIVisible(false);
          }
        }        
        m_Count++;
        if (m_IntervalTime > 0.0f) {
          Invoke("NextStoryItem", m_IntervalTime);
        }        
      } else {
        Debug.LogError("Wrong Story id = " + arg.m_StoryId);
      }     
    } 
  }
  public void OnNextBtnClicked()
  {   
    this.NextStoryItem();
  }
  public void OnStopBtnClicked()
  {    
    this.StopStoryDlg();
  }  
  private void UpdateStoryDlg(Transform storyTrans, StoryDlgItem item)
  {
    UILabel lblName = storyTrans.Find("SpeakerName").GetComponent<UILabel>();    
    UILabel lblWords = storyTrans.Find("SpeakerWords").GetComponent<UILabel>();    
    UISprite spriteLeft = storyTrans.Find("SpeakerImageLeft").GetComponent<UISprite>();     
    UISprite spriteRight = storyTrans.Find("SpeakerImageRight").GetComponent<UISprite>();
    if (m_StoryDlgType == StoryDlgType.Small) {
      lblName.text = string.Format("[c9b2ae]{0}:[-]", item.SpeakerName);
      lblWords.text = item.Words;
      spriteLeft.spriteName = item.ImageLeftSmall;
      spriteRight.spriteName = item.ImageRightSmall;
    } else {
      lblName.text = string.Format("[c9b2ae]{0}:[-]", item.SpeakerName);
      lblWords.text = item.Words;
      spriteLeft.spriteName = item.ImageLeftBig;
      spriteRight.spriteName = item.ImageRightBig;
    }   
  }
  //下一句
  private void NextStoryItem()
  {
    //剧情对话框处于活跃状态时，处理单击操作    
    if (m_IsStoryDlgActive == true) {
      CancelInvoke("NextStoryItem");
      if (null != m_StoryDlgGO) {
        bool isActive = NGUITools.GetActive(m_StoryDlgGO);
        if (isActive == true) {
          if (m_Count < m_StepNumber) {
            StoryDlgItem item = m_StoryInfo.StoryItems[m_Count];            
            UpdateStoryDlg(m_StoryDlgGO.transform, item);
            NGUITools.SetActive(m_StoryDlgGO, true);
            m_Count++;
            if (m_IntervalTime > 0.0f) {
              Invoke("NextStoryItem", m_IntervalTime);
            } 
          } else {
            FinishStoryDlg();
          }
        }
      }
    }
  }
  //直接结束剧情对话
  private void StopStoryDlg()
  {
    //剧情对话框处于活跃状态时，处理单击操作
    CancelInvoke("NextStoryItem");
    if (m_IsStoryDlgActive == true) {
      if (null != m_StoryDlgGO) {
        FinishStoryDlg();
      }
    }
  }
  private void FinishStoryDlg()
  {
    m_IsStoryDlgActive = false;
    NGUITools.SetActive(m_StoryDlgGO, false);
    if (m_StoryDlgType == StoryDlgType.Big) {
      TouchManager.TouchEnable = true;
      JoyStickInputProvider.JoyStickEnable = true;
      GameObject dfmUIRootGO = this.transform.parent.gameObject;
      if (dfmUIRootGO != null) {
        dfmUIRootGO.GetComponent<DFMUiRoot>().SetUIVisible(true);
      }
    }    
    m_StoryDlgGO = null;
    m_StoryInfo = null;
    RaiseStoryDlgOverEvent();
  }
  //剧情对话结束引发事件
  private void RaiseStoryDlgOverEvent()
  {
    GameObject introObj = GameObject.Find("/EventObj/Story/IntroductionObj");
    if (null != introObj) {     
      introObj.SendMessage("OnLogicTrigger", m_StoryId);
    }

    if (StoryDlgOver != null) {
      Debug.LogError("RaiseStoryDlgOverEvent!!!!" + m_StoryId);
      StoryDlgOver(this.gameObject, m_StoryId);
    }      
  }
}
	

