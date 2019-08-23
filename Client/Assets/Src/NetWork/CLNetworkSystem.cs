using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Threading;
using WebSocket4Net;
using Newtonsoft.Json;
using System.Text.RegularExpressions;
using SuperSocket.ClientEngine;

namespace DashFire.NetWork
{
  
  public class CLNetworkSystem
  {
    #region Singleton
    private static CLNetworkSystem s_Instance = new CLNetworkSystem();
    public static CLNetworkSystem Instance
    {
      get { return s_Instance; }
    }
    #endregion

    public long Guid
    {
      get { return m_Guid; }
      set { m_Guid = value; }
    }
    public void Init()
    {
      //初始化消息处理器
      m_JsonMsgDispatcher.Init();
      InitMessageHandler();

      m_IsWaitStart = true;
      m_IsLogged = false;
      m_IsQuited = false;

      GfxSystem.EventChannelForLogic.Subscribe<string, int, string, string>("ge_login_lobby", "lobby", LoginLobby);
      GfxSystem.EventChannelForLogic.Subscribe("ge_select_singlepvp", "lobby", SelectSinglePvp);
      GfxSystem.EventChannelForLogic.Subscribe<int>("ge_select_hero", "lobby", SelectHero);
      GfxSystem.EventChannelForLogic.Subscribe("ge_start_game", "lobby", StartGame);
      GfxSystem.EventChannelForLogic.Subscribe<int>("ge_select_scene", "lobby", SelectScene);
    }
    public void Tick()
    {
      if (!m_IsWaitStart) {
        long curTime = TimeUtility.GetLocalMilliseconds();
        if (m_LastConnectTime + 10000 < curTime) {
          ConnectIfNotOpen();
        }
        if (m_LastHeartbeatTime + USERINTERVAL * 1000 < curTime) {
          m_LastHeartbeatTime = curTime;
          SendHeartbeat();
        }
      }
    }
    public void QuitClient()
    {
      CloseWebSocketClient();
      m_IsQuited = true;
    }
    public void SendEventMessage(JsonMessage msg)
    {
      try {
        if (msg != null) {
          string s = JsonConvert.SerializeObject(msg);
          string msgStr = "5:::{\"name\":\"" + msg.m_ID.ToString() + "\",\"args\":[" + s + "]}";
          this.SendMessage(msgStr);
        }
      } catch (System.Exception ex) {
        GfxSystem.GfxLog("Build Json Message String Error: {0}", ex);
      }
    }
    public void SendTextMessage(JsonMessage msg)
    {
      try
      {
        if (msg != null)
        {
          string jsonStr = JsonConvert.SerializeObject(msg);
          string bodyStr = string.Format("{0}|{1}|{2}", msg.m_ID, m_Guid, jsonStr); ;
          string msgStr = "3:::" + bodyStr;
          this.SendMessage(msgStr);
        }
      }
      catch (System.Exception ex)
      {
        GfxSystem.GfxLog("Build Json Message String Error: {0}", ex);
      }
    }

    private void LoginLobby(string ip, int port, string user, string passwd)
    {
      GfxSystem.GfxLog("LoginLobby:{0}:{1} {2}/{3}", ip, port, user, passwd);      
      m_Address = string.Format("http://{0}:{1}", ip, port);
      m_Uri = new Uri(m_Address);
      ConnectIfNotOpen();
      m_Account = user;
      m_Password = passwd;
      m_IsWaitStart = false;
    }
    private void SelectSinglePvp()
    {
      CLJsonMsgSinglePVP singlePvp = new CLJsonMsgSinglePVP();
      singlePvp.m_Guid = m_Guid;
      singlePvp.m_SceneType = 4;
      SendTextMessage(singlePvp);
    }
    private void SelectHero(int heroId)
    {
      CLJsonMsgSelectHero selectHero = new CLJsonMsgSelectHero();
      selectHero.m_Guid = m_Guid;
      selectHero.m_HeroId = heroId;
      SendTextMessage(selectHero); 
    }
    private void StartGame()
    {
      CLJsonMsgStartGame startGame = new CLJsonMsgStartGame();
      startGame.m_Guid = m_Guid;
      SendTextMessage(startGame);
    }

