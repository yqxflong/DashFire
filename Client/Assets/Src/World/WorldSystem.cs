
/**
 * @file GameSystem.cs
 * @brief 游戏系统
 *          负责：
 *                  切换场景
 *                  预加载资源
 *
 * @author lixiaojiang
 * @version 1.0.0
 * @date 2012-12-16
 */

using System;
using System.Collections.Generic;
using System.Text;
//using System.Diagnostics;
using DashFireSpatial;
using DashFireMessage;
using DashFire.Network;
using ScriptRuntime;

namespace DashFire
{
  public class WorldSystemProfiler
  {
    public long sceneTickTime;
    public long entityMgrTickTime;
    public long controlSystemTickTime;
    public long movementSystemTickTime;
    public long spatialSystemTickTime;
    public long aiSystemTickTime;
    public long usersTickTime;
    public long npcsTickTime;
    public long combatSystemTickTime;

    public string GenerateLogString(long tickTime)
    {
      StringBuilder builder = new StringBuilder();

      builder.Append("WorldSystem.Tick consume ").Append(tickTime).AppendLine();

      builder.Append("=>sceneTickTime:").Append(sceneTickTime).AppendLine();
      builder.Append("=>entityMgrTickTime:").Append(entityMgrTickTime).AppendLine();
      builder.Append("=>controlSystemTickTime:").Append(controlSystemTickTime).AppendLine();
      builder.Append("=>movementSystemTickTime:").Append(movementSystemTickTime).AppendLine();
      builder.Append("=>spatialSystemTickTime:").Append(spatialSystemTickTime).AppendLine();
      builder.Append("=>aiSystemTickTime:").Append(aiSystemTickTime).AppendLine();
      builder.Append("=>usersTickTime:").Append(usersTickTime).AppendLine();
      builder.Append("=>npcsTickTime:").Append(npcsTickTime).AppendLine();
      builder.Append("=>combatSystemTickTime:").Append(combatSystemTickTime).AppendLine();

      return builder.ToString();
    }
  }
  /**
   * @brief 游戏系统
   */
  public class WorldSystem
  {

    /**
     * @brief Singleton
     *
     * @return 
     */
    #region Singleton
    private static WorldSystem s_Instance = new WorldSystem();
    public static WorldSystem Instance
    {
      get { return s_Instance; }
    }

    #endregion

    //---------------------------------------------------------
    // 标准接口
    //---------------------------------------------------------

    /**
     * @brief 初始化
     *
     * @return 
     */
    public void Init()
    {
      m_IsObserver = false;
      m_CurScene = null;

      GfxSystem.EventChannelForLogic.Subscribe("ge_change_hero", "game", ChangeHero);
      GfxSystem.EventChannelForLogic.Subscribe<int>("ge_change_scene", "game", ChangeScene);
      GfxSystem.EventChannelForLogic.Subscribe("ge_reload_npc", "npc", ReloadNpc);
      GfxSystem.EventChannelForLogic.Subscribe<int, float, float, float, float>("ge_create_npc", "npc", CreateNpcWithPos);
      GfxSystem.EventChannelForLogic.Subscribe<int, int>("ge_create_npc_by_story", "npc", CreateNpcByStory);
      GfxSystem.EventChannelForLogic.Subscribe<int, bool>("ge_set_ai_enable", "ai", SetAIEnable);
      GfxSystem.EventChannelForLogic.Subscribe<int>("ge_notify_npc_dead", "npc", OnNpcDead);
      GfxSystem.EventChannelForLogic.Subscribe("ge_player_relive", "player", OnUserRelive);
      GfxSystem.EventChannelForLogic.Subscribe("ge_set_max_rage", "player", OnSetRageMax);
      NpcManager.OnDamage = new DamageDelegation(NpcManager_OnDamage);
      UserManager.OnDamage = new DamageDelegation(UserManager_OnDamage);
    }

    void UserManager_OnDamage(int receiver, int caster, bool /*isShootDamage*/isOrdinaryDamage, bool isCritical, int hpDamage, int npDamage)
    {
      if (receiver == PlayerSelfId) {
        UserInfo charObj = GetPlayerSelf();
        if (null != charObj) {
          Vector3 pos = charObj.GetMovementStateInfo().GetPosition3D();
          if (hpDamage != 0) {
            GfxSystem.PublishGfxEvent("ge_hero_blood", "ui", pos.X, pos.Y, pos.Z, hpDamage);
          }
        }
      }
    }

