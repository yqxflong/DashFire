using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using StoryDlg;

public class DFMUiRoot : MonoBehaviour
{
  // Use this for initialization
  public enum SceneId
  {
    DEMO1 = 2,
    DEMO2 = 3,
    LOGIN = 6
  }
  void Start()
  {
    if (uiConfig != null)
      UIManager.Instance.Init(uiConfig);
    DontDestroyOnLoad(this.gameObject.transform.parent);
    m_UIRootGO = this.gameObject;
    DashFire.LogicSystem.EventChannelForGfx.Subscribe<string, string, long, int>("ge_init_userinfo", "lobby", this.InitUserinfo);
    DashFire.LogicSystem.EventChannelForGfx.Subscribe<int>("ge_load_ui_in_game", "ui", this.LoadUiInGame);
    DashFire.LogicSystem.EventChannelForGfx.Subscribe("ge_loading_finish", "ui", OnEnterNewScene);
    //Loading
    DashFire.LogicSystem.EventChannelForGfx.Subscribe("ge_loading_start", "ui", StartLoading);
    DashFire.LogicSystem.EventChannelForGfx.Subscribe("ge_loading_finish", "ui", EndLoading);
    DashFire.LogicSystem.EventChannelForGfx.Subscribe("ge_show_relive", "ui", ShowRelive);
    m_ReliveCtrl.Init(this.gameObject);
  }
  // Update is called once per frame

  void Update()
  {
    if (!m_IsBloomEnd) {
      m_BloomTime -= RealTime.deltaTime;
      if (m_BloomTime <= 0) {
        m_IsBloomEnd = true;
        ShowLogin();
      }
    }
  }

  public void ShowRelive()
  {
    m_ReliveCtrl.ShowReliveUi();
    TouchManager.TouchEnable = false;
    JoyStickInputProvider.JoyStickEnable = false;
  }
  public void ShowLogin()
  {
    GameObject go = DashFire.ResourceSystem.GetSharedResource("UI/LoginPrefab") as GameObject;
    if (null == go)
      return;
    go = NGUITools.AddChild(this.gameObject, go);
  }

  public void LoadUiInGame(int sceneId)
  {
    sceneIdForLoading = sceneId;
    if (sceneId == (int)SceneId.LOGIN) {
      UIManager um = UIManager.Instance;
      if (um == null)
        return;
      if (m_IsUiLoaded) {

        um.SetAllUiVisible(this.gameObject, false);
        um.LoadAllWindows(this.gameObject, sceneId, UICamera.mainCamera);
      } else {
        um.LoadAllWindows(this.gameObject, sceneId, UICamera.mainCamera);
      }
    } else if (null != uiConfig && !m_IsUiLoaded) {
      m_IsUiLoaded = true;
      UIManager.Instance.LoadAllWindows(this.gameObject, -1, UICamera.mainCamera);
      InitDirection();
      InitTouchCircle();
      InitEX();
      InitSkillTeach();
      InitResource();
      //InitAndShowSceneName(); //ShowShieldSword(99999999L);
      InitEvaluate();

      DashFire.LogicSystem.EventChannelForGfx.Subscribe<float, float, float, int>("ge_hero_blood", "ui", ShowAddHPForHero);
      DashFire.LogicSystem.EventChannelForGfx.Subscribe<float, float, float, int>("ge_npc_odamage", "ui", ShowbloodFlyTemplate);
      DashFire.LogicSystem.EventChannelForGfx.Subscribe<float, float, float, int>("ge_npc_cdamage", "ui", ShowCriticalDamage);
      DashFire.LogicSystem.EventChannelForGfx.Subscribe<long>("ge_endscore", "ui", ShowShieldSword);
      //hit
      DashFire.LogicSystem.EventChannelForGfx.Subscribe<int>("ge_hitcount", "ui",
        (int number) => {
          if (number <= 0) {
            EndChainBeat();
          } else {
            ShowChainBeat(number);
          }
        }
        );
      //蓄力
      DashFire.LogicSystem.EventChannelForGfx.Subscribe<float, float, float, float, int>("ge_monster_power", "ui", ShowMonsterPrePower);

      //初始化剧情配置数据
      InitStoryDlg();
    } else if (m_IsUiLoaded) {
      UIManager.Instance.SetUiVisibleByConfig(this.gameObject, -1, true);
    }
  }

