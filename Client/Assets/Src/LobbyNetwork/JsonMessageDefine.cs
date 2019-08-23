using System;
using LitJson;

namespace DashFire.Network
{
  public enum JsonMessageID : int
  {
    Zero = 0,
    Login,
    LoginResult,
    Logout,
    UserInfo,
    FindTeam,
    CancelFindTeam,
    FindTeamResult,
    SelectHero,
    StartGame,
    StartGameResult,
    NodeJsRegister,
    NodeJsRegisterResult,
    SelectWeapon,
    QuitClient,
    UserHeartbeat,
    SyncHero,
    SyncWeapon,
    SyncPrepared,
    SyncQuitClient,
    RequestRoomList,
    SyncRoomList,
    CreateRoom,
    CreateRoomResult,
    JoinRoom,
    SyncJoinRoom,
    LeaveRoom,
    SyncLeaveRoom,
    KickUser,
    SyncRoomUsers,
    SelectCamp,
    SyncCamp,
    CreateNick,
    CreateNickResult,
    AddFriend,
    AddFriendResult,
    ConfirmFriend,
    DelFriend,
    DelFriendResult,
    GroupRequestTeam,
    FriendList,
    SyncFriendList,
    InviteInform,
    InvitePlayer,
    SyncCancelFindTeam,
    JoinGroup,
    SyncGroupUsers,
    AddBlack,
    AddBlackResult,
    DelBlack,
    RefuseFriend,
    SyncTeamingState,
    SinglePVP,
    UserQuitRoom,
    UserReconnectRoom,
    RequestSceneList,
    SyncSceneList,
    RequestUserWeaponInfo,
    SyncUserWeaponInfo,
    SelectSubWeapons,
    RequestUserCurWeaponInfo,
    SyncUserCurWeaponInfo,
    RequestBagItems,
    SyncBagItems,
    UseBagItem,
    UseBagItemResult,
    SyncPvpBattleResult,
    CYPConfirm,
    CYPConfirmResult,
    AccountRepeatLogin,
    DelBagItem,
    DelBagItemResult,
    AddMachine,
    MaxNum
  }

  public delegate void JsonMessageHandlerDelegate(int id, JsonData msg);

