
/**
 * @file CharacterBase.cs
 * @brief 角色基类
 *
 * @author lixiaojiang
 * @version 0
 * @date 2012-11-14
 */

using System;
using System.Collections.Generic;
using ScriptRuntime;

namespace DashFire
{
  public class OperationCmdMgr
  {
    public enum Status
    {
      Moving = 0,
             Shooting,
             Reloading,
             ChangingWeapon,
             Deadth,
             BeingHurted,
             UsingSkill,

             Count
    }

    public enum CMD
    {
      Move = 0,
           ChangeFaceDir,
           UseSkill,
           Shoot,
           Reload,
           ChangeWeapon,
           Die,

           CMD_Count
    }

    public enum OPERATION
    {
      StopCmd,
        ExecuteCmd,
    }

    public delegate bool IsInStateFunc(Status status);

    IsInStateFunc	m_isInStatusCallback = null;
    OPERATION[,] 	m_Cmd_State_Operation = null;

    public OperationCmdMgr()
    {
      int cmdCount = (int)CMD.CMD_Count;
      int stateCount = (int)Status.Count;
      m_Cmd_State_Operation = new OPERATION[cmdCount,stateCount];

      for(int i = 0; i < cmdCount; ++i)
      {
        for(int j = 0; j < stateCount; ++j)
        {
          m_Cmd_State_Operation[i, j] = OPERATION.ExecuteCmd;
        }
      }

      SetCmdStateOperation(CMD.ChangeFaceDir, Status.ChangingWeapon, OPERATION.StopCmd);
      SetCmdStateOperation(CMD.ChangeFaceDir, Status.Deadth, OPERATION.StopCmd);
      SetCmdStateOperation(CMD.ChangeFaceDir, Status.Reloading, OPERATION.StopCmd);
      SetCmdStateOperation(CMD.ChangeFaceDir, Status.UsingSkill, OPERATION.StopCmd);

      SetCmdStateOperation(CMD.ChangeWeapon, Status.Deadth, OPERATION.StopCmd);
      SetCmdStateOperation(CMD.ChangeWeapon, Status.Reloading, OPERATION.StopCmd);
      SetCmdStateOperation(CMD.ChangeWeapon, Status.UsingSkill, OPERATION.StopCmd);

      SetCmdStateOperation(CMD.Move, Status.Deadth, OPERATION.StopCmd);

      SetCmdStateOperation(CMD.Reload, Status.ChangingWeapon, OPERATION.StopCmd);
      SetCmdStateOperation(CMD.Reload, Status.Deadth, OPERATION.StopCmd);
      SetCmdStateOperation(CMD.Reload, Status.Reloading, OPERATION.StopCmd);
      SetCmdStateOperation(CMD.Reload, Status.UsingSkill, OPERATION.StopCmd);

      SetCmdStateOperation(CMD.Shoot, Status.ChangingWeapon, OPERATION.StopCmd);
      SetCmdStateOperation(CMD.Shoot, Status.Deadth, OPERATION.StopCmd);
      SetCmdStateOperation(CMD.Shoot, Status.Reloading, OPERATION.StopCmd);
      SetCmdStateOperation(CMD.Shoot, Status.UsingSkill, OPERATION.StopCmd);

      SetCmdStateOperation(CMD.UseSkill, Status.Deadth, OPERATION.StopCmd);
      SetCmdStateOperation(CMD.UseSkill, Status.ChangingWeapon, OPERATION.StopCmd);
      SetCmdStateOperation(CMD.UseSkill, Status.Reloading, OPERATION.StopCmd);
    }

    public void SetCmdStateOperation(CMD cmd, Status state, OPERATION operation)
    {
      m_Cmd_State_Operation[(int)cmd, (int)state] = operation;
    }

    public void SetIsInStatusCallback (IsInStateFunc callback)
    {
      m_isInStatusCallback = callback;
    }

    public bool CanStartCmd (CMD cmd)
    {
      if (m_isInStatusCallback == null) {
        return false;
      }

      int cmdIndex = (int)cmd;
      int stateCount = (int)Status.Count;

      for (int stateIndex = 0; stateIndex < stateCount; stateIndex++) {
        Status state = (Status)stateIndex;
        if(m_isInStatusCallback(state) && m_Cmd_State_Operation[cmdIndex, stateIndex] == OPERATION.StopCmd)
        {
          return false;
        }
      }

      return true;
    }
  }
}
