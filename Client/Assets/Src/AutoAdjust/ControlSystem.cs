using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DashFire
{
  public sealed class ControlSystem
  {
    public bool ExistController(int id)
    {
      return m_Controllers.ContainsKey(id);
    }
    public IController GetController(int id)
    {
      IController ctrl = null;
      if (m_Controllers.ContainsKey(id)) {
        ctrl = m_Controllers[id];
      }
      return ctrl;
    }
    public void AddController(IController ctrl)
    {
      if (null != ctrl) {
        if (m_Controllers.ContainsKey(ctrl.Id)) {
          IController oldCtrl = m_Controllers[ctrl.Id];
          m_Controllers[ctrl.Id] = ctrl;
          oldCtrl.Recycle();
        } else {
          m_Controllers.Add(ctrl.Id, ctrl);
        }
      }
    }
    public void Tick()
    {
      try {
        foreach (IController ctrl in m_Controllers.Values) {
          ctrl.Adjust();
          if (ctrl.IsTerminated) {
            m_WaitDeletedControllers.Add(ctrl);
          }
        }
        foreach (IController ctrl in m_WaitDeletedControllers) {
          m_Controllers.Remove(ctrl.Id);
          ctrl.Recycle();
        }
        m_WaitDeletedControllers.Clear();
      } catch (Exception ex) {
        LogSystem.Debug("ControlSystem Exception:%s\n%s", ex.Message, ex.StackTrace);
      }
    }
    public ObjectPool<FaceController> FaceControllerPool
    {
      get { return m_FaceControllerPool; }
    }
    public ObjectPool<MoveController> MoveControllerPool
    {
      get { return m_MoveControllerPool; }
    }
    public ObjectPool<PositionController> PositionControllerPool
    {
      get { return m_PositionControllerPool; }
    }
    public ControlSystem()
    {
      m_FaceControllerPool.Init(128);
      m_MoveControllerPool.Init(128);
      m_PositionControllerPool.Init(128);
    }

    private MyDictionary<int,IController> m_Controllers = new MyDictionary<int,IController>();
    private List<IController> m_WaitDeletedControllers = new List<IController>();

    private ObjectPool<FaceController> m_FaceControllerPool = new ObjectPool<FaceController>();
    private ObjectPool<MoveController> m_MoveControllerPool = new ObjectPool<MoveController>();
    private ObjectPool<PositionController> m_PositionControllerPool = new ObjectPool<PositionController>();

    public static ControlSystem Instance
    {
      get { return s_Instance; }
    }
    private static ControlSystem s_Instance = new ControlSystem();
  }
}
