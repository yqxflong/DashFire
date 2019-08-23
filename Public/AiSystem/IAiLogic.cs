using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DashFire
{
  public enum AiStateLogicId : int
  {
    Invalid = 0,
    PveNpc_General,
    PvpNpc_General,
    PvpNpc_Tower,
    PveNpc_OneSkill,
    PveGun_Fixed,
    PveNpc_Monster,
    PveNpc_Trap,
    PveNpc_Monster_CloseCombat,
    PvpUser_General = 10000,
    UserSelf_General = 10001,
    Demo_Melee = 10002,
    Demo_Ranged = 10003,
    Demo_Boss = 10004,
    UserSelfRange_General = 11000,
    MaxNum
  }
  public interface INpcStateLogic
  {
    void Execute(NpcInfo npc, AiCommandDispatcher aiCmdDispatcher, long deltaTime);
  }
  public interface IUserStateLogic
  {
    void Execute(UserInfo user, AiCommandDispatcher aiCmdDispatcher, long deltaTime);
  }
  public delegate void NpcMoveDelegation(NpcInfo npc);
  public delegate void NpcFaceDelegation(NpcInfo npc);
  public delegate void NpcFaceClientDelegation(NpcInfo npc, float faceDirection);
  public delegate void NpcTargetDelegation(NpcInfo npc);
  public delegate void NpcShootDelegation(NpcInfo npc,CharacterInfo target);
  public delegate void NpcSkillDelegation(NpcInfo npc, int skillId, CharacterInfo target);
  public delegate void NpcAiStateHandler(NpcInfo npc, AiCommandDispatcher aiCmdDispatcher, long deltaTime);
  public delegate void UserMoveDelegation(UserInfo user);
  public delegate void UserFaceDelegation(UserInfo user);
  public delegate void UserStartAttackDelegation(UserInfo user, float x, float y, float z);
  public delegate void UserStopAttackDelegation(UserInfo user);
  public delegate void UserReloadDelegation(UserInfo user);
  public delegate void UserSkillDelegation(UserInfo user);
  public delegate void UserPropertyChangedDelegation(UserInfo user);
  public delegate void UserItemChangedDelegation(UserInfo user,int pos,int num,int id);
  public delegate void UserAiStateHandler(UserInfo user, AiCommandDispatcher aiCmdDispatcher, long deltaTime);
  public delegate void NpcMeetEnemy(NpcInfo npc, Animation_Type animType);
  public delegate void NpcSetIdleAnim(NpcInfo npc, List<int> anims);
  public delegate void UserPursuitDelegation(UserInfo user);
  public abstract class AbstractNpcStateLogic : INpcStateLogic
  {
    public static NpcMoveDelegation OnNpcMove;
    public static NpcFaceDelegation OnNpcFace;
    public static NpcFaceClientDelegation OnNpcFaceClient;
    public static NpcTargetDelegation OnNpcTargetChange;
    public static NpcShootDelegation OnNpcShoot;
    public static NpcSkillDelegation OnNpcSkill;
    public static NpcMeetEnemy OnNpcMeetEnemy;
    public static NpcSetIdleAnim OnSetNpcIdleAnim;
    public AbstractNpcStateLogic()
    {
      OnInitStateHandlers();
    }
    public void Execute(NpcInfo npc, AiCommandDispatcher aiCmdDispatcher, long deltaTime)
    {
      if (npc.GetAIEnable()) {
        NpcAiStateInfo npcAi = npc.GetAiStateInfo();
        if(npcAi.CommandQueue.Count<=0) {
          int curState = npcAi.CurState;
          if (curState > (int)AiStateId.Invalid && curState < (int)AiStateId.MaxNum) {
            if (m_Handlers.ContainsKey(curState)) {
              NpcAiStateHandler handler = m_Handlers[curState];
              if (null != handler) {
                handler(npc, aiCmdDispatcher, deltaTime);
              }
            } else {
              LogSystem.Error("Illegal ai state: " + curState + " npc:" + npc.GetId());
            }
          } else {
            OnStateLogicInit(npc, aiCmdDispatcher, deltaTime);
            ChangeToState(npc, (int)AiStateId.Idle);
          }
        }
        ExecuteCommandQueue(npc, deltaTime);
      }
    }
    public void ChangeToState(NpcInfo npc, int state)
    {
      npc.GetAiStateInfo().ChangeToState(state);
    }
    public void PushState(NpcInfo npc, int state)
    {
      npc.GetAiStateInfo().PushState(state);
    }
    public void PopState(NpcInfo npc)
    {
      npc.GetAiStateInfo().PopState();
    }
    public void NotifyNpcMove(NpcInfo npc)
    {
      if (null != OnNpcMove)
        OnNpcMove(npc);
    }
    public void NotifyNpcFace(NpcInfo npc, float faceDirection = 0.0f)
    {
      if (null != OnNpcFace)
        OnNpcFace(npc);

      if (GlobalVariables.Instance.IsClient) {
        if (null != OnNpcFaceClient) {
          OnNpcFaceClient(npc, faceDirection);
        }
      }
    }
    public void NotifyNpcTargetChange(NpcInfo npc)
    {
      if (null != OnNpcTargetChange)
        OnNpcTargetChange(npc);
    }
    public void ServerNpcSkill(NpcInfo npc,
                           int skillId,
                           CharacterInfo target,
                           ScriptRuntime.Vector3 targetPos,
                           float targetAngle)
    {
      //bool ret = SkillSystem.Instance.StartSkill(npc, skillId, target, targetPos, targetAngle);
      //if (ret && null != OnNpcSkill)
        //OnNpcSkill(npc, skillId, target, targetPos, targetAngle);
    }
    public void ServerNpcImpact(CharacterInfo source, int impactId, CharacterInfo target)
    {
      //ImpactSystem.Instance.SendImpactToEntity(source, impactId, source.GetMovementStateInfo().GetPosition3D(), target, -1);
    }
    public void ServerNpcImpact(ScriptRuntime.Vector3 srcPos, int impactId, CharacterInfo target)
    {
      //ImpactSystem.Instance.SendImpactToEntity(null, impactId, srcPos, target, -1);
    }
    public void ServerNpcStopImpact(CharacterInfo npc, int impactId)
    {
      //ImpactSystem.Instance.StopEntityImpact(npc, impactId);
    }
    protected void SetStateHandler(int state, NpcAiStateHandler handler)
    {
      if (state > (int)AiStateId.Invalid && state < (int)AiStateId.MaxNum) {
        if (null != handler) {
          if (m_Handlers.ContainsKey(state))
            m_Handlers[state] = handler;
          else
            m_Handlers.Add(state, handler);
        } else {
          m_Handlers.Remove(state);
        }
      }
    }
    protected abstract void OnInitStateHandlers();
    protected abstract void OnStateLogicInit(NpcInfo npc, AiCommandDispatcher aiCmdDispatcher, long deltaTime);

    private void ExecuteCommandQueue(NpcInfo npc, long deltaTime)
    {
      NpcAiStateInfo npcAi = npc.GetAiStateInfo();
      while (npcAi.CommandQueue.Count > 0) {
        IAiCommand cmd = npcAi.CommandQueue.Peek();
        if (cmd.Execute(deltaTime)) {
          npcAi.CommandQueue.Dequeue();
        } else {
          break;
        }
      }
    }

    private Dictionary<int, NpcAiStateHandler> m_Handlers = new Dictionary<int, NpcAiStateHandler>();
  }
  public abstract class AbstractUserStateLogic : IUserStateLogic
  {
    public static UserMoveDelegation OnUserMove;
    public static UserFaceDelegation OnUserFace;
    public static UserStartAttackDelegation OnUserStartAttack;
    public static UserStopAttackDelegation OnUserStopAttack;
    public static UserReloadDelegation OnUserReload;
    public static UserSkillDelegation OnUserSkill;
    public static UserPropertyChangedDelegation OnUserPropertyChanged;
    public static UserItemChangedDelegation OnUserItemChanged;
    public static UserPursuitDelegation OnSkillPursuit;
    public AbstractUserStateLogic()
    {
      OnInitStateHandlers();
    }
    public void Execute(UserInfo user, AiCommandDispatcher aiCmdDispatcher, long deltaTime)
    {
      if (user.GetAIEnable()) {
        UserAiStateInfo userAi = user.GetAiStateInfo();
        if (userAi.CommandQueue.Count <= 0) {
          int curState = userAi.CurState;
          if (curState > (int)AiStateId.Invalid && curState < (int)AiStateId.MaxNum) {
            if (m_Handlers.ContainsKey(curState)) {
              UserAiStateHandler handler = m_Handlers[curState];
              if (null != handler) {
                handler(user, aiCmdDispatcher, deltaTime);
              }
            } else {
              LogSystem.Error("Illegal ai state: " + curState + " user:" + user.GetId());
            }
          } else {
            OnStateLogicInit(user, aiCmdDispatcher, deltaTime);
            ChangeToState(user, (int)AiStateId.Idle);
          }
        }
        ExecuteCommandQueue(user, deltaTime);
      }
    }
    public void ChangeToState(UserInfo user, int state)
    {
      user.GetAiStateInfo().ChangeToState(state);
    }
    public void PushState(UserInfo user, int state)
    {
      user.GetAiStateInfo().PushState(state);
    }
    public void PopState(UserInfo user)
    {
      user.GetAiStateInfo().PopState();
    }
    public void NotifyUserMove(UserInfo user)
    {
      if (null != OnUserMove)
        OnUserMove(user);
    }
    public void NotifyUserFace(UserInfo user)
    {
      if (null != OnUserFace)
        OnUserFace(user);
    }

    public void ServerUserSkill(UserInfo user,
                           int skillId,
                           CharacterInfo target,
                           ScriptRuntime.Vector3 targetPos,
                           float targetAngle,
                           int itemId)
    {
      bool ret = true;//SkillSystem.Instance.StartSkill(user, skillId, target, targetPos, targetAngle);
      if (ret && null != OnUserSkill)
        OnUserSkill(user);
    }
    public void ServerUserImpact(CharacterInfo source, int impactId, CharacterInfo target)
    {
      //ImpactSystem.Instance.SendImpactToEntity(source, impactId, source.GetMovementStateInfo().GetPosition3D(), target, -1);
    }
    public void ServerUserImpact(ScriptRuntime.Vector3 srcPos, int impactId, CharacterInfo target)
    {
      //ImpactSystem.Instance.SendImpactToEntity(null, impactId, srcPos, target, -1);
    }
    public void ServerUserStopImpact(CharacterInfo user, int impactId)
    {
      //ImpactSystem.Instance.StopEntityImpact(user, impactId);
    }
    public void NotifyUserPropertyChanged(UserInfo user)
    {
      if (null != OnUserPropertyChanged)
        OnUserPropertyChanged(user);
    }
    public void NotifyUserItemChanged(UserInfo user, int pos, int num, int id)
    {
      if (null != OnUserItemChanged)
        OnUserItemChanged(user, pos, num, id);
    }
    protected void SetStateHandler(int state, UserAiStateHandler handler)
    {
      if (state > (int)AiStateId.Invalid && state < (int)AiStateId.MaxNum) {
        if (null != handler) {
          if (m_Handlers.ContainsKey(state))
            m_Handlers[state] = handler;
          else
            m_Handlers.Add(state, handler);
        } else {
          m_Handlers.Remove(state);
        }
      }
    }
    protected abstract void OnInitStateHandlers();
    protected abstract void OnStateLogicInit(UserInfo user, AiCommandDispatcher aiCmdDispatcher, long deltaTime);

    private void ExecuteCommandQueue(UserInfo user, long deltaTime)
    {
      UserAiStateInfo userAi = user.GetAiStateInfo();
      while (userAi.CommandQueue.Count > 0) {
        IAiCommand cmd = userAi.CommandQueue.Peek();
        if (cmd.Execute(deltaTime)) {
          userAi.CommandQueue.Dequeue();
        } else {
          break;
        }
      }
    }

    private Dictionary<int, UserAiStateHandler> m_Handlers = new Dictionary<int, UserAiStateHandler>();
  }
}
