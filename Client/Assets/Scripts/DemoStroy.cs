using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DashFire;

public class DemoStroy : MonoBehaviour {

  public List<NpcSpawner> m_NpcSpawners;
  public GameObject m_Introdctionobj;
  private int m_CurIndex = -1;
  private int m_CurAliveNpcCount = 0;
	// Use this for initialization
	void Start () {
    DashFire.LogicSystem.EventChannelForGfx.Subscribe<int>("ge_on_npc_dead", "story", OnNpcDead);
    DashFire.LogicSystem.EventChannelForGfx.Subscribe("ge_on_game_start", "story", OnGameStart);
	}

  private void OnGameStart() {
    Debug.LogError("On Game Start");
    m_CurIndex = -1;
    m_CurAliveNpcCount = 0;
  }
  private void OnNpcDead(int unitId) {
    m_CurAliveNpcCount--;
    if (0 == m_CurAliveNpcCount) {
      StartCoroutine(ShowStoryDlg());
    }
  }

  void Update() {
  }

  private IEnumerator ShowStoryDlg() {
    yield return new WaitForSeconds(5.0f);
    if (null != m_Introdctionobj) {
      m_Introdctionobj.SendMessage("OnLogicTrigger");
    }
  }
  public void OnStroyDlgEnd() {
    ChangeToNextSpawner();
  }

  private void ChangeToNextSpawner() {
    m_CurIndex++;
    if (m_CurIndex < m_NpcSpawners.Count) {
      NpcSpawner spawner = m_NpcSpawners[m_CurIndex];
      if (null != spawner) {
        LogicSystem.PublishLogicEvent("ge_create_npc_by_story", "npc", spawner.UnitIdStart, spawner.unitIdEnd);
        m_CurAliveNpcCount = spawner.unitIdEnd - spawner.UnitIdStart + 1;
      }
    } else {
      EndSection();
    }
  }

  private void EndSection() {
    GameObject triggerObj = GameObject.Find("/EventObj/Story/TriggerObj");
    if (null != triggerObj) {
      triggerObj.SendMessage("OnLogicTrigger");
    } else {
      Debug.LogError("MainStory does not find TriggerObj");
    }

    GameObject uiRoot = GameObject.FindGameObjectWithTag("UI");
    if (null != uiRoot) {
      uiRoot.SendMessage("OkEX", false);
    } else {
      Debug.LogError("MainStory can't find UICamera");
    }
  }
}

[System.Serializable]
public class NpcSpawner {
  public int UnitIdStart;
  public int unitIdEnd;
}

