/**
 * @file Helper.cs
 * @brief 帮助类
 *
 * @author lixiaojiang
 * @version 0.0.1
 * @date 2012-12-12
 */

using System;
using System.Diagnostics;

namespace DashFire
{
  public sealed class Helper
  {
    public static bool StringIsNullOrEmpty(string str)
    {
      if (str == null || str == "")
        return true;
      return false;
    }

    public static void LogCallStack()
    {
      StackTrace trace = new StackTrace();
      LogSystem.Debug("LogCallStack:\n{0}\n",trace.ToString());
    }

    public sealed class Random
    {
      static public int Next()
      {
        return Instance.Next(100);
      }
      static public int Next(int max)
      {
        return Instance.Next(max);
      }
      static public int Next(int min,int max)
      {
        return Instance.Next(min, max);
      }
      static public float NextFloat()
      {
        return (float)Instance.NextDouble();
      }

      static private System.Random Instance
      {
        get
        {
          if (null == rand) {
            rand = new System.Random();
          }
          return rand;
        }
      }
      [ThreadStatic]
      static private System.Random rand = null;
    }
  }
}

