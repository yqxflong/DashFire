using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DashFire;
using LitJson;

namespace DashFire.Network
{
  public sealed class LobbyNetworkSystem
  {
    public void Init()
    {
      GfxSystem.EventChannelForLogic.Subscribe<string, string, string>("ge_login_lobby", "lobby", LoginLobby);
      GfxSystem.EventChannelForLogic.Subscribe<int>("ge_select_scene", "lobby", SelectScene);
      GfxSystem.EventChannelForLogic.Subscribe<int>("ge_select_hero", "lobby", SelectHero);
      GfxSystem.EventChannelForLogic.Subscribe("ge_start_game", "lobby", StartGame);

      JsonMessageDispatcher.Init();
      InitMessageHandler();

      m_IsWaitStart = true;
      m_IsLogged = false;
    }
    public void Tick()
    {
      if (GlobalVariables.Instance.IsFullClient)
        return;
      if (!m_IsWaitStart) {
        long curTime = TimeUtility.GetLocalMilliseconds();
        
        if (!IsConnected){
          if (m_LastConnectTime + 10000 < curTime) {
            ConnectIfNotOpen();
          }
        } else {
          if (m_IsLogged && m_LastHeartbeatTime + 60000 < curTime) {
            m_LastHeartbeatTime = curTime;

            SendHeartbeat();
          }
        }
      }
    }
    public void Release()
    {
      if (null != m_WebSocket && m_WebSocket.IsConnected) {
        m_WebSocket.Close();
      }
    }
    public void QuitClient()
    {
    }
    public bool IsConnected
    {
      get
      {
        bool ret = false;
        if(null!=m_WebSocket)
          ret = m_WebSocket.IsConnected;
        return ret;
      }
    }
    public ulong Guid
    {
      get { return m_Guid; }
    }

    internal bool SendMessage(string msgStr)
    {
      bool ret = false;
      if (null!=m_WebSocket && m_WebSocket.IsConnected) {
        m_WebSocket.Send(msgStr);
        GfxSystem.GfxLog("SendToLobby {0}", msgStr);
        ret = true;
      }
      return ret;
    }

    private void ConnectIfNotOpen()
    {
      if (!IsConnected) {
        m_IsLogged = false;
        m_LastConnectTime = TimeUtility.GetLocalMilliseconds();
        GfxSystem.GfxLog("ConnectIfNotOpen at time {0}", m_LastConnectTime);

        m_WebSocket = new SocketIOClient.Client(m_Url);
        m_WebSocket.Opened += OnOpened;
        m_WebSocket.Error += OnError;
        m_WebSocket.SocketConnectionClosed += OnClosed;
        m_WebSocket.Message += OnMessageReceived;
        m_WebSocket.Connect();
      }
    }
    private void SendHeartbeat()
    {
      JsonData msg = new JsonData();
      msg.SetJsonType(JsonType.Object);
      msg.Set("m_Guid", m_Guid);
      SendMessage(JsonMessageID.UserHeartbeat, msg);
    }

    private void LoginLobby(string url, string user, string pass)
    {
      if (GlobalVariables.Instance.IsFullClient) {
        m_Url = url;
        m_User = user;
        m_Pass = pass;

        m_Guid = 1;
        GfxSystem.PublishGfxEvent("ge_start_login", "lobby", "user", 0);
        GfxSystem.PublishGfxEvent("ge_init_userinfo", "lobby", user, user, (int)m_Guid, 100);
      } else {
        if (IsConnected) {
          m_WebSocket.Close();
        }

        m_IsWaitStart = false;
        m_IsLogged = false;

        m_Url = url;
        m_User = user;
        m_Pass = pass;

        ConnectIfNotOpen();
      }
      GfxSystem.GfxLog("LoginLobby {0} {1} {2}", url, user, pass);
    }
    private void SelectScene(int id)
    {
      if (GlobalVariables.Instance.IsFullClient) {
        NetworkSystem.Instance.SceneId = id;
      } else {
        JsonData singlePvpMsg = new JsonData();
        singlePvpMsg.SetJsonType(JsonType.Object);
        singlePvpMsg.Set("m_Guid", m_Guid);
        singlePvpMsg.Set("m_SceneType", id);
        SendMessage(JsonMessageID.SinglePVP, singlePvpMsg);
      }
    }
    private void SelectHero(int id)
    {
      if (GlobalVariables.Instance.IsFullClient) {
        NetworkSystem.Instance.HeroId = id;
        NetworkSystem.Instance.CampId = (int)CampIdEnum.Blue;  
      } else {
        JsonData selectHeroMsg = new JsonData();
        selectHeroMsg.SetJsonType(JsonType.Object);
        selectHeroMsg.Set("m_Guid", m_Guid);
        selectHeroMsg.Set("m_HeroId", id);
        SendMessage(JsonMessageID.SelectHero, selectHeroMsg);
      }
    }
    private void StartGame()
    {
      if (GlobalVariables.Instance.IsFullClient) {
        WorldSystem.Instance.ChangeNextScene(NetworkSystem.Instance.SceneId);
      } else {
        JsonData startGameMsg = new JsonData();
        startGameMsg.Set("m_Guid", m_Guid);
        SendMessage(JsonMessageID.StartGame, startGameMsg);
      }
    }