    private void SelectScene(int sceneId)
    {
      CLJsonMsgSinglePVP sceneType = new CLJsonMsgSinglePVP();
      sceneType.m_Guid = m_Guid;
      sceneType.m_SceneType = sceneId;
      SendTextMessage(sceneType);
    }
    private void ConnectIfNotOpen()
    {  
      if (!(this.m_ReadyState == WebSocketState.Connecting || this.m_ReadyState == WebSocketState.Open)) {
        try {
          m_Handshake = RequestHandshake(m_Uri);
          if (m_Handshake == null || m_Handshake.SID.Trim() == string.Empty || m_Handshake.HadError) {
            GfxSystem.GfxLog("Failed to request socketio handshake ");
          } else {
            string wsScheme = (m_Uri.Scheme == Uri.UriSchemeHttps ? "wss" : "ws");
            m_WebsocketClient = new WebSocket(
              string.Format("{0}://{1}:{2}/socket.io/1/websocket/{3}", wsScheme, m_Uri.Host, m_Uri.Port, m_Handshake.SID),
              string.Empty,
              m_SocketVersion);
            m_WebsocketClient.EnableAutoSendPing = false;
            m_WebsocketClient.add_Opened(new EventHandler<EventArgs>(webSocketClient_Opened));
            m_WebsocketClient.add_Closed(new EventHandler<EventArgs>(webSocketClient_Closed));
            m_WebsocketClient.add_Error(new EventHandler<ErrorEventArgs>(webSocketClient_Error));
            m_WebsocketClient.add_MessageReceived(new EventHandler<MessageReceivedEventArgs>(webSocketClient_MessageReceived));
            /*
            m_WebsocketClient.Opened += this.webSocketClient_Opened;
            m_WebsocketClient.MessageReceived += this.webSocketClient_MessageReceived;
            m_WebsocketClient.Closed += webSocketClient_Closed;
            m_WebsocketClient.Error += this.webSocketClient_Error;
            */
            m_WebsocketClient.Open();
            m_LastConnectTime = TimeUtility.GetLocalMilliseconds();
            GfxSystem.GfxLog("Try to Open ... {0}", DateTime.Now.ToString());
          }
        } catch (Exception ex) {
          GfxSystem.GfxLog("Websocket connect error : " + ex.ToString());
        }
      }   
    }  
    private void webSocketClient_Opened(object sender, EventArgs e)
    {
      GfxSystem.GfxLog("Websocket Opened");
      if (!m_IsLogged) {        
        CLJsonMsgLogin loginMsg = new CLJsonMsgLogin();
        loginMsg.m_Account = m_Account;
        loginMsg.m_Passwd = m_Password;
        loginMsg.m_IP = "127.0.0.1";
        loginMsg.m_MacAddr = "6666666";        
        SendTextMessage(loginMsg);
        m_Password = "";
        m_IsLogged = true;
      }
    }    
    private void webSocketClient_Error(object sender, EventArgs e)
    {      
      GfxSystem.GfxLog("Websocket Error : " + e.ToString());
    }
    private void webSocketClient_Closed(object sender, EventArgs e)
    {
      GfxSystem.GfxLog("Websocket Closed ");
      CloseWebSocketClient();
    }        
    private void webSocketClient_MessageReceived(object sender, MessageReceivedEventArgs e)
    {      
      //收到消息后进行处理
      string rawMsgString = e.Message;
      GfxSystem.GfxLog("==> Websocket Recived Message: {0}", rawMsgString);
      LogSystem.Info("==> Websocket Recived Message: {0}", rawMsgString);
      try
      {
        WebSocketMessageTypes websocketMsgType = GetWebSocketMessageType(rawMsgString);        
        if (websocketMsgType == WebSocketMessageTypes.Event) {
          //处理事件消息
          JsonMessage msg = null;
          string jsonMsgString = "";
          string eventName = HandleRawMsg(rawMsgString, out jsonMsgString);
          int msgId = int.Parse(eventName);
          Type type = m_JsonMsgDispatcher.GetMessageType(msgId);
          msg = JsonConvert.DeserializeObject(jsonMsgString, type) as JsonMessage;          
          if (msg != null) {
            msg.m_ID = msgId;
            m_JsonMsgDispatcher.HandleJsonMessage(msg);
          }
        } else if (websocketMsgType == WebSocketMessageTypes.Message) {
          GfxSystem.GfxLog("!!!!!! Text Message: {0}", rawMsgString);
          int msgId = 0;
          JsonMessage msg = null;
          string[] rawArgs = rawMsgString.Split(SPLITTER_VERTICAL, 2);
          if (rawArgs.Length == 2)
          {
            int id;
            if (int.TryParse(rawArgs[0], out id))
            {
              msgId = id;
            }
            GfxSystem.GfxLog("!!!!!! Client recive message id : {0}", msgId);
            Type type = m_JsonMsgDispatcher.GetMessageType(msgId);
            msg = JsonConvert.DeserializeObject(rawArgs[1], type) as JsonMessage;
            if (msg != null)
            {
              GfxSystem.GfxLog("!!!!!! Client recive message: {0}", msg);
            }
          }
        } else if (websocketMsgType == WebSocketMessageTypes.Heartbeat) {
          //向node.js回复心跳消息
          AnswerHeartbeat();
        }      
      }
      catch (System.Exception ex)
      {
        GfxSystem.GfxLog("Recived Message Error:{0} " ,ex); 
      }       
    }