    void NpcManager_OnDamage(int receiver, int caster, bool /*isShootDamage*/isOrdinaryDamage, bool isCritical, int hpDamage, int npDamage)
    {
      if (caster == PlayerSelfId) {
        CharacterInfo charObj = GetCharacterById(receiver);

        if (null != charObj) {
          Vector3 pos = charObj.GetMovementStateInfo().GetPosition3D();
          if (isOrdinaryDamage) {
            GfxSystem.PublishGfxEvent("ge_npc_odamage", "ui", pos.X, pos.Y, pos.Z, hpDamage);
          }
          if (isCritical) {
            GfxSystem.PublishGfxEvent("ge_npc_cdamage", "ui", pos.X, pos.Y, pos.Z, hpDamage);
          }
        }
        GfxSystem.PublishGfxEvent("ge_small_monster_healthbar", "ui", charObj.Hp, charObj.GetActualProperty().HpMax, hpDamage);
      }
    }

    /**
     * @brief 释放
     *
     * @return 
     */
    public void Release()
    {
      if (null != m_CurScene) {
        m_CurScene.Release();
      }
    }

    public void QuitBattle()
    {
      NetworkSystem.Instance.QuitBattle();
    }

    public void QuitClient()
    {
      NetworkSystem.Instance.QuitClient();
      LobbyNetworkSystem.Instance.QuitClient();
    }

    public bool IsPveScene()
    {
      return m_CurScene.IsPve;
    }

    public bool IsPvpScene()
    {
      return m_CurScene.IsPvp;
    }

    public bool IsMultiPveScene()
    {
      return m_CurScene.IsMultiPve;
    }

    /**
     * @brief 逻辑循环
     */
    public void Tick()
    {
      TimeSnapshot.Start();
      TimeSnapshot.DoCheckPoint();
      if (m_CurScene == null || !m_CurScene.IsSuccessEnter) {
        return;
      }
      m_Profiler.sceneTickTime = TimeSnapshot.DoCheckPoint();

      EntityManager.Instance.Tick();
      m_Profiler.entityMgrTickTime = TimeSnapshot.DoCheckPoint();

      ControlSystem.Instance.Tick();
      m_Profiler.controlSystemTickTime = TimeSnapshot.DoCheckPoint();

      m_Profiler.movementSystemTickTime = TimeSnapshot.DoCheckPoint();

      m_SpatialSystem.Tick();
      m_Profiler.spatialSystemTickTime = TimeSnapshot.DoCheckPoint();
      if (m_Profiler.spatialSystemTickTime > 50000) {
        LogSystem.Warn("*** SpatialSystem tick time is {0}", m_Profiler.spatialSystemTickTime);
        for (LinkedListNode<UserInfo> node = UserManager.Users.FirstValue; null != node; node = node.Next) {
          UserInfo userInfo = node.Value;
          if (null != userInfo) {
            LogSystem.Warn("===>User:{0} Pos:{1}", userInfo.GetId(), userInfo.GetMovementStateInfo().GetPosition3D().ToString());
          }
        }
        for (LinkedListNode<NpcInfo> node = NpcManager.Npcs.FirstValue; null != node; node = node.Next) {
          NpcInfo npcInfo = node.Value;
          if (null != npcInfo) {
            LogSystem.Warn("===>Npc:{0} Pos:{1}", npcInfo.GetId(), npcInfo.GetMovementStateInfo().GetPosition3D().ToString());
          }
        }
      }

      m_AiSystem.Tick();
      m_Profiler.aiSystemTickTime = TimeSnapshot.DoCheckPoint();

      //obj特殊逻辑处理
      TickUsers();
      m_Profiler.usersTickTime = TimeSnapshot.DoCheckPoint();

      TickNpcs();
      m_Profiler.npcsTickTime = TimeSnapshot.DoCheckPoint();

      try {
        TickSystemByCharacters();
      } catch (Exception e) {
        GfxSystem.GfxLog("Exception:{0}\n{1}", e.Message, e.StackTrace);
      }
      m_Profiler.combatSystemTickTime = TimeSnapshot.DoCheckPoint();

      if (IsPveScene()) {
        TickPve();
      }

      long tickTime = TimeSnapshot.End();
      if (tickTime > 100000) {
        LogSystem.Debug("*** PerformanceWarning: {0}", m_Profiler.GenerateLogString(tickTime));
      }
    }

    public void SwitchDebug()
    {
      GlobalVariables.Instance.IsDebug = !GlobalVariables.Instance.IsDebug;
      if (!IsPveScene()) {
        Msg_CR_SwitchDebug builder = new Msg_CR_SwitchDebug();
        builder.is_debug = GlobalVariables.Instance.IsDebug;
        NetworkSystem.Instance.SendMessage(builder);
      }
      if (!GlobalVariables.Instance.IsDebug) {
        EntityManager.Instance.MarkSpaceInfoViews();
        EntityManager.Instance.DestroyUnusedSpaceInfoViews();
      } else {
        DrawObstacle();
      }
    }

