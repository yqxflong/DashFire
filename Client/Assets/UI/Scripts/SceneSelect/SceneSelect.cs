using UnityEngine;
using System.Collections;

public class SceneSelect : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

  public void OnSceneBtnClick()
  {
    string clickBtnName = "";
    if (UICamera.currentTouch != null && UICamera.currentTouch.current != null) {
      clickBtnName = UICamera.currentTouch.current.name.ToString();
    }
    m_SceneId = GetSceneIdByName(clickBtnName);

  }
  int GetSceneIdByName(string sceneName)
  {
    switch (sceneName) {
      case "Scene1": return 1;
      case "Scene2": return 2;
      case "Scene3": return 3;
      case "Scene4": return 4; 
      default: return -1;
     }
  }
  public void OnNextBtnClick()
  {
    DashFire.LogicSystem.PublishLogicEvent("ge_select_scene", "lobby", m_SceneId);
    
    NGUITools.SetActive(this.gameObject, false);
    GameObject go = DashFire.ResourceSystem.GetSharedResource("HeroSelectPrefab") as GameObject;
    if (null != go) {
      NGUITools.AddChild(this.transform.parent.gameObject, go);
      NGUITools.SetActive(go, true);
    } 
  }

  private int m_SceneId = 4;
  
}
