using UnityEngine;
using System.Collections;
using DashFire;

public class GunChange : MonoBehaviour {
  void Start() {
    NGUITools.SetActive(this.gameObject, false);
  }
  void Update() {
  }
  void OnClick() {
    if (LogicSystem.PlayerSelf != null) {
      IShooterSkill curSkill = TriggerImpl.GetCurSkill(LogicSystem.PlayerSelf);
      if (curSkill != null && curSkill.IsExecuting()) {
        return;
      }

      TriggerImpl.SwitchNextWeapon(LogicSystem.PlayerSelf);
      SkillWeaponInfo info = TriggerImpl.GetCurWeaponInfo(LogicSystem.PlayerSelf);
      if (info != null) {
        UISprite sp = this.GetComponent<UISprite>();
        if (null != sp) {
          sp.spriteName = info.SpriteName;
        }
      }
    }
  }
  public void SetActive(bool active) {
    NGUITools.SetActive(this.gameObject, active);
  }
}