    public void SwitchObserver()
    {
      if (m_IsObserver) {
        m_IsFollowObserver = !m_IsFollowObserver;
        if (m_IsFollowObserver) {
          LinkedListNode<UserInfo> node = null;
          for (node = UserManager.Users.FirstValue; null != node; node = node.Next) {
            if (node.Value == m_PlayerSelf)
              continue;
            if (node.Value.GetId() == m_FollowTargetId) {
              break;
            }
          }
          if (null == node) {
            for (node = UserManager.Users.FirstValue; null != node; node = node.Next) {
              if (node.Value == m_PlayerSelf)
                continue;
              else
                break;
            }
          }
          if (null != node) {
            m_FollowTargetId = node.Value.GetId();
            GfxSystem.PublishGfxEvent("ge_pvpSelfCombatInfo", "ui", node.Value.GetCombatStatisticInfo());
            if (!node.Value.IsDead()) {
              UserView view = EntityManager.Instance.GetUserViewById(m_FollowTargetId);
              if (null != view) {
                GfxSystem.SendMessage("GfxGameRoot", "CameraFollow", view.Actor);
              }
            }
          }
        }
      }
    }
    public bool InteractObject()
    {
      if (m_IsObserver) {
        if (m_IsFollowObserver) {
          LinkedListNode<UserInfo> node = null;
          for (node = UserManager.Users.FirstValue; null != node; node = node.Next) {
            if (node.Value == m_PlayerSelf)
              continue;
            if (node.Value.GetId() == m_FollowTargetId) {
              node = node.Next;
              break;
            }
          }
          if (null == node) {
            for (node = UserManager.Users.FirstValue; null != node; node = node.Next) {
              if (node.Value == m_PlayerSelf)
                continue;
              else
                break;
            }
          }
          if (null != node) {
            m_FollowTargetId = node.Value.GetId();
            GfxSystem.PublishGfxEvent("ge_pvpSelfCombatInfo", "ui", node.Value.GetCombatStatisticInfo());
            /*if (!node.Value.IsDead())
              GfxSystemExt.GfxSystem.Instance.MainCamera.FollowCharacter(m_FollowTargetId, GetCharacterPosition);*/
          }
        }
        return true;
      }
      bool sendMsg = false;
      if (null != m_PlayerSelf) {
        int initiator = m_PlayerSelf.GetId();
        int receiver = 0;

        //释放控制为低优先级操作
        if (!sendMsg && null != m_PlayerSelf.ControlledObject) {
          receiver = m_PlayerSelf.ControlledObject.GetId();
          sendMsg = true;
        }

        if (sendMsg) {
          Msg_CRC_InteractObject builder = new Msg_CRC_InteractObject();
          //builder.InitiatorId = initiator;
          builder.receiver_id = receiver;
          NetworkSystem.Instance.SendMessage(builder);
        }
      }
      return sendMsg;
    }

    public void ChangeScene(int sceneId)
    {
      DestroyHero();
      ChangeNextScene(sceneId);
    }
    private void OnLoadFinish()
    {
      GfxSystem.PublishGfxEvent("ge_loading_finish", "ui");
    }

    public bool ChangeNextScene(int sceneId)
    {
      GfxSystem.PublishGfxEvent("ge_load_ui_in_game", "ui", sceneId);
      /*==========================
       * 登录场景Loading的ID为6
       * 游戏结束时跳回该场景
       */
      if (sceneId == 6) {
        UserInfo user = GetPlayerSelf();
        if (null != user) {
          EntityManager.Instance.DestroyUserView(user.GetId());
          DestroyCharacterById(user.GetId());
        }
        if (null != m_CurScene) {
          /*
          if (m_CurScene.ResId == sceneId) {
            return true;
          }
          */
          m_CurScene.Release();
          m_CurScene = null;
        }
        Data_SceneConfig sceneConfig = SceneConfigProvider.Instance.GetSceneConfigById(sceneId);
        GfxSystem.LoadScene(sceneConfig.m_ClientSceneFile, OnLoadFinish);
        return true;
      }
      //==============================

      //GfxSystem.PublishGfxEvent("ge_load_ui_in_game", "ui", sceneId);

      if (null != m_CurScene) {
        /*
        if (m_CurScene.ResId == sceneId) {
          return true;
        }
        */
        m_CurScene.Release();
        m_CurScene = null;
      }
      m_CurScene = new SceneResource();
      if (null != m_CurScene) {
        m_CurScene.Init(sceneId);
        if (null != m_CurScene.SceneConfig) {
          Data_SceneConfig scene_config = SceneConfigProvider.Instance.GetSceneConfigById(m_CurScene.ResId);
          m_SpatialSystem.Init(FilePathDefine_Client.C_RootPath + scene_config.m_BlockInfoFile, scene_config.m_ReachableSet);
          m_SpatialSystem.LoadPatch(FilePathDefine_Client.C_RootPath + scene_config.m_BlockInfoFile + ".patch");
          m_SpatialSystem.LoadObstacle(FilePathDefine_Client.C_RootPath + scene_config.m_ObstacleFile, 1 / scene_config.m_TiledDataScale);

          LogSystem.Debug("init SpatialSystem:{0}", FilePathDefine_Client.C_RootPath + scene_config.m_BlockInfoFile);
          LogSystem.Debug("GameSystem.ChangeNextScene:{0}", m_CurScene.ResId);
          if (sceneId == 6) {
            UserInfo user = GetPlayerSelf();
            if (null != user) {
              EntityManager.Instance.DestroyUserView(user.GetId());
              DestroyCharacterById(user.GetId());
            }
          }
          return true;
        }

      }


      return false;
    }

