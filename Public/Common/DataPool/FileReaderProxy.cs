using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace DashFire
{
  public delegate byte[] delegate_ReadFile(string path);

  public static class FileReaderProxy
  {
    private static delegate_ReadFile handlerReadFile;

    public static MemoryStream ReadFileAsMemoryStream(string filePath)
    {
      try {
        byte[] buffer = ReadFileAsArray(filePath);
        if (buffer == null)
        {
          LogSystem.Debug("Err ReadFileAsMemoryStream failed:{0}\n", filePath);
          return null;
        }
        return new MemoryStream(buffer);
      } catch (Exception e) {
        LogSystem.Debug("Exception:{0}\n", e.Message);
        Helper.LogCallStack();
        return null;
      }
    }

    public static byte [] ReadFileAsArray(string filePath)
    {
      byte[] buffer = null;
      try {
        //Note:客户端使用引擎接口读取, by lixiaojiang
        if (GlobalVariables.Instance.IsClient) {
          if (handlerReadFile != null) {
            buffer = handlerReadFile(filePath);
          } else {
            LogSystem.Debug("ReadFileByEngine handler have not register: {0}", filePath);
          }
        } else {
          using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read)) {
            buffer = new byte[fs.Length];
            fs.Read(buffer, 0, (int)fs.Length);
            fs.Close();
          }
        }
      } catch (Exception e) {
        LogSystem.Debug("Exception:{0}\n", e.Message);
        Helper.LogCallStack();
        return null;
      }
      return buffer;
    }

    public static bool Exists(string filePath)
    {
      return File.Exists(filePath);
    }

    //************************************************************************
    // Note:此函数只供客户端使用, by lixiaojiang                                                                  
    //************************************************************************
    public static void RegisterReadFileHandler(delegate_ReadFile handler)
    {
      if (GlobalVariables.Instance.IsClient) {
        // Note:Client确保只拥有唯一的读取函数,即ScriptRuntime.Util.ReadFile(...), by lixiaojiang 
        handlerReadFile = handler;
      } else {
        LogSystem.Debug("[Err] RegisterReadFileHandler only for Client.");
      }
    }

  }
}
