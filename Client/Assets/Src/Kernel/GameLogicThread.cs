using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DashFire.Network;

namespace DashFire
{
  public class GameLogicThread : MyThread
  {
    protected override void OnStart()
    {
    }

    protected override void OnTick()
    {
      //这里是在逻辑线程执行的tick，渲染线程的在GameControler.cs:TickGame里。
      try {
        TimeUtility.SampleClientTick();

        long curTime = TimeUtility.GetLocalMilliseconds();
        if (m_LastLogTime + 10000 < curTime) {
          m_LastLogTime = curTime;

          GfxSystem.GfxLog("LogicThread.Tick actionNum {0}", this.CurActionNum);
        }

        NetworkSystem.Instance.Tick();
        LobbyNetworkSystem.Instance.Tick();
        PlayerControl.Instance.Tick();
        WorldSystem.Instance.Tick();

        GameControler.LogicLoggerInstance.Tick();
      } catch (Exception ex) {
        GfxSystem.GfxLog("GameLogicThread.Tick throw Exception:{0}\n{1}", ex.Message, ex.StackTrace);
      }
    }

    protected override void OnQuit()
    {
    }

    private long m_LastLogTime = 0;
  }
}
