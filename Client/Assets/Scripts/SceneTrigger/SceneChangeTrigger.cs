using UnityEngine;
using DashFire;

class SceneChangeTrigger : MonoBehaviour
{
  public int NextSceneId = 0;

  void OnTriggerEnter(Collider other)
  {       
    //玩家进入该区域触发
    if (other.tag != "Player") {
      return;
    }
    if (NextSceneId <= 0) {
      return;
    }
    GameObject uiRoot = GameObject.FindGameObjectWithTag("UI");
    if (uiRoot != null)
    {
      uiRoot.BroadcastMessage("OnStopBtnClicked");      
    }
    ///
    JoyStickInputProvider.JoyStickEnable = false;
    GestureArgs e = new GestureArgs();
    e.name = "OnSingleTap";
    e.airWelGamePosX = 0f;
    e.airWelGamePosY = 0f;
    e.airWelGamePosZ = 0f;
    e.selectedObjID = -1;
    LogicSystem.FireGestureEvent(e);

    //通知逻辑层切换场景
    Debug.LogError("Change to Scene : " + NextSceneId);  
    LogicSystem.PublishLogicEvent("ge_change_scene", "game", NextSceneId);    
  } 
}