  //枪手换枪按钮
  public void ShowGunChangeButton(bool state)
  {
    GameObject go = null;
    Transform trans = null;
    //是否已经存在
    trans = this.transform.Find("GunChange(Clone)");
    if (trans == null) {
      Debug.LogError("Can not find GunChange Button");
    } else {
      go = trans.gameObject;
      NGUITools.SetActive(go, state);
    }
  }

  public void SetUIVisible(bool flag)
  {   
    UIManager.Instance.SetUiVisibleByConfig(this.gameObject, -1, flag);
  }

  private void InitStoryDlg()
  {
    StoryDlgManager.Instance.Init();
    GameObject parentGO = GameObject.Find("/UI Root/Camera");
    if (parentGO != null) {
      Transform parentTransform = parentGO.transform;  //剧情对话框的父Transform
      Transform t = parentTransform.Find("StoryDlgSmall");
      if (t == null) {
        //第一次加载
        GameObject go = DashFire.ResourceSystem.GetSharedResource("UI/StoryDlgSmall") as GameObject;
        if (go != null) {
          m_StoryDlgSmallGO = NGUITools.AddChild(parentTransform.gameObject, go); //NGUI加载
          m_StoryDlgSmallGO.name = "StoryDlgSmall";
          //int dlgHeight = m_StoryDlgSmallGO.GetComponent<UISprite>().height;
          Vector3 position = new Vector3();
          position.x = Screen.width / 2.1f;
          position.y = Screen.height / 3.3f;
          position.z = 0.0f;
          m_StoryDlgSmallGO.transform.position = UICamera.mainCamera.ScreenToWorldPoint(position);
          //初始化时显示剧情对话框，避免游戏中第一次显示卡的问题
          NGUITools.SetActive(m_StoryDlgSmallGO, true);
        }
      }
      t = parentTransform.Find("StoryDlgBig");
      if (t == null) {
        //第一次加载
        GameObject go = DashFire.ResourceSystem.GetSharedResource("UI/StoryDlgBig") as GameObject;
        if (go != null) {
          m_StoryDlgBigGO = NGUITools.AddChild(parentTransform.gameObject, go); //NGUI加载
          m_StoryDlgBigGO.name = "StoryDlgBig";
          //int dlgHeight = m_StoryDlgBigGO.GetComponent<UISprite>().height;
          //初始化时显示剧情对话框，避免游戏中第一次显示卡的问题
          NGUITools.SetActive(m_StoryDlgBigGO, true);
        }
      }
    }
  }
  private void OnEnterNewScene()
  {
    //新场景加载完成时
    //注册剧情对话框结束事件StoryDlgOver监听者
    /*
    GameObject portalListener = GameObject.Find("/EventObj/Story/PortalListener");
    if (null != portalListener) {
      var handlerGO = portalListener.GetComponent<PortalTrigger>();
      if (handlerGO != null) {
        m_StoryDlgSmallGO.GetComponent<StoryDlgPanel>().StoryDlgOver += handlerGO.HandleStoryDlgOverEvent;
        m_StoryDlgBigGO.GetComponent<StoryDlgPanel>().StoryDlgOver += handlerGO.HandleStoryDlgOverEvent;
      }
    } else {
      //Debug.LogError("Cannot find portalListener");
    }
    */
    //隐藏剧情对话框
    if (m_StoryDlgSmallGO != null) {
      NGUITools.SetActive(m_StoryDlgSmallGO, false);
    }
    if (m_StoryDlgBigGO != null) {
      NGUITools.SetActive(m_StoryDlgBigGO, false);
    }
  }

  void InitUserinfo(string account, string nickname, long guid, int level)
  {
    GameUser.Instance.Account = account;
    GameUser.Instance.Nickname = nickname;
    GameUser.Instance.Guid = guid;
    GameUser.Instance.Level = level;
  }

