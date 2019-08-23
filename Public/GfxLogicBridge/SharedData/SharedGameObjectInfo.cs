using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DashFire
{
  public sealed class SharedGameObjectInfo
  {
    public int m_LogicObjectId = 0;
    public bool IsFloat = false;
    public float X = 0;
    public float Y = 0;
    public float Z = 0;
    public float Sx = 1;
    public float Sy = 1;
    public float Sz = 1;
    public float Blood = 0;
    public float MaxBlood = 0;
    public float Rage = 0;
    public float MaxRage = 0;
    public float VerticlaSpeed = 0;
    public int StateFlag = 0;
    public bool DataChangedByLogic = false;
    public bool DataChangedByGfx = false;
    public bool IsGfxAnimation = false;
    public bool IsGfxMoveControl = false;
    //逻辑层移动控制
    public bool IsLogicMoving = false;
    public float MoveCos = 0;
    public float MoveSin = 0;
    public float MoveSpeed = 0;
    public float MoveTargetDistanceSqr = 0;
    public float FaceDir = 0;
  }
}
