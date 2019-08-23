using System;
using DashFire.Network;
using ScriptRuntime;
using System.Collections.Generic;

namespace DashFire
{
  /*
    * TODO: 
    * 1. 由於多數操作依賴playerself的存在, 而playerself可能未創建或者已經刪除
    *    所以是否可以考慮外部輸入還是設計成可以與某個obj綁定?
    * 2. 組合鍵, 設定一組鍵的狀態同時彙報, 例如wasd的狀態希望能夠在一個囘調函數中得到通知
    *    (不適用主動查詢的情況下), 使用主動查詢就必須存在一個Tick函數
    *    
    */
  public class PlayerControl
  {
    // static methods
    private static PlayerControl inst_ = new PlayerControl();
    public static PlayerControl Instance { get { return inst_; } }
    // properties
    public bool EnableMoveInput { get; set; }
    public bool EnableRotateInput { get; set; }

    // methods
    public PlayerControl()
    {
      pm_ = new PlayerMovement();
      EnableMoveInput = true;
      EnableRotateInput = true;
    }

    public void Init()
    {
      GfxSystem.ListenKeyPressState(
        Keyboard.Code.Z,
        Keyboard.Code.X,
        Keyboard.Code.Space,
        Keyboard.Code.Period,
        Keyboard.Code.W,
        Keyboard.Code.S,
        Keyboard.Code.A,
        Keyboard.Code.D,
        Keyboard.Code.P);
      GfxSystem.ListenKeyboardEvent(Keyboard.Code.P, this.SwitchHero);
      GfxSystem.ListenKeyboardEvent(Keyboard.Code.Z, this.SwitchDebug);
      GfxSystem.ListenKeyboardEvent(Keyboard.Code.X, this.SwitchObserver);
      GfxSystem.ListenKeyboardEvent(Keyboard.Code.Space, this.InteractObject);
      GfxSystem.ListenKeyboardEvent(Keyboard.Code.Period, this.PrintPosition);
      GfxSystem.ListenTouchEvent(TouchEvent.Cesture, this.TouchHandle);
    }

    public void Tick()
    {
      long now = TimeUtility.GetServerMilliseconds();
      m_LastTickIntervalMs = now - m_LastTickTime;

      m_LastTickTime = now;

      if (WorldSystem.Instance.IsObserver && !WorldSystem.Instance.IsFollowObserver) {
        bool keyPressed = false;
        float x = 0.5f, y = 0.5f;
        if (GfxSystem.IsKeyPressed(Keyboard.Code.A)) {
          x = 0.1f;
          keyPressed = true;
        } else if (GfxSystem.IsKeyPressed(Keyboard.Code.D)) {
          x = 0.9f;
          keyPressed = true;
        }
        if (GfxSystem.IsKeyPressed(Keyboard.Code.W)) {
          y = 0.1f;
          keyPressed = true;
        } else if (GfxSystem.IsKeyPressed(Keyboard.Code.S)) {
          y = 0.9f;
          keyPressed = true;
        }
        if (keyPressed)
          WorldSystem.Instance.UpdateObserverCamera(x, y);
        return;
      }

      // if move input is disable
      // MotionStatus is MoveStop, and MotionChanged is reflect the change accordingly 
      // pm_.Update(EnableMoveInput);

      UserInfo playerself = WorldSystem.Instance.GetPlayerSelf();
      if (null == playerself)
        return;

      Vector3 pos = playerself.GetMovementStateInfo().GetPosition3D();
      Vector3 mouse_pos = new Vector3(GfxSystem.GetMouseX(), GfxSystem.GetMouseY(), GfxSystem.GetMouseZ());//GfxSystem.Instance.MainScene.GetMousePos(pos.Y);

      if (pm_.MotionStatus == PlayerMovement.Motion.Moving) {
        if (pm_.MotionChanged) {
          WorldSystem.Instance.InputMoveDir = pm_.MoveDir;
          playerself.GetMovementStateInfo().SetWantMoveDir(pm_.MoveDir);

          if (WorldSystem.Instance.IsPveScene()) {
            playerself.GetMovementStateInfo().SetMoveDir(pm_.MoveDir);
            playerself.GetMovementStateInfo().IsMoving = true;
            playerself.GetMovementStateInfo().TargetPosition = Vector3.Zero;
          } else {
            NetworkSystem.Instance.SyncPlayerMoveStart((float)pm_.MoveDir);
          }

          if (EnableRotateInput) {
            MovementStateInfo msi = playerself.GetMovementStateInfo();
            msi.SetFaceDir(pm_.MoveDir);
            NetworkSystem.Instance.SyncFaceDirection((float)pm_.MoveDir);
          }
        }
      } else {
        if (pm_.MotionChanged) {
          WorldSystem.Instance.LastMoveDirAdjust = 0;

          if (WorldSystem.Instance.IsPveScene()) {
            playerself.GetMovementStateInfo().IsMoving = false;
          } else {
            NetworkSystem.Instance.SyncPlayerMoveStop();
          }
        }
      }

      old_mouse_pos_ = mouse_pos_;
      mouse_pos_.X = GfxSystem.GetMouseX();
      mouse_pos_.Y = GfxSystem.GetMouseY();

      UserAiStateInfo aiInfo = playerself.GetAiStateInfo();
      if (null != aiInfo && (int)AiStateId.Idle == aiInfo.CurState) {
        m_lastSelectObjId = -1;
      }
    }

