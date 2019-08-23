using System;
using System.Diagnostics;
using System.Collections.Generic;
using ScriptRuntime;

namespace DashFire
{
  public delegate void OutputDelegation(string msg);
  public delegate float DeltaTimeDelegation();

  class Profiler
  {
    string name;
    double m_TotalElapsedTime;
    double m_CurElapsedTime;
    double m_TickCount;

    double m_MaxElapsedTimePerFrame;
    double m_TickPerFrame;
    Stopwatch m_Stopwatch;
    bool m_IsValid;

    public Profiler(string name)
    {
      this.name = name;
      Reset();
    }

    ~Profiler()
    {
      Reset();
    }

    public void Start()
    {
      m_Stopwatch = Stopwatch.StartNew();
      m_CurElapsedTime = 0;
      m_IsValid = true;
      m_TickCount++;
    }

    public void Stop()
    {
      if (m_Stopwatch != null)
      {
        m_Stopwatch.Stop();
        m_CurElapsedTime = m_Stopwatch.Elapsed.TotalSeconds;
        m_TotalElapsedTime += m_CurElapsedTime;

        m_TickPerFrame++;
        m_MaxElapsedTimePerFrame = Math.Max(m_CurElapsedTime, m_MaxElapsedTimePerFrame);
      }
    }

    public void Reset()
    {
      if (m_Stopwatch != null)
      {
        m_Stopwatch.Stop();
        m_Stopwatch.Reset();
      }
      m_Stopwatch = null;
      m_CurElapsedTime = 0;
      m_TotalElapsedTime = 0;
      m_IsValid = false;
      m_TickCount = 0;
      m_TickPerFrame = 0;
      m_MaxElapsedTimePerFrame = 0;
    }

    public double TotalElapsedTime
    {
      get { return m_TotalElapsedTime; }
    }

    public double CurElapsedTime
    {
      get { return m_CurElapsedTime; }
    }

    public double TickPerFrame
    {
      get { return m_TickPerFrame; }
    }

    public double MaxElapsedTimePerFrame
    {
      get { return m_MaxElapsedTimePerFrame; }
    }

    public bool IsValid
    {
      get { return m_IsValid; }
      set { m_IsValid = value; }
    }

    public double TickCount
    {
      get { return m_TickCount; }
    }

    public void Print(double totalTime, double deltaTime, double fps, OutputDelegation handlerOutput)
    {
      string info = string.Format("[Profiler]:{0,-40} Fps:{1,-5:F3} Cur:{2,-5:F3}/{3,-5:F3} Total:{4,-5:F3}/{5,-5:F3} Tick:{6,-5:F0} Tick:{7,-5:F3} Max:{8,-5:F3}",
          name,
          fps,
          m_CurElapsedTime,
          deltaTime, //m_CurElapsedTime * 100 / deltaTime,
          m_TotalElapsedTime,
          totalTime,
          m_TickCount,
          TickPerFrame,
          MaxElapsedTimePerFrame
          );//,m_TotalElapsedTime * 100 / totalTime);
      handlerOutput(info);
      m_CurElapsedTime = 0;
    }
  }

  public class ProfilerMgr
  {
    /**
     * @brief Singleton
     *
     * @return 
     */
#region Singleton
    private static ProfilerMgr s_Instance = new ProfilerMgr();
    public static ProfilerMgr Instance
    {
      get { return s_Instance; }
    }
#endregion

    public static event OutputDelegation handlerOutput;
    public static event DeltaTimeDelegation handlerDeltatime;

    public const bool c_ProfilerEnable = false;
    public const double c_ProfilerInterval = 1000;
    public const double c_ProfilerMinFps = 0.0015f;
    public const double c_ProfilerMinElapsed = 0.001f;

    public DateTime m_tLastProfiler = DateTime.Now;

    const double c_DurationMax = 0;
    double m_TotalTime;
    double m_DeltaTime;
    double m_LogicTime;
    double m_Fps;
    static Dictionary<string, Profiler> m_AllProfilers = new Dictionary<string, Profiler>();

    private ProfilerMgr ()
    {
    }

    public void Init ()
    {
      Clean();
    }

    public void Tick ()
    {
      if (!c_ProfilerEnable) {
        return;
      }

      if (m_AllProfilers.Count > 200)
          LogSystem.Error("ProfilerMgr m_AllProfilers Count too much!");

      m_DeltaTime = handlerDeltatime();
      m_TotalTime += m_DeltaTime;
      m_Fps = 1 / m_DeltaTime;

      if ((DateTime.Now - m_tLastProfiler).TotalMilliseconds > c_ProfilerInterval
        && m_DeltaTime >= c_ProfilerMinFps)
      {
        m_tLastProfiler = DateTime.Now;
      }
      else
        return;

      handlerOutput("---------------------------------Profiler(Last Frame)----------------------------------------");
      string info = string.Format("[Profiler]:Fps:{0,-40:F3} Total:{1,-5:F3} Delta:{2,-5:F3} Logic:{3,-5:F3}",
          m_Fps,
          m_TotalTime, 
          m_DeltaTime,
          m_LogicTime);
      handlerOutput(info);


      foreach (Profiler profiler in m_AllProfilers.Values)
      {
        if (profiler.IsValid && profiler.CurElapsedTime >= c_ProfilerMinElapsed)
        {
          profiler.Print(m_TotalTime, m_DeltaTime, m_Fps, handlerOutput);
          profiler.IsValid = false;
        }
      }
    }

    public void Release()
    {
      Clean();
    }

    public static bool ProfilerEnable
    {
      get {return c_ProfilerEnable;}
    }

    public double TotalTime 
    {
      get {return m_TotalTime;}
    }

    public double DeltaTime 
    {
      get {return m_DeltaTime;}
    }

    public double LogicTime 
    {
      get {return m_LogicTime;}
      set {m_LogicTime = value;}
    }

    public static void ProfilerStart (string name)
    {
      if (!c_ProfilerEnable) {
        return;
      }

      Profiler profiler = null;
      if (m_AllProfilers.ContainsKey (name)) {
        profiler = m_AllProfilers [name];
      } else {
        profiler = new Profiler (name);
        m_AllProfilers.Add(name, profiler);
      }

      if (profiler != null) {
        profiler.Start();
      }
    }

    public static void ProfilerStop (string name)
    {
      if (!c_ProfilerEnable) {
        return;
      }

      Profiler profiler = null;
      if (m_AllProfilers.ContainsKey (name)) {
        profiler = m_AllProfilers [name];
      }

      if (profiler != null) {
        profiler.Stop();
      }
    }

    public static double GetProfilerElapsed (string name)
    {
      Profiler profiler = null;
      if (m_AllProfilers.ContainsKey (name)) {
        profiler = m_AllProfilers [name];
      }

      if (profiler != null) {
        return profiler.CurElapsedTime;
      }

      return -1.0f;
    }

    private void Clean()
    {
      m_TotalTime = 0;
      m_DeltaTime = 0;

      foreach (Profiler profiler in m_AllProfilers.Values) {
        profiler.Stop();
      }
      m_AllProfilers.Clear();
    }
  }
}