  //蓄力
  public void ShowMonsterPrePower(float x, float y, float z, float duration, int monsterId)
  {
    if (duration <= 0)
      return;
    Vector3 pos = new Vector3(x, y, z);
    if (Camera.main != null)
      pos = Camera.main.WorldToScreenPoint(pos);
    pos.z = 0;
    Vector3 nguiPos = Vector3.zero;
    if (UICamera.mainCamera != null) {
      nguiPos = UICamera.mainCamera.ScreenToWorldPoint(pos);
    }

    GameObject go = DashFire.ResourceSystem.GetSharedResource("UI/MonsterPrePower") as GameObject;
    GameObject prePowerGo = NGUITools.AddChild(this.gameObject, go);
    if (prePowerGo == null)
      return;
    prePowerGo.transform.position = nguiPos;
    MonsterPrePower power = prePowerGo.GetComponent<MonsterPrePower>();
    if (power != null) {
      power.Duration = duration;
      power.PowerId = monsterId;
      power.Position = new Vector3(x, y, z);
    } else {
      NGUITools.SetActive(prePowerGo, false);
    }
  }
  //打断蓄力
  public void BreakPrePower(int monsterId)
  {
    for (int index = 0; index < this.transform.childCount; ++index) {
      Transform trans = this.transform.GetChild(index);
      GameObject go = null;
      if (trans != null)
        go = trans.gameObject;
      if (go != null && go.name == "MonsterPrePower(Clone)") {
        MonsterPrePower power = go.GetComponent<MonsterPrePower>();
        if (power == null)
          return;
        if (power.PowerId == monsterId) {
          NGUITools.SetActive(go, false);
          NGUITools.Destroy(go);
        }
      }
    }
  }