  public static class JsonDataExtensions
  {
    //--------------------------------------------------------------------------
    public static bool AsBoolean(this JsonData data)
    {
      bool ret = false;
      if (data.IsInt) {
        ret = (int)data != 0;
      } else if (data.IsLong) {
        ret = (long)data != 0;
      } else if (data.IsDouble) {
        ret = (float)data != 0;
      } else if (data.IsBoolean) {
        ret = (bool)data;
      } else if (data.IsString) {
        ret = long.Parse((string)data) != 0;
      }
      return ret;
    }
    public static int AsInt(this JsonData data)
    {
      int ret = 0;
      if (data.IsInt) {
        ret = (int)data;
      } else if (data.IsLong) {
        ret = (int)(long)data;
      } else if (data.IsDouble) {
        ret = (int)(float)(float)data;
      } else if (data.IsBoolean) {
        ret = ((bool)data ? 1 : 0);
      } else if (data.IsString) {
        ret = int.Parse((string)data);
      }
      return ret;
    }
    public static long AsLong(this JsonData data)
    {
      long ret = 0;
      if (data.IsInt) {
        ret = (int)data;
      } else if (data.IsLong) {
        ret = (long)data;
      } else if (data.IsDouble) {
        ret = (int)(float)(float)data;
      } else if (data.IsBoolean) {
        ret = ((bool)data ? 1 : 0);
      } else if (data.IsString) {
        ret = long.Parse((string)data);
      }
      return ret;
    }
    public static float AsDouble(this JsonData data)
    {
      float ret = 0;
      if (data.IsInt) {
        ret = (int)data;
      } else if (data.IsLong) {
        ret = (long)data;
      } else if (data.IsDouble) {
        ret = (float)data;
      } else if (data.IsBoolean) {
        ret = ((bool)data ? 1 : 0);
      } else if (data.IsString) {
        ret = float.Parse((string)data);
      }
      return ret;
    }
    public static string AsString(this JsonData data)
    {
      string ret = null;
      if (data.IsInt) {
        ret = ((int)data).ToString();
      } else if (data.IsLong) {
        ret = ((long)data).ToString();
      } else if (data.IsDouble) {
        ret = ((int)(float)(float)data).ToString();
      } else if (data.IsBoolean) {
        ret = ((bool)data ? 1 : 0).ToString();
      } else if (data.IsString) {
        ret = (string)data;
      }
      return ret;
    }
    public static uint AsUint(this JsonData data)
    {
      return (uint)data.AsInt();
    }
    public static ulong AsUlong(this JsonData data)
    {
      return (ulong)data.AsLong();
    }
    public static float AsFloat(this JsonData data)
    {
      return (float)data.AsDouble();
    }
    //--------------------------------------------------------------------------
    public static bool Get(this JsonData data, ref bool val)
    {
      bool ret = false;
      if (data.IsBoolean) {
        ret = true;
        val = (bool)data;
      }
      return ret;
    }
    public static bool Get(this JsonData data, ref int val)
    {
      bool ret = false;
      if (data.IsDouble || data.IsInt || data.IsLong) {
        ret = true;
        val = data.AsInt();
      }
      return ret;
    }
    public static bool Get(this JsonData data, ref long val)
    {
      bool ret = false;
      if (data.IsDouble || data.IsInt || data.IsLong) {
        ret = true;
        val = data.AsLong();
      }
      return ret;
    }
    public static bool Get(this JsonData data, ref double val)
    {
      bool ret = false;
      if (data.IsDouble || data.IsInt || data.IsLong) {
        ret = true;
        val = data.AsDouble();
      }
      return ret;
    }
    public static bool Get(this JsonData data, ref string val)
    {
      bool ret = false;
      if (data.IsString) {
        ret = true;
        val = (string)data;
      }
      return ret;
    }
    public static bool Get(this JsonData data, ref uint val)
    {
      int temp = 0;
      bool ret = Get(data, ref temp);
      if (ret) {
        val = (uint)temp;
      }
      return ret;
    }
    public static bool Get(this JsonData data, ref ulong val)
    {
      long temp = 0;
      bool ret = Get(data, ref temp);
      if (ret) {
        val = (ulong)temp;
      }
      return ret;
    }
    public static bool Get(this JsonData data, ref float val)
    {
      double temp = 0;
      bool ret = Get(data, ref temp);
      if (ret) {
        val = (float)temp;
      }
      return ret;
    }
    //--------------------------------------------------------------------------
    public static bool Get(this JsonData data, string key, ref bool val)
    {
      bool ret = false;
      if (data.IsObject) {
        ret = data[key].Get(ref val);
      }
      return ret;
    }
    public static bool Get(this JsonData data, string key, ref int val)
    {
      bool ret = false;
      if (data.IsObject) {
        ret = data[key].Get(ref val);
      }
      return ret;
    }
    public static bool Get(this JsonData data, string key, ref long val)
    {
      bool ret = false;
      if (data.IsObject) {
        ret = data[key].Get(ref val);
      }
      return ret;
    }
    public static bool Get(this JsonData data, string key, ref double val)
    {
      bool ret = false;
      if (data.IsObject) {
        ret = data[key].Get(ref val);
      }
      return ret;
    }
    public static bool Get(this JsonData data, string key, ref string val)
    {
      bool ret = false;
      if (data.IsObject) {
        ret = data[key].Get(ref val);
      }
      return ret;
    }
    public static bool Get(this JsonData data, string key, ref uint val)
    {
      bool ret = false;
      if (data.IsObject) {
        ret = data[key].Get(ref val);
      }
      return ret;
    }
    public static bool Get(this JsonData data, string key, ref ulong val)
    {
      long temp = 0;
      bool ret = Get(data, key, ref temp);
      if (ret) {
        val = (ulong)temp;
      }
      return ret;
    }
    public static bool Get(this JsonData data, string key, ref float val)
    {
      double temp = 0;
      bool ret = Get(data, key, ref temp);
      if (ret) {
        val = (float)temp;
      }
      return ret;
    }
    //--------------------------------------------------------------------------
    public static void Set(this JsonData data, string key, bool val)
    {
      data[key] = val;
    }
    public static void Set(this JsonData data, string key, int val)
    {
      data[key] = val;
    }
    public static void Set(this JsonData data, string key, long val)
    {
      data[key] = val;
    }
    public static void Set(this JsonData data, string key, double val)
    {
      data[key] = val;
    }
    public static void Set(this JsonData data, string key, string val)
    {
      data[key] = val;
    }
    public static void Set(this JsonData data, string key, uint val)
    {
      data[key] = (int)val;
    }
    public static void Set(this JsonData data, string key, ulong val)
    {
      data[key] = (long)val;
    }
    public static void Set(this JsonData data, string key, float val)
    {
      data[key] = (float)val;
    }
    //--------------------------------------------------------------------------
    public static bool Get(this JsonData data, int index, ref bool val)
    {
      bool ret = false;
      if ((data.IsObject || data.IsArray) && data.Count > index) {
        ret = data[index].Get(ref val);
      }
      return ret;
    }
    public static bool Get(this JsonData data, int index, ref int val)
    {
      bool ret = false;
      if ((data.IsObject || data.IsArray) && data.Count > index) {
        ret = data[index].Get(ref val);
      }
      return ret;
    }
    public static bool Get(this JsonData data, int index, ref long val)
    {
      bool ret = false;
      if ((data.IsObject || data.IsArray) && data.Count > index) {
        ret = data[index].Get(ref val);
      }
      return ret;
    }
    public static bool Get(this JsonData data, int index, ref double val)
    {
      bool ret = false;
      if ((data.IsObject || data.IsArray) && data.Count > index) {
        ret = data[index].Get(ref val);
      }
      return ret;
    }
    public static bool Get(this JsonData data, int index, ref string val)
    {
      bool ret = false;
      if ((data.IsObject || data.IsArray) && data.Count > index) {
        ret = data[index].Get(ref val);
      }
      return ret;
    }
    public static bool Get(this JsonData data, int index, ref uint val)
    {
      bool ret = false;
      if ((data.IsObject || data.IsArray) && data.Count > index) {
        ret = data[index].Get(ref val);
      }
      return ret;
    }
    public static bool Get(this JsonData data, int index, ref ulong val)
    {
      long temp = 0;
      bool ret = Get(data, index, ref temp);
      if (ret) {
        val = (ulong)temp;
      }
      return ret;
    }
    public static bool Get(this JsonData data, int index, ref float val)
    {
      float temp = 0;
      bool ret = Get(data, index, ref temp);
      if (ret) {
        val = (float)temp;
      }
      return ret;
    }
    //--------------------------------------------------------------------------
    public static void Set(this JsonData data, int index, bool val)
    {
      data[index] = val;
    }
    public static void Set(this JsonData data, int index, int val)
    {
      data[index] = val;
    }
    public static void Set(this JsonData data, int index, long val)
    {
      data[index] = val;
    }
    public static void Set(this JsonData data, int index, double val)
    {
      data[index] = val;
    }
    public static void Set(this JsonData data, int index, string val)
    {
      data[index] = val;
    }
    public static void Set(this JsonData data, int index, uint val)
    {
      data[index] = (int)val;
    }
    public static void Set(this JsonData data, int index, ulong val)
    {
      data[index] = (long)val;
    }
    public static void Set(this JsonData data, int index, float val)
    {
      data[index] = (float)val;
    }
    //--------------------------------------------------------------------------
    public static bool GetBoolean(this JsonData data)
    {
      bool ret = false;
      data.Get(ref ret);
      return ret;
    }
    public static int GetInt(this JsonData data)
    {
      int ret = 0;
      data.Get(ref ret);
      return ret;
    }
    public static long GetLong(this JsonData data)
    {
      long ret = 0;
      data.Get(ref ret);
      return ret;
    }
    public static float GetDouble(this JsonData data)
    {
      float ret = 0;
      data.Get(ref ret);
      return ret;
    }
    public static string GetString(this JsonData data)
    {
      string ret = null;
      data.Get(ref ret);
      return ret;
    }
    public static uint GetUint(this JsonData data)
    {
      uint ret = 0;
      data.Get(ref ret);
      return ret;
    }
    public static ulong GetUlong(this JsonData data)
    {
      ulong ret = 0;
      data.Get(ref ret);
      return ret;
    }
    public static float GetFloat(this JsonData data)
    {
      float ret = 0;
      data.Get(ref ret);
      return ret;
    }
    //--------------------------------------------------------------------------
    public static bool GetBoolean(this JsonData data, string key)
    {
      bool ret = false;
      data.Get(key, ref ret);
      return ret;
    }
    public static int GetInt(this JsonData data, string key)
    {
      int ret = 0;
      data.Get(key, ref ret);
      return ret;
    }
    public static long GetLong(this JsonData data, string key)
    {
      long ret = 0;
      data.Get(key, ref ret);
      return ret;
    }
    public static float GetDouble(this JsonData data, string key)
    {
      float ret = 0;
      data.Get(key, ref ret);
      return ret;
    }
    public static string GetString(this JsonData data, string key)
    {
      string ret = null;
      data.Get(key, ref ret);
      return ret;
    }
    public static uint GetUint(this JsonData data, string key)
    {
      uint ret = 0;
      data.Get(key, ref ret);
      return ret;
    }
    public static ulong GetUlong(this JsonData data, string key)
    {
      ulong ret = 0;
      data.Get(key, ref ret);
      return ret;
    }
    public static float GetFloat(this JsonData data, string key)
    {
      float ret = 0;
      data.Get(key, ref ret);
      return ret;
    }
    //--------------------------------------------------------------------------
    public static bool GetBoolean(this JsonData data, int index)
    {
      bool ret = false;
      data.Get(index, ref ret);
      return ret;
    }
    public static int GetInt(this JsonData data, int index)
    {
      int ret = 0;
      data.Get(index, ref ret);
      return ret;
    }
    public static long GetLong(this JsonData data, int index)
    {
      long ret = 0;
      data.Get(index, ref ret);
      return ret;
    }
    public static float GetDouble(this JsonData data, int index)
    {
      float ret = 0;
      data.Get(index, ref ret);
      return ret;
    }
    public static string GetString(this JsonData data, int index)
    {
      string ret = null;
      data.Get(index, ref ret);
      return ret;
    }
    public static uint GetUint(this JsonData data, int index)
    {
      uint ret = 0;
      data.Get(index, ref ret);
      return ret;
    }
    public static ulong GetUlong(this JsonData data, int index)
    {
      ulong ret = 0;
      data.Get(index, ref ret);
      return ret;
    }
    public static float GetFloat(this JsonData data, int index)
    {
      float ret = 0;
      data.Get(index, ref ret);
      return ret;
    }
  }
}