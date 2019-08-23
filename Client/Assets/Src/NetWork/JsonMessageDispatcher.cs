using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WebSocket4Net;
using Newtonsoft.Json;
using System.Threading;

namespace DashFire.NetWork
{
  public delegate void JsonMessageHandlerDelegate(JsonMessage msg);  
  internal class JsonMessageHandlerInfo
  {
    public Type m_Type = null;
    public JsonMessageHandlerDelegate m_Handler = null;
  }
  class JsonMessageDispatcher
  {
    public void Init()
    {
      if (!s_Inited) {
        s_MessageHandlers = new JsonMessageHandlerInfo[(int)LCJsonMessageID.MaxNum];
        for (int i = (int)LCJsonMessageID.Zero; i < (int)LCJsonMessageID.MaxNum; ++i) {
          s_MessageHandlers[i] = new JsonMessageHandlerInfo();
        }
        s_Inited = true;
      }
    }
    public Type GetMessageType(int id)
    {
      Type type = null;
      if (id >= (int)LCJsonMessageID.Zero && id < (int)LCJsonMessageID.MaxNum) {
        type = s_MessageHandlers[id].m_Type;
      }
      return type;
    }
    public void HandleJsonMessage(JsonMessage msg)
    {
      if (s_Inited && msg != null) {
        JsonMessageHandlerInfo info = s_MessageHandlers[(int)msg.m_ID];
        if (info != null && info.m_Handler != null) {
          info.m_Handler(msg);
        }
      }
    }
    public static void RegisterMessageHandler(int id, Type type, JsonMessageHandlerDelegate handler)
    {
      if (s_Inited) {
        if (id >= (int)LCJsonMessageID.Zero && id < (int)LCJsonMessageID.MaxNum) {
          s_MessageHandlers[id].m_Type = type;
          s_MessageHandlers[id].m_Handler = handler;
        }
      }
    }
    
    private static bool s_Inited = false;
    private static JsonMessageHandlerInfo[] s_MessageHandlers = null;    
  }
}
