using System;
using UnityEngine;
using System.Collections;

public class PortalTrigger : MonoBehaviour {

  public GameObject m_PortalPrefab = null;
  public float m_WaitTime = 1.5f;

  //逻辑事件触发
  public void OnLogicTrigger()
  {    
    TriggerPortalImpl();
  }

  public void TriggerPortalImpl()
  {
    //消除阻挡
    GameObject areaEnd = GameObject.Find("/EventObj/Area/AreaEnd");
    if (null != areaEnd) {
      BoxCollider boxCollider = areaEnd.GetComponent<BoxCollider>();
      boxCollider.isTrigger = true;
    }
    //启用传送门
    if (m_PortalPrefab != null) {
      m_PortalPrefab.SetActive(true);
    }    
  }

  public void HandleStoryDlgOverEvent(object sender, int storyId)
  {
    if (storyId == 1) {
      StartCoroutine(WaitAndShowLevelName(m_WaitTime));     
    } 
    else if (storyId == 2) {
      //消除阻挡
      GameObject areaEnd = GameObject.Find("/EventObj/Area/AreaEnd");
      if (null != areaEnd) {
        BoxCollider boxCollider = areaEnd.GetComponent<BoxCollider>();
        boxCollider.isTrigger = true;
      }
      //启用传送门
      if (m_PortalPrefab != null) {
        m_PortalPrefab.SetActive(true);
      }      
    } 
  }
  
  IEnumerator WaitAndShowLevelName(float waitTime)
  {
    yield return new WaitForSeconds(waitTime);
    GameObject dfmUIRootGO = GameObject.FindGameObjectWithTag("UI");
    if (dfmUIRootGO != null) {
      dfmUIRootGO.GetComponent<DFMUiRoot>().InitAndShowSceneName();
    }
  }
}
