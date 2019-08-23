using UnityEngine;
using System.Collections;
using DashFire;

/// <summary>
/// Joystick event
/// </summary>
public class ShooterJoystickEvent : MonoBehaviour {
  bool m_IsMoving = false;
  Vector3 rotate = Vector3.zero;
  void OnEnable() {
    EasyJoystick.On_JoystickMove += On_JoystickMove;
    EasyJoystick.On_JoystickMoveEnd += On_JoystickMoveEnd;
  }
  void OnDisable() {
    EasyJoystick.On_JoystickMove -= On_JoystickMove;
    EasyJoystick.On_JoystickMoveEnd -= On_JoystickMoveEnd;
    if (m_IsMoving) {
      m_IsMoving = false;
      TouchManager.TouchEnable = true;
    }
  }
  void OnDestroy() {
    EasyJoystick.On_JoystickMove -= On_JoystickMove;
    EasyJoystick.On_JoystickMoveEnd -= On_JoystickMoveEnd;
    if (m_IsMoving) {
      m_IsMoving = false;
      TouchManager.TouchEnable = true;
    }
  }
  void On_JoystickMoveEnd(MovingJoystick move) {
    m_IsMoving = false;
    TouchManager.TouchEnable = true;
    Rotate(move);
    //if (LogicSystem.PlayerSelf != null) {
    //  LogicSystem.NotifyGfxMoveControlFinish(LogicSystem.PlayerSelf);
    //}
  }
  void On_JoystickMove(MovingJoystick move) {
    m_IsMoving = true;
    TouchManager.TouchEnable = false;
    //if (!m_IsMoving) {
    //  LogicSystem.NotifyGfxMoveControlStart(LogicSystem.PlayerSelf);
    //  m_IsMoving = true;
    //}
    Rotate(move);
  }
  private void Rotate(MovingJoystick move) {
    GameObject playerSelf = LogicSystem.PlayerSelf;
    if (playerSelf != null && Camera.main != null && move.joystickAxis != Vector2.zero) {
      Vector3 cameraAngles = Camera.main.transform.rotation.eulerAngles;
      cameraAngles.x = 0;
      Quaternion cameraRotate = Quaternion.Euler(cameraAngles);

      Vector2 joyStickDir = move.joystickAxis * 10.0f;
      Quaternion joyStickRotate = Quaternion.FromToRotation(
        Vector3.forward,
        new Vector3(joyStickDir.x, 0, joyStickDir.y));

      Quaternion targetRot = cameraRotate * joyStickRotate;
      Vector3 posOff = new Vector3(0, 0, 2.0f);
      Vector3 targetPos = playerSelf.transform.position + targetRot * posOff;
      TriggerImpl.SetFacePos(playerSelf, targetPos);
      //RotatePlayer(playerSelf, targetPos);
    }
  }
}
