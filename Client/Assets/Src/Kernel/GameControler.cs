#define USE_DISK_LOG

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using DashFire.Network;

namespace DashFire
{
  /**
   * @brief 游戏控制器
   */
  public sealed class GameControler
  {
  	public sealed class LogicLogger : IDisposable
  	{
  		public void Log(string format, params object[] args)
  		{
  			string msg = string.Format(format, args);
#if USE_DISK_LOG
        m_LogStream.WriteLine(msg);
#else
  			m_LogQueue.Enqueue(msg);
  			if(m_LogQueue.Count>=c_FlushCount){
          m_LastFlushTime = TimeUtility.GetLocalMilliseconds();

          RequestFlush();
  			}
#endif
  		}
  		public void Init(string logPath)
  		{
  			string logFile = string.Format("{0}/Game_{1}.log", logPath, DateTime.Now.ToString("yyyy-MM-dd"));
  			m_LogStream = new StreamWriter(logFile, true);
#if !USE_DISK_LOG
  			m_LogQueue = m_LogQueues[m_CurQueueIndex];
        m_Thread.OnQuitEvent = OnThreadQuit;
        m_Thread.Start();
#endif
        Log("======GameLog Start ({0}, {1})======", DateTime.Now.ToLongDateString(), DateTime.Now.ToLongTimeString());
  		}
  		public void Dispose()
  		{
  			Release();
  		}
      public void Tick()
      {
#if !USE_DISK_LOG
        long curTime = TimeUtility.GetLocalMilliseconds();
        if (m_LastFlushTime + 10000 < curTime) {
          m_LastFlushTime = curTime;

          RequestFlush();
          GfxSystem.GfxLog("LogicLogger.Tick.");
        }
#endif
      }
  		private void Release()
  		{
#if !USE_DISK_LOG
  			m_Thread.Stop();
#endif
  			m_LogStream.Close();
  			m_LogStream.Dispose();
  		}
#if !USE_DISK_LOG
      private void RequestFlush()
      {
        lock (m_LogQueueLock) {
          m_Thread.QueueAction(FlushToFile, m_LogQueue);
          m_CurQueueIndex = 1 - m_CurQueueIndex;
          m_LogQueue = m_LogQueues[m_CurQueueIndex];
        }
      }
  		private void OnThreadQuit()
  		{
  			FlushToFile(m_LogQueue);
  		}
  		private void FlushToFile(Queue<string> logQueue)
  		{
        lock (m_LogQueueLock) {
          GfxSystem.GfxLog("LogicLogger.FlushToFile, count {0}.", logQueue.Count);
	  			while(logQueue.Count>0){
	  				string msg = logQueue.Dequeue();
	  				m_LogStream.WriteLine(msg);
	  			}
	  		}
  		}
  		
  		private Queue<string>[] m_LogQueues = new Queue<string>[]{new Queue<string>(),new Queue<string>()};
  		private MyThread m_Thread = new MyThread();
  		private int m_CurQueueIndex = 0;
  		private Queue<string> m_LogQueue;
  		private object m_LogQueueLock = new object();

      private long m_LastFlushTime = 0;  		
  		private const int c_FlushCount = 4096;
#endif
  		private StreamWriter m_LogStream;
  	}
    //----------------------------------------------------------------------
    // 标准接口
    //----------------------------------------------------------------------
    public static bool IsInited
    {
      get { return s_IsInited; }
    }
    public static void InitGame(string logPath, string dataPath)
    {
      s_IsInited = true;
      s_LogicLogger.Init(logPath);
      HomePath.CurHomePath = dataPath;
      GlobalVariables.Instance.IsClient = true;
      GlobalVariables.Instance.IsDebug = false;

      FileReaderProxy.RegisterReadFileHandler((string filePath) => {
        byte[] buffer = null;
        try {
          using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read)) {
            buffer = new byte[fs.Length];
            fs.Read(buffer, 0, (int)fs.Length);
            fs.Close();
          }
        } catch (Exception e) {
          GfxSystem.GfxLog("Exception:{0}\n{1}", e.Message, e.StackTrace);
          return null;
        }
        return buffer;
      });

      LogSystem.OnOutput = (Log_Type type, string msg) => { GfxSystem.GfxLog("{0}", msg); s_LogicLogger.Log("{0}", msg); };

      GfxSystem.GfxLog("GameControler.InitGame");      
      _InitSystems();
    }
    public static void StartGame()
    {
      GfxSystem.GfxLog("GameControler.StartGame");
      s_LogicThread.Start();
    }
    public static void ChangeScene(int sceneId)
    {
      LogSystem.Info("GameControler.ChangeScene {0}", sceneId);
      WorldSystem.Instance.ChangeNextScene(sceneId);
    }
    public static void StopGame()
    {
      GfxSystem.GfxLog("GameControler.StopGame");
      s_LogicThread.Stop();
    }
    public static void ReleaseGame()
    {
      GfxSystem.GfxLog("GameControler.ReleaseGame");

      WorldSystem.Instance.Release();
      EntityManager.Instance.Release();
      NetworkSystem.Instance.Release();
      GfxSystem.Release();
      s_LogicLogger.Dispose();
    }
    public static void TickGame()
    { 
      //这里是在渲染线程执行的tick，逻辑线程的tick在GameLogicThread.cs文件里执行。
      GfxSystem.Tick();
    }
    
    //----------------------------------------------------------------------
    // 私有接口
    //----------------------------------------------------------------------
    private static void _InitSystems()
    {      
      // GfxSystem
      GfxSystem.Init();
      GfxSystem.SetLogicInvoker(s_LogicThread);
      GfxSystem.SetLogicLogCallback((string format,object[] args)=>{
      	s_LogicLogger.Log(format,args);
      });
      GfxSystem.SetGameLogicNotification(GameLogicNotification.Instance);
      // Managers
      EntityManager.Instance.Init();
      // System
      WorldSystem.Instance.Init();
      WorldSystem.Instance.LoadData();

      PlayerControl.Instance.Init();
      LobbyNetworkSystem.Instance.Init();
      NetworkSystem.Instance.Init();
      AiViewManager.Instance.Init();
    }

    internal static LogicLogger LogicLoggerInstance
    {
      get
      {
        return s_LogicLogger;
      }
    }
	
    private static LogicLogger s_LogicLogger = new LogicLogger();
    private static GameLogicThread s_LogicThread = new GameLogicThread();
    private static bool s_IsInited = false;
  }
}

