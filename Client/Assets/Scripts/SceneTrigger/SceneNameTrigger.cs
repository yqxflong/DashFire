using System;
using UnityEngine;
using System.Collections;

public class SceneNameTrigger : MonoBehaviour 
{
  void OnTriggerEnter(Collider other)
  {
    if (mIsTriggered == true) {
      return;
    }
    //玩家进入该区域触发
    if (other.tag != "Player") {
      return;
    }
    //显示场景名称UI
    GameObject dfmUIRootGO = GameObject.FindGameObjectWithTag("UI");
    if (dfmUIRootGO != null) {
      dfmUIRootGO.GetComponent<DFMUiRoot>().InitAndShowSceneName();
      mIsTriggered = true;
    }
  }

  private bool mIsTriggered = false;

}