    public int GetCurSceneId()
    {
      if (m_CurScene != null) {
        return m_CurScene.ResId;
      }
      return 0;
    }

    public SceneResource GetCurScene()
    {
      return m_CurScene;
    }

    public void StartGame()
    {
      UserInfo user = GetPlayerSelf();
      if (null != user) {
        EntityManager.Instance.DestroyUserView(user.GetId());
        DestroyCharacterById(user.GetId());
      }
      user = CreatePlayerSelf(1, NetworkSystem.Instance.HeroId);
      user.SetCampId(NetworkSystem.Instance.CampId);
      Data_Unit unit = m_CurScene.StaticData.ExtractData(DataMap_Type.DT_Unit, GlobalVariables.GetUnitIdByCampId(NetworkSystem.Instance.CampId)) as Data_Unit;
      if (null != unit) {
        user.GetMovementStateInfo().SetPosition(unit.m_Pos);
        user.GetMovementStateInfo().SetFaceDir(unit.m_RotAngle);
        user.SetHp(Operate_Type.OT_Absolute, user.GetActualProperty().HpMax);
      }
      EntityManager.Instance.CreatePlayerSelfView(1);
      UserView view = EntityManager.Instance.GetUserViewById(1);
      if (null != view) {
        view.Visible = true;
      }

      foreach (Data_Unit npcUnit in m_CurScene.StaticData.m_UnitMgr.GetData().Values) {
        if (npcUnit.m_IsEnable) {
          NpcInfo npc = m_NpcMgr.GetNpcInfoByUnitId(npcUnit.GetId());
          if (null == npc) {
            npc = m_NpcMgr.AddNpc(npcUnit);
          }
          if (null != npc) {
            EntityManager.Instance.CreateNpcView(npc.GetId());
          }
        }
      }
      GfxSystem.PublishGfxEvent("ge_on_game_start", "story");
      LogSystem.Debug("start game");
    }

    public void ReloadNpc()
    {
      foreach (Data_Unit npcUnit in m_CurScene.StaticData.m_UnitMgr.GetData().Values) {
        if (npcUnit.m_IsEnable) {
          NpcInfo npc = m_NpcMgr.GetNpcInfoByUnitId(npcUnit.GetId());
          if (null == npc) {
            npc = m_NpcMgr.AddNpc(npcUnit);
          }
          if (null != npc) {
            EntityManager.Instance.CreateNpcView(npc.GetId());
          }
        }
      }
    }
    public void ChangeHero()
    {
      UserInfo user = GetPlayerSelf();
      if (null != user) {
        Vector3 pos = user.GetMovementStateInfo().GetPosition3D();
        float dir = user.GetMovementStateInfo().GetFaceDir();
        int hp = user.Hp;
        int rage = user.Rage;

        EntityManager.Instance.DestroyUserView(user.GetId());
        DestroyCharacterById(user.GetId());

        NetworkSystem.Instance.HeroId = (NetworkSystem.Instance.HeroId + 1)%4;
        if (NetworkSystem.Instance.HeroId == 0)
          NetworkSystem.Instance.HeroId = 1;

        user = CreatePlayerSelf(1, NetworkSystem.Instance.HeroId);
        user.SetCampId(NetworkSystem.Instance.CampId);
        /*Data_Unit unit = m_CurScene.StaticData.ExtractData(DataMap_Type.DT_Unit, GlobalVariables.GetUnitIdByCampId(NetworkSystem.Instance.CampId)) as Data_Unit;
        if (null != unit) {
          user.GetMovementStateInfo().SetPosition(unit.m_Pos);
          user.GetMovementStateInfo().SetFaceDir(unit.m_RotAngle);
          user.SetHp(Operate_Type.OT_Absolute, 1000);
        }*/
        user.GetMovementStateInfo().SetPosition(pos);
        user.GetMovementStateInfo().SetFaceDir(dir);
        user.SetHp(Operate_Type.OT_Absolute, hp);
        user.SetRage(Operate_Type.OT_Absolute, rage);
        EntityManager.Instance.CreatePlayerSelfView(1);
        UserView view = EntityManager.Instance.GetUserViewById(1);
        if (null != view) {
          view.Visible = true;
        }
      }
    }
    public void DestroyHero()
    {
      UserInfo user = GetPlayerSelf();
      if (null != user) {
        EntityManager.Instance.DestroyUserView(user.GetId());
        DestroyCharacterById(user.GetId());
      }
    }

