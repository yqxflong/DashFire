using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DashFire;

public class ReceiveInput : MonoBehaviour 
{
  public delegate void EventHandler(float towards);
  public static EventHandler OnJoystickMove;
  public static bool IsMoving = false;
  public static float CurDirection;

  internal void OnEnable()
  {
    EasyJoystick.On_JoystickMoveStart += On_JoystickMoveStart;
    EasyJoystick.On_JoystickMove += On_JoystickMove;
    EasyJoystick.On_JoystickMoveEnd += On_JoystickMoveEnd;
  }
  internal void OnDisable()
  {
    EasyJoystick.On_JoystickMoveStart -= On_JoystickMoveStart;
    EasyJoystick.On_JoystickMove -= On_JoystickMove;
    EasyJoystick.On_JoystickMoveEnd -= On_JoystickMoveEnd;
  }
  internal void OnDestroy()
  {
    EasyJoystick.On_JoystickMoveStart -= On_JoystickMoveStart;
    EasyJoystick.On_JoystickMove -= On_JoystickMove;
    EasyJoystick.On_JoystickMoveEnd -= On_JoystickMoveEnd;
    IsMoving = false;
  }
  void On_JoystickMoveStart(MovingJoystick move)
  {
    IsMoving = true;
    SkillControllerInterface player_skill_ctrl = GetControl();
    if (null != player_skill_ctrl) {
      player_skill_ctrl.AddBreakSkillTask();
    }
  }
  void On_JoystickMoveEnd(MovingJoystick move)
  {
    TriggerMove(move, true);
    IsMoving = false;
  }
  void On_JoystickMove(MovingJoystick move)
  {
    if (TouchManager.Touches.Count > 0) {
      TriggerMove(move, false);
    }
  }
  private void TriggerMove(MovingJoystick move, bool isLift)
  {
    if (isLift) {
      GestureArgs e = new GestureArgs();
      e.name = "OnSingleTap";
      e.airWelGamePosX = 0f;
      e.airWelGamePosY = 0f;
      e.airWelGamePosZ = 0f;
      e.selectedObjID = -1;
      LogicSystem.FireGestureEvent(e);
      return;
    }

    GameObject playerSelf = LogicSystem.PlayerSelf;
    if (playerSelf != null && move.joystickAxis != Vector2.zero) {
      Vector2 joyStickDir = move.joystickAxis * 10.0f;
      Vector3 targetRot = new Vector3(joyStickDir.x, 0, joyStickDir.y);
      Vector3 targetPos = playerSelf.transform.position + targetRot;

      GestureArgs e = new GestureArgs();
      e.name = "OnSingleTap";
      e.selectedObjID = -1;
      e.towards = Mathf.Atan2(targetPos.x - playerSelf.transform.position.x, targetPos.z - playerSelf.transform.position.z);
      e.airWelGamePosX = targetPos.x;
      e.airWelGamePosY = targetPos.y;
      e.airWelGamePosZ = targetPos.z;
      LogicSystem.FireGestureEvent(e);
      CurDirection = e.towards;
      ///
      if (null != OnJoystickMove) {
        OnJoystickMove(e.towards);
      }
    }
  }

  public SkillControllerInterface GetControl()
  {
    SkillControllerInterface SkillCtrl = null;
    GameObject go = DashFire.LogicSystem.PlayerSelf;
    if (null != go) {
      BaseSkillManager skill_Manager = go.GetComponent<BaseSkillManager>();
      if (null != skill_Manager) {
        SkillCtrl = skill_Manager.GetSkillController();
      }
    }
    return SkillCtrl;
  }
}