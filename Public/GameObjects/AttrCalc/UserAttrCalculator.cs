using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DashFire
{
  public sealed class UserAttrCalculator
  {
    public static void Calc(UserInfo user)
    {
      AttrCalculateUtility.ResetBaseProperty(user);
      AttrCalculateUtility.RefixAttrByEquipment(user);
      AttrCalculateUtility.RefixAttrByImpact(user);
    }
  }
}
