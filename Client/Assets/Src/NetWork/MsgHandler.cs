using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Lidgren.Network;
using DashFire;
using DashFireMessage;
using DashFire.Network;
using ScriptRuntime;
using DashFire.Debug;

public class MsgPongHandler
{
  public static void Execute(object msg, NetConnection conn)
  {
    Msg_Pong pong_msg = msg as Msg_Pong;
    if (pong_msg == null) {
      return;
    }
    long time = TimeUtility.GetLocalMilliseconds();
    NetworkSystem.Instance.OnPong(time, pong_msg.send_ping_time, pong_msg.send_pong_time);
  }
}

public class MsgShakeHandsRetHandler
{
  public static void Execute(object msg, NetConnection conn)
  {
    Msg_RC_ShakeHands_Ret ret_msg = msg as Msg_RC_ShakeHands_Ret;
    if (msg == null) {
      return;
    }
    if (ret_msg.auth_result == Msg_RC_ShakeHands_Ret.RetType.SUCCESS) {
      LogSystem.Debug("auth ok!!!!");
    } else {
      LogSystem.Debug("auth failed!!!!");
    }
  }
}

public class Msg_CRC_Create_Handler
{
  public static void Execute(object msg, NetConnection conn)
  {
    Msg_CRC_Create enter = msg as Msg_CRC_Create;
    if (null == enter) {
      return;
    }

    CharacterInfo cb = WorldSystem.Instance.GetCharacterById((int)enter.role_id);
    if (cb != null) {
      return;
    }
    LogSystem.Debug("Msg_CRC_Create, PlayerId={0}", enter.role_id);

    if (enter.is_player_self) {

      UserInfo user = WorldSystem.Instance.CreatePlayerSelf(enter.role_id, enter.hero_id);

      user.SetCampId(enter.camp_id);
      user.GetMovementStateInfo().SetPosition(enter.position.x, enter.position.y, enter.position.z);
      user.GetMovementStateInfo().SetFaceDir(enter.face_dirction);

      EntityManager.Instance.CreateUserView(enter.role_id);
      for (int index = 0; index < enter.Weapons.Count; ++index) {
        WeaponStateInfo.AddSubWeapon(user, enter.Weapons[index]);
      }
      for (int index = 0; index < enter.skill_levels.Count; index++) {
        int skillId = 0;//SkillSystem.Instance.GetSkillIdByIndexAndLevel(user, index, enter.skill_levels[index]);
        SkillInfo skillInfo = new SkillInfo(skillId);
        skillInfo.SkillLevel = enter.skill_levels[index];
        user.GetSkillStateInfo().AddSkill(index, skillInfo);
      }
      for (int index = 0; index < enter.shop_equipments_id.Count; ++index) {
        user.SetShopEquipmentsId(index, enter.shop_equipments_id[index]);
      }
      GfxSystem.PublishGfxEvent("ge_create_all_board", "ui");
      if (enter.scene_start_time > 0) {
        WorldSystem.Instance.SceneStartTime = enter.scene_start_time;
        GfxSystem.PublishGfxEvent("ge_updatepvptime", "ui");
      }

      user.SetNickName(enter.nickname);

      UserView view = EntityManager.Instance.GetUserViewById(enter.role_id);
      if (view != null) {
        //GfxSystemExt.GfxSoundMgr.Instance.AttachListener(view.Actor);
        //view.HideCylinder();

        GfxSystem.SendMessage("GfxGameRoot", "CameraFollow", view.Actor);
      }

      if (WorldSystem.Instance.IsPvpScene()) {
        int campId = WorldSystem.Instance.CampId;
        if (campId == (int)CampIdEnum.Blue) {
          GfxSystem.SendMessage("GfxGameRoot", "CameraFixedYaw", Math.PI / 2);
        } else if (campId == (int)CampIdEnum.Red) {
          GfxSystem.SendMessage("GfxGameRoot", "CameraFixedYaw", -Math.PI / 2);
        }

        SceneResource scene = WorldSystem.Instance.GetCurScene();
        scene.NotifyUserEnter();
      } else {
        SceneResource scene = WorldSystem.Instance.GetCurScene();
        scene.NotifyUserEnter();
      }

    } else {

      UserInfo other = WorldSystem.Instance.CreateUser(enter.role_id, enter.hero_id);

      other.SetCampId(enter.camp_id);
      other.GetMovementStateInfo().SetPosition(enter.position.x, enter.position.y, enter.position.z);
      other.GetMovementStateInfo().SetFaceDir(enter.face_dirction);

      EntityManager.Instance.CreateUserView(enter.role_id);
      for (int index = 0; index < enter.Weapons.Count; ++index) {
        WeaponStateInfo.AddSubWeapon(other, enter.Weapons[index]);
      }
      for (int index = 0; index < enter.skill_levels.Count; index++) {
        int skillId = 0;//SkillSystem.Instance.GetSkillIdByIndexAndLevel(other, index, enter.skill_levels[index]);
        SkillInfo skillInfo = new SkillInfo(skillId);
        skillInfo.SkillLevel = enter.skill_levels[index];
        other.GetSkillStateInfo().AddSkill(index, skillInfo);
      }
      other.SetNickName(enter.nickname);

      if (WorldSystem.Instance.IsObserver) {
        if (enter.scene_start_time > 0) {
          WorldSystem.Instance.SceneStartTime = enter.scene_start_time;
          GfxSystem.PublishGfxEvent("ge_updatepvptime", "ui");
        }

        LogSystem.Debug("User {0}({1}) create", enter.nickname, enter.role_id);
      }

      UserView view = EntityManager.Instance.GetUserViewById(enter.role_id);
      if (view != null) {
        //view.HideCylinder();
      }
    }
    GfxSystem.PublishGfxEvent("ge_createuser", "ui", enter.role_id);
  }
}

public class Msg_RC_Enter_Handler
{
  public static void Execute(object msg, NetConnection conn)
  {
    Msg_RC_Enter enter = msg as Msg_RC_Enter;
    if (null == enter) {
      return;
    }

    CharacterInfo cb = WorldSystem.Instance.GetCharacterById((int)enter.role_id);
    if (cb == null) {
      return;
    }
    LogSystem.Debug("Msg_RC_Enter, PlayerId={0}", enter.role_id);

    UserInfo other = cb.CastUserInfo();
    if (null != other) {
      other.SetCampId(enter.camp_id);
      other.GetMovementStateInfo().SetPosition2D(enter.position.x, enter.position.z);
      other.GetMovementStateInfo().SetFaceDir(enter.face_dir);
      other.GetMovementStateInfo().SetMoveDir(enter.move_dir);
      other.GetMovementStateInfo().IsMoving = enter.is_moving;
      if (enter.is_moving) {
        other.GetMovementStateInfo().StartMove();
      }

      UserView view = EntityManager.Instance.GetUserViewById(enter.role_id);
      if (null != view && !view.Visible) {
        view.Visible = true;
        //view.HideCylinder();
        LogSystem.Debug("Show Player {0} link id {1} position {2} for Msg_RC_Enter", other.GetId(), other.GetLinkId(), other.GetMovementStateInfo().GetPosition3D().ToString());
      }
    }
  }
}