    private void InitMessageHandler()
    {
      JsonMessageDispatcher.RegisterMessageHandler((int)LCJsonMessageID.Zero, typeof(LCJsonMsgZero), this.HandleZero);
      JsonMessageDispatcher.RegisterMessageHandler((int)LCJsonMessageID.LoginResult, typeof(LCJsonMsgLoginResult), this.HandleLoginResult);
      JsonMessageDispatcher.RegisterMessageHandler((int)LCJsonMessageID.UserInfo, typeof(LCJsonMsgUserInfo), this.HandleUserInfo);
      JsonMessageDispatcher.RegisterMessageHandler((int)LCJsonMessageID.StartGameResult, typeof(LCJsonMsgStartGameResult), this.HandleStartGameResult);
    }
    private void HandleZero(JsonMessage msg)
    {
    }
    private void HandleLoginResult(JsonMessage msg)
    {
      LCJsonMsgLoginResult lcMsg = msg as LCJsonMsgLoginResult;   
      if (lcMsg != null) {
        GfxSystem.PublishGfxEvent("start_login", "ui", lcMsg.m_Account, lcMsg.m_Result);
      }      
    }
    private void HandleUserInfo(JsonMessage msg)
    {
      LCJsonMsgUserInfo lcMsg = msg as LCJsonMsgUserInfo;      
      if (lcMsg != null) {
        m_Guid = lcMsg.m_Guid;
        GfxSystem.PublishGfxEvent("init_userinfo", "ui", 
          lcMsg.m_Account, lcMsg.m_Nick, lcMsg.m_Guid, lcMsg.m_Level);
      }
    }
    private void HandleStartGameResult(JsonMessage msg)
    {
      LCJsonMsgStartGameResult lcMsg = msg as LCJsonMsgStartGameResult;
      if (lcMsg != null) {
        GfxSystem.GfxLog("!!!!! Start Game Args:{0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}",
          lcMsg.m_Guid, lcMsg.m_ServerIp, lcMsg.m_ServerPort, lcMsg.m_Key, lcMsg.m_CampId, lcMsg.m_HeroId, lcMsg.m_WeaponId, lcMsg.m_SceneType);

        GfxSystem.LoadScene("Test");
        NetWorkSystem.Instance.Start(lcMsg.m_Key, lcMsg.m_ServerIp, (int)lcMsg.m_ServerPort, lcMsg.m_HeroId, lcMsg.m_CampId);
        GameControler.ChangeScene(lcMsg.m_SceneType);
      }
    }
   