    private void TickPve()
    {
      if (null != m_PlayerSelf) {
        long curTime = TimeUtility.GetLocalMilliseconds();
        if (m_PlayerSelf.LastHitTime + 1500 < curTime) {
          m_PlayerSelf.MultiHitCount = 0;
          GfxSystem.PublishGfxEvent("ge_hitcount", "ui", 0);
        }
      }
    }

    private void TickUsers()
    {
      for (LinkedListNode<UserInfo> linkNode = m_UserMgr.Users.FirstValue; null != linkNode; linkNode = linkNode.Next) {
        UserInfo info = linkNode.Value;
        if (info.LevelChanged || info.GetShootStateInfo().WeaponChanged || info.GetSkillStateInfo().BuffChanged || info.GetEquipmentStateInfo().EquipmentChanged) {
          LogSystem.Debug("UserAttrCalculate LevelChanged:{0} WeaponChanged:{1} BuffChanged:{2} EquipmentChanged:{3}", info.LevelChanged, info.GetShootStateInfo().WeaponChanged, info.GetSkillStateInfo().BuffChanged, info.GetEquipmentStateInfo().EquipmentChanged);
          UserAttrCalculator.Calc(info);
          info.LevelChanged = false;
          info.GetShootStateInfo().WeaponChanged = false;
          info.GetSkillStateInfo().BuffChanged = false;
          info.GetEquipmentStateInfo().EquipmentChanged = false;
        }
      }
      UserInfo player = WorldSystem.Instance.GetPlayerSelf();
      if (null != player && player.Hp <= 0) {
        if (player.DeadTime <= 0) {
          GfxSystem.PublishGfxEvent("ge_show_relive", "ui", null);
          player.DeadTime = TimeUtility.GetServerMilliseconds();
        }
      }
    }

    private void TickNpcs()
    {
      List<NpcInfo> deletes = new List<NpcInfo>();
      for (LinkedListNode<NpcInfo> linkNode = m_NpcMgr.Npcs.FirstValue; null != linkNode; linkNode = linkNode.Next) {
        NpcInfo info = linkNode.Value;
        if (info.LevelChanged || info.GetShootStateInfo().WeaponChanged || info.GetSkillStateInfo().BuffChanged || info.GetEquipmentStateInfo().EquipmentChanged) {
          NpcAttrCalculator.Calc(info);
          info.LevelChanged = false;
          info.GetShootStateInfo().WeaponChanged = false;
          info.GetSkillStateInfo().BuffChanged = false;
          info.GetEquipmentStateInfo().EquipmentChanged = false;
        }

        // 约定npc的高度低于140时，直接判定npc死亡。
        if (140.0f > info.GetMovementStateInfo().GetPosition3D().Y) {
          info.SetHp(Operate_Type.OT_Absolute, 0);
        }
        if (info.NeedDelete) {
          deletes.Add(info);
        } else if (info.Hp <= 0) {
          if (!info.LogicDead) {
            GfxSystem.PublishGfxEvent("ge_on_npc_dead", "story", info.GetUnitId());
            info.LogicDead = true;
          }
          if (info.DeadTime <= 0) {
          } else if (TimeUtility.GetServerMilliseconds() - info.DeadTime > info.ReleaseTime) {
            deletes.Add(info);
          }
        }
        if (info.IsBorning && IsNpcBornOver(info)) {
          info.IsBorning = false;
          info.SetAIEnable(true);
          info.SetStateFlag(Operate_Type.OT_RemoveBit, CharacterState_Type.CST_Invincible);
        }
      }
      if (deletes.Count > 0) {
        foreach (NpcInfo ni in deletes) {
          CharacterView view = EntityManager.Instance.GetCharacterViewById(ni.GetId());
          if (null != view) {
            GfxSystem.SendMessage(view.Actor, "OnDead", null);
          }
          EntityManager.Instance.DestroyNpcView(ni.GetId());
          WorldSystem.Instance.DestroyCharacterById(ni.GetId());
          return;
        }
      }
    }

    private bool IsNpcBornOver(NpcInfo npc)
    {
      if (npc == null) {
        return false;
      }
      long cur_time = TimeUtility.GetServerMilliseconds();
      long born_anim_time = npc.BornAnimTimeMs;
      if ((npc.BornTime + born_anim_time) > cur_time) {
        return false;
      } else {
        return true;
      }
    }

