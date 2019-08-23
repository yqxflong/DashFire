using System;
using System.Collections.Generic;

namespace DashFire
{
  public class GameLogicNotification : IGameLogicNotification
  {
    public void OnGfxHitTarget(int id, int impactId, int targetId, int hitCount)
    {
      UserInfo playerSelf = WorldSystem.Instance.GetPlayerSelf();
      if (id == WorldSystem.Instance.PlayerSelfId && playerSelf != null) {
        long curTime = TimeUtility.GetLocalMilliseconds();
        if (hitCount > 0) {
          long lastHitTime = playerSelf.LastHitTime;
          if (lastHitTime + 1500 > curTime) {
            playerSelf.MultiHitCount = playerSelf.MultiHitCount + hitCount;
          }
        }
        WorldSystem.Instance.GetPlayerSelf().LastHitTime = curTime;
        GfxSystem.PublishGfxEvent("ge_hitcount", "ui", playerSelf.MultiHitCount);
      }
      CharacterInfo src = WorldSystem.Instance.GetCharacterById(id);
      CharacterInfo target = WorldSystem.Instance.GetCharacterById(targetId);
      if (null != src && null != target) {
        ImpactSystem.Instance.SendImpactToCharacter(src, impactId, targetId);
      }
    }

    public void OnGfxMoveMeetObstacle(int id)
    {
      CharacterInfo charObj = WorldSystem.Instance.GetCharacterById(id);
      if (null != charObj) {
        charObj.GetMovementStateInfo().IsMoveMeetObstacle = true;
      }
    }

    public static GameLogicNotification Instance
    {
      get { return s_Instance; }
    }
    private static GameLogicNotification s_Instance = new GameLogicNotification();
  }
}