public class Msg_RC_Disappear_Handler
{
  public static void Execute(object msg, NetConnection conn)
  {
    Msg_RC_Disappear disappear = msg as Msg_RC_Disappear;
    if (disappear == null) {
      return;
    }
    CharacterInfo info = WorldSystem.Instance.GetCharacterById(disappear.role_id);
    if (null == info) {
      return;
    }
    LogSystem.Debug("Msg_RC_Disappear, PlayerId={0}", disappear.role_id);
    UserInfo player = info.CastUserInfo();
    if (null != player) {
      LogSystem.Debug("Hide Player {0} link id {1} position {2} for Msg_RC_Disappear", player.GetId(), player.GetLinkId(), player.GetMovementStateInfo().GetPosition3D().ToString());

      if (!player.GetSkillStateInfo().IsSkillActivated()) {
        player.GetMovementStateInfo().SetPosition2D(0, 0);
      }
      player.GetMovementStateInfo().IsMoving = false;
      UserView view = EntityManager.Instance.GetUserViewById(disappear.role_id);
      if (null != view) {
        view.Visible = false;
        GfxSystem.PublishGfxEvent("ge_move_camera_to_next_friend", "script", disappear.role_id, 1);
      }
    }
  }
}

public class Msg_RC_Dead_Handler
{
  public static void Execute(object msg, NetConnection conn)
  {
    Msg_RC_Dead revive = msg as Msg_RC_Dead;
    if (revive == null) {
      return;
    }

    CharacterInfo cb = WorldSystem.Instance.GetCharacterById((int)revive.role_id);
    if (null == cb) {
      return;
    }

    UserInfo player = cb.CastUserInfo();
    if (null != player) {
      if (!player.IsDead()) {
        player.SetHp(Operate_Type.OT_Absolute, 0);
        player.GetMovementStateInfo().IsMoving = false;
      }
      player.SetStateFlag(Operate_Type.OT_AddBit, CharacterState_Type.CST_BODY);

      if (WorldSystem.Instance.PlayerSelfId == revive.role_id || WorldSystem.Instance.IsObserver && WorldSystem.Instance.IsFollowObserver && WorldSystem.Instance.FollowTargetId == revive.role_id) {
        Vector3 pos = player.GetMovementStateInfo().GetPosition3D();
        GfxSystem.SendMessage("GfxGameRoot", "CameraLookat", new float[] { pos.X, pos.Y, pos.Z });
        GfxSystem.PublishGfxEvent("ge_screenmask", "ui", false);
        GfxSystem.PublishGfxEvent("ge_setrelivetime", "ui");
        LogSystem.Debug("[Camera State] LookAtPos:{0}", pos.ToString());
      }
      GfxSystem.PublishGfxEvent("ge_pvpteammemberdeath", "ui", revive.role_id);
    }
  }
}

public class Msg_RC_Revive_Handler
{
  public static void Execute(object msg, NetConnection conn)
  {
    Msg_RC_Revive revive = msg as Msg_RC_Revive;
    if (revive == null) {
      return;
    }

    CharacterInfo cb = WorldSystem.Instance.GetCharacterById((int)revive.role_id);
    if (cb == null) {
      return;
    }
    UserInfo user = cb.CastUserInfo();
    if (null != user) {
      user.SetCampId(revive.camp_id);
      user.SetStateFlag(Operate_Type.OT_RemoveBit, CharacterState_Type.CST_BODY);
      user.GetMovementStateInfo().IsMoving = false;
      user.SetEnergyCore(Operate_Type.OT_Absolute, user.GetActualProperty().EnergyCoreMax);

      user.GetMovementStateInfo().SetPosition(revive.position.x, 0, revive.position.z);
      user.GetMovementStateInfo().SetFaceDir(revive.face_dirction);

      UserView view = EntityManager.Instance.GetUserViewById(revive.role_id);
      if (null != view && !view.Visible) {
        view.Visible = true;
        //view.HideCylinder();
        LogSystem.Debug("Show Player {0} link id {1} position {2} for Msg_RC_Revive", user.GetId(), user.GetLinkId(), user.GetMovementStateInfo().GetPosition3D().ToString());
      }

      GfxSystem.PublishGfxEvent("ge_pvpteammemberrelive", "ui", revive.role_id);
      int playerselfid = WorldSystem.Instance.PlayerSelfId;
      if (playerselfid == revive.role_id || WorldSystem.Instance.IsObserver && WorldSystem.Instance.IsFollowObserver && WorldSystem.Instance.FollowTargetId == revive.role_id) {
        if (null != view) {
          GfxSystem.SendMessage("GfxGameRoot", "CameraFollow", view.Actor);
        }
        if (WorldSystem.Instance.IsPvpScene() && !WorldSystem.Instance.IsObserver) {
          int campId = WorldSystem.Instance.CampId;
          if (campId == (int)CampIdEnum.Blue) {
            GfxSystem.SendMessage("GfxGameRoot", "CameraFixedYaw", Math.PI / 2);
          } else if (campId == (int)CampIdEnum.Red) {
            GfxSystem.SendMessage("GfxGameRoot", "CameraFixedYaw", -Math.PI / 2);
          }
        }
        GfxSystem.PublishGfxEvent("ge_screenmask", "ui", true);
        LogSystem.Debug("[Camera State] FollowCharacter:{0} {1}", revive.role_id, user.GetMovementStateInfo().GetPosition3D().ToString());
      }
    }

  }
}

public class Msg_CRC_Exit_Handler
{
  public static void Execute(object msg, NetConnection conn)
  {
    Msg_CRC_Exit targetmsg = msg as Msg_CRC_Exit;
    if (null == targetmsg) {
      return;
    }

    CharacterInfo cb = WorldSystem.Instance.GetCharacterById(targetmsg.role_id);
    if (cb == null) {
      return;
    }
    UserInfo other = cb.CastUserInfo();
    if (other.GetId() == WorldSystem.Instance.PlayerSelfId) {
      UserView view = EntityManager.Instance.GetUserViewById(other.GetId());
      if (view != null) {
        //GfxSystemExt.GfxSoundMgr.Instance.DeattachListener(view.Actor);
      }
    }

    EntityManager.Instance.DestroyUserView(other.GetId());
    WorldSystem.Instance.DestroyCharacterById(other.GetId());
  }
}