    private void DrawObstacle()
    {
      if (m_IsDebugObstacleCreated)
        return;
      m_IsDebugObstacleCreated = true;
      if (null != m_SpatialSystem && null != m_PlayerSelf) {
        CellManager cellMgr = m_SpatialSystem.GetCellManager();
        if (null != cellMgr) {
          int maxRows = cellMgr.GetMaxRow();
          int maxCols = cellMgr.GetMaxCol();
          for (int i = 0; i < maxRows; ++i) {
            for (int j = 0; j < maxCols; ++j) {
              Vector3 pt = cellMgr.GetCellCenter(i, j);
              byte status = cellMgr.GetCellStatus(i, j);
              if (BlockType.GetBlockType(status) != BlockType.NOT_BLOCK) {
                GfxSystem.DrawCube(pt.X, pt.Y, pt.Z, true);
              }
            }
          }
        }
      }
    }

    public void LoadData()
    {
      SceneConfigProvider.Instance.Load(FilePathDefine_Client.C_SceneConfig, "ScenesConfigs");
      SceneConfigProvider.Instance.LoadAllSceneConfig(FilePathDefine_Client.C_RootPath);

      ActionConfigProvider.Instance.Load(FilePathDefine_Client.C_ActionConfig, "ActionConfig");
      NpcConfigProvider.Instance.LoadNpcConfig(FilePathDefine_Client.C_NpcConfig, "NpcConfig");
      NpcConfigProvider.Instance.LoadNpcLevelupConfig(FilePathDefine_Client.C_NpcLevelupConfig, "NpcLevelupConfig");
      PlayerConfigProvider.Instance.LoadPlayerConfig(FilePathDefine_Client.C_PlayerConfig, "PlayerConfig");
      PlayerConfigProvider.Instance.LoadPlayerLevelupConfig(FilePathDefine_Client.C_PlayerLevelupConfig, "PlayerLevelupConfig");
      PlayerConfigProvider.Instance.LoadPlayerLevelupExpConfig(FilePathDefine_Client.C_PlayerLevelupExpConfig, "PlayerLevelupExpConfig");
      CriticalConfigProvider.Instance.Load(FilePathDefine_Client.C_CriticalConfig, "CriticalConfig");

      ItemConfigProvider.Instance.Load(FilePathDefine_Client.C_ItemConfig, "ItemConfig");
      EquipmentConfigProvider.Instance.LoadEquipmentConfig(FilePathDefine_Client.C_EquipmentConfig, "EquipmentConfig");
      EquipmentConfigProvider.Instance.LoadEquipmentLevelupConfig(FilePathDefine_Client.C_EquipmentLevelupConfig, "EquipmentLevelupConfig");

      MuzzlePosConfigProvider.Instance.Load(FilePathDefine_Client.C_PlayerMuzzleConfig, FilePathDefine_Client.C_NpcMuzzleConfig);

      SkillConfigProvider.Instance.CollectData(SkillConfigType.SCT_SOUND, FilePathDefine_Client.C_SoundConfig, "SoundConfig");
      SkillConfigProvider.Instance.CollectData(SkillConfigType.SCT_SKILL, FilePathDefine_Client.C_SkillSystemConfig, "SkillConfig");
      SkillConfigProvider.Instance.CollectData(SkillConfigType.SCT_IMPACT, FilePathDefine_Client.C_ImpactSystemConfig, "ImpactConfig");

      BuffConfigProvider.Instance.Load(FilePathDefine_Client.C_BuffConfig, "BuffConfig");

      WeaponConfigProvider.Instance.CollectData(FilePathDefine_Client.C_WeaponSystemConfig, "Include");
      SoundConfigProvider.Instance.Load(FilePathDefine_Client.C_GlobalSoundConfig, "C_GlobalSoundConfig");
      StrDictionaryProvider.Instance.Load(FilePathDefine_Client.C_StrDictionary, "StrDictionary");

      DynamicSceneConfigProvider.Instance.CollectData(FilePathDefine_Client.C_DynamicSceneConfig, "DynamicSceneConfig");
    }

    public CharacterInfo GetCharacterById(int id)
    {
      CharacterInfo obj = null;
      if (null != m_NpcMgr)
        obj = m_NpcMgr.GetNpcInfo(id);
      if (null != m_UserMgr && null == obj)
        obj = m_UserMgr.GetUserInfo(id);
      return obj;
    }

    public CharacterInfo GetCharacterByUnitId(string id)
    {
      return null;
    }

    public UserInfo GetPlayerSelf()
    {
      return m_PlayerSelf;
    }

    public void DestroyCharacterById(int id)
    {
      if (m_NpcMgr.Npcs.Contains(id)) {
        m_NpcMgr.RemoveNpc(id);
      }
      if (m_PlayerSelfId == id) {
        m_PlayerSelf = null;
      }
      if (m_UserMgr.Users.Contains(id)) {
        if (null != m_SpatialSystem) {
          CharacterInfo info = m_UserMgr.Users[id];
          if (null != info) {
            info.SceneContext = null;
            m_SpatialSystem.RemoveObj(info.SpaceObject);
          }
        }
        m_UserMgr.RemoveUser(id);
      }
    }

