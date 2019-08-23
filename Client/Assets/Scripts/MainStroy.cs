using UnityEngine;
using System.Collections;
using DashFire;

public class MainStroy : MonoBehaviour {

  public enum StoryEndType {
    SectionEnd,
    GameEnd,
  }

  public float m_TickIntevalTime = 2;
  private float m_LastTriggerTime = 0;

  public GameObject[] m_Colliders;
  public StoryEndType m_EndType = StoryEndType.SectionEnd;
  
  private StroyHandler m_CurrentHandler;
  private int m_CurIndex = -1;
	// Use this for initialization
	void Start () {
    DashFire.LogicSystem.EventChannelForGfx.Subscribe<int>("ge_on_npc_dead", "story", OnNpcDead);
    DashFire.LogicSystem.EventChannelForGfx.Subscribe("ge_on_game_start", "story", OnGameStart);
    m_LastTriggerTime = Time.time;
	}


  private void OnGameStart() {
    Debug.LogError("On Game Start");
    m_CurIndex = -1;
    m_CurrentHandler = null;
    ChangeToNextScene();
  }
  private void OnNpcDead(int unitId) {
    if (null != m_CurrentHandler) {
      m_CurrentHandler.OnNpcDead(unitId);
      if (m_CurrentHandler.GetAliveUnitsCount() <= 0) {
        ChangeToNextScene();
      }
    }
  }

  public void OnStroyDlgEnd() {
    ChangeToNextScene();
  }

  private void ChangeToNextScene() {
    if (null != m_CurrentHandler) {
      m_CurrentHandler.EndStory();
    }
    m_CurIndex++;
    if (m_Colliders.Length > m_CurIndex) {
      if (null != m_Colliders[m_CurIndex]) {
        m_CurrentHandler = m_Colliders[m_CurIndex].GetComponent<StroyHandler>();
      }
      if (null != m_CurrentHandler) {
        m_CurrentHandler.StartStory();
      } else {
        Debug.LogError("AirWall must have a StoryHandler");
      }
    } 
    if (m_Colliders.Length == m_CurIndex + 1) {
      if (StoryEndType.SectionEnd == m_EndType) {
        EndSection();
      } else if(StoryEndType.GameEnd == m_EndType) {
        StartCoroutine(EndGame());
      }
    }
  }

  private void EndSection() {
    GameObject triggerObj = GameObject.Find("/EventObj/Story/TriggerObj");
    if (null != triggerObj) {
      triggerObj.SendMessage("OnLogicTrigger");
    } else {
      Debug.LogError("MainStory does not find TriggerObj");
    }
    /*
    GameObject dfmUIRootGO = GameObject.FindGameObjectWithTag("UI");
    if (dfmUIRootGO != null) {
      dfmUIRootGO.GetComponent<DFMUiRoot>().SetUIVisible(false);
    }
    flag = 1;
    OnShieldSword();   
    */
  }

  private IEnumerator EndGame() {
    // 关掉输入和移动
    TouchManager.TouchEnable = false;
    GameObject player = GetPlayer();
    if (null != player) {
      SharedGameObjectInfo info = LogicSystem.GetSharedGameObjectInfo(gameObject);
      if(null != info){
        LogicSystem.PublishLogicEvent("ge_set_ai_enable", "ai", info.m_LogicObjectId, false);
      }
    }
    // 慢放 3s
    Time.timeScale = 0.1f;
    yield return new WaitForSeconds(0.3f);
    Time.timeScale = 1.0f;
    // stage clear
    OnStageClear();
    player = GetPlayer();
    if (null != player) {
      BaseSkillManager bsm = player.GetComponent<BaseSkillManager>();
      if (null != bsm) {
        if (!bsm.IsUsingSkill()) {
        } else {
          SkillControllerInterface sc = bsm.GetSkillController();
          if (null != sc) {
            sc.ForceStopCurSkill();
          }
        }
      }
    }
    yield return new WaitForSeconds(0.9f);
    // 耍帅动作
    GameObject gfxGameRoot = GameObject.Find("GfxGameRoot");
    MainCamera cameraControl = gfxGameRoot.GetComponent<MainCamera>();
    cameraControl.m_Distance = 10.0f;
    cameraControl.m_Height = 7.0f;
    player = GetPlayer();
    if (null != player) {
      player.GetComponent<Animation>().PlayQueued("jianshi_shenguizhan_01", QueueMode.PlayNow);
    }
    yield return new WaitForSeconds(1.2f);
    cameraControl.m_Distance = 12.5f;
    cameraControl.m_Height = 7.9f;
    TouchManager.TouchEnable = true;
    OnShieldSword();
  }

  private void OnShieldSword() {
    GameObject uiRoot = GameObject.FindGameObjectWithTag("UI");
    if (null != uiRoot) {
      uiRoot.SendMessage("ShowShieldSword", 99999999);
    } else {
      Debug.LogError("MainStory can't find UICamera");
    }
  }
  private void OnStageClear() {
    GameObject uiRoot = GameObject.FindGameObjectWithTag("UI");
    if (null != uiRoot) {
      uiRoot.SendMessage("ShowStageClear", "Stage");
    } else {
      Debug.LogError("MainStory can't find UICamera");
    }
  }
	// Update is called once per frame
	void Update () {
    bool playSkill = false;
    if (Input.GetKeyDown(KeyCode.N)) {
      playSkill = true;
    } else if (Input.touchCount == 1) {
      var touch = Input.GetTouch(0);
      //DashFire.LogSystem.Debug("test new skill,touch.phase {0}, touch.tapCount {1}, isLastHitUi {2}", touch.phase, touch.tapCount, DashFire.LogicSystem.IsLastHitUi);
      if ((touch.phase == TouchPhase.Canceled || touch.phase == TouchPhase.Ended) && touch.tapCount == 3 && !DashFire.LogicSystem.IsLastHitUi) {
        playSkill = true;
      }
    }
    if (playSkill) {
      if (null != DashFire.LogicSystem.PlayerSelf) {
      }
    }
  }

  private GameObject GetPlayer() {
    return GameObject.FindWithTag("Player");
  }
}
