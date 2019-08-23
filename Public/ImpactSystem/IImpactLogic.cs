using System;
using System.Collections.Generic;

namespace DashFire
{
  public interface IImpactLogic
  {
    void Tick(CharacterInfo obj, int impactId);
  }
}