    private void SwitchHero(int key_code, int what)
    {
      if ((int)Keyboard.Event.Up == what) {
        if (WorldSystem.Instance.IsPveScene()) {
          WorldSystem.Instance.ChangeHero();
        } else {
          //多人情形切英雄还不知道需求
        }
      }
    }

    private void SwitchDebug(int key_code, int what)
    {
      WorldSystem.Instance.SwitchDebug();
    }

    private void SwitchObserver(int key_code, int what)
    {
      WorldSystem.Instance.SwitchObserver();
    }

    private void InteractObject(int key_code, int what)
    {
      UserInfo myself = WorldSystem.Instance.GetPlayerSelf();
      if (myself != null && myself.IsDead()) {
        GfxSystem.PublishGfxEvent("ge_move_camera_to_next_friend", "script", -1, -1);
      }
      WorldSystem.Instance.InteractObject();
    }

    private void PrintPosition(int key_code, int what)
    {
      UserInfo myself = WorldSystem.Instance.GetPlayerSelf();
      if (null != myself) {
        Vector3 pos = myself.GetMovementStateInfo().GetPosition3D();
        float dir = myself.GetMovementStateInfo().GetFaceDir();
        LogSystem.Info("PrintPosition {0:F2} {1:F2} {2:F2} {3:F2}", pos.X, pos.Y, pos.Z, dir);
      }
    }

    private void StopFindPath(UserInfo playerself, UserAiStateInfo aiInfo)
    {
      if (null == playerself || null == aiInfo) {
        return;
      }
      AiData_UserSelf_General data = playerself.GetAiStateInfo().AiDatas.GetData<AiData_UserSelf_General>();
      if (null == data) {
        data = new AiData_UserSelf_General();
        playerself.GetAiStateInfo().AiDatas.AddData(data);
      }
      playerself.GetMovementStateInfo().IsMoving = false;
      aiInfo.Time = 0;
      data.Time = 0;
      data.FoundPath.Clear();
      aiInfo.ChangeToState((int)AiStateId.Idle);
    }

    private void FindPath(UserInfo playerself, Vector3 targetpos, float towards)
    {
      CharacterView view = EntityManager.Instance.GetUserViewById(playerself.GetId());
      if (view != null && view.ObjectInfo.IsGfxMoveControl && Vector3.Zero != targetpos) {
        return;
      }
      playerself.GetMovementStateInfo().SetFaceDir(towards);
      PlayerMovement.Motion m = Vector3.Zero == targetpos ? PlayerMovement.Motion.Stop : PlayerMovement.Motion.Moving;
      pm_.MotionChanged = pm_.MotionStatus != m || m_lastDir != towards;
      m_lastDir = towards;
      pm_.MotionStatus = m;
      pm_.MoveDir = towards;
      if (Vector3.Zero == targetpos) {
        pm_.MotionStatus = PlayerMovement.Motion.Stop;
      }

      /*
      UserAiStateInfo aiInfo = playerself.GetAiStateInfo();
      if(null == aiInfo)
        return;
      if (Vector3.Zero == targetpos) {
        StopFindPath(playerself, aiInfo);
        return;
      }
      bool ret = (m_lastDir > towards) ? ((m_lastDir - towards) > Math.PI / 4.0f) : ((towards - m_lastDir) > Math.PI / 4.0f);
      if (ret) {
        StopFindPath(playerself, aiInfo);
        m_lastDir = towards;
      }
      aiInfo.TargetPos = targetpos;
      aiInfo.ChangeToState((int)AiStateId.Move);
      */
    }

