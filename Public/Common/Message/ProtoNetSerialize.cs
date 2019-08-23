using System;
using System.IO;
using System.Collections.Generic;
using DashFireMessage;

namespace DashFire.Network
{
  public class Serialize
  {
    private static ProtobufSerializer m_Serializer = new ProtobufSerializer();
    private static MemoryStream m_Stream = new MemoryStream(4096);
    private static MyDictionary<int, Type> m_DicIDMsg = new MyDictionary<int, Type>();
    private static MyDictionary<Type, int> m_DicIDName = new MyDictionary<Type, int>();
    public static bool Init()
    {
      RegisterIDName((int)MessageDefine.Msg_Ping, typeof(Msg_Ping));
      RegisterIDName((int)MessageDefine.Msg_Pong, typeof(Msg_Pong));
      RegisterIDName((int)MessageDefine.Msg_CR_ShakeHands, typeof(Msg_CR_ShakeHands));
      RegisterIDName((int)MessageDefine.Msg_RC_ShakeHands_Ret, typeof(Msg_RC_ShakeHands_Ret));
      RegisterIDName((int)MessageDefine.Msg_CR_Observer, typeof(Msg_CR_Observer));
      RegisterIDName((int)MessageDefine.Msg_CRC_Create, typeof(Msg_CRC_Create));
      RegisterIDName((int)MessageDefine.Msg_RC_Enter, typeof(Msg_RC_Enter));
      RegisterIDName((int)MessageDefine.Msg_RC_Disappear, typeof(Msg_RC_Disappear));
      RegisterIDName((int)MessageDefine.Msg_RC_Dead, typeof(Msg_RC_Dead));
      RegisterIDName((int)MessageDefine.Msg_RC_Revive, typeof(Msg_RC_Revive));
      RegisterIDName((int)MessageDefine.Msg_CRC_Exit, typeof(Msg_CRC_Exit));
      RegisterIDName((int)MessageDefine.Msg_CRC_MoveStart, typeof(Msg_CRC_MoveStart));
      RegisterIDName((int)MessageDefine.Msg_CRC_MoveStop, typeof(Msg_CRC_MoveStop));
      RegisterIDName((int)MessageDefine.Msg_RC_MoveMeetObstacle, typeof(Msg_RC_MoveMeetObstacle));
      RegisterIDName((int)MessageDefine.Msg_CRC_Face, typeof(Msg_CRC_Face));
      RegisterIDName((int)MessageDefine.Msg_CRC_Skill, typeof(Msg_CRC_Skill));
      RegisterIDName((int)MessageDefine.Msg_CRC_Effect, typeof(Msg_CRC_Effect));
      RegisterIDName((int)MessageDefine.Msg_CRC_Action, typeof(Msg_CRC_Action));
      RegisterIDName((int)MessageDefine.Msg_RC_CreateNpc, typeof(Msg_RC_CreateNpc));
      RegisterIDName((int)MessageDefine.Msg_RC_DestroyNpc, typeof(Msg_RC_DestroyNpc));
      RegisterIDName((int)MessageDefine.Msg_RC_NpcEnter, typeof(Msg_RC_NpcEnter));
      RegisterIDName((int)MessageDefine.Msg_RC_NpcMove, typeof(Msg_RC_NpcMove));
      RegisterIDName((int)MessageDefine.Msg_RC_NpcFace, typeof(Msg_RC_NpcFace));
      RegisterIDName((int)MessageDefine.Msg_RC_NpcTarget, typeof(Msg_RC_NpcTarget));
      RegisterIDName((int)MessageDefine.Msg_RC_NpcSkill, typeof(Msg_RC_NpcSkill));
      RegisterIDName((int)MessageDefine.Msg_RC_NpcDead, typeof(Msg_RC_NpcDead));
      RegisterIDName((int)MessageDefine.Msg_RC_NpcDisappear, typeof(Msg_RC_NpcDisappear));
      RegisterIDName((int)MessageDefine.Msg_RC_Levelup, typeof(Msg_RC_Levelup));
      RegisterIDName((int)MessageDefine.Msg_RC_SyncLevel, typeof(Msg_RC_SyncLevel));
      RegisterIDName((int)MessageDefine.Msg_RC_SyncExpMoney, typeof(Msg_RC_SyncExpMoney));
      RegisterIDName((int)MessageDefine.Msg_RC_SyncProperty, typeof(Msg_RC_SyncProperty));
      RegisterIDName((int)MessageDefine.Msg_RC_SyncImpactEffect, typeof(Msg_RC_SyncImpactEffect));
      RegisterIDName((int)MessageDefine.Msg_RC_SyncItem, typeof(Msg_RC_SyncItem));
      RegisterIDName((int)MessageDefine.Msg_RC_ItemChanged, typeof(Msg_RC_ItemChanged));
      RegisterIDName((int)MessageDefine.Msg_RC_CreateSceneLogic, typeof(Msg_RC_CreateSceneLogic));
      RegisterIDName((int)MessageDefine.Msg_RC_DestroySceneLogic, typeof(Msg_RC_DestroySceneLogic));
      RegisterIDName((int)MessageDefine.Msg_RC_SceneLogicImpact, typeof(Msg_RC_SceneLogicImpact));
      RegisterIDName((int)MessageDefine.Msg_CR_KeyPress, typeof(Msg_CR_KeyPress));
      RegisterIDName((int)MessageDefine.Msg_RC_DebugSpaceInfo, typeof(Msg_RC_DebugSpaceInfo));
      RegisterIDName((int)MessageDefine.Msg_CR_SwitchDebug, typeof(Msg_CR_SwitchDebug));
      RegisterIDName((int)MessageDefine.Msg_CR_BuyItem, typeof(Msg_CR_BuyItem));
      RegisterIDName((int)MessageDefine.Msg_CR_SellItem, typeof(Msg_CR_SellItem));
      RegisterIDName((int)MessageDefine.Msg_CR_SwapItem, typeof(Msg_CR_SwapItem));
      RegisterIDName((int)MessageDefine.Msg_CR_DiscardItem, typeof(Msg_CR_DiscardItem));
      RegisterIDName((int)MessageDefine.Msg_RC_PvpFinish, typeof(Msg_RC_PvpFinish));
      RegisterIDName((int)MessageDefine.Msg_RC_PveFinish, typeof(Msg_RC_PveFinish));
      RegisterIDName((int)MessageDefine.Msg_RC_SyncCombatStatisticInfo, typeof(Msg_RC_SyncCombatStatisticInfo));
      RegisterIDName((int)MessageDefine.Msg_RC_PvpCombatInfo, typeof(Msg_RC_PvpCombatInfo));
      RegisterIDName((int)MessageDefine.Msg_CRC_UpdateSkill, typeof(Msg_CRC_UpdateSkill));
      RegisterIDName((int)MessageDefine.Msg_RC_SendImpactToEntity, typeof(Msg_RC_SendImpactToEntity));
      RegisterIDName((int)MessageDefine.Msg_RC_RemoveImpactFromEntity, typeof(Msg_RC_RemoveImpactFromEntity));
      RegisterIDName((int)MessageDefine.Msg_CRC_MousePos, typeof(Msg_CRC_MousePos));
      RegisterIDName((int)MessageDefine.Msg_CRC_InteractObject, typeof(Msg_CRC_InteractObject));
      RegisterIDName((int)MessageDefine.Msg_RC_ControlObject, typeof(Msg_RC_ControlObject));
      RegisterIDName((int)MessageDefine.Msg_RC_RefreshItemSkills, typeof(Msg_RC_RefreshItemSkills));
      RegisterIDName((int)MessageDefine.Msg_RC_HighlightPrompt, typeof(Msg_RC_HighlightPrompt));
      RegisterIDName((int)MessageDefine.Msg_CRC_DescriptorSync, typeof(GM.Msg_CRC_DescriptorSync));
      RegisterIDName((int)MessageDefine.Msg_CR_Execute, typeof(GM.Msg_CR_Execute));
      RegisterIDName((int)MessageDefine.Msg_RC_ExecuteResult, typeof(GM.Msg_RC_ExecuteResult));
      RegisterIDName((int)MessageDefine.Msg_RC_NotifyEarnMoney, typeof(Msg_RC_NotifyEarnMoney));
      RegisterIDName((int)MessageDefine.Msg_CR_QuitClient, typeof(Msg_CR_QuitClient));
      RegisterIDName((int)MessageDefine.Msg_RC_UserMove, typeof(Msg_RC_UserMove));
      RegisterIDName((int)MessageDefine.Msg_RC_UserFace, typeof(Msg_RC_UserFace));
      RegisterIDName((int)MessageDefine.Msg_CRC_UserFindPath, typeof(Msg_CRC_UserFindPath));
      return true;
    }
    private static void RegisterIDName(int id, Type msgtype)
    {
      m_DicIDMsg[id] = msgtype;
      m_DicIDName[msgtype] = id;
    }
    public static byte[] Encode(object msg)
    {
      if (m_DicIDName.ContainsKey(msg.GetType())) {
        m_Stream.SetLength(0);
        m_Serializer.Serialize(m_Stream, msg);
        byte[] ret = new byte[2 + m_Stream.Length];
        int id = m_DicIDName[msg.GetType()];
        ret[0] = (byte)(id >> 8);
        ret[1] = (byte)(id);
        m_Stream.Position = 0;
        m_Stream.Read(ret, 2, ret.Length - 2);
        return ret;
      } else {
        return null;
      }
    }
    public static object Decode(byte[] msgbuf)
    {
      int id = (int)(((int)msgbuf[0] << 8) | ((int)msgbuf[1]));
      if (id < 0) {
        LogSystem.Debug("decode error:message id({0}) error !!!", id);
        return null;
      }

      if(m_DicIDMsg.ContainsKey(id)){
        Type t = m_DicIDMsg[id];
        m_Stream.SetLength(0);
        m_Stream.Write(msgbuf,2,msgbuf.Length-2);
        m_Stream.Position = 0;
        object msg = m_Serializer.Deserialize(m_Stream, null, t);
        if (msg == null) {
          LogSystem.Debug("decode error:can't find id {0}  !!!", id);
          return null;
        }
        return msg;
      }
      return null;
    }
  }
}
