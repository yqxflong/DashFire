using System;
using UnityEngine;
using System.Collections;
using DashFire;

public class IntroductionTrigger : MonoBehaviour
{
  public int StoryDlgIdRegion1 = 4;
  public string UnlockSkillGroup1 = "";
  public int StoryDlgIdRegion2 = 5;
  public string UnlockSkillGroup2 = "";
  public int StoryDlgIdRegion3 = 6;
  public string UnlockSkillGroup3 = "";
  public int StoryDlgIdRegion4 = 7;
  public string UnlockSkillGroup4 = "";

  public GameObject StroyHandler;
  //逻辑事件触发
  public void OnLogicTrigger(int storyId)
  {
    IntroductionTriggerImpl(storyId);
  }

  public void IntroductionTriggerImpl(int storyId)
  {
    if (storyId == StoryDlgIdRegion1) {
      //解锁技能      
      UnlockSkillGroup(UnlockSkillGroup1);
      //触发区域逻辑
    } else if (storyId == StoryDlgIdRegion2) {
      //区域2
      UnlockSkillGroup(UnlockSkillGroup2);
    } else if (storyId == StoryDlgIdRegion3) {
      //区域3
      UnlockSkillGroup(UnlockSkillGroup3);
    } else if (storyId == StoryDlgIdRegion4) {
      //区域4
      UnlockSkillGroup(UnlockSkillGroup4);
      LogicSystem.PublishLogicEvent("ge_set_max_rage", "player");
    } else if (storyId == 8)
    {
        //最后的对话框说完
        EndDemoLearn();
    } 
    else {
      Debug.LogError("Unhandled Story ID by IntroductionObj !!!" + storyId);
    }
    if (null != StroyHandler) {
      StroyHandler.SendMessage("OnStroyDlgEnd");
    }
  }

  private void EndDemoLearn()
  {
      //黑屏效果
      //找到场景中的Main Camera对象，
      GameObject mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
      if (mainCamera != null)
      {
          Tonemapping com = (Tonemapping)mainCamera.GetComponent("Tonemapping");          
          if (com != null)
          {
              com.enabled = true;
              com.SetActive(true);
              GameObject dfmUIRootGO = GameObject.FindGameObjectWithTag("UI");
              if (dfmUIRootGO != null)
              {
                  dfmUIRootGO.GetComponent<DFMUiRoot>().SetUIVisible(false);
              }
              StartCoroutine(WaitAndChangeScene(com.totalTime)); 
          }          
      }
  }

  IEnumerator WaitAndChangeScene(float waitTime)
  {
      yield return new WaitForSeconds(waitTime);
      int nextSceneId = 2;
      LogicSystem.PublishLogicEvent("ge_change_scene", "game", nextSceneId);
      /*
      GameObject dfmUIRootGO = GameObject.FindGameObjectWithTag("UI");
      if (dfmUIRootGO != null)
      {
          dfmUIRootGO.GetComponent<DFMUiRoot>().SetUIVisible(true);
      }
      */
  }

  private void UnlockSkillGroup(string skillGroup)
  {
    string[] skillGroupStr = skillGroup.Split(m_ValueSeparator, StringSplitOptions.RemoveEmptyEntries);
    for (int i = 0; i != skillGroupStr.Length; i++) {
      int id = -1;
      bool ret = Int32.TryParse(skillGroupStr[i], out id);
      if (ret == true && id != -1) {
        LogicSystem.EventChannelForGfx.Publish("ge_unlock_skill", "skill", id);        
      }
    }
  }

  private static readonly string[] m_ValueSeparator = new string[] {"|"};
}

