using System;
using System.Text;
using LitJson;

namespace DashFire.Network
{
  public class JsonMessageDispatcher
  {
    public static void Init ()
    {
      if (!s_Inited) {
        s_MessageHandlers = new JsonMessageHandlerDelegate[(int)JsonMessageID.MaxNum];
        s_Inited = true;
      }
    }

    public static bool Inited {
      get {
        return s_Inited;
      }
    }

    public static void RegisterMessageHandler (int id, JsonMessageHandlerDelegate handler)
    {
      if (s_Inited) {
        if (id >= (int)JsonMessageID.Zero && id < (int)JsonMessageID.MaxNum) {
          s_MessageHandlers [id] = handler;
        }
      }
    }

    public static unsafe void HandleNodeMessage (string msgStr)
    {
      if (s_Inited) {
        int id;
        JsonData msg = DecodeJsonMessage(msgStr,out id);
        if (null != msg) {
          HandleNodeMessage(id, msg);
        }
      }
    }

    private static void HandleNodeMessage (int id, JsonData msg)
    {
      if (s_Inited && msg!=null) {
        JsonMessageHandlerDelegate handler = s_MessageHandlers[id];
          if(handler!=null) {
            try {
            handler (id, msg);
          } catch (Exception ex) {
            GfxSystem.GfxLog("[Exception] HandleNodeMessage:{0} throw:{1}\n{2}", id, ex.Message, ex.StackTrace);
          }
        }
      }
    }

    private static unsafe JsonData DecodeJsonMessage (string msgStr, out int id)
    {
      JsonData msg = null;
      id = 0;
      if (s_Inited) {
        try {
          GfxSystem.GfxLog("DecodeJsonMessage:{0}", msgStr);

          int ix = msgStr.IndexOf('|');
          if (ix > 0) {
            id = int.Parse(msgStr.Substring(0, ix));
            msg = JsonMapper.ToObject(msgStr.Substring(ix + 1));
          }
        } catch (Exception ex) {
          GfxSystem.GfxLog("[Exception] DecodeJsonMessage:{0} throw:{1}\n{2}", msgStr, ex.Message, ex.StackTrace);
        }
      }
      return msg;
    }

    public static bool SendMessage(int id, JsonData msg)
    {
      string msgStr = BuildNodeMessage(id, msg);
      return LobbyNetworkSystem.Instance.SendMessage(msgStr);
    }

    private static string BuildNodeMessage (int id, JsonData msg)
    {
      string msgStr = id + "|" + JsonMapper.ToJson(msg);
      return msgStr;
    }

    private static bool s_Inited = false;
    private static JsonMessageHandlerDelegate[] s_MessageHandlers = null;
  }
}

