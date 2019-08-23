using System;
using System.Collections.Generic;

namespace DashFire
{
  /// <summary>
  /// GfxLogic约定的游戏逻辑层通知接口，用于Gfx对游戏逻辑层的事件触发。
  /// 这里未采用PublishSubscribe是出于性能考虑。
  /// </summary>
  public interface IGameLogicNotification
  {
    void OnGfxHitTarget(int id, int skillId, int targetId, int hitCount);
    void OnGfxMoveMeetObstacle(int id);
  }
}
