using System;
using System.Collections.Generic;
using System.Text;
using Lidgren.Network;
using System.Threading;
using System.Reflection;

using DashFireMessage;
using System.Runtime.InteropServices;

namespace DashFire.Network
{
  public class NetworkSystem
  {
    #region
    private static NetworkSystem s_Instance = new NetworkSystem();
    public static NetworkSystem Instance
    {
      get { return s_Instance; }
    }
    #endregion

    public bool Init()
    {
      Serialize.Init();
      InitMessageHandler();

      m_IsWaitStart = true;
      m_IsQuited = false;
      m_IsConnected = false;

      m_Config = new NetPeerConfiguration("RoomServer");
      m_Config.AutoFlushSendQueue = false;
      m_Config.DisableMessageType(NetIncomingMessageType.DebugMessage);
      m_Config.DisableMessageType(NetIncomingMessageType.VerboseDebugMessage);
      m_Config.EnableMessageType(NetIncomingMessageType.ErrorMessage);
      m_Config.EnableMessageType(NetIncomingMessageType.WarningMessage);
      m_NetClient = new NetClient(m_Config);
      m_NetClient.Start();
      m_NetThread = new Thread(new ThreadStart(NetworkThread));
      m_NetThread.Start ();
      return true;
    }

    public void Start(uint key, string ip, int port, int heroId, int campId, int sceneId)
    {
      m_Key = key;
      m_Ip = ip;
      m_Port = port;
      m_HeroId = heroId;
      m_CampId = campId;
      m_SceneId = sceneId;

      m_IsWaitStart = false;
      m_IsConnected = false;

      GfxSystem.GfxLog("NetworkSystem.Start key {0} ip {1} port {2} hero {3} camp {4} scene {5}", key, ip, port, heroId, campId, sceneId);
    }

    public void Tick()
    {
      try {
        if (m_NetClient == null)
          return;
        if (m_IsConnected) {
          if (TimeUtility.GetLocalMilliseconds() - m_LastPingTime >= m_PingInterval) {
            InternalPing();
          }
        }
        ProcessMsg();
      } catch (Exception e) {
        string err = "Exception:" + e.Message + "\n" + e.StackTrace + "\n";
        GfxSystem.GfxLog("Exception:{0}\n{1}", e.Message, e.StackTrace);
      }
    }

    public void QuitBattle()
    {
      Msg_CR_QuitClient msg = new Msg_CR_QuitClient();
      SendMessage(msg);
      m_IsWaitStart = true;
    }

    public void QuitClient()
    {
      m_IsQuited = true;
    }

    public void SendLoginMsg(object msg)
    {
      SendMessage(msg);
    }

    public void SendMessage(object msg)
    {
      if (!m_IsConnected) {
        return;
      }
      NetOutgoingMessage om = m_NetClient.CreateMessage();
      byte[] bt = Serialize.Encode(msg);
      om.Write(bt);
      NetSendResult result = m_NetClient.SendMessage(om, NetDeliveryMethod.ReliableOrdered);
      if (result == NetSendResult.FailedNotConnected) {
        m_IsConnected = false;
        LogSystem.Debug("SendMessage FailedNotConnected");
      } else if (result == NetSendResult.Dropped) {
        LogSystem.Error("SendMessage {0} Dropped", msg.ToString());
      }
      m_NetClient.FlushSendQueue();
    }

    public void Release()
    {
      if (m_NetClient != null)
        m_NetClient.Shutdown("bye!");
    }

    public void OnPong(long time, long sendPingTime, long sendPongTime)
    {
      if (time < sendPingTime) return;
      ++m_PingPongNumber;

      long rtt = time - sendPingTime;
      if (TimeUtility.AverageRoundtripTime == 0)
        TimeUtility.AverageRoundtripTime = rtt;
      else
        TimeUtility.AverageRoundtripTime = (long)(TimeUtility.AverageRoundtripTime * 0.7f + rtt * 0.3f);

      LogSystem.Debug("RoundtripTime:{0} AverageRoundtripTime:{1}", rtt, TimeUtility.AverageRoundtripTime);

      long diff = sendPongTime + rtt/2 - time;
      TimeUtility.RemoteTimeOffset = (TimeUtility.RemoteTimeOffset * (m_PingPongNumber - 1) + diff) / m_PingPongNumber;
    }