    public NpcInfo CreateNpc(int id, int unitId)
    {
      NpcInfo ret = null;
      Data_Unit mapUnit = GetCurScene().StaticData.ExtractData(DataMap_Type.DT_Unit, unitId) as Data_Unit;
      if (null != mapUnit)
        ret = m_NpcMgr.AddNpc(id, mapUnit);
      return ret;
    }

    private void SetAIEnable(int characterId, bool enable)
    {
      CharacterInfo info = WorldSystem.Instance.GetCharacterById(characterId);
      if (null != info) {
        info.SetAIEnable(enable);
        info.GetMovementStateInfo().IsMoving = false;
      }
    }

    // 脚本里控制怪物的真实死亡时间。
    private void OnNpcDead(int characterId) {
      CharacterInfo info = WorldSystem.Instance.GetCharacterById(characterId);
      if (info.Hp > 0) {
        LogSystem.Error("Npc {0} receive dead command while hp > 0", characterId);
      } else if(info.DeadTime > 0){
        LogSystem.Error("Npc {0} has received dead command already", characterId);
      } else {
        info.DeadTime = TimeUtility.GetServerMilliseconds();
      }
    }

    private void OnUserRelive() {
      CharacterInfo player = GetPlayerSelf();
      if (null != player) {
        player.DeadTime = 0;
        player.SetHp(Operate_Type.OT_Absolute, player.GetActualProperty().HpMax);
      }
    }

    private void OnSetRageMax() {
      CharacterInfo player = GetPlayerSelf();
      if (null != player) {
        player.SetRage(Operate_Type.OT_Absolute, player.GetActualProperty().RageMax);
      }
    }
    private void CreateNpcByStory(int unitIdFrom, int unitIdTo)
    {
      LogSystem.Debug("start = {0}, to = {1}", unitIdFrom, unitIdTo);
      for (int i = unitIdFrom; i <= unitIdTo; i++) {
        Data_Unit mapUnit = GetCurScene().StaticData.ExtractData(DataMap_Type.DT_Unit, i) as Data_Unit;
        if (null == mapUnit) {
          LogSystem.Debug("i = {0}", i);
        }
        if (null != mapUnit) {
          NpcInfo npc = m_NpcMgr.AddNpc(mapUnit);
          if (null != npc) {
            EntityManager.Instance.CreateNpcView(npc.GetId());
          }
        }
      }
    }
    public void CreateNpcWithPos(int unitId, float x, float y, float z, float rotateAngle)
    {
      Data_Unit mapUnit = GetCurScene().StaticData.ExtractData(DataMap_Type.DT_Unit, unitId) as Data_Unit;
      if (null != mapUnit) {
        mapUnit.m_Pos = new Vector3(x, y, z);
        mapUnit.m_RotAngle = rotateAngle;
        NpcInfo npc = m_NpcMgr.AddNpc(mapUnit);
        if (null != npc) {
          EntityManager.Instance.CreateNpcView(npc.GetId());
        }
      }
    }
    public NpcInfo CreateNpcByLinkId(int id, int linkId)
    {
      Data_Unit mapUnit = new Data_Unit();
      mapUnit.m_Id = -1;
      mapUnit.m_LinkId = linkId;
      return m_NpcMgr.AddNpc(id, mapUnit);
    }

    public UserInfo CreateUser(int id, int resId)
    {
      UserInfo info = m_UserMgr.AddUser(id, resId);
      if (null != info) {
        info.SceneContext = m_SceneContext;
        if (null != m_SpatialSystem) {
          m_SpatialSystem.AddObj(info.SpaceObject);
        }
      }
      return info;
    }

    public UserInfo CreatePlayerSelf(int id, int resId)
    {
      m_PlayerSelf = CreateUser(id, resId);
      m_PlayerSelfId = id;
      return m_PlayerSelf;
    }

    public void TickSystemByCharacters()
    {
      for (LinkedListNode<NpcInfo> linkNode = m_NpcMgr.Npcs.FirstValue; null != linkNode; linkNode = linkNode.Next) {
        CharacterInfo character = linkNode.Value;
        ImpactSystem.Instance.Tick(character);
      }

      for (LinkedListNode<UserInfo> linkNode = m_UserMgr.Users.FirstValue; null != linkNode; linkNode = linkNode.Next) {
        CharacterInfo character = linkNode.Value;
        ImpactSystem.Instance.Tick(character);
      }
    }

    public void UpdateObserverCamera(float x, float y)
    {
      if (!m_IsFollowObserver) {
        GetCurScene().UpdateObserverCamera(x, y);
      }
    }