  //技能图标
  public void ShowHintFlag(Vector2 pos)
  {

  }
  /*
  public void ShowSkills(SkillIconInfo node)
  {
    GameObject go = DashFire.ResourceSystem.GetSharedResource("UI/SkillIcon") as GameObject;
    if (go == null) {
      Debug.LogError("Can not find UI/SkillIcon!");
      return;
    }
    go = NGUITools.AddChild(this.gameObject, go);
    if (go == null)
      return;
    m_GameObjectList.Add(go);
    SkillIcon sIcon = go.GetComponent<SkillIcon>();
    if (null != sIcon) {
      sIcon.SetSkillId(node.skillType);
      sIcon.SetCountDown(node.time);
    }
    if (UICamera.currentCamera != null) {
      if (node.skillType == SkillCategory.kSkillQ) {
        int height = (int)(Screen.height * (1 / (float)3));
        int width = (int)(Screen.width * (7 / (float)17));
        Vector3 screenPos = new Vector3(width, height, 0);
        go.transform.position = UICamera.currentCamera.ScreenToWorldPoint(screenPos);
      } else {
        int height = (int)(Screen.height * (1 / (float)3));
        int width = (int)(Screen.width * (10 / (float)17));
        Vector3 screenPos = new Vector3(width, height, 0);
        go.transform.position = UICamera.currentCamera.ScreenToWorldPoint(screenPos);
      }

    }

  }
  */
  //end
  public void ShowbloodFlyTemplate(float x, float y, float z, int blood)
  {
    Object obj = DashFire.ResourceSystem.NewObject("UI/attrackEffect", timeRecycle);
    GameObject go = obj as GameObject;
    if (null != go) {
      UILabel bloodPanel = go.GetComponent<UILabel>();
      if (null != bloodPanel)
        bloodPanel.text = blood.ToString();
      Vector3 pos = new Vector3(x, y, z);
      pos = Camera.main.WorldToScreenPoint(pos);
      pos.z = 0; pos.y += 100;
      pos = UICamera.mainCamera.ScreenToWorldPoint(pos);
      GameObject cube = NGUITools.AddChild(gameObject, obj);
      cube.transform.position = pos;
      NGUITools.SetActive(cube, true);
    }
  }
  public void ShowAddHPForHero(float x, float y, float z, int blood)
  {
    Object obj = null;
    GameObject go = null;
    if (blood > 0) {
      obj = DashFire.ResourceSystem.NewObject("UI/DamageForAddHero", timeRecycle);
      go = obj as GameObject;
      if (null != go) {
        UILabel bloodPanel = go.GetComponent<UILabel>();
        if (null != bloodPanel)
          bloodPanel.text = blood.ToString();
        Vector3 pos = new Vector3(x, y, z);
        pos = Camera.main.WorldToScreenPoint(pos);
        pos.z = 0; pos.y += 100;
        pos = UICamera.mainCamera.ScreenToWorldPoint(pos);
        GameObject cube = NGUITools.AddChild(gameObject, obj);
        cube.transform.position = pos;
        NGUITools.SetActive(cube, true);
      }
    }
    if (blood < 0) {
      obj = DashFire.ResourceSystem.NewObject("UI/DamageForCutHero", timeRecycle);
      go = obj as GameObject;
      if (null != go) {
        UILabel bloodPanel = go.GetComponent<UILabel>();
        if (null != bloodPanel)
          bloodPanel.text = blood.ToString();
        Vector3 pos = new Vector3(x, y, z);
        pos = Camera.main.WorldToScreenPoint(pos);
        pos.z = 0; pos.y -= 50;
        pos = UICamera.mainCamera.ScreenToWorldPoint(pos);
        GameObject cube = NGUITools.AddChild(gameObject, obj);
        cube.transform.position = pos;
        NGUITools.SetActive(cube, true);
      }
    }
  }
  public void ShowCriticalDamage(float x, float y, float z, int blood)
  {
    Object obj = DashFire.ResourceSystem.NewObject("UI/CriticalDamage", timeRecycle);
    GameObject go = obj as GameObject;
    if (null != go) {
      string damage = System.Math.Abs(blood).ToString();
      int i = damage.Length;
      for (int j = 0; j < 6; j++) {
        if (j < i) {
          Transform CD = go.transform.Find("Number" + j);
          if (CD != null) {
            UISprite SP = CD.gameObject.GetComponent<UISprite>();
            if (SP != null) {
              SP.spriteName = damage.ToCharArray()[j].ToString();
            }
          }
        } else {
          Transform CD = go.transform.Find("Number" + j);
          if (CD != null) {
            UISprite SP = CD.gameObject.GetComponent<UISprite>();
            if (SP != null) {
              SP.spriteName = null;
            }
          }
        }
      }
      Vector3 pos = new Vector3(x, y, z);
      pos = Camera.main.WorldToScreenPoint(pos);
      pos.z = 0; pos.y += 100;
      pos = UICamera.mainCamera.ScreenToWorldPoint(pos);
      GameObject cube = NGUITools.AddChild(gameObject, obj);
      cube.transform.position = pos;
      NGUITools.SetActive(cube, true);
    }
  }
  void ShowChainBeat(int chainnum)
  {
    int num = System.Math.Abs(chainnum) % 100;
    if (num > beatnum) {
      beatnum = num;
    } else {
      num = beatnum;
    }

    if (beatnum <= 33) {
      if (evaluateStr != "good") {
        evaluateStr = "good";
        ShowEvaluateChain();
      }
    } else if (beatnum <= 66) {
      if (evaluateStr != "great") {
        evaluateStr = "great";
        ShowEvaluateChain();
      }
    } else if (beatnum <= 99) {
      if (evaluateStr != "perfect") {
        evaluateStr = "perfect";
        ShowEvaluateChain();
      }
    }

    string chainstr = num.ToString();
    int i = chainstr.Length;
    if (i == 1) {
      chainstr = "0" + chainstr;
      i = 2;
    }
    if (ChainBeat != null) {
      if (chainBeatObj == null) {
        for (int j = 0; j < 2; j++) {
          if (j < i) {
            Transform CB = ChainBeat.transform.Find("Number" + j);
            if (CB != null) {
              UISprite SP = CB.gameObject.GetComponent<UISprite>();
              if (SP != null) {
                SP.spriteName = chainstr.ToCharArray()[j].ToString();
              }
            }
          } else {
            Transform CB = ChainBeat.transform.Find("Number" + j);
            if (CB != null) {
              UISprite SP = CB.gameObject.GetComponent<UISprite>();
              if (SP != null) {
                SP.spriteName = null;
              }
            }
          }
        }
        chainBeatObj = NGUITools.AddChild(gameObject, ChainBeat);
        UIManager um = UIManager.Instance;
        if (um.m_WindowsInfoDic.ContainsKey("ChainBeat")) {
          WindowInfo info = um.m_WindowsInfoDic["ChainBeat"];

          chainBeatObj.transform.position = UICamera.mainCamera.ScreenToWorldPoint(info.windowPos);

          NGUITools.SetActive(chainBeatObj, true);
          chainBeatObj.AddComponent("ChainBeat");
        }
      } else {
        for (int j = 0; j < 2; j++) {
          if (j < i) {
            Transform CB = chainBeatObj.transform.Find("Number" + j);
            if (CB != null) {
              UISprite SP = CB.gameObject.GetComponent<UISprite>();
              if (SP != null) {
                SP.spriteName = chainstr.ToCharArray()[j].ToString();
              }
            }
          } else {
            Transform CB = chainBeatObj.transform.Find("Number" + j);
            if (CB != null) {
              UISprite SP = CB.gameObject.GetComponent<UISprite>();
              if (SP != null) {
                SP.spriteName = null;
              }
            }
          }
        }
        Component cb = chainBeatObj.GetComponent("ChainBeat");
        if (cb == null) {
          chainBeatObj.AddComponent("ChainBeat");
        }
      }
    }
  }
  void EndChainBeat()
  {
    if (chainBeatObj != null) {
      beatnum = 0;
      evaluateStr = null;
      NGUITools.DestroyImmediate(chainBeatObj);
      chainBeatObj = null;
      ShowEvaluateChain();
    }
  }
  void ShowEvaluateChain()
  {
    if (evaluateObj != null) {
      Transform tf = evaluateObj.transform.Find("EvaluateImage");
      if (tf != null) {
        UISprite us = tf.gameObject.GetComponent<UISprite>();
        if (us != null) {
          us.spriteName = evaluateStr;
        }
      }
    }
  }
  void InitEvaluate()
  {
    if (EvaluateChainBeat != null) {
      Transform tf = EvaluateChainBeat.transform.Find("EvaluateImage");
      if (tf != null) {
        UISprite us = tf.gameObject.GetComponent<UISprite>();
        if (us != null) {
          us.spriteName = null;

          evaluateObj = NGUITools.AddChild(gameObject, EvaluateChainBeat);
          UIManager um = UIManager.Instance;
          if (um.m_WindowsInfoDic.ContainsKey("EvaluateChain")) {
            WindowInfo info = um.m_WindowsInfoDic["EvaluateChain"];
            info.windowPos.z = 0;
            evaluateObj.transform.position = UICamera.mainCamera.ScreenToWorldPoint(info.windowPos);

            NGUITools.SetActive(evaluateObj, true);
          }
        }
      }
    }
  }
  void InitHit()
  {
    if (hit == null) {
      GameObject go = DashFire.ResourceSystem.GetSharedResource("UI/Hit") as GameObject;
      if (go != null) {
        Transform tf = go.transform.Find("Label");
        UILabel label = tf.gameObject.GetComponent<UILabel>();
        label.text = "0";

        hit = NGUITools.AddChild(gameObject, go);

        UIManager um = UIManager.Instance;
        if (um.m_WindowsInfoDic.ContainsKey("Hit")) {
          WindowInfo info = um.m_WindowsInfoDic["Hit"];
          info.windowPos.z = 0;
          hit.transform.position = UICamera.mainCamera.ScreenToWorldPoint(info.windowPos);

          NGUITools.SetActive(hit, true);
        }
      }
    }
  }
  void ShowHit(int hitnum)
  {
    if (hit == null) return;
    Transform tf = hit.transform.Find("Label");
    if (tf != null) {
      UILabel label = tf.GetComponent<UILabel>();
      if (label != null) {
        label.text = System.Math.Abs(hitnum).ToString();
      }
    }
  }
  void InitChainKill()
  {
    if (chainkill == null) {
      GameObject go = DashFire.ResourceSystem.GetSharedResource("UI/ChainKill") as GameObject;
      if (go != null) {
        Transform tf = go.transform.Find("Label");
        UILabel label = tf.gameObject.GetComponent<UILabel>();
        label.text = "0";

        chainkill = NGUITools.AddChild(gameObject, go);

        UIManager um = UIManager.Instance;
        if (um.m_WindowsInfoDic.ContainsKey("ChainKill")) {
          WindowInfo info = um.m_WindowsInfoDic["ChainKill"];
          info.windowPos.z = 0;
          chainkill.transform.position = UICamera.mainCamera.ScreenToWorldPoint(info.windowPos);

          NGUITools.SetActive(chainkill, true);
        }
      }
    }
  }
  void ShowChainKill(int chainkillnum)
  {
    if (chainkill == null) return;
    Transform tf = chainkill.transform.Find("Label");
    if (tf != null) {
      UILabel label = tf.GetComponent<UILabel>();
      if (label != null) {
        int killnum = System.Math.Abs(chainkillnum);
        label.text = killnum.ToString();
        if (killnum <= 10) {
          label.color = new Color(0, 1, 0);
        } else if (killnum <= 20) {
          label.color = new Color(0, 0, 1);
        } else if (killnum <= 30) {
          label.color = new Color(0.8196f, 0, 1);
        } else {
          label.color = new Color(1, 0.64314f, 0);
        }
      }
    }
  }
  void InitScore()
  {
    if (score == null) {
      GameObject go = DashFire.ResourceSystem.GetSharedResource("UI/Score") as GameObject;
      if (go != null) {
        Transform tf = go.transform.Find("Label");
        UILabel label = tf.gameObject.GetComponent<UILabel>();
        label.text = "0";

        score = NGUITools.AddChild(gameObject, go);

        UIManager um = UIManager.Instance;
        if (um.m_WindowsInfoDic.ContainsKey("Score")) {
          WindowInfo info = um.m_WindowsInfoDic["Score"];
          info.windowPos.z = 0;
          score.transform.position = UICamera.mainCamera.ScreenToWorldPoint(info.windowPos);

          NGUITools.SetActive(score, true);
        }
      }
    }
  }
  void ShowScore(long scorenum)
  {
    if (score == null) return;
    Transform tf = score.transform.Find("Label");
    if (tf != null) {
      UILabel label = tf.GetComponent<UILabel>();
      if (label != null) {
        label.text = GetScoreText(System.Math.Abs(scorenum));
      }
    }
  }
  string GetScoreText(long score)
  {
    string str = score.ToString();
    int i = str.Length;
    string newstr = "";
    for (int j = 0; j < i; ++j) {
      if (j % 3 == 0 && j != 0) {
        newstr += ",";
      }
      newstr += str.ToCharArray()[j];
    }
    return newstr;
  }
  void ShowBossEncounter()
  {
    GameObject go = DashFire.ResourceSystem.GetSharedResource("UI/BossEncounter") as GameObject;
    if (go != null) {
      GameObject be = NGUITools.AddChild(gameObject, go);
      be.transform.localPosition = new Vector3(0, 0, 0);
      NGUITools.SetActive(be, true);
    }
  }
  public void InitAndShowSceneName()
  {
    if (null != SceneNameImage) {
      UISprite us = SceneNameImage.GetComponent<UISprite>();
      if (null != us)
        us.name = "scene1";

      GameObject scenename = NGUITools.AddChild(gameObject, SceneNameImage);
      scenename.transform.localPosition = new Vector3(0, 0, 0);
      NGUITools.SetActive(scenename, true);
    }
  }