    private void UpdateTowards(UserInfo playerself, float towards)
    {
      if (null != playerself && float.NegativeInfinity != towards) {
        playerself.GetMovementStateInfo().SetFaceDir(towards);
        playerself.GetMovementStateInfo().SetMoveDir(towards);
      }
    }

    private void RevokeSkill(UserInfo playerself, UserAiStateInfo aiInfo)
    {
      if (null == playerself || null == aiInfo) {
        return;
      }
      StopFindPath(playerself, aiInfo);
      aiInfo.Target = 0;
      aiInfo.IsAttacked = false;
      aiInfo.Time = 0;
      aiInfo.TargetPos = Vector3.Zero;
      aiInfo.AttackRange = 0;
      aiInfo.ChangeToState((int)AiStateId.Idle);
    }

    private void PushSkill(UserInfo playerself, Vector3 targetpos, float attackrange)
    {
      if (null != playerself && Vector3.Zero != targetpos) {
        UserAiStateInfo info = playerself.GetAiStateInfo();
        RevokeSkill(playerself, info);
        info.Time = 0;
        info.TargetPos = targetpos;
        info.AttackRange = attackrange;
        info.IsAttacked = false;
        info.ChangeToState((int)AiStateId.Combat);
      }
    }

    private void Combat(UserInfo playerself, int targetId, float attackrange)
    {
      if (null != playerself && m_lastSelectObjId != targetId) {
        UserAiStateInfo info = playerself.GetAiStateInfo();
        if ((int)AiStateId.Move == info.CurState) {
          StopFindPath(playerself, info);
        }
        info.Time = 0;
        info.Target = targetId;
        info.IsAttacked = false;
        info.AttackRange = attackrange;

        info.ChangeToState((int)AiStateId.Combat);
      }
    }
    
    private void TouchHandle(int what, GestureArgs e)
    {
      UserInfo playerself = WorldSystem.Instance.GetPlayerSelf();
      if (null == playerself || null == e)
        return;
      if ((int)TouchEvent.Cesture == what) {
        string ename = e.name;
        if (GestureEvent.OnSingleTap.ToString() == ename) {
          if (EnableMoveInput) {
            if (WorldSystem.Instance.IsPveScene()) {
              if (e.selectedObjID < 0) {
                FindPath(playerself, new Vector3(e.airWelGamePosX, e.airWelGamePosY, e.airWelGamePosZ), e.towards);
              } else {
                Combat(playerself, e.selectedObjID, e.attackRange); 
              }
              m_lastSelectObjId = e.selectedObjID;
              ///
              GfxSystem.PublishGfxEvent("Op_InputEffect", "Input", GestureEvent.OnSingleTap, e.airWelGamePosX, e.airWelGamePosY, e.airWelGamePosZ, e.selectedObjID < 0 ? false : true, true);
            } else {
              Vector3 world_pos = new Vector3(GfxSystem.GetTouchRayPointX(), GfxSystem.GetTouchRayPointY(), GfxSystem.GetTouchRayPointZ());
              NetworkSystem.Instance.SyncPlayerFindPath(world_pos);
            }
          }
        } else if (GestureEvent.OnFingerMove.ToString() == ename) {
          if (EnableMoveInput) {
            if (TouchType.Attack == e.moveType) {
              UpdateTowards(playerself, e.towards);
            }
          }
        } else if (GestureEvent.OnSkillStart.ToString() == ename) {
          if (null != playerself) {
            UserAiStateInfo info = playerself.GetAiStateInfo();
            if ((int)AiStateId.Move == info.CurState) {
              StopFindPath(playerself, info);
            }
          }
        } else if (GestureEvent.OnEasyGesture.ToString() == ename) {
          Vector3 targetpos = new Vector3(e.startGamePosX, e.startGamePosY, e.startGamePosZ);
          if(Vector3.Zero != targetpos){
            PushSkill(playerself, targetpos, e.attackRange);
          }
        }

        //LogSystem.Debug("userid:{0}, input event:{1}", playerself.GetId(), e.name);
      }
    }

    // members
    private float m_lastDir = -1f;
    private int m_lastSelectObjId = -1;
    private long m_LastTickTime = 0;
    private long m_LastTickIntervalMs = 0;
    private PlayerMovement pm_;
    private ScriptRuntime.Vector2 old_mouse_pos_;
    private ScriptRuntime.Vector2 mouse_pos_;
  }

  class PlayerMovement
  {
    private enum KeyIndex
    {
      W = 0,
      A,
      S,
      D
    }
    private enum KeyHit
    {
      None = 0,
      Up = 1,
      Down = 2,
      Left = 4,
      Right = 8,
    }