    public void SyncFaceDirection(float face_direction)
    {
      if (Math.Abs(m_LastFaceDir - face_direction) <= 0.01) {
        return;
      }
      m_LastFaceDir = face_direction;
      Msg_CRC_Face bd = new Msg_CRC_Face();
      bd.face_direction = face_direction;
      SendMessage(bd);
    }

    public void SyncPlayerMoveStart(float dir)
    {
      DashFireMessage.Msg_CRC_MoveStart builder = new DashFireMessage.Msg_CRC_MoveStart();
      builder.dir = dir;
      SendMessage(builder);
    }

    public void SyncPlayerMoveStop()
    {
      DashFireMessage.Msg_CRC_MoveStop builder = new DashFireMessage.Msg_CRC_MoveStop();
      SendMessage(builder);
    }

    public void SyncPlayerSkill(CharacterInfo entity,
        int skillId,
        CharacterInfo target,
        ScriptRuntime.Vector3 targetPos,
        float targetAngle,
        ScriptRuntime.Vector3 standPos,
        float faceAngle,
        int itemId)
    {
      if (entity.IsHaveStateFlag(CharacterState_Type.CST_Sleep)) {
        return;
      }
      int target_serverid = 0;
      if (target != null) {
        target_serverid = target.GetId();
      }
      Msg_CRC_Skill bd = new Msg_CRC_Skill();
      bd.role_id = entity.GetId();
      bd.skill_id = skillId;
      bd.target_id = target_serverid;
      bd.target_pos = new DashFireMessage.Position();
      bd.target_pos.x = targetPos.X;
      bd.target_pos.y = targetPos.Y;
      bd.target_pos.z = targetPos.Z;
      bd.target_angle = (float)targetAngle;
      bd.stand_pos = new DashFireMessage.Position();
      bd.stand_pos.x = standPos.X;
      bd.stand_pos.y = standPos.Y;
      bd.stand_pos.z = standPos.Z;
      bd.face_direction = (float)faceAngle;
      bd.item_id = itemId;
      SendMessage(bd);
    }

    public void  SyncUpdateSkill(CharacterInfo entity, int index)
    {
        Msg_CRC_UpdateSkill bd = new Msg_CRC_UpdateSkill();
        bd.role_id = entity.GetId();
        bd.skill_index = index;
        SendMessage(bd);
    }

