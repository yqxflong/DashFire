using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace DashFire.NetWork
{
  //WebSocket消息类型，主要用到Event类型
  public enum WebSocketMessageTypes
  {
    Undefined = -1,   // undefined message type
    Disconnect = 0,   // Signals disconnection. If no endpoint is specified, disconnects the entire socket.
    Connect = 1,      // Only used for multiple sockets. Signals a connection to the endpoint. Once the server receives it, it's echoed back to the client.
    Heartbeat = 2,    // Heartbeat
    Message = 3,      // A regular message
    JSONMessage = 4,  // A JSON message
    Event = 5,        // An event is like a JSON message, but has mandatory name and args fields.
    ACK = 6,          // An acknowledgment contains the message id as the message data. If a + sign follows the message id, it's treated as an event message packet.
    Error = 7,        // Error
    Noop = 8          // No operation
  }

  public class JsonMessage
  {
    [JsonIgnore]
    public int m_ID = -1;
    protected JsonMessage(int id)
    {
      m_ID = id;
    }    
  }

  // 客户端接收服务器的消息类型
  public enum LCJsonMessageID : int
  {
    Zero = 0,
    LoginResult = 2,
    UserInfo = 4,
    //TeamResult = 7,
    StartGameResult = 10,
    //SyncHero = 16,
    MaxNum
  }
  public class LCJsonMsgZero : JsonMessage
  {
    public string m_Zero = "";
    public LCJsonMsgZero()
      : base((int)LCJsonMessageID.Zero)
    { }
  }
  public enum LoginResult : int
  {
    LOGIN_SUCCESS = 0,
    LOGIN_FAIL,
    LOGIN_USER_ERROR,
    LOGIN_PWD_ERROR,
    ERROR
  }
  public class LCJsonMsgLoginResult : JsonMessage
  {
    public string m_Account = "";
    public int m_Result = (int)LoginResult.ERROR;
    public LCJsonMsgLoginResult()
      : base((int)LCJsonMessageID.LoginResult)
    { }
  }
  public class LCJsonMsgUserInfo : JsonMessage
  {
    public string m_Account = "";
    public string m_Nick = "";
    public long m_Guid = 0;
    public int m_Level = 1;
    public string m_Sign = "";
    public LCJsonMsgUserInfo()
      : base((int)LCJsonMessageID.UserInfo)
    { }
  }
  public class LCJsonMsgStartGameResult : JsonMessage
  {
    public ulong m_Guid = 0;
    public string m_ServerIp = "";
    public uint m_ServerPort = 0;
    public uint m_Key = 0;
    public int m_HeroId = 0;
    public int m_WeaponId = 0;
    public int m_CampId = 0;
    public int m_SceneType = 0;
    public LCJsonMsgStartGameResult()
      : base((int)LCJsonMessageID.StartGameResult)
    { }
  }


  //=======================================================================
  // 客户端向服务器发送的消息类型
  public enum CLJsonMessageID : int
  {
    Login = 1,
    Logout = 3, 
    SelectHero = 8,
	  StartGame = 9,
    UserHeartbeat = 15,
    SinglePVP = 52,
    MaxNum
  }  
  public class CLJsonMsgLogin : JsonMessage
  {
    public string m_Account = "";
    public string m_Passwd = "";
    public string m_IP = "";
    public string m_MacAddr = "";
    public CLJsonMsgLogin()
      : base((int)CLJsonMessageID.Login)
    { }
  }
  public class CLJsonMsgLogout : JsonMessage
  {
    public long m_Guid = 0;
    public CLJsonMsgLogout()
      : base((int)CLJsonMessageID.Logout)
    { }
  }
  public class CLJsonMsgSelectHero : JsonMessage 
  {
    public long m_Guid = 0;
    public int m_HeroId = 0;
    public CLJsonMsgSelectHero()
      : base((int)CLJsonMessageID.SelectHero)
    { }
  }
  public class CLJsonMsgStartGame : JsonMessage
  {
    public long m_Guid = 0;
    public CLJsonMsgStartGame()
      : base((int)CLJsonMessageID.StartGame)
    { }
  }  
  public class CLJsonMsgHeartbeat : JsonMessage
  {
    public long m_Guid = 0;
    public CLJsonMsgHeartbeat()
      : base((int)CLJsonMessageID.UserHeartbeat)
    { }
  }
  public class CLJsonMsgSinglePVP : JsonMessage
  {
    public long m_Guid = 0;
    public int m_SceneType = 0;
    public CLJsonMsgSinglePVP()
      : base((int)CLJsonMessageID.SinglePVP)
    { }
  }  
}
