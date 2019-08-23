using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DashFire
{
  public sealed class AiLogicManager
  {
    public INpcStateLogic GetNpcStateLogic(int id)
    {
      INpcStateLogic logic = null;
      if (m_NpcStateLogics.ContainsKey(id))
        logic = m_NpcStateLogics[id];
      return logic;
    }
    public IUserStateLogic GetUserStateLogic(int id)
    {
      IUserStateLogic logic = null;
      if (m_UserStateLogics.ContainsKey(id))
        logic = m_UserStateLogics[id];
      return logic;
    }
    private AiLogicManager()
    {
      //这里初始化所有的Ai状态逻辑，并记录到对应的列表(客户端的逻辑因为通常比较简单，很多会使用通用的ai逻辑)
      if (GlobalVariables.Instance.IsClient) {
        INpcStateLogic movableNpc = new AiLogic_MovableNpc_Client();
        INpcStateLogic immovableNpc = new AiLogic_ImmovableNpc_Client();
        INpcStateLogic fixedNpc = new AiLogic_FixedNpc_Client();
        m_NpcStateLogics.Add((int)AiStateLogicId.PveNpc_General, new AiLogic_PveNpc_General());
        m_NpcStateLogics.Add((int)AiStateLogicId.Demo_Melee, new AiLogic_Demo_Melee());
        m_NpcStateLogics.Add((int)AiStateLogicId.Demo_Ranged, new AiLogic_Demo_Ranged());
        m_NpcStateLogics.Add((int)AiStateLogicId.Demo_Boss, new AiLogic_Demo_Boss());
        m_NpcStateLogics.Add((int)AiStateLogicId.PvpNpc_Tower, immovableNpc);
        m_NpcStateLogics.Add((int)AiStateLogicId.PveNpc_OneSkill, movableNpc);
        m_NpcStateLogics.Add((int)AiStateLogicId.PveGun_Fixed, fixedNpc);
        m_NpcStateLogics.Add((int)AiStateLogicId.PveNpc_Monster, movableNpc);
        m_NpcStateLogics.Add((int)AiStateLogicId.PveNpc_Trap, fixedNpc);
        m_NpcStateLogics.Add((int)AiStateLogicId.PveNpc_Monster_CloseCombat, movableNpc);

        //-------------------------------------------------------------------------------------
        AiLogic_User_Client userLogic = new AiLogic_User_Client();
        m_UserStateLogics.Add((int)AiStateLogicId.PvpUser_General, userLogic);
        m_UserStateLogics.Add((int)AiStateLogicId.UserSelf_General, new AiLogic_UserSelf_General());
        m_UserStateLogics.Add((int)AiStateLogicId.UserSelfRange_General, new AiLogic_UserSelfRange_General());
      } else {
        m_NpcStateLogics.Add((int)AiStateLogicId.PveNpc_General, new AiLogic_PveNpc_General());
        m_NpcStateLogics.Add((int)AiStateLogicId.PvpNpc_General, new AiLogic_PvpNpc_General());
        m_NpcStateLogics.Add((int)AiStateLogicId.PvpNpc_Tower, new AiLogic_PvpNpc_Tower());
        m_NpcStateLogics.Add((int)AiStateLogicId.PveNpc_OneSkill, new AiLogic_PveNpc_OneSkill());
        m_NpcStateLogics.Add((int)AiStateLogicId.PveGun_Fixed, new AiLogic_PveGun_Fixed());
        m_NpcStateLogics.Add((int)AiStateLogicId.PveNpc_Monster, new AiLogic_PveNpc_Monster());
        m_NpcStateLogics.Add((int)AiStateLogicId.PveNpc_Trap, new AiLogic_PveNpc_Trap());
        m_NpcStateLogics.Add((int)AiStateLogicId.PveNpc_Monster_CloseCombat, new AiLogic_PveNpc_Monster_CloseCombat());

        //-------------------------------------------------------------------------------------
        m_UserStateLogics.Add((int)AiStateLogicId.UserSelf_General, new AiLogic_UserSelf_General());
        m_UserStateLogics.Add((int)AiStateLogicId.UserSelfRange_General, new AiLogic_UserSelfRange_General());
      }
    }
    private Dictionary<int, INpcStateLogic> m_NpcStateLogics = new Dictionary<int, INpcStateLogic>();
    private Dictionary<int, IUserStateLogic> m_UserStateLogics = new Dictionary<int, IUserStateLogic>();

    public static AiLogicManager Instance
    {
      get { return s_AiLogicManager; }
    }
    private static AiLogicManager s_AiLogicManager = new AiLogicManager();
  }
}
