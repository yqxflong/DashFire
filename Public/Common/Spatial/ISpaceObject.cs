/**
 * @file   ISpaceObject.cs
 * @author carl <zhangnaisheng@cyou-inc.com>
 * @date   Wed Apr 24 10:07:18 2013
 * 
 * @brief  collide module object need implement this interface.
 * 
 * 
 */

using System;
using System.Collections.Generic;
using ScriptRuntime;
using DashFire;

namespace DashFireSpatial
{
  public enum SpatialObjType
  {
    kTypeBegin = 0,
    kUser,
    kNPC,
    kBullet,
    kTypeEnd
  }
  public class BlockType
  {
    public const byte OUT_OF_BLOCK = BlockType.STATIC_BLOCK | BlockType.LEVEL_GROUND;
    public const byte NOT_BLOCK = (byte)'\x00';
    public const byte STATIC_BLOCK = (byte)'\x01';
    public const byte DYNAMIC_BLOCK = (byte)'\x02';
    public const byte RESERVED_BLOCK = (byte)'\x03';
    public const byte TYPE_MASK = (byte)'\x03';
    public const byte SUBTYPE_MASK = (byte)'\x0c';
    public const byte SUBTYPE_OBSTACLE = (byte)'\x00';
    public const byte SUBTYPE_SHOTPROOF = (byte)'\x04';
    public const byte SUBTYPE_ROADBLOCK = (byte)'\x08';
    public const byte SUBTYPE_ENERGYWALL = (byte)'\x0c';
    public const byte BLINDING_MASK = (byte)'\x10';
    public const byte BLINDING_NOTHING = (byte)'\x00';
    public const byte BLINDING_BLINDING = (byte)'\x10';
    public const byte LEVEL_MASK = (byte)'\xe0';
    public const byte LEVEL_UNDERFLOOR_2 = (byte)'\x00';
    public const byte LEVEL_UNDERFLOOR_1 = (byte)'\x20';
    public const byte LEVEL_GROUND = (byte)'\x40';
    public const byte LEVEL_FLOOR_1 = (byte)'\x60';
    public const byte LEVEL_FLOOR_2 = (byte)'\x80';
    public const byte LEVEL_FLOOR_3 = (byte)'\xa0';
    public const byte LEVEL_FLOOR_4 = (byte)'\xc0';
    public const byte LEVEL_FLOOR_BLINDAGE = (byte)'\xe0';

    public static byte GetBlockType(byte status)
    {
      return (byte)(status & TYPE_MASK);
    }
    public static byte GetBlockSubType(byte status)
    {
      return (byte)(status & SUBTYPE_MASK);
    }
    public static byte GetBlockBlinding(byte status)
    {
      return (byte)(status & BLINDING_MASK);
    }
    public static byte GetBlockLevel(byte status)
    {
      return (byte)(status & LEVEL_MASK);
    }
    public static byte GetBlockTypeAndSubType(byte status)
    {
      return (byte)(status & (TYPE_MASK | SUBTYPE_MASK));
    }
    public static byte GetBlockTypeWithoutBlinding(byte status)
    {
      return (byte)(status & (TYPE_MASK | SUBTYPE_MASK | LEVEL_MASK));
    }
    public static byte GetBlockTypeWithoutLevel(byte status)
    {
      return (byte)(status & (TYPE_MASK | SUBTYPE_MASK | BLINDING_MASK));
    }
  };
  public struct CellPos
  {
    public CellPos(int row, int col)
    {
      this.row = row;
      this.col = col;
    }
    public int row;
    public int col;
  };

  public interface ISpaceObject
  {
    uint GetID();
    SpatialObjType GetObjType();
    Vector3 GetPosition();
    float GetRadius();
    Vector3 GetVelocity();
    bool IsAvoidable();
    Shape GetCollideShape();                  // ȡ����ײ��״
    List<ISpaceObject> GetCollideObjects();   // ȡ���뵱ǰ������ײ������
    void OnCollideObject(ISpaceObject obj);   // �����뵱ǰ������ײ������
    void OnDepartObject(ISpaceObject obj);    // ɾ���뵱ǰ������ײ������
    object RealObject {get;}
  }
} // namespace DashFire
