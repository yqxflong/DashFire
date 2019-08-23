using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DashFire
{
  public sealed class NpcAttrCalculator
  {
    public static void Calc(NpcInfo npc)
    {
      AttrCalculateUtility.ResetBaseProperty(npc);
      AttrCalculateUtility.RefixAttrByEquipment(npc);
      AttrCalculateUtility.RefixAttrByImpact(npc);
    }
  }
}
