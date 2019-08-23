using UnityEngine;
using System;
using System.IO;
using System.Collections;
using DashFire;

public class GameLogic : MonoBehaviour
{
  internal void Awake()
  {
    DontDestroyOnLoad(this.gameObject);
  }
  // Use this for initialization
  internal void Start()
  {
    Application.targetFrameRate = 60;
    QualitySettings.vSyncCount = 1;
    QualitySettings.SetQualityLevel(1);
	try {
    if (!GameControler.IsInited) {
      Application.runInBackground = true;
      string dataPath = Application.dataPath;
      string persistentDataPath = Application.persistentDataPath;
      string streamingAssetsPath = Application.streamingAssetsPath;
      string tempPath = Application.temporaryCachePath;
      LogicSystem.GfxLog("dataPath:{0} persistentDataPath:{1} streamingAssetsPath:{2} tempPath:{3}", dataPath, persistentDataPath, streamingAssetsPath, tempPath);
#if UNITY_ANDROID
    GameControler.InitGame(tempPath,streamingAssetsPath);
#elif UNITY_IPHONE
    GameControler.InitGame(tempPath,streamingAssetsPath);
#else
      GameControler.InitGame(dataPath, streamingAssetsPath);
#endif
      GlobalVariables.Instance.IsMobile = true;
      GameControler.StartGame();
	  LogicSystem.SetLoadingBarScene("LoadingBar");
      //LogicSystem.PublishLogicEvent("ge_change_scene", "game", 6);
      Application.LoadLevel("Loading");	
    }
	} catch(Exception ex) {
	  LogicSystem.GfxLog("GameLogic.Start throw exception:{0}\n{1}", ex.Message, ex.StackTrace);
	}
  }

  // Update is called once per frame
  internal void Update()
  {
    if (!m_IsSettingModified) {
      QualitySettings.vSyncCount = 1;
      if (QualitySettings.vSyncCount == 1) {
        m_IsSettingModified = true;
      }
    }

    bool isLastHitUi = (UICamera.lastHit.collider != null);
    LogicSystem.IsLastHitUi = isLastHitUi;
    DebugConsole.IsLastHitUi = isLastHitUi;
    GameControler.TickGame();
  }

  public void LogToConsole(string msg)
  {
    DebugConsole.Log(msg);
  }

  public void OnGfxDebugCommand(string script)
  {

  }

  public void OnLogicDebugCommand(string script)
  {

  }

  public void SendImpact(object[] args)
  {
    if (args.Length == 3) {
      SendImpact(args[0] as GameObject, args[1] as GameObject, (int)args[2]);
    }
  }

  private void SendImpact(GameObject sender, GameObject target, int hitCount)
  {
    ImpactInfo info = new ImpactInfo();
    info.m_Type = ImpactType.HitFly;
    ImpactSystem.Instance.SendImpact(sender, target, info, hitCount);
  }

  private bool m_IsSettingModified = false;
  private bool m_IsInit = false;
}