    public enum Motion
    {
      Moving,
      Stop,
    }

    public PlayerMovement()
    {
      MoveDir = 0;
      MotionStatus = Motion.Stop;
      MotionChanged = false;
      last_key_hit_ = KeyHit.None;
    }

    public void Update(bool move_enable)
    {
      UserInfo playerself = WorldSystem.Instance.GetPlayerSelf();
      if (playerself == null || playerself.IsDead()) {
        return;
      }
      KeyHit kh = KeyHit.None;
      if (move_enable) {
        if (DashFireSpatial.SpatialObjType.kNPC == playerself.GetRealControlledObject().SpaceObject.GetObjType()) {
          NpcInfo npcInfo = playerself.GetRealControlledObject().CastNpcInfo();
          if (null != npcInfo) {
            if (!npcInfo.CanMove) {
              return;
            }
          }
        }
        if (GfxSystem.IsKeyPressed(GetKeyCode(KeyIndex.W)))
          kh |= KeyHit.Up;
        if (GfxSystem.IsKeyPressed(GetKeyCode(KeyIndex.A)))
          kh |= KeyHit.Left;
        if (GfxSystem.IsKeyPressed(GetKeyCode(KeyIndex.S)))
          kh |= KeyHit.Down;
        if (GfxSystem.IsKeyPressed(GetKeyCode(KeyIndex.D)))
          kh |= KeyHit.Right;
      }

      Motion m = kh == KeyHit.None ? Motion.Stop : Motion.Moving;
      MotionChanged = MotionStatus != m || last_key_hit_ != kh;

      if (MotionChanged) {
        //LogSystem.Debug("MotionChanged:{0}!={1} || {2}!={3}", MotionStatus, m, last_key_hit_, kh);
      }

      last_key_hit_ = kh;
      MotionStatus = m;
      MoveDir = CalcMoveDir(kh);
      if (MoveDir < 0) {
        MotionStatus = Motion.Stop;
      }
      if (MotionChanged) {
        //GfxSystem.GfxLog(string.Format("InputMoveDir:{0} Pos:{1}", MoveDir, playerself.GetMovementStateInfo().GetPosition3D().ToString()));
      }
    }

    public float MoveDir { get; set; }
    public bool MotionChanged { get; set; }
    public Motion MotionStatus { get; set; }

    private Keyboard.Code GetKeyCode(KeyIndex index)
    {
      Keyboard.Code ret = Keyboard.Code.W;
      if (index >= KeyIndex.W && index <= KeyIndex.D) {
        Keyboard.Code[] list = s_Normal;
        if (WorldSystem.Instance.IsPvpScene()) {
          int campId = WorldSystem.Instance.CampId;
          if (campId == (int)CampIdEnum.Blue)
            list = s_Blue;
          else if (campId == (int)CampIdEnum.Red)
            list = s_Red;
        }
        ret = list[(int)index];
      }
      return ret;
    }

    /**
      * @brief 计算移动方向
      *           0       
      *          /|\
      *           |
      *3π/2 -----*-----> π/2
      *           |
      *           |
      *           |
      *           π
      */
    private float CalcMoveDir(KeyHit kh)
    {
      return s_MoveDirs[(int)kh];
    }

    private KeyHit last_key_hit_;

    private static readonly Keyboard.Code[] s_Normal = new Keyboard.Code[] { Keyboard.Code.W, Keyboard.Code.A, Keyboard.Code.S, Keyboard.Code.D };
    private static readonly Keyboard.Code[] s_Blue = new Keyboard.Code[] { Keyboard.Code.A, Keyboard.Code.S, Keyboard.Code.D, Keyboard.Code.W };
    private static readonly Keyboard.Code[] s_Red = new Keyboard.Code[] { Keyboard.Code.D, Keyboard.Code.W, Keyboard.Code.A, Keyboard.Code.S };
      //                                                          N   U  D        UD  L            UL           DL
    private static readonly float[] s_MoveDirs = new float[] { -1,  0, (float)Math.PI, -1, 3*(float)Math.PI/2, 7*(float)Math.PI/4, 5*(float)Math.PI/4, 
      //                    UDL          R          UR         DR           UDR        LR  ULR  LRD      UDLR
                            3*(float)Math.PI/2, (float)Math.PI/2, (float)Math.PI/4, 3*(float)Math.PI/4, (float)Math.PI/2, -1, 0,   (float)Math.PI, -1 };
  }
}