public class Msg_CRC_Move_Handler
{
  public static void OnMoveStart(object msg, NetConnection conn)
  {
    Msg_CRC_MoveStart move_msg = msg as Msg_CRC_MoveStart;
    if (null == move_msg)
      return;

    CharacterInfo cb = WorldSystem.Instance.GetCharacterById((int)move_msg.role_id);
    if (cb == null)
      return;
    LogSystem.Debug("Msg_CRC_MoveStart, EntityId={0}, MoveDir={1}", move_msg.role_id, move_msg.dir);
    UserInfo other = cb.CastUserInfo();
    if (other != null) {
      MovementStateInfo msi = other.GetMovementStateInfo();
      ControlSystemOperation.AdjustUserPosition(cb.GetId(), move_msg.position.x, move_msg.position.y, move_msg.position.z, TimeUtility.AverageRoundtripTime, move_msg.dir);

      msi.SetMoveDir(move_msg.dir);
      msi.StartMove();
      msi.TargetPosition = Vector3.Zero;
      other.IsMoving = true;      

      /*UserView view = EntityManager.Instance.GetUserViewById(move_msg.role_id);
      if (null != view && !view.Visible) {
        view.Visible = true;
        LogSystem.Debug("Show Player {0} link id {1} position {2} for Msg_CRC_MoveStart", other.GetId(), other.GetLinkId(), other.GetMovementStateInfo().GetPosition3D().ToString());
      }*/
    }
  }

  public static void OnMoveStop(object msg, NetConnection conn)
  {
    Msg_CRC_MoveStop move_msg = msg as Msg_CRC_MoveStop;
    if (null == move_msg) {
      return;
    }

    CharacterInfo cb = WorldSystem.Instance.GetCharacterById((int)move_msg.role_id);
    if (cb == null)
      return;
    LogSystem.Debug("Msg_CRC_MoveStop, EntityId={0}", move_msg.role_id);
    UserInfo other = cb.CastUserInfo();
    if (other != null) {
      MovementStateInfo msi = other.GetMovementStateInfo();
      msi.StopMove();
      other.IsMoving = false;

      /*UserView view = EntityManager.Instance.GetUserViewById(move_msg.role_id);
      if (null != view && !view.Visible) {
        view.Visible = true;
        LogSystem.Debug("Show Player {0} link id {1} position {2} for Msg_CRC_MoveStop", other.GetId(), other.GetLinkId(), other.GetMovementStateInfo().GetPosition3D().ToString());
      }*/
    }
  }
}

public class Msg_RC_MoveMeetObstacle_Handler
{
  public static void Execute(object msg, NetConnection conn)
  {
    Msg_RC_MoveMeetObstacle pos_msg = msg as Msg_RC_MoveMeetObstacle;
    if (null == pos_msg) {
      return;
    }
    CharacterInfo cb = WorldSystem.Instance.GetCharacterById((int)pos_msg.role_id);
    if (cb == null)
      return;
    UserInfo user = cb.CastUserInfo();
    if (user != null) {
      if (user.IsHaveStateFlag(CharacterState_Type.CST_Sleep)) {
        return;
      }
      user.GetMovementStateInfo().IsMoveMeetObstacle = true;
      user.GetMovementStateInfo().SetPosition2D(pos_msg.cur_pos_x, pos_msg.cur_pos_z);
      if (WorldSystem.Instance.PlayerSelfId == pos_msg.role_id) {
        ControlSystemOperation.AdjustUserMove();
      }
    }
  }
}

public class Msg_CRC_Face_Handler
{
  public static void Execute(object msg, NetConnection conn)
  {
    Msg_CRC_Face face_msg = msg as Msg_CRC_Face;
    if (null == face_msg) {
      return;
    }
    CharacterInfo cb = WorldSystem.Instance.GetCharacterById((int)face_msg.role_id);
    if (cb == null)
      return;
    UserInfo other = cb.CastUserInfo();
    if (other != null) {
      if (other.IsHaveStateFlag(CharacterState_Type.CST_Sleep)) {
        return;
      }
      other.GetMovementStateInfo().SetFaceDir(face_msg.face_direction);
    }
  }
}

public class Msg_CRC_Skill_Handler
{
  public static void Execute(object msg, NetConnection conn)
  {
    Msg_CRC_Skill skill_msg = msg as Msg_CRC_Skill;
    if (null == skill_msg) {
      return;
    }
    CharacterInfo cb = WorldSystem.Instance.GetCharacterById((int)skill_msg.role_id);
    if (cb != null) {
      cb = cb.GetRealControlledObject();
      LogSystem.Debug("Msg_CRC_Skill, EntityId={0}, SkillId={1}", skill_msg.role_id, skill_msg.skill_id);
      ScriptRuntime.Vector3 stand_pos = new ScriptRuntime.Vector3(skill_msg.stand_pos.x, skill_msg.stand_pos.y, skill_msg.stand_pos.z);
      if (!cb.IsHaveStateFlag(CharacterState_Type.CST_Hidden))   // 如果角色处于隐身状态，不同步位置信息
      {
        cb.GetMovementStateInfo().SetPosition2D(stand_pos.X, stand_pos.Z);
        cb.GetMovementStateInfo().SetFaceDir(skill_msg.face_direction);
      }

      ScriptRuntime.Vector3 pos = new ScriptRuntime.Vector3(skill_msg.target_pos.x, skill_msg.target_pos.y, skill_msg.target_pos.z);
      CharacterInfo target = WorldSystem.Instance.GetCharacterById(skill_msg.target_id);
      //SkillSystem.Instance.StartSkill(cb, skill_msg.skill_id, target, pos, skill_msg.target_angle, skill_msg.item_id);
      cb.LastAnimTime = TimeUtility.GetServerMilliseconds();
      if (skill_msg.role_id == WorldSystem.Instance.PlayerSelfId) {
        Data_PlayerConfig playerConfig = PlayerConfigProvider.Instance.GetPlayerConfigById(cb.GetLinkId());
        if (null == playerConfig) return;

        int skillIndex = playerConfig.GetSkillIndex(skill_msg.skill_id);
        if (-1 != skillIndex && skillIndex < 3) {
          UserInfo user = WorldSystem.Instance.GetPlayerSelf();
          SkillInfo skillInfo = user.GetSkillStateInfo().GetSkillInfoById(skill_msg.skill_id);
          if (null == skillInfo) return;
          long curTime = TimeUtility.GetServerMilliseconds();
          GfxSystem.PublishGfxEvent("ge_castskill", "ui", skillIndex, skillInfo.ConfigData.CoolDownTime, cb.GetId());
        }
      }
    }
  }
}

public class Msg_RC_UserMove_Handler
{
  public static void Execute(object msg, NetConnection conn)
  {
    Msg_RC_UserMove targetmsg = msg as Msg_RC_UserMove;
    if (null == targetmsg) {
      return;
    }
    CharacterInfo charObj = WorldSystem.Instance.GetCharacterById(targetmsg.role_id);
    if (null == charObj) {
      return;
    }
    UserInfo user = charObj.CastUserInfo();
    if (user == null) {
      return;
    }
    if (targetmsg.is_moving) {
      LogSystem.Debug("UserMove, user:{0} pos:{1} move:{2}->{3}", user.GetId(), user.GetMovementStateInfo().GetPosition3D().ToString(), new Vector3(targetmsg.cur_pos_x, 0, targetmsg.cur_pos_z).ToString(), new Vector3(targetmsg.target_pos_x, targetmsg.target_pos_y, targetmsg.target_pos_z).ToString());
      user.GetMovementStateInfo().SetPosition2D(targetmsg.cur_pos_x, targetmsg.cur_pos_z);
      user.GetMovementStateInfo().SetMoveDir(targetmsg.move_direction);
      //user.GetMovementStateInfo().SetFaceDir(targetmsg.face_direction);
      ControlSystemOperation.AdjustCharacterFace(targetmsg.role_id, targetmsg.face_direction);
      user.GetMovementStateInfo().IsMoving = targetmsg.is_moving;
      user.VelocityCoefficient = targetmsg.velocity_coefficient;

      UserAiStateInfo data = user.GetAiStateInfo();
      user.GetMovementStateInfo().TargetPosition = new Vector3(targetmsg.target_pos_x, user.GetMovementStateInfo().GetPosition3D().Y, targetmsg.target_pos_z);
      if (data.AiLogic != (int)AiStateLogicId.PvpUser_General) {
        data.AiLogic = (int)AiStateLogicId.PvpUser_General;
      }
      data.ChangeToState((int)AiStateId.Move);
    } else {
      LogSystem.Debug("UserMove stop, user:{0} pos:{1}", user.GetId(), user.GetMovementStateInfo().GetPosition3D().ToString());

      user.GetMovementStateInfo().SetPosition2D(targetmsg.cur_pos_x, targetmsg.cur_pos_z);
      user.GetMovementStateInfo().IsMoving = false;
      UserAiStateInfo data = user.GetAiStateInfo();
      data.ChangeToState((int)AiStateId.Wait);
    }

    /*NpcView view = EntityManager.Instance.GetNpcViewById(targetmsg.npc_id);
    if (null != view && !view.Visible) {
      view.Visible = true;
    }*/
  }
}

public class Msg_RC_UserFace_Handler
{
  public static void Execute(object msg, NetConnection conn)
  {
    Msg_RC_UserFace targetmsg = msg as Msg_RC_UserFace;
    if (null == targetmsg) {
      return;
    }
    CharacterInfo charObj = WorldSystem.Instance.GetCharacterById(targetmsg.role_id);
    if (null == charObj) {
      return;
    }
    UserInfo user = charObj.CastUserInfo();
    if (user == null) {
      return;
    }
    if (user.IsHaveStateFlag(CharacterState_Type.CST_Sleep)) {
      return;
    }
    //user.GetMovementStateInfo().SetFaceDir(targetmsg.face_direction);
    ControlSystemOperation.AdjustCharacterFace(targetmsg.role_id, targetmsg.face_direction);
  }
}

public class Msg_RC_CreateNpc_Handler
{
  public static void Execute(object msg, NetConnection conn)
  {
    Msg_RC_CreateNpc targetmsg = msg as Msg_RC_CreateNpc;
    if (null == targetmsg) {
      return;
    }

    CharacterInfo cb = WorldSystem.Instance.GetCharacterById(targetmsg.npc_id);
    if (cb != null) {
      LogSystem.Debug("NpcCreate obj already exist:" + targetmsg.npc_id + " unit:" + targetmsg.unit_id);
      return;
    }
    LogSystem.Debug("NpcCreate:" + targetmsg.npc_id + " unit:" + targetmsg.unit_id);

    NpcInfo npc = null;
    if (-1 == targetmsg.unit_id) {
      npc = WorldSystem.Instance.CreateNpcByLinkId(targetmsg.npc_id, targetmsg.link_id);
    } else {
      npc = WorldSystem.Instance.CreateNpc(targetmsg.npc_id, targetmsg.unit_id);
    }
    if (null != npc) {
      ScriptRuntime.Vector3 pos = new ScriptRuntime.Vector3();
      pos.X = targetmsg.cur_pos.x;
      pos.Y = targetmsg.cur_pos.y;
      pos.Z = targetmsg.cur_pos.z;

      npc.GetMovementStateInfo().SetPosition2D(pos.X, pos.Z);
      npc.GetMovementStateInfo().SetFaceDir(targetmsg.face_direction);
      npc.GetMovementStateInfo().IsMoving = false;
      if (targetmsg.camp_id>0) {
        npc.SetCampId(targetmsg.camp_id);
      }
      if (targetmsg.owner_id>0) {
        npc.OwnerId = targetmsg.owner_id;
      }

      EntityManager.Instance.CreateNpcView(targetmsg.npc_id);
    }
  }
}

public class Msg_RC_DestroyNpc_Handler
{
  public static void Execute(object msg, NetConnection conn)
  {
    Msg_RC_DestroyNpc destroyMsg = msg as Msg_RC_DestroyNpc;
    if (destroyMsg == null) {
      return;
    }
    CharacterInfo info = WorldSystem.Instance.GetCharacterById(destroyMsg.npc_id);
    if (null == info) {
      LogSystem.Debug("NpcDestroy can't find obj:" + destroyMsg.npc_id);
      return;
    }
    LogSystem.Debug("NpcDestroy:" + destroyMsg.npc_id);

    NpcInfo npc = info.CastNpcInfo();
    if (null != npc) {
      EntityManager.Instance.DestroyNpcView(npc.GetId());
      WorldSystem.Instance.DestroyCharacterById(npc.GetId());
      return;
    }
  }
}

