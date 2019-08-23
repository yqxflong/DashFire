using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ScriptRuntime;

namespace DashFire
{
  public class PositionController : AbstractController<PositionController>
  {
    public override void Adjust()
    {
      CharacterInfo info = WorldSystem.Instance.GetCharacterById(m_ObjId);
      if (null != info) {
        if (!info.GetMovementStateInfo().IsMoving) {
          m_IsTerminated = true;
        } else {
          float curTime = TimeUtility.GetServerMilliseconds();
          float delta = curTime - m_LastTime;
          m_LastTime = curTime;
          m_CurTotalTime += delta;
          if (m_CurTotalTime > m_TotalTime)
            delta -= m_CurTotalTime - m_TotalTime;

          float dx = (float)(m_Dx * delta / m_TotalTime);
          float dy = (float)(m_Dy * delta / m_TotalTime);
          float dz = (float)(m_Dz * delta / m_TotalTime);

          Vector2 pos = info.GetMovementStateInfo().GetPosition2D() + new Vector2(dx, dz);
          info.GetMovementStateInfo().SetPosition2D(pos);

          if (m_CurTotalTime >= m_TotalTime) {
            m_IsTerminated = true;
          }
        }
      } else {
        m_IsTerminated = true;
      }
    }

    public void Init(int id, int objId, float dx, float dy, float dz, float totalTime)
    {
      m_Id = id;
      m_ObjId = objId;
      m_Dx = dx;
      m_Dy = dy;
      m_Dz = dz;
      m_CurTotalTime = 0;
      m_LastTime = TimeUtility.GetServerMilliseconds();
      m_TotalTime = totalTime;
    }

    private int m_ObjId = 0;
    private float m_Dx = 0;
    private float m_Dy = 0;
    private float m_Dz = 0;
    private float m_CurTotalTime = 0;
    private float m_LastTime = 0;
    private float m_TotalTime = 0;
  }
}