    private void SendHeartbeat()
    {
      CLJsonMsgHeartbeat msg = new CLJsonMsgHeartbeat();
      msg.m_Guid = m_Guid;
      SendTextMessage(msg);
      GfxSystem.GfxLog("Send User Heartbeat : {0} , Time : {1}",m_UserHeartbeatCount++, DateTime.Now.ToString());
    }
    private void AnswerHeartbeat()
    {
      if (m_ReadyState == WebSocketState.Open) {
        //SocketIO心跳消息
        SendMessage(HEARTBEAT);
      }
    }
    private void CloseWebSocketClient()
    {
      if (this.m_WebsocketClient != null) {
        // unwire events
        m_WebsocketClient.remove_Opened(new EventHandler<EventArgs>(webSocketClient_Opened));
        m_WebsocketClient.remove_Closed(new EventHandler<EventArgs>(webSocketClient_Closed));
        m_WebsocketClient.remove_Error(new EventHandler<ErrorEventArgs>(webSocketClient_Error));
        m_WebsocketClient.remove_MessageReceived(new EventHandler<MessageReceivedEventArgs>(webSocketClient_MessageReceived));
        /*
        this.m_WebsocketClient.Closed -= this.webSocketClient_Closed;
        this.m_WebsocketClient.MessageReceived -= webSocketClient_MessageReceived;
        this.m_WebsocketClient.Error -= webSocketClient_Error;
        this.m_WebsocketClient.Opened -= this.webSocketClient_Opened;
        */
        if (this.m_WebsocketClient.State == WebSocketState.Connecting || this.m_WebsocketClient.State == WebSocketState.Open)
        {
          try { this.m_WebsocketClient.Close(); }
          catch { GfxSystem.GfxLog("exception raised trying to close websocket: can safely ignore, socket is being closed"); }
        }
        this.m_WebsocketClient = null;
      }
    }        
    private string HandleRawMsg(string rawMsgString, out string jsonMsgString)
    {
      string eventName = "";
      jsonMsgString = "";
      //socket.io消息原始格式：
      //  '5:' [message id ('+')] ':' [message endpoint] ':' [json encoded event]
      //   5:1::{"a":"b"}
      int ackId = -1;
      string endPoint = "";
      string socketioMsgString = "";
      try
      {
        string[] rawArgs = rawMsgString.Split(SPLITTER_COLON, 4); // limit the number of pieces
        if (rawArgs.Length == 4) {
          int id;
          if (int.TryParse(rawArgs[1], out id))
            ackId = id;
          endPoint = rawArgs[2];
          socketioMsgString = rawArgs[3];
          if (!string.IsNullOrEmpty(socketioMsgString) &&
            socketioMsgString.Contains("name") &&
            socketioMsgString.Contains("args"))
          {
            //处理socketio Json Message
            // {\"name\":\"eventName\",\"args\":[{\"name\":\"\"value"\"}]}
            string[] jsonArgs = socketioMsgString.Split(SPLITTER_COLON, 3);
            if (jsonArgs.Length == 3) {
              //解析出事件名称
              string[] tmpArgs = jsonArgs[1].Split(SPLITTER_QUOTE, 3);
              eventName = tmpArgs[1];
              //解析出EventMessage Json字符串              
              jsonMsgString = jsonArgs[2].Substring(1, jsonArgs[2].Length - 3);
            }
          }
        }
      }
      catch (Exception ex)
      {
        Console.WriteLine("Handle socketio raw message error :{0}", ex);
      }
      return eventName;
    }
    private string HandleRawTextMsg(string rawMsgString, out string jsonMsgString)
    {
      string eventName = "";
      jsonMsgString = "";
      //socket.io message类型消息原始格式：
      //  '3:' [message id ('+')] ':' [message endpoint] ':' [json encoded event]
      //   3:1::{"a":"b"}
      int ackId = -1;
      string endPoint = "";
      string socketioMsgString = "";
      try
      {
        string[] rawArgs = rawMsgString.Split(SPLITTER_COLON, 4); // limit the number of pieces
        if (rawArgs.Length == 4)
        {
          int id;
          if (int.TryParse(rawArgs[1], out id))
            ackId = id;
          endPoint = rawArgs[2];
          socketioMsgString = rawArgs[3];
          if (!string.IsNullOrEmpty(socketioMsgString))
          {            
          }
        }
      }
      catch (Exception ex)
      {
        Console.WriteLine("Handle socketio raw message error :{0}", ex);
      }
      return eventName;
    }
    private static Regex reMessageType = new Regex("^[0-8]{1}:", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static WebSocketMessageTypes GetWebSocketMessageType(string rawMessage)
		{
			if (reMessageType.IsMatch(rawMessage)) {
				char id = rawMessage.First();
				switch (id)	{
					case '0':
						return WebSocketMessageTypes.Disconnect;
					case '1':
						return WebSocketMessageTypes.Connect;
					case '2':
						return WebSocketMessageTypes.Heartbeat;
					case '3':
						return WebSocketMessageTypes.Message;
					case '4':
						return WebSocketMessageTypes.JSONMessage;
					case '5':
						return WebSocketMessageTypes.Event;
					case '6':
						return WebSocketMessageTypes.ACK;
					case '7':
						return WebSocketMessageTypes.Error;
					case '8':
						return WebSocketMessageTypes.Noop;
					default:
            return WebSocketMessageTypes.Undefined;
				}
			}	else {
				return WebSocketMessageTypes.Undefined;
			}
		}
    
    private void SendMessage(string msgStr)
    {
      GfxSystem.GfxLog("Websocket Send Message ==> {0}", msgStr);
      m_WebsocketClient.Send(msgStr);      
    }     
    private CLHandshake RequestHandshake(Uri uri)
    {
      string value = string.Empty;
      string errorText = string.Empty;
      CLHandshake handshake = null;
      using (WebClient client = new WebClient())
      {
        value = client.DownloadString(string.Format("{0}://{1}:{2}/socket.io/1/{3}", uri.Scheme, uri.Host, uri.Port, uri.Query));
        if (string.IsNullOrEmpty(value)) {
          errorText = "Did not receive handshake string from server";
        }       
      }
      if (string.IsNullOrEmpty(errorText)) {
        handshake = CLHandshake.LoadFromString(value);
      } else {
        handshake = new CLHandshake();
        handshake.ErrorMessage = errorText;
      }
      return handshake;
    }            

    private WebSocket m_WebsocketClient;
    private CLHandshake m_Handshake;
    private string m_Address;
    private Uri m_Uri;
    private WebSocketVersion m_SocketVersion = WebSocketVersion.Rfc6455;      
    private WebSocketState m_ReadyState
    {
      get
      {
        if (this.m_WebsocketClient != null)
          return this.m_WebsocketClient.State;
        else
          return WebSocketState.None;
      }
    }

    private bool m_IsWaitStart = true;
    private bool m_IsQuited = false;
    private bool m_IsLogged = false;
    private string m_Account;
    private string m_Password;

    private JsonMessageDispatcher m_JsonMsgDispatcher = new JsonMessageDispatcher();

    private long m_Guid = 0;
    private static char[] SPLITTER_VERTICAL = new char[] { '|' };
    private static char[] SPLITTER_COLON = new char[] { ':' };
    private static char[] SPLITTER_QUOTE = new char[] { '\"' };
    private static string HEARTBEAT = "2::";

    private int m_UserHeartbeatCount = 0;
    private const int USERINTERVAL = 15;        //发向Lobby的玩家心跳的时间间隔 单位：秒
    private long m_LastConnectTime = 0;
    private long m_LastHeartbeatTime = 0;
  }
}