public class Msg_RC_NpcEnter_Handler
{
  public static void Execute(object msg, NetConnection conn)
  {
    Msg_RC_NpcEnter targetmsg = msg as Msg_RC_NpcEnter;
    if (null == targetmsg) {
      return;
    }

    CharacterInfo cb = WorldSystem.Instance.GetCharacterById(targetmsg.npc_id);
    if (cb == null) {
      LogSystem.Debug("NpcEnter obj don't exist:" + targetmsg.npc_id);
      return;
    }
    LogSystem.Debug("NpcEnter:" + targetmsg.npc_id + " unit:" + cb.GetUnitId() + " FaceDir:" + targetmsg.face_direction);

    NpcInfo npc = cb.CastNpcInfo();
    if (null != npc) {
      ScriptRuntime.Vector3 pos = new ScriptRuntime.Vector3();
      pos.X = targetmsg.cur_pos_x;
      pos.Y = targetmsg.cur_pos_y;
      pos.Z = targetmsg.cur_pos_z;

      npc.GetMovementStateInfo().SetPosition2D(pos.X, pos.Z);
      npc.GetMovementStateInfo().SetFaceDir(targetmsg.face_direction);
      npc.GetMovementStateInfo().IsMoving = false;
      npc.GetAiStateInfo().Reset();

      NpcView view = EntityManager.Instance.GetNpcViewById(targetmsg.npc_id);
      if (null != view && !view.Visible) {
        view.Visible = true;
      }
    }
  }
}

public class Msg_RC_NpcMove_Handler
{
  public static void Execute(object msg, NetConnection conn)
  {
    Msg_RC_NpcMove targetmsg = msg as Msg_RC_NpcMove;
    if (null == targetmsg) {
      return;
    }
    CharacterInfo charObj = WorldSystem.Instance.GetCharacterById(targetmsg.npc_id);
    if (null == charObj) {
      return;
    }
    NpcInfo npc = charObj.CastNpcInfo();
    if (npc == null) {
      return;
    }
    if (targetmsg.is_moving) {
      LogSystem.Debug("NpcMove, npc:{0} pos:{1} move:{2}->{3}", npc.GetId(), npc.GetMovementStateInfo().GetPosition3D().ToString(), new Vector3(targetmsg.cur_pos_x, 0, targetmsg.cur_pos_z).ToString(), new Vector3(targetmsg.target_pos_x, targetmsg.target_pos_y, targetmsg.target_pos_z).ToString());
      npc.GetMovementStateInfo().SetPosition2D(targetmsg.cur_pos_x, targetmsg.cur_pos_z);
      npc.GetMovementStateInfo().SetMoveDir(targetmsg.move_direction);
      //npc.GetMovementStateInfo().SetFaceDir(targetmsg.face_direction);
      ControlSystemOperation.AdjustCharacterFace(targetmsg.npc_id, targetmsg.face_direction);
      npc.GetMovementStateInfo().IsMoving = targetmsg.is_moving;
      npc.VelocityCoefficient = targetmsg.velocity_coefficient;

      NpcAiStateInfo data = npc.GetAiStateInfo();
      data.HomePos = npc.GetMovementStateInfo().GetPosition3D();
      npc.GetMovementStateInfo().TargetPosition = new Vector3(targetmsg.target_pos_x, npc.GetMovementStateInfo().GetPosition3D().Y, targetmsg.target_pos_z);
      data.ChangeToState((int)AiStateId.Move);
    } else {
      LogSystem.Debug("NpcMove stop, npc:{0} pos:{1}", npc.GetId(), npc.GetMovementStateInfo().GetPosition3D().ToString());

      npc.GetMovementStateInfo().SetPosition2D(targetmsg.cur_pos_x, targetmsg.cur_pos_z);
      npc.GetMovementStateInfo().IsMoving = false;
      NpcAiStateInfo data = npc.GetAiStateInfo();
      data.ChangeToState((int)AiStateId.Wait);
    }

    /*NpcView view = EntityManager.Instance.GetNpcViewById(targetmsg.npc_id);
    if (null != view && !view.Visible) {
      view.Visible = true;
    }*/
  }
}

public class Msg_RC_NpcFace_Handler
{
  public static void Execute(object msg, NetConnection conn)
  {
    Msg_RC_NpcFace face_msg = msg as Msg_RC_NpcFace;
    if (null == face_msg) {
      return;
    }
    CharacterInfo cb = WorldSystem.Instance.GetCharacterById((int)face_msg.npc_id);
    if (cb == null)
      return;
    NpcInfo other = cb.CastNpcInfo();
    if (other != null) {
      if (other.IsHaveStateFlag(CharacterState_Type.CST_Sleep)) {
        return;
      }
      //other.GetMovementStateInfo().SetFaceDir(face_msg.face_direction);
      ControlSystemOperation.AdjustCharacterFace(face_msg.npc_id, face_msg.face_direction);
    }

    LogSystem.Debug("NpcFace, npc:{0} face:{1}", other.GetId(), face_msg.face_direction);
  }
}

public class Msg_RC_NpcTarget_Handler
{
  public static void Execute(object msg, NetConnection conn)
  {
    Msg_RC_NpcTarget targetmsg = msg as Msg_RC_NpcTarget;
    if (null == targetmsg) {
      return;
    }
    CharacterInfo charObj = WorldSystem.Instance.GetCharacterById(targetmsg.npc_id);
    if (null == charObj) {
      return;
    }
    NpcInfo npc = charObj.CastNpcInfo();
    if (npc == null) {
      return;
    }
    NpcAiStateInfo data = npc.GetAiStateInfo();
    data.Target = targetmsg.target_id;
    data.Time = 0;

    LogSystem.Debug("NpcTarget, npc:{0} target:{1}", npc.GetId(), targetmsg.target_id);
  }
}

public class Msg_RC_NpcSkill_Handler
{
  public static void Execute(object msg, NetConnection conn)
  {
    Msg_RC_NpcSkill targetmsg = msg as Msg_RC_NpcSkill;
    if (null == targetmsg) {
      return;
    }
    CharacterInfo charObj = WorldSystem.Instance.GetCharacterById(targetmsg.npc_id);
    if (null == charObj) {
      return;
    }
    NpcInfo npc = charObj.CastNpcInfo();
    if (npc == null) {
      return;
    }
    CharacterInfo target = WorldSystem.Instance.GetCharacterById(targetmsg.target_id);
    if (target == null)
      return;

    npc.GetMovementStateInfo().SetFaceDir(targetmsg.face_direction);
    Vector3 pos = new Vector3(targetmsg.target_pos.x, targetmsg.target_pos.y, targetmsg.target_pos.z);
    Vector3 stand = new Vector3(targetmsg.stand_pos.x, targetmsg.stand_pos.y, targetmsg.stand_pos.z);
    npc.GetMovementStateInfo().SetPosition2D(stand.X, stand.Z);
    LogSystem.Debug("Receive Msg_RC_NpcSkill, EntityId={0}, SkillId={1}, TargetAngle={2}",
      npc.GetId(), targetmsg.skill_id, targetmsg.target_angle);
    //SkillSystem.Instance.StartSkill(npc, targetmsg.skill_id, target, pos, targetmsg.target_angle);
  }
}

public class Msg_RC_NpcDead_Handler
{
  public static void Execute(object msg, NetConnection conn)
  {
    Msg_RC_NpcDead targetmsg = msg as Msg_RC_NpcDead;
    if (null == targetmsg) {
      return;
    }

    CharacterInfo cb = WorldSystem.Instance.GetCharacterById(targetmsg.npc_id);
    if (null == cb) {
      LogSystem.Debug("NpcDead can't find obj:" + targetmsg.npc_id);
      return;
    }
    LogSystem.Debug("NpcDead:" + targetmsg.npc_id);

    //死亡的是NPC
    NpcInfo npc = cb.CastNpcInfo();
    if (null != npc) {
      npc.SetHp(Operate_Type.OT_Absolute, 0);
      npc.GetMovementStateInfo().IsMoving = false;

      NpcView view = EntityManager.Instance.GetNpcViewById(npc.GetId());
      if (null != view) {
        //view.PlaySound(npc.DeathSound, view.Actor.WorldPosition);
      }
      return;
    }
  }
}

public class Msg_RC_NpcDisappear_Handler
{
  public static void Execute(object msg, NetConnection conn)
  {
    Msg_RC_NpcDisappear disappear = msg as Msg_RC_NpcDisappear;
    if (disappear == null) {
      return;
    }
    CharacterInfo info = WorldSystem.Instance.GetCharacterById(disappear.npc_id);
    if (null == info) {
      LogSystem.Debug("NpcDisappear can't find obj:" + disappear.npc_id);
      return;
    }
    LogSystem.Debug("NpcDisappear:" + disappear.npc_id);

    NpcInfo npc = info.CastNpcInfo();
    if (null != npc) {
      npc.GetMovementStateInfo().SetPosition2D(0, 0);
      npc.GetMovementStateInfo().IsMoving = false;

      NpcView view = EntityManager.Instance.GetNpcViewById(disappear.npc_id);
      if (null != view) {
        view.Visible = false;
      }
    }
  }
}

public class Msg_RC_Levelup_Handler
{
  public static void Execute(object msg, NetConnection conn)
  {
    Msg_RC_Levelup targetmsg = msg as Msg_RC_Levelup;
    if (null == targetmsg) {
      return;
    }

    CharacterInfo cb = WorldSystem.Instance.GetCharacterById(targetmsg.role_id);
    if (null == cb) {
      return;
    }
    cb.SetLevel(targetmsg.level);
  }
}

public class Msg_RC_SyncLevel_Handler
{
  public static void Execute(object msg, NetConnection conn)
  {
    Msg_RC_SyncLevel targetmsg = msg as Msg_RC_SyncLevel;
    if (null == targetmsg) {
      return;
    }
    CharacterInfo cb = WorldSystem.Instance.GetCharacterById(targetmsg.role_id);
    if (null == cb) {
      return;
    }
    cb.SetLevel(targetmsg.level);
  }
}

public class Msg_RC_SyncExpMoney_Handler
{
  public static void Execute(object msg, NetConnection conn)
  {
    Msg_RC_SyncExpMoney targetmsg = msg as Msg_RC_SyncExpMoney;
    if (null == targetmsg) {
      return;
    }

    CharacterInfo cb = WorldSystem.Instance.GetCharacterById(targetmsg.role_id);
    if (null == cb) {
      return;
    }
    cb.SetExp(targetmsg.exp);
    cb.Money = targetmsg.money;
    cb.TotalMoney = targetmsg.total_money;
    if (WorldSystem.Instance.PlayerSelfId == targetmsg.role_id) {
      GfxSystem.PublishGfxEvent("ge_updatemoney", "ui", targetmsg.money);
    }
  }
}

public class Msg_RC_SyncProperty_Handler
{
  public static void Execute(object msg, NetConnection conn)
  {
    Msg_RC_SyncProperty targetmsg = msg as Msg_RC_SyncProperty;
    if (null == targetmsg) {
      return;
    }

    CharacterInfo cb = WorldSystem.Instance.GetCharacterById(targetmsg.role_id);
    if (null == cb) {
      return;
    }
    cb.SetHp(Operate_Type.OT_Absolute, targetmsg.hp);
    cb.SetEnergy(Operate_Type.OT_Absolute, targetmsg.np);
    cb.StateFlag = targetmsg.state;
    cb.EnergyCoreNum = targetmsg.energy_num;
  }
}

public class Msg_RC_SyncImpactEffect_Handler
{
  public static void Execute(object msg, NetConnection conn)
  {
    Msg_RC_SyncImpactEffect impacteffect = msg as Msg_RC_SyncImpactEffect;
    if (impacteffect == null) {
      return;
    }
    CharacterInfo entity = WorldSystem.Instance.GetCharacterById(impacteffect.role_id);
    if (null == entity) return;
    int hpDamage = 0;
    int npDamage = 0;
    for (int i = 0; i < impacteffect.propertys.Count; ++i) {
      Msg_RC_SyncImpactEffect.Property property = impacteffect.propertys[i];
      int index = property.property_index;
      int value = property.property_value;
      if (index == (int)PropertyIndex.IDX_HP) {
        hpDamage = value - entity.Hp;
        entity.SetHp(Operate_Type.OT_Absolute, value);
      } else if (index == (int)PropertyIndex.IDX_MP) {
        npDamage = value - entity.Energy;
        entity.SetEnergy(Operate_Type.OT_Absolute, value);
      } else if (index == (int)PropertyIndex.IDX_STATE) {
        bool isHiddenBefore = entity.IsHaveStateFlag(CharacterState_Type.CST_Hidden);
        entity.StateFlag = value;
        CharacterInfo playerSelf = WorldSystem.Instance.GetPlayerSelf();
        if (null != playerSelf) {
          CharacterView view = EntityManager.Instance.GetCharacterViewById(entity.GetId());
          if (null != view) {
            bool isHidden = entity.IsHaveStateFlag(CharacterState_Type.CST_Hidden);
            if (isHiddenBefore ^ isHidden) {
              if (CharacterRelation.RELATION_FRIEND == CharacterInfo.GetRelation(entity, playerSelf)) {
                if (isHidden) {
                  //view.Actor.SetTransparent(0.2f);
                } else {
                  //view.Actor.SetTransparent(1.0f);
                }
              }
            }
          }
        }
      }
    }
    entity.SetAttackerInfo(impacteffect.attacker_id, impacteffect.attacker_type, impacteffect.is_killer, false, false, hpDamage, npDamage);
  }
}