    private void RegisterMsgHandler(Type msgtype, MessageDispatch.MsgHandler handler)
    {
      m_Dispatch.RegisterHandler(msgtype, handler);
    }
    private void InitMessageHandler()
    {
      RegisterMsgHandler(typeof(Msg_Pong), new MessageDispatch.MsgHandler(MsgPongHandler.Execute));
      RegisterMsgHandler(typeof(Msg_RC_ShakeHands_Ret), new MessageDispatch.MsgHandler(MsgShakeHandsRetHandler.Execute));
      RegisterMsgHandler(typeof(Msg_CRC_Create), new MessageDispatch.MsgHandler(Msg_CRC_Create_Handler.Execute));
      RegisterMsgHandler(typeof(Msg_RC_Enter), new MessageDispatch.MsgHandler(Msg_RC_Enter_Handler.Execute));
      RegisterMsgHandler(typeof(Msg_RC_Disappear), new MessageDispatch.MsgHandler(Msg_RC_Disappear_Handler.Execute));
      RegisterMsgHandler(typeof(Msg_RC_Dead), new MessageDispatch.MsgHandler(Msg_RC_Dead_Handler.Execute));
      RegisterMsgHandler(typeof(Msg_RC_Revive), new MessageDispatch.MsgHandler(Msg_RC_Revive_Handler.Execute));
      RegisterMsgHandler(typeof(Msg_CRC_Exit), new MessageDispatch.MsgHandler(Msg_CRC_Exit_Handler.Execute));
      RegisterMsgHandler(typeof(Msg_CRC_MoveStart), new MessageDispatch.MsgHandler(Msg_CRC_Move_Handler.OnMoveStart));
      RegisterMsgHandler(typeof(Msg_CRC_MoveStop), new MessageDispatch.MsgHandler(Msg_CRC_Move_Handler.OnMoveStop));
      RegisterMsgHandler(typeof(Msg_RC_MoveMeetObstacle), new MessageDispatch.MsgHandler(Msg_RC_MoveMeetObstacle_Handler.Execute));
      RegisterMsgHandler(typeof(Msg_CRC_Face), new MessageDispatch.MsgHandler(Msg_CRC_Face_Handler.Execute));
      RegisterMsgHandler(typeof(Msg_CRC_Skill), new MessageDispatch.MsgHandler(Msg_CRC_Skill_Handler.Execute));
      RegisterMsgHandler(typeof(Msg_RC_UserMove), new MessageDispatch.MsgHandler(Msg_RC_UserMove_Handler.Execute));
      RegisterMsgHandler(typeof(Msg_RC_UserFace), new MessageDispatch.MsgHandler(Msg_RC_UserFace_Handler.Execute));
      RegisterMsgHandler(typeof(Msg_RC_CreateNpc), new MessageDispatch.MsgHandler(Msg_RC_CreateNpc_Handler.Execute));
      RegisterMsgHandler(typeof(Msg_RC_DestroyNpc), new MessageDispatch.MsgHandler(Msg_RC_DestroyNpc_Handler.Execute));
      RegisterMsgHandler(typeof(Msg_RC_NpcEnter), new MessageDispatch.MsgHandler(Msg_RC_NpcEnter_Handler.Execute));
      RegisterMsgHandler(typeof(Msg_RC_NpcMove), new MessageDispatch.MsgHandler(Msg_RC_NpcMove_Handler.Execute));
      RegisterMsgHandler(typeof(Msg_RC_NpcFace), new MessageDispatch.MsgHandler(Msg_RC_NpcFace_Handler.Execute));
      RegisterMsgHandler(typeof(Msg_RC_NpcTarget), new MessageDispatch.MsgHandler(Msg_RC_NpcTarget_Handler.Execute));
      RegisterMsgHandler(typeof(Msg_RC_NpcSkill), new MessageDispatch.MsgHandler(Msg_RC_NpcSkill_Handler.Execute));
      RegisterMsgHandler(typeof(Msg_RC_NpcDead), new MessageDispatch.MsgHandler(Msg_RC_NpcDead_Handler.Execute));
      RegisterMsgHandler(typeof(Msg_RC_NpcDisappear), new MessageDispatch.MsgHandler(Msg_RC_NpcDisappear_Handler.Execute));
      RegisterMsgHandler(typeof(Msg_RC_Levelup), new MessageDispatch.MsgHandler(Msg_RC_Levelup_Handler.Execute));
      RegisterMsgHandler(typeof(Msg_RC_SyncLevel), new MessageDispatch.MsgHandler(Msg_RC_SyncLevel_Handler.Execute));
      RegisterMsgHandler(typeof(Msg_RC_SyncExpMoney), new MessageDispatch.MsgHandler(Msg_RC_SyncExpMoney_Handler.Execute));
      RegisterMsgHandler(typeof(Msg_RC_SyncProperty), new MessageDispatch.MsgHandler(Msg_RC_SyncProperty_Handler.Execute));
      RegisterMsgHandler(typeof(Msg_RC_SyncImpactEffect), new MessageDispatch.MsgHandler(Msg_RC_SyncImpactEffect_Handler.Execute));
      RegisterMsgHandler(typeof(Msg_RC_SyncItem), new MessageDispatch.MsgHandler(Msg_RC_SyncItem_Handler.Execute));
      RegisterMsgHandler(typeof(Msg_RC_ItemChanged), new MessageDispatch.MsgHandler(Msg_RC_ItemChanged_Handler.Execute));
      RegisterMsgHandler(typeof(Msg_RC_CreateSceneLogic), new MessageDispatch.MsgHandler(Msg_RC_CreateSceneLogic_Handler.Execute));
      RegisterMsgHandler(typeof(Msg_RC_DestroySceneLogic), new MessageDispatch.MsgHandler(Msg_RC_DestroySceneLogic_Handler.Execute));
      RegisterMsgHandler(typeof(Msg_RC_SceneLogicImpact), new MessageDispatch.MsgHandler(Msg_RC_SceneLogicImpact_Handler.Execute));
      RegisterMsgHandler(typeof(Msg_RC_DebugSpaceInfo), new MessageDispatch.MsgHandler(Msg_RC_DebugSpaceInfo_Handler.Execute));
      RegisterMsgHandler(typeof(Msg_RC_PvpFinish), new MessageDispatch.MsgHandler(Msg_RC_PvpFinish_Handler.Execute));
      RegisterMsgHandler(typeof(Msg_RC_PveFinish), new MessageDispatch.MsgHandler(Msg_RC_PveFinish_Handler.Execute));
      RegisterMsgHandler(typeof(Msg_RC_SyncCombatStatisticInfo), new MessageDispatch.MsgHandler(Msg_RC_SyncCombatStatisticInfo_Handler.Execute));
      RegisterMsgHandler(typeof(Msg_RC_PvpCombatInfo), new MessageDispatch.MsgHandler(Msg_RC_PvpCombatInfo_Handler.Execute));
      RegisterMsgHandler(typeof(Msg_CRC_UpdateSkill), new MessageDispatch.MsgHandler(Msg_CRC_UpdateSkill_Handler.Execute));
      RegisterMsgHandler(typeof(Msg_RC_SendImpactToEntity), new MessageDispatch.MsgHandler(Msg_RC_SendImpactToEntity_Handler.Execute));
      RegisterMsgHandler(typeof(Msg_RC_RemoveImpactFromEntity), new MessageDispatch.MsgHandler(Msg_RC_RemoveImpactFromEntity_Handler.Execute));
      RegisterMsgHandler(typeof(Msg_CRC_InteractObject), new MessageDispatch.MsgHandler(Msg_CRC_InteractObject_Handler.Execute));
      RegisterMsgHandler(typeof(Msg_RC_ControlObject), new MessageDispatch.MsgHandler(Msg_RC_ControlObject_Handler.Execute));
      RegisterMsgHandler(typeof(Msg_RC_RefreshItemSkills), new MessageDispatch.MsgHandler(Msg_RC_RefreshItemSkills_Handler.Execute));
      RegisterMsgHandler(typeof(Msg_RC_HighlightPrompt), new MessageDispatch.MsgHandler(Msg_RC_HighlightPrompt_Handler.Execute));
      RegisterMsgHandler(typeof(Msg_RC_NotifyEarnMoney), new MessageDispatch.MsgHandler(Msg_RC_NotifyEarnMoney_Handler.Execute));
    }
    private void NetworkThread()
    {
      while (!m_IsQuited) {
        if (m_IsWaitStart) {
          Thread.Sleep(1000);
        } else {
          while (!m_IsConnected) {
            LogSystem.Debug("Connect ip:{0} port:{1} key:{2}\nNetPeer Statistic:{3}", m_Ip, m_Port, m_Key, m_NetClient.Statistics.ToString());
            m_NetClient.Connect(m_Ip, m_Port);
            for (int ct = 0; ct < 10 && !m_IsConnected; ++ct) {
              OnRecvMessage();
              LogSystem.Debug("Wait NetConnectionStatus.Connected ...");
            }
          }
          OnRecvMessage();
        }
      }
    }
    private void OnConnected(NetConnection conn)
    {
      m_Connection = conn;
      m_IsConnected = true;
      Msg_CR_ShakeHands bd = new Msg_CR_ShakeHands();
      bd.auth_key = m_Key;
      SendMessage(bd);
    }
    private void OnRecvMessage()
    {
      m_NetClient.MessageReceivedEvent.WaitOne(1000);
      NetIncomingMessage im;
      while ((im = m_NetClient.ReadMessage()) != null) {
        switch (im.MessageType) {
          case NetIncomingMessageType.DebugMessage:
          case NetIncomingMessageType.VerboseDebugMessage:
            LogSystem.Debug("Debug Message: {0}", im.ReadString());
            break;
          case NetIncomingMessageType.ErrorMessage:
            LogSystem.Debug("Error Message: {0}", im.ReadString());
            break;
          case NetIncomingMessageType.WarningMessage:
            LogSystem.Debug("Warning Message: {0}", im.ReadString());
            break;
          case NetIncomingMessageType.StatusChanged:
            NetConnectionStatus status = im.SenderConnection.Status;

            string reason = im.ReadString();
            LogSystem.Debug("Network Status Changed:{0} Reason:{1}\nStatistic:{2}", status.ToString(), reason, im.SenderConnection.Statistics.ToString());
            if (NetConnectionStatus.Disconnected == status) {
              m_IsConnected = false;
            } else if (NetConnectionStatus.Connected == status) {
              OnConnected(im.SenderConnection);
            }
            break;
          case NetIncomingMessageType.Data:
            if (m_IsConnected == false) {
              break;
            }
            try {
              object msg = Serialize.Decode(im.ReadBytes(im.LengthBytes));
              if (msg != null) {
                PushMsg(msg, im.SenderConnection);
              }
            } catch (Exception ex) {
              GfxSystem.GfxLog("Decode Message exception:{0}\n{1}", ex.Message, ex.StackTrace);
            }
            break;
          default:
            break;
        }
        m_NetClient.Recycle(im);
      }
    }
    private bool PushMsg(object msg, NetConnection conn)
    {
      lock (m_Lock) {
        m_QueuePair.Enqueue(new KeyValuePair<NetConnection, object>(conn, msg));
        return true;
      }
    }
    private int ProcessMsg()
    {
      lock (m_Lock) {
        if (m_QueuePair.Count <= 0)
          return -1;
        foreach (KeyValuePair<NetConnection, object> kv in m_QueuePair) {
          object msg = kv.Value;
          m_Dispatch.Dispatch(msg, kv.Key);
        }
        m_QueuePair.Clear();
        return 0;
      }
    }