    private void HandleLoginResult(int id, JsonData msg)
    {
      if (0 == msg.GetInt("m_Result")) {
        GfxSystem.PublishGfxEvent("ge_start_login", "lobby", "user", 0);
      }
      GfxSystem.GfxLog("HandleLoginResult");
    }
    private void HandleUserInfo(int id, JsonData msg)
    {
      m_Guid = msg.GetUlong("m_Guid");
      string account  = msg.GetString("m_Account");
      string nick = msg.GetString("m_Nick");
      int level = msg.GetInt("m_Level");

      m_IsLogged = true;
            
      GfxSystem.PublishGfxEvent("ge_init_userinfo","lobby", account, nick, (int)m_Guid, level);

      GfxSystem.GfxLog("HandleUserInfo");
    }
    private void HandleFindTeamResult(int id, JsonData msg)
    {
      GfxSystem.GfxLog("HandleFindTeamResult");
    }
    private void HandleStartGameResult(int id, JsonData msg)
    {
      int sceneType = msg.GetInt("m_SceneType");
      uint key = msg.GetUint("m_Key");
      string ip = msg.GetString("m_ServerIp");
      int port = msg.GetInt("m_ServerPort");
      int heroId = msg.GetInt("m_HeroId");
      int campId = msg.GetInt("m_CampId");
      int weaponId = msg.GetInt("m_WeaponId");
      int sceneId = msg.GetInt("m_SceneType");

      Data_SceneConfig cfg = SceneConfigProvider.Instance.GetSceneConfigById(sceneType);
      if (null == cfg || cfg.m_Type == (int)SceneTypeEnum.TYPE_PVE) {
        NetworkSystem.Instance.HeroId = heroId;
        NetworkSystem.Instance.CampId = campId;
        NetworkSystem.Instance.SceneId = sceneId;
      } else {
        NetworkSystem.Instance.Start(key, ip, port, heroId, campId, sceneId);
      }
      GameControler.ChangeScene((int)msg["m_SceneType"]);

      GfxSystem.GfxLog("HandleStartGameResult");
    }

    private void InitMessageHandler()
    {
      RegisterMsgHandler(JsonMessageID.LoginResult, HandleLoginResult);
      RegisterMsgHandler(JsonMessageID.UserInfo, HandleUserInfo);
      RegisterMsgHandler(JsonMessageID.FindTeamResult, HandleFindTeamResult);
      RegisterMsgHandler(JsonMessageID.StartGameResult, HandleStartGameResult);
    }
    private void RegisterMsgHandler(JsonMessageID id, JsonMessageHandlerDelegate handler)
    {
      JsonMessageDispatcher.RegisterMessageHandler((int)id, handler);
    }
    private void SendMessage(JsonMessageID id, JsonData msg)
    {
      JsonMessageDispatcher.SendMessage((int)id, msg);
    }

    private void DoConnect()
    {
      if (null != m_WebSocket && m_WebSocket.IsConnected) {
        m_WebSocket.Close();
      }
      m_WebSocket = new SocketIOClient.Client(m_Url);
      m_WebSocket.Opened += OnOpened;
      m_WebSocket.Error += OnError;
      m_WebSocket.SocketConnectionClosed += OnClosed;
      m_WebSocket.Message += OnMessageReceived;
      m_WebSocket.Connect();
    }
    private void OnOpened(object sender, EventArgs e)
    {
      JsonData loginMsg = new JsonData();
      loginMsg["m_Account"] = m_User;
      loginMsg["m_Passwd"] = m_Pass;
      loginMsg["m_Ip"] = "127.0.0.1";
      loginMsg["m_MacAddr"] = "FFFFFFFFFFFF";
      SendMessage(JsonMessageID.Login, loginMsg);

      GfxSystem.GfxLog("LobbyConnect opened.");
    }
    private void OnError(object sender, SocketIOClient.ErrorEventArgs e)
    {
      if (null == e.Exception) {
        GfxSystem.GfxLog("LobbyNetworkSystem.OnError:{0}", e.Message);
      } else {
        GfxSystem.GfxLog("LobbyNetworkSystem.OnError:{0} Exception:{1}\n{2}", e.Message, e.Exception.Message, e.Exception.StackTrace);
      }
    }
    private void OnClosed(object sender, EventArgs e)
    {
    }
    private void OnMessageReceived(object sender, SocketIOClient.MessageEventArgs e)
    {
      SocketIOClient.Messages.TextMessage msg = e.Message as SocketIOClient.Messages.TextMessage;
      if (null != msg) {
        JsonMessageDispatcher.HandleNodeMessage(msg.MessageText);
      }
    }
    
    private bool m_IsWaitStart = true;
    private bool m_IsLogged = false;
    private long m_LastConnectTime = 0;
    private long m_LastHeartbeatTime = 0;

    private string m_Url;
    private string m_User;
    private string m_Pass;
    private ulong m_Guid;

    private SocketIOClient.Client m_WebSocket;

    public static LobbyNetworkSystem Instance
    {
      get
      {
        return s_Instance;
      }
    }
    private static LobbyNetworkSystem s_Instance = new LobbyNetworkSystem();
  }
}
