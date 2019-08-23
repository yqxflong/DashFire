using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Text.RegularExpressions;
using WebSocket4Net;
using SuperSocket.ClientEngine;
using Newtonsoft.Json;
using DashFire.NetWork;

namespace WebSocketClient
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
  
  public class TestClient
  {
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
    private static string HEARTBEAT = "2::";

    private long m_Guid = 250;

    public void Run()
    {
      //m_Address = "http://127.0.0.1:9001/";
      //m_Address = "http://127.0.0.1:8080/";
      //m_Address = "http://localhost:8080/";
      m_Address = "http://localhost:9001/";
      m_Uri = new Uri(m_Address);
      this.Connect();
      
      do 
      {
        string input = Console.ReadLine().Trim();
        if (input == string.Empty)
        {
          continue;
        }
        if (input != "1")
        {
          CLJsonMsgLogin loginMsg = new CLJsonMsgLogin();
          loginMsg.m_Account = input;
          loginMsg.m_Passwd = input;
          loginMsg.m_IP = "127.0.0.1";
          loginMsg.m_MacAddr = "6666666";
          SendTextMessage(loginMsg);
          //string msg = "5:::{\"name\":\"say\",\"args\":[{\"w\":\"" + input + "\"}]}";
          //SendMessage(msg);
        }
        else
        {
          break;
        }
      } while (true);     
      CloseWebSocketClient();
      Console.ReadLine();
    }

    private void Connect()
    {
      if (!(this.m_ReadyState == WebSocketState.Connecting || this.m_ReadyState == WebSocketState.Open))
      {
        try
        {
          m_Handshake = RequestHandshake(m_Uri);
          if (m_Handshake == null || m_Handshake.SID.Trim() == string.Empty || m_Handshake.HadError)
          {
            Console.WriteLine("Failed to request socketio handshake ");
          }
          else
          {
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
            Console.WriteLine("Try to Open WebSocket Client ... {0}", DateTime.Now.ToString());
          }
        }
        catch (Exception ex)
        {
          Console.WriteLine("Websocket connect error : " + ex.ToString());
        }
      }
    }
    private void webSocketClient_Opened(object sender, EventArgs e)
    {
      Console.WriteLine("Websocket Opened");      
    }
    private void webSocketClient_Error(object sender, ErrorEventArgs e)
    {
      Console.WriteLine("Websocket Error : " + e.Exception.ToString());
    }
    private void webSocketClient_Closed(object sender, EventArgs e)
    {
      Console.WriteLine("Websocket Closed ");
      CloseWebSocketClient();
    }
    private void webSocketClient_MessageReceived(object sender, MessageReceivedEventArgs e)
    {
      //收到消息后进行处理
      string rawMsgString = e.Message;
      Console.WriteLine("==> Websocket Recived Message: {0}", rawMsgString);
      try
      {
        WebSocketMessageTypes websocketMsgType = GetWebSocketMessageType(rawMsgString);
        if (websocketMsgType == WebSocketMessageTypes.Event)
        {          
        }
        else if (websocketMsgType == WebSocketMessageTypes.Message)
        {
          Console.WriteLine("@@@@@@@@@@@@@@@ WebSocket Client Receive Text Message:" + rawMsgString);
        }
        else if (websocketMsgType == WebSocketMessageTypes.Heartbeat)
        {
          //向node.js回复心跳消息
          if (m_ReadyState == WebSocketState.Open)
          {            
            SendMessage(HEARTBEAT);
          }
        }
      }
      catch (System.Exception ex)
      {
        Console.WriteLine("Recived Message Error:{0} ", ex);
      }
    }
    private void CloseWebSocketClient()
    {
      if (this.m_WebsocketClient != null)
      {
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
          catch { Console.WriteLine("exception raised trying to close websocket: can safely ignore, socket is being closed"); }
        }
        this.m_WebsocketClient = null;
      }
    }        

    private static Regex reMessageType = new Regex("^[0-8]{1}:", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static WebSocketMessageTypes GetWebSocketMessageType(string rawMessage)
    {
      if (reMessageType.IsMatch(rawMessage))
      {
        char id = rawMessage.First();
        switch (id)
        {
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
      }
      else
      {
        return WebSocketMessageTypes.Undefined;
      }
    }


    public void SendTextMessage(JsonMessage msg)
    {
      try {
        if (msg != null) {
          string jsonStr = JsonConvert.SerializeObject(msg);
          string bodyStr = string.Format("{0}|{1}|{2}", msg.m_ID, m_Guid, jsonStr); ;
          string msgStr = "3:::" + bodyStr;
          this.SendMessage(msgStr);
        }
      }
      catch (System.Exception ex) {
        Console.WriteLine("Build Json Message String Error: {0}", ex);
      }
    }

    private void SendMessage(string msgStr)
    {
      Console.WriteLine("Websocket Send Message ==> {0}", msgStr);
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
        if (string.IsNullOrEmpty(value))
        {
          errorText = "Did not receive handshake string from server";
        }
      }
      if (string.IsNullOrEmpty(errorText))
      {
        handshake = CLHandshake.LoadFromString(value);
      }
      else
      {
        handshake = new CLHandshake();
        handshake.ErrorMessage = errorText;
      }
      return handshake;
    }            
  }
}