    public bool GetCharacterPosition(int entityId, out ScriptRuntime.Vector3 pos)
    {
      pos = ScriptRuntime.Vector3.Zero;

      if (m_UserMgr.Users.Contains(entityId)) {
        if (null != m_SpatialSystem) {
          CharacterInfo info = m_UserMgr.Users[entityId];
          if (null != info) {
            pos = info.SpaceObject.GetPosition();
            return true;
          }
        }
      }

      if (m_NpcMgr.Npcs.Contains(entityId)) {
        if (null != m_SpatialSystem) {
          CharacterInfo info = m_NpcMgr.Npcs[entityId];
          if (null != info) {
            pos = info.SpaceObject.GetPosition();
            return true;
          }
        }
      }

      LogSystem.Debug("GetCharacterPosition return false, id:{0}", entityId);
      return false;
    }

    public void HighlightPrompt(int id, params object[] args)
    {
      var str = StrDictionaryProvider.Instance.Format(id, args);
      GfxSystem.PublishGfxEvent("ge_cdbar", "ui", str);
    }
    public int PlayerSelfId
    {
      get { return m_PlayerSelfId; }
      set { m_PlayerSelfId = value; }
    }
    public int HeroId
    {
      get { return NetworkSystem.Instance.HeroId; }
    }
    public int CampId
    {
      get { return NetworkSystem.Instance.CampId; }
    }
    public DashFireSpatial.ICellMapView CellMapView
    {
      get { return m_CellMapView; }
    }

    public SceneContextInfo SceneContext
    {
      get { return m_SceneContext; }
    }
    public ISpatialSystem SpatialSystem
    {
      get
      {
        return m_SpatialSystem;
      }
    }
    public SceneLogicInfoManager SceneLogicInfoManager
    {
      get
      {
        return m_SceneLogicInfoMgr;
      }
    }
    public NpcManager NpcManager
    {
      get
      {
        return m_NpcMgr;
      }
    }
    public UserManager UserManager
    {
      get
      {
        return m_UserMgr;
      }
    }

    public long SceneStartTime
    {
      get { return m_SceneStartTime; }
      set
      {
        m_SceneStartTime = value;
        m_SceneContext.StartTime = m_SceneStartTime;
      }
    }
    public int LastMoveDirAdjust
    {
      get { return m_LastMoveDirAdjust; }
      set { m_LastMoveDirAdjust = value; }
    }
    public float InputMoveDir
    {
      get { return m_InputMoveDir; }
      set
      {
        m_InputMoveDir = value;
        m_LastMoveDirAdjust = 0;
      }
    }
    public int MoveDirAdjustCount
    {
      get { return m_MoveDirAdjustCount; }
      set { m_MoveDirAdjustCount = value; }
    }
    public bool IsObserver
    {
      get { return m_IsObserver; }
    }
    public bool IsFollowObserver
    {
      get { return m_IsFollowObserver; }
      set { m_IsFollowObserver = value; }
    }
    public int FollowTargetId
    {
      get { return m_FollowTargetId; }
      set { m_FollowTargetId = value; }
    }

    /**
     * @brief 构造函数
     *
     * @return 
     */
    private WorldSystem()
    {
      m_SceneContext.OnHighlightPrompt = (int userId, int dict, object[] args) => {
        WorldSystem.Instance.HighlightPrompt(dict, args);
      };

      m_SceneContext.SpatialSystem = m_SpatialSystem;
      m_SceneContext.SceneLogicInfoManager = m_SceneLogicInfoMgr;
      m_SceneContext.NpcManager = m_NpcMgr;
      m_SceneContext.UserManager = m_UserMgr;
      m_SceneContext.BlackBoard = m_BlackBoard;

      m_NpcMgr.SetSceneContext(m_SceneContext);
      m_SceneLogicInfoMgr.SetSceneContext(m_SceneContext);
      
      m_AiSystem.SetNpcManager(m_NpcMgr);
      m_AiSystem.SetUserManager(m_UserMgr);

    }

    private NpcManager m_NpcMgr = new NpcManager(256);
    private UserManager m_UserMgr = new UserManager(16);
    private SceneLogicInfoManager m_SceneLogicInfoMgr = new SceneLogicInfoManager(256);

    private AiSystem m_AiSystem = new AiSystem();
    private SpatialSystem m_SpatialSystem = new SpatialSystem();
    private BlackBoard m_BlackBoard = new BlackBoard();

    private UserInfo m_PlayerSelf = null;
    private int m_PlayerSelfId = -1;

    private SceneContextInfo m_SceneContext = new SceneContextInfo();

    private ICellMapView m_CellMapView = null;

    private const long c_AutoPickInterval = 1000;
    private long m_SceneStartTime = 0;

    private int m_LastMoveDirAdjust = 0;
    private float m_InputMoveDir = 0;
    private int m_MoveDirAdjustCount = 0;

    private bool m_IsObserver = false;
    private bool m_IsFollowObserver = false;
    private int m_FollowTargetId = 0;

    private WorldSystemProfiler m_Profiler = new WorldSystemProfiler();
    private SceneResource m_CurScene;

    private bool m_IsDebugObstacleCreated = false;
  }
}