  void InitResource()
  {
    ChainBeat = DashFire.ResourceSystem.GetSharedResource("UI/ChainBeat") as GameObject;
    EvaluateChainBeat = DashFire.ResourceSystem.GetSharedResource("UI/EvaluateChainBeat") as GameObject;
    SceneNameImage = DashFire.ResourceSystem.GetSharedResource("UI/SceneName") as GameObject;
  }

  public void HideUiWhenGameOver()
  {
    GameObject go = null;
    Transform ts = this.transform.Find("HeroPanel(Clone)");
    if (ts != null)
      go = ts.gameObject;
    if (go != null) {
      HeroPanel heroPanel = go.GetComponent<HeroPanel>();
      heroPanel.Hide();
    }
    ts = null;
    go = null;
    ts = this.transform.Find("SkillBar(Clone)");
    if (ts != null) {
      go = ts.gameObject;
      SkillBar skillBar = go.GetComponent<SkillBar>();
      if (skillBar != null) {
        skillBar.Hide();
      }
    }
    ts = null;
    go = null;
    ts = this.transform.Find("HeroChange(Clone)");
    if (ts != null) {
      go = ts.gameObject;
      NGUITools.SetActive(go, false);
    }
  }

  public void ShowStageClear(string type)
  {
    Transform existTrans = this.transform.Find("StageClear(Clone)");
    if (null != existTrans) {
      GameObject existGo = existTrans.gameObject;
      if (null != existGo)
        Destroy(existGo);
    }

    GameObject go = DashFire.ResourceSystem.GetSharedResource("UI/StageClear") as GameObject;
    if (null == go)
      return;
    go = NGUITools.AddChild(this.gameObject, go);
    if (go == null)
      return;
    StageClear stageClear = go.GetComponent<StageClear>();
    if (stageClear != null)
      stageClear.SetClearType(type);
    HideUiWhenGameOver();
  }
  void ShowShieldSword(long scorenum)
  {
    if (scorenum > 100000000) scorenum = 100000000;
    GameObject shieldsword = DashFire.ResourceSystem.GetSharedResource("UI/EndShield") as GameObject;
    if (shieldsword != null) {
      Transform tf = shieldsword.transform.Find("RShield");
      if (tf != null) {
        UISprite us = tf.gameObject.GetComponent<UISprite>();
        if (us != null) {
          us.fillAmount = 0;
        }
        tf = shieldsword.transform.Find("Label");
        if (tf != null) {
          UILabel ul = tf.gameObject.GetComponent<UILabel>();
          if (ul != null) {
            ul.text = scorenum.ToString();
            ul.alpha = 0.0f;
          }
        }
        tf = shieldsword.transform.Find("Sprite0");
        if (tf != null) {
          UISprite us2 = tf.gameObject.GetComponent<UISprite>();
          if (us2 != null) {
            us2.spriteName = null;
            us2.color = new Color(255, 255, 255);
          }
        }

        endshield = NGUITools.AddChild(gameObject, shieldsword);

        if (endshield != null) {
          UIManager um = UIManager.Instance;
          if (um.m_WindowsInfoDic.ContainsKey("EndShield")) {
            WindowInfo info = um.m_WindowsInfoDic["EndShield"];
            endshield.transform.position = UICamera.mainCamera.ScreenToWorldPoint(info.windowPos);

            NGUITools.SetActive(endshield, true);

            //endshield.AddComponent("EndShieldScore");
          }
        }
      }
    }
  }
  void HideShieldSword()
  {
    if (endshield != null) {
      DestroyImmediate(endshield);
      endshield = null;
    }
  }
  void StartLoading()
  {
    //end all blood ui
    gameObject.BroadcastMessage("EndForChangeSceneBlood");
    //

    if (loading != null) return;
    GameObject go = DashFire.ResourceSystem.GetSharedResource("Loading/Loading2") as GameObject;
    if (go != null) {
      loading = NGUITools.AddChild(gameObject, go);
      if (loading != null) {
        UISprite us = loading.GetComponent<UISprite>();
        if (us != null) {
          if (sceneIdForLoading == 3) {
            us.spriteName = "loading01";
          }
          if (sceneIdForLoading == 2) {
            us.spriteName = "Background";
          }
        }
        loading.transform.localPosition = new Vector3(0, 0, 0);
        NGUITools.SetActive(loading, true);
      }
    }
    OkEX(true);
  }
  void EndLoading()
  {
    //end all blood ui
    gameObject.BroadcastMessage("EndForChangeSceneBlood");
    //

    if (loading != null) {
      loading.transform.Find("ProgressBar").SendMessage("EndLoading");
      loading = null;
    }

    JoyStickInputProvider.JoyStickEnable = true;

    //if (sceneIdForLoading == (int)SceneId.DEMO1) {
    //  InitAndShowSceneName();
    //}
  }
  void InitDirection()
  {
    GameObject go = DashFire.ResourceSystem.GetSharedResource("UI/Direction") as GameObject;
    if (go != null) {
      GameObject ld = NGUITools.AddChild(gameObject, go);
      if (ld != null) {
        NGUITools.SetActive(ld, true);
      }
    }
  }
  void InitTouchCircle()
  {
    GameObject go = DashFire.ResourceSystem.GetSharedResource("UI/TouchCircle") as GameObject;
    if (go != null) {
      GameObject ld = NGUITools.AddChild(gameObject, go);
      if (ld != null) {
        NGUITools.SetActive(ld, true);
      }
    }
  }
  void OkEX(bool show)
  {
    if (ex != null) {
      ex.SendMessage("SetOkShow",show);
    }
  }
  void InitEX()
  {
    GameObject go = DashFire.ResourceSystem.GetSharedResource("UI/EX") as GameObject;
    if (go != null) {
      ex = NGUITools.AddChild(gameObject, go);
      if (ex != null) {
        ex.AddComponent("EXVisible");
        NGUITools.SetActive(ex, true);
      }
    }
  }
  void InitSkillTeach()
  {
    GameObject go = DashFire.ResourceSystem.GetSharedResource("UI/SkillTeach") as GameObject;
    if (go != null) {
      skillteach = NGUITools.AddChild(gameObject, go);
      if (skillteach != null) {
        NGUITools.SetActive(skillteach, true);
      }
    }
  }
  void ShowSkillTeach(int skillid)
  {
    if (skillteach != null) {
      Transform tf = skillteach.transform.Find("Sprite");
      if (tf != null) {
        UISprite us = tf.GetComponent<UISprite>();
        if (us != null) {
          us.spriteName = "skillteach" + skillid;
        }
      }
      NGUITools.SetActive(skillteach, true);
    }
  }
  private GameObject m_UIRootGO = null;
  private int beatnum = 0;
  private GameObject chainBeatObj = null;
  private GameObject hit = null;
  private GameObject chainkill = null;
  private GameObject score = null;
  private GameObject evaluateObj = null;
  private GameObject loading = null;
  private GameObject skillteach = null;
  private GameObject ex = null;
  private GameObject endshield = null;