public class Msg_RC_SyncItem_Handler
{
  public static void Execute(object msg, NetConnection conn)
  {
    Msg_RC_SyncItem targetmsg = msg as Msg_RC_SyncItem;
    if (null == targetmsg) {
      return;
    }

    CharacterInfo cb = WorldSystem.Instance.GetCharacterById(targetmsg.role_id);
    if (null == cb) {
      return;
    }

    cb.GetEquipmentStateInfo().ResetItemData();
    foreach (Msg_RC_SyncItem.ItemInfo info in targetmsg.items) {
      ItemDataInfo item = new ItemDataInfo();
      item.ItemNum = info.item_num;
      item.ItemConfig = ItemConfigProvider.Instance.GetDataById(info.item_id);
      if (null != item.ItemConfig) {
        cb.GetEquipmentStateInfo().SetItemData(info.item_pos, item);
      }
    }
    if (WorldSystem.Instance.PlayerSelfId == targetmsg.role_id) {
      GfxSystem.PublishGfxEvent("ge_SyncItem", "ui");
    }
  }
}

public class Msg_RC_ItemChanged_Handler
{
  public static void Execute(object msg, NetConnection conn)
  {
    Msg_RC_ItemChanged targetmsg = msg as Msg_RC_ItemChanged;
    if (null == targetmsg) {
      return;
    }

    CharacterInfo cb = WorldSystem.Instance.GetCharacterById(targetmsg.role_id);
    if (null == cb) {
      return;
    }

    if (targetmsg.item_id <= 0 || targetmsg.item_num <= 0) {
      cb.GetEquipmentStateInfo().SetItemData(targetmsg.item_pos, null);
      if (WorldSystem.Instance.PlayerSelfId == targetmsg.role_id) {
        GfxSystem.PublishGfxEvent("ge_ItemChanged", "ui", targetmsg.item_pos, 0);
      }
    } else {
      ItemDataInfo item = new ItemDataInfo();
      item.ItemNum = targetmsg.item_num;
      item.ItemConfig = ItemConfigProvider.Instance.GetDataById(targetmsg.item_id);
      if (null != item.ItemConfig) {
        cb.GetEquipmentStateInfo().SetItemData(targetmsg.item_pos, item);
        if (WorldSystem.Instance.PlayerSelfId == targetmsg.role_id) {
          GfxSystem.PublishGfxEvent("ge_ItemChanged", "ui", targetmsg.item_pos, item.ItemConfig.m_ItemId);
        }
      }
    }
  }
}

public class Msg_RC_CreateSceneLogic_Handler
{
  public static void Execute(object msg, NetConnection conn)
  {
    Msg_RC_CreateSceneLogic targetmsg = msg as Msg_RC_CreateSceneLogic;
    if (null == targetmsg) {
      return;
    }

    SceneLogicInfo info = WorldSystem.Instance.SceneLogicInfoManager.GetSceneLogicInfo(targetmsg.id);
    if (null != info) return;

    SceneResource scene = WorldSystem.Instance.GetCurScene();
    if (null == scene) return;
    SceneLogicConfig sc = SceneLogicConfigProvider.Instance.GetSceneLogicConfig(scene.ResId, targetmsg.config_id);
    if (null == sc) return;

    info = WorldSystem.Instance.SceneLogicInfoManager.AddSceneLogicInfo(targetmsg.id, sc);
    info.CreateTime = targetmsg.create_time;
    LogSystem.Debug("---laser: create scene logic servertime={0} remotetimeoffset={1} local-create-time={2} cur-server-time:{3}",
      targetmsg.create_time, TimeUtility.RemoteTimeOffset, info.CreateTime, TimeUtility.GetServerMilliseconds());
  }
}

public class Msg_RC_DestroySceneLogic_Handler
{
  public static void Execute(object msg, NetConnection conn)
  {
    Msg_RC_DestroySceneLogic targetmsg = msg as Msg_RC_DestroySceneLogic;
    if (null == targetmsg) {
      return;
    }

    SceneLogicInfo info = WorldSystem.Instance.SceneLogicInfoManager.GetSceneLogicInfo(targetmsg.id);
    if (null == info) return;

    // TODO: 销毁处理
    //
    WorldSystem.Instance.SceneLogicInfoManager.RemoveSceneLogicInfo(targetmsg.id);
  }
}

public class Msg_RC_SceneLogicImpact_Handler
{
  public static void Execute(object msg, NetConnection conn)
  {
    Msg_RC_SceneLogicImpact targetmsg = msg as Msg_RC_SceneLogicImpact;
    if (null == targetmsg) return;

    int sceneLogicInfoId = targetmsg.id;
    List<int> roleIdList = new List<int>();
    for (int i = 0; i < targetmsg.role_id.Count; i++) {
      roleIdList.Add(targetmsg.role_id[i]);
    }
  }
}

public class Msg_RC_DebugSpaceInfo_Handler
{
  public static void Execute(object msg, NetConnection conn)
  {
    Msg_RC_DebugSpaceInfo targetmsg = msg as Msg_RC_DebugSpaceInfo;
    if (null == targetmsg) return;

    EntityManager.Instance.MarkSpaceInfoViews();
    if (GlobalVariables.Instance.IsDebug) {
      lock (GfxSystem.SyncLock) {
        foreach (Msg_RC_DebugSpaceInfo.DebugSpaceInfo info in targetmsg.space_infos) {
          EntityManager.Instance.UpdateSpaceInfoView(info.obj_id, info.is_player, info.pos_x, info.pos_y, info.pos_z, info.face_dir);
        }
      }
    }
    EntityManager.Instance.DestroyUnusedSpaceInfoViews();
  }
}

public class Msg_RC_PvpFinish_Handler
{
  public static void Execute(object msg, NetConnection conn)
  {
    Msg_RC_PvpFinish message = msg as Msg_RC_PvpFinish;
    if (null == message) return;

    GfxSystem.PublishGfxEvent("ge_pvpfinish", "ui", message.win_camp_id);
  }
}

public class Msg_RC_PveFinish_Handler
{
  public static void Execute(object msg, NetConnection conn)
  {
    Msg_RC_PveFinish message = msg as Msg_RC_PveFinish;
    if (null == message) return;

    GfxSystem.PublishGfxEvent("ge_pvefinish", "ui", message.is_win);
  }
}