    private void InternalPing()
    {
      Msg_Ping builder = new Msg_Ping();
      m_LastPingTime = TimeUtility.GetLocalMilliseconds();
      builder.send_ping_time = (int)m_LastPingTime;
      SendMessage(builder);
    }

    public void SyncPlayerFindPath(ScriptRuntime.Vector3 target_pos)
    {
      Msg_CRC_UserFindPath builder = new Msg_CRC_UserFindPath();
      builder.target_pos_x = target_pos.X;
      builder.target_pos_y = target_pos.Y;
      builder.target_pos_z = target_pos.Z;
      SendMessage(builder);
    }

    public int HeroId
    {
      get { return m_HeroId; }
      set { m_HeroId = value; }
    }
    public int CampId
    {
      get { return m_CampId; }
      set { m_CampId = value; }
    }
    public int SceneId
    {
      get { return m_SceneId; }
      set { m_SceneId = value; }
    }

    private long m_PingPongNumber = 0;
    private long m_LastPingTime = TimeUtility.GetLocalMilliseconds();        // ms
    private int m_PingInterval = 1000;         // ms

    private NetPeerConfiguration m_Config;
    private NetClient m_NetClient;
    private NetConnection m_Connection;
    private Thread m_NetThread;
    private string m_Ip;
    private int m_Port;
    private bool m_IsConnected = false;
    private bool m_IsWaitStart = true;
    private bool m_IsQuited = false;
    private MessageDispatch m_Dispatch = new MessageDispatch();
    private Queue<KeyValuePair<NetConnection, object>> m_QueuePair = new Queue<KeyValuePair<NetConnection, object>>();
    private object m_Lock = new object();
    private uint m_Key = 0;
    private float m_LastFaceDir = 0.0f;
    private int m_HeroId = 0;
    private int m_CampId = 0;
    private int m_SceneId = 0;
  }

  public struct PingRecord
  {
    public long m_Ping;
    public long m_TimeDifferental;
  }
}