  private GameObject ChainBeat = null;
  private GameObject EvaluateChainBeat = null;
  private GameObject SceneNameImage = null;
  private List<GameObject> m_GameObjectList = new List<GameObject>();
  private bool m_IsUiLoaded = false;
  private string evaluateStr = null;
  public TextAsset uiConfig = null;
  //循环时间
  private float timeRecycle = 10.0f;
  private int sceneIdForLoading = -1;
  //Loading场景Bloom所用时间
  private float m_BloomTime = 2.0f;
  private bool m_IsBloomEnd = false;

  private GameObject m_StoryDlgSmallGO = null;
  private GameObject m_StoryDlgBigGO = null;

  private ReliveCtrl m_ReliveCtrl = ReliveCtrl.Instance;
}

public class GameUser
{
  #region Singleton
  private static GameUser m_instance = new GameUser();
  public static GameUser Instance
  {
    get { return m_instance; }
  }
  #endregion

  public string Account
  {
    get { return m_Account; }
    set { m_Account = value; }
  }
  public string Nickname
  {
    get { return m_Nickname; }
    set { m_Nickname = value; }
  }
  public long Guid
  {
    get { return m_Guid; }
    set { m_Guid = value; }
  }
  public int Level
  {
    get { return m_Level; }
    set { m_Level = value; }
  }

  private string m_Account;
  private string m_Nickname;
  private long m_Guid = 0;
  private int m_Level;
}