public class Msg_RC_SyncCombatStatisticInfo_Handler
{
  public static void Execute(object msg, NetConnection conn)
  {

    Msg_RC_SyncCombatStatisticInfo message = msg as Msg_RC_SyncCombatStatisticInfo;
    if (null == message) return;

    UserInfo user = WorldSystem.Instance.UserManager.GetUserInfo(message.role_id);
    if (null != user) {
      CombatStatisticInfo info = user.GetCombatStatisticInfo();
      info.KillHeroCount = message.kill_hero_count;
      info.AssitKillCount = message.assit_kill_count;
      info.KillNpcCount = message.kill_npc_count;
      info.DeadCount = message.dead_count;

      if (message.role_id == WorldSystem.Instance.PlayerSelfId || WorldSystem.Instance.IsObserver && WorldSystem.Instance.IsFollowObserver && message.role_id == WorldSystem.Instance.FollowTargetId) {
        GfxSystem.PublishGfxEvent("ge_pvpSelfCombatInfo", "ui", info);
      }
    }
  }
}

public class Msg_RC_PvpCombatInfo_Handler
{
  public static void Execute(object msg, NetConnection conn)
  {
    Msg_RC_PvpCombatInfo message = msg as Msg_RC_PvpCombatInfo;
    if (null == message) return;
    GfxSystem.PublishGfxEvent("ge_pvpcombatinfo", "ui", message.kill_hero_count_for_blue,
                                    message.kill_hero_count_for_red,
                                    message.link_id_for_killer,
                                    message.link_id_for_killed, message.killer_nickname, message.killed_nickname);
  }
}

public class Msg_CRC_UpdateSkill_Handler
{
  public static void Execute(object msg, NetConnection conn)
  {
    Msg_CRC_UpdateSkill message = msg as Msg_CRC_UpdateSkill;
    if (null == message) return;

    int entityId = message.role_id;
    int skillIndex = message.skill_index;

    CharacterInfo entity = WorldSystem.Instance.SceneContext.GetCharacterInfoById(entityId);
    //SkillSystem.Instance.UpdateSkill(entity, skillIndex);
  }
}

public class Msg_RC_SendImpactToEntity_Handler
{
  public static void Execute(object msg, NetConnection conn)
  {
    Msg_RC_SendImpactToEntity message = msg as Msg_RC_SendImpactToEntity;
    if (null == message) return;
    CharacterInfo target = WorldSystem.Instance.SceneContext.GetCharacterInfoById(message.target_id);
    if (null == target) {
      LogSystem.Debug("Receive Msg_RC_SendImpactToEntity, message.target_id={0} is not available", message.target_id);
      return;
    } else {
      LogSystem.Debug("Receive Msg_RC_SendImpactToEntity, TargetId={0}, ImpactId={1}, SenderId={2}, SkillId={3}",
        message.target_id, message.impact_id, message.sender_id, message.skill_id);
    }
    CharacterView view = EntityManager.Instance.GetCharacterViewById(target.GetId());
    if (null == view) return;
    if (null != message.target_pos && view.Visible) {
      ScriptRuntime.Vector2 pos = new ScriptRuntime.Vector2(
        message.target_pos.x, message.target_pos.z);
      target.GetMovementStateInfo().SetPosition2D(pos);
    }
  }
}

public class Msg_RC_RemoveImpactFromEntity_Handler
{
  public static void Execute(object msg, NetConnection conn)
  {
    Msg_RC_RemoveImpactFromEntity message = msg as Msg_RC_RemoveImpactFromEntity;
    if (null == message) return;

    CharacterInfo entity = WorldSystem.Instance.SceneContext.GetCharacterInfoById(message.role_id);
    if (null != entity) {
      //ImpactSystem.Instance.OnClientStopEntityImpact(entity, message.impact_id);
    }
  }
}

public class Msg_CRC_InteractObject_Handler
{
  public static void Execute(object msg, NetConnection conn)
  {
    Msg_CRC_InteractObject _msg = msg as Msg_CRC_InteractObject;
    if (null == _msg)
      return;

    int initiatorId = _msg.initiator_id;
    int receiverId = _msg.receiver_id;

    UserInfo initiator = WorldSystem.Instance.GetCharacterById(initiatorId) as UserInfo;
    NpcInfo receiver = WorldSystem.Instance.GetCharacterById(receiverId) as NpcInfo;

    if (null != initiator && null != receiver) {
    }
  }
}

public class Msg_RC_ControlObject_Handler
{
  public static void Execute(object msg, NetConnection conn)
  {
    Msg_RC_ControlObject _msg = msg as Msg_RC_ControlObject;
    if (null == _msg)
      return;



    CharacterInfo controller = WorldSystem.Instance.GetCharacterById((int)_msg.controller_id);
    if (controller == null)
      return;
    CharacterInfo controlled = WorldSystem.Instance.GetCharacterById((int)_msg.controlled_id);
    if (_msg.control_or_release) {
      if (null != controlled && !controller.IsDead() && !controlled.IsDead()) {
        CharacterInfo.ControlObject(controller, controlled);
        controller.GetMovementStateInfo().SetPosition2D(controlled.GetMovementStateInfo().GetPosition2D());
        controller.GetMovementStateInfo().SetFaceDir(controlled.GetMovementStateInfo().GetFaceDir());
      }
    } else {
      CharacterInfo.ReleaseControlObject(controller, controlled);
      if (null != controlled) {
        controlled.GetMovementStateInfo().IsMoving = false;
      }
      if (null != controller) {
        UserView view = EntityManager.Instance.GetUserViewById(_msg.controller_id);
        if (null != view) {
          view.Visible = true;
          LogSystem.Debug("Show Player {0} link id {1} position {2} for Msg_RC_ControlObject", controller.GetId(), controller.GetLinkId(), controller.GetMovementStateInfo().GetPosition3D().ToString());
        }
      }
    }
  }
}

public class Msg_RC_RefreshItemSkills_Handler
{
  public static void Execute(object msg, NetConnection conn)
  {
    Msg_RC_RefreshItemSkills _msg = msg as Msg_RC_RefreshItemSkills;
    if (null == _msg)
      return;
    CharacterInfo charObj = WorldSystem.Instance.GetCharacterById(_msg.role_id);
    if (null == charObj) {
      return;
    }
    UserInfo user = charObj.CastUserInfo();
    if (null != user) {
      user.RefreshItemSkills();
    }
  }
}

public class Msg_RC_HighlightPrompt_Handler
{
  public static void Execute(object msg, NetConnection conn)
  {
    Msg_RC_HighlightPrompt _msg = msg as Msg_RC_HighlightPrompt;
    if (null == _msg)
      return;
    WorldSystem.Instance.HighlightPrompt(_msg.dict_id, _msg.argument.ToArray());
  }
}

public class Msg_RC_NotifyEarnMoney_Handler
{
  public static void Execute(object msg, NetConnection conn)
  {
    Msg_RC_NotifyEarnMoney _msg = msg as Msg_RC_NotifyEarnMoney;
    if (null == _msg)
      return;
    GfxSystem.PublishGfxEvent("ge_getsomething", "ui", _msg.id, "coin", _msg.money, 110);
  }
}