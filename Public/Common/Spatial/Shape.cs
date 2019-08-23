﻿/**
 * @file   Shape.cs
 * @author carl <zhangnaisheng@cyou-inc.com>
 * @date   Thu Apr 25 15:45:11 2013
 * 
 * @brief  碰撞形状定义
 * 
 * 
 */

using System;
using System.Collections.Generic;
using ScriptRuntime;
using DashFire;

namespace DashFireSpatial
{
  public enum ShapeType
  {
    kShapePolygon = 0,
    kShapeCircle,
    kLine,
    kShapeUnknow
  };

  public enum PolygonType
  {
    kPolygonRect = 0,
    kPolygonUnknow
  };

  public class Shape : ICloneable
  {
    public virtual ShapeType GetShapeType()
    {
      return ShapeType.kShapeUnknow;
    }
    public virtual Vector3 GetCenter()
    {
      return new Vector3(0, 0, 0);
    }
    public virtual float GetRadius()
    {
      return 1.0f;
    }

    public virtual void GetShadowOnAxis(Vector3 axis_vector, out Vector3 min, out Vector3 max)
    {
      min = new Vector3(0, 0, 0);
      max = new Vector3(0, 0, 0);
    }

    //坐标变换
    public static Vector3 TransformToWorldPos(Vector3 sys_pos, 
                                            Vector3 relative_pos, float cos_angle, float sin_angle)
    {
      Vector3 new_pos = new Vector3(0, sys_pos.Y, 0);
      new_pos.X = (float)(relative_pos.X * cos_angle + relative_pos.Z * sin_angle + sys_pos.X);
      new_pos.Y = sys_pos.Y + relative_pos.Y;
      new_pos.Z = (float)(relative_pos.Z * cos_angle - relative_pos.X * sin_angle + sys_pos.Z);
      return new_pos;
    }

    public virtual void IdenticalTransform()
    {
    }

    public virtual void Transform(Vector3 owner_sys_pos, float cos_angle, float sin_angle)
    {
    }

    public virtual object Clone()
    {
      return new Shape();
    }
  }

  public class Line : Shape
  {
    public Line(Vector3 start, Vector3 end)
    {
      start_ = start;
      end_ = end;
      center_ = (start_ + end_) / 2;
      radius_ = Geometry.Distance(start_, end_) / 2;
    }

    public override ShapeType GetShapeType()
    {
      return ShapeType.kLine;
    }
    public override Vector3 GetCenter()
    {
      return center_;
    }
    public override float GetRadius()
    {
      return radius_;
    }

    public override void IdenticalTransform()
    {
      world_vertex_.Clear();
      world_vertex_.Add(start_);
      world_vertex_.Add(end_);
    }

    public override void Transform(Vector3 owner_sys_pos, float cos_angle, float sin_angle)
    {
      world_vertex_.Clear();
      world_vertex_.Add(Shape.TransformToWorldPos(owner_sys_pos, start_, cos_angle, sin_angle));
      world_vertex_.Add(Shape.TransformToWorldPos(owner_sys_pos, end_, cos_angle, sin_angle));

      center_ = (world_vertex_[0] + world_vertex_[1]) / 2;
    }

    public override void GetShadowOnAxis(Vector3 axis_vector, out Vector3 min, out Vector3 max)
    {
      float p00 = 1 - (float)Math.Pow(axis_vector.X, 2);
      float p01 = -axis_vector.X * axis_vector.Z;
      float p10 = p01;
      float p11 = 1 - (float)Math.Pow(axis_vector.Z, 2);

      List<Vector3> one_list = world_vertex_;
      min = new Vector3(0, 0, 0);
      max = new Vector3(0, 0, 0);
      for (int i = 0; i < one_list.Count; i++) {
        Vector3 pass_pos = new Vector3(0, 0, 0);
        pass_pos.X = (float)(one_list[i].X * p00 + one_list[i].Z * p10);
        pass_pos.Z = (float)(one_list[i].X * p01 + one_list[i].Z * p11);
        if (i == 0) {
          min = pass_pos;
          max = pass_pos;
        }
        if (pass_pos.X < min.X ||
            (pass_pos.X == min.X && pass_pos.Z < min.Z)) {
          min = pass_pos;
        }
        if (pass_pos.X > max.X ||
            (pass_pos.X == max.X && pass_pos.Z > max.Z)) {
          max = pass_pos;
        }
      }
    }

    public override object Clone()
    {
      return new Line(start_, end_);
    }

    public Vector3 GetStartPos() { return start_; }
    public Vector3 GetEndPos() { return end_; }

    public virtual List<Vector3> world_vertex() { return world_vertex_; }
    protected List<Vector3> world_vertex_ = new List<Vector3>();
    private Vector3 start_;
    private Vector3 end_;
    private Vector3 center_;
    private float radius_ = 0;
  }

  public class Polygon : Shape
  {
    public override ShapeType GetShapeType()
    {
      return ShapeType.kShapePolygon;
    }
    public override Vector3 GetCenter()
    {
      RecalcCenterAndRadius();
      return center_;
    }
    public override float GetRadius()
    {
      RecalcCenterAndRadius();
      return radius_;
    }

    public virtual PolygonType GetPolygonType()
    {
      return PolygonType.kPolygonUnknow;
    }

    public void AddVertex(Vector3 pos)
    {
      if (relation_vertex_ == null) {
        relation_vertex_ = new List<Vector3>();
      }
      relation_vertex_.Add(pos);
      MarkRecalc();
    }

    public void AddVertex(float x, float y)
    {
      if (relation_vertex_ == null) {
        relation_vertex_ = new List<Vector3>();
      }
      relation_vertex_.Add(new Vector3((float)x, 0, (float)y));
      MarkRecalc();
    }

    public override void IdenticalTransform()
    {
      if (world_vertex_ == null) {
        world_vertex_ = new List<Vector3>();
      }
      world_vertex_.Clear();
      world_vertex_.AddRange(relation_vertex_);
    }

    public override void Transform(Vector3 owner_sys_pos, float cos_angle, float sin_angle)
    {
      if (world_vertex_== null) {
        world_vertex_= new List<Vector3>();
      }
      world_vertex_.Clear();
      for (int i = 0; i < relation_vertex_.Count; i++) {
        world_vertex_.Add(Shape.TransformToWorldPos(
                                                  owner_sys_pos, relation_vertex_[i], cos_angle, sin_angle));
      }
      MarkRecalc();
      RecalcCenterAndRadius();
      center_ = Shape.TransformToWorldPos(owner_sys_pos, center_, cos_angle, sin_angle);
    }

    public override void GetShadowOnAxis(Vector3 axis_vector, out Vector3 min, out Vector3 max)
    {
      float p00 = 1 - (float)Math.Pow(axis_vector.X, 2);
      float p01 = -axis_vector.X * axis_vector.Z;
      float p10 = p01;
      float p11 = 1 - (float)Math.Pow(axis_vector.Z, 2);

      List<Vector3> one_list = world_vertex_;
      min = new Vector3(0, 0, 0);
      max = new Vector3(0, 0, 0);
      for (int i = 0; i < one_list.Count; i++) {
        Vector3 pass_pos = new Vector3(0, 0, 0);
        pass_pos.X = (float)(one_list[i].X * p00 + one_list[i].Z * p10);
        pass_pos.Z = (float)(one_list[i].X * p01 + one_list[i].Z * p11);
        if (i == 0) {
          min = pass_pos;
          max = pass_pos;
        }
        if (pass_pos.X < min.X ||
            (pass_pos.X == min.X && pass_pos.Z < min.Z)) {
          min = pass_pos;
        }
        if (pass_pos.X > max.X ||
            (pass_pos.X == max.X && pass_pos.Z > max.Z)) {
          max = pass_pos;
        }
      }
    }

    public override object Clone()
    {
      Polygon pl = new Polygon();
      for (int i = 0; i < relation_vertex_.Count; i++) {
        pl.AddVertex(relation_vertex_[i]);
      }
      return pl;
    }

    public virtual List<Vector3> world_vertex() { return world_vertex_; }

    protected void MarkRecalc()
    {
      need_recalc_ = true;
    }
    protected void RecalcCenterAndRadius()
    {
      if (need_recalc_) {
        float x=0, y=0, z=0;
        foreach (Vector3 pt in relation_vertex_) {
          x += pt.X;
          y += pt.Y;
          z += pt.Z;
        }
        int ct=relation_vertex_.Count;
        center_ = new Vector3(x / ct, y / ct, z / ct);
        radius_ = 0;
        foreach (Vector3 pt in relation_vertex_) {
          radius_ = Math.Max(radius_, Geometry.Distance(center_, pt));
        }        
        need_recalc_ = false;
      }
    }

    protected List<Vector3> relation_vertex_;
    protected List<Vector3> world_vertex_;
    private bool need_recalc_ = true;
    private Vector3 center_;
    private float radius_ = 0;
  }

  public class Rect : Polygon
  {
    public Rect(float width, float height)
    {
      this.width_ = width;
      this.height_ = height;
      relation_vertex_ = new List<Vector3>();
      world_vertex_ = new List<Vector3>();
      AddVertex(-width / 2, height / 2);
      AddVertex(-width / 2, -height / 2);
      AddVertex(width / 2, -height / 2);
      AddVertex(width / 2, height / 2);
    }

    /*
     * 创建以pos为中心，width为宽度， height为高度的矩形
     */
    public Rect(Vector3 pos, float width, float height)
    {
      this.width_ = width;
      this.height_ = height;
      relation_vertex_ = new List<Vector3>();
      world_vertex_ = new List<Vector3>();
      AddVertex(pos.X - width / 2, pos.Z + height / 2);
      AddVertex(pos.X - width / 2, pos.Z - height / 2);
      AddVertex(pos.X + width / 2, pos.Z - height / 2);
      AddVertex(pos.X + width / 2, pos.Z + height / 2);
    }

    public override PolygonType GetPolygonType()
    {
      return PolygonType.kPolygonRect;
    }

    public override object Clone()
    {
      return new Rect(width_, height_);
    }

    public float width() { return width_; }
    public float height() { return height_; }

    private float width_;
    private float height_;
  }

  public class Circle : Shape
  {
    public Circle(Vector3 centerpos, float radius)
    {
      relate_center_pos_ = centerpos;
      world_center_pos_ = relate_center_pos_;
      radius_ = radius;
    }

    public override ShapeType GetShapeType()
    {
      return ShapeType.kShapeCircle;
    }
    public override Vector3 GetCenter()
    {
      return world_center_pos_;
    }
    public override float GetRadius()
    {
      return radius_;
    }

    public override void IdenticalTransform()
    {
      world_center_pos_ = relate_center_pos_;
    }

    public override void Transform(Vector3 owner_sys_pos, float cos_angle, float sin_angle)
    {
      world_center_pos_ = Shape.TransformToWorldPos(owner_sys_pos, 
                                                  relate_center_pos_, cos_angle, sin_angle);
    }

    public override void GetShadowOnAxis(Vector3 axis_vector, out Vector3 min, out Vector3 max)
    {
      float p00 = 1 - (float)Math.Pow(axis_vector.X, 2);
      float p01 = -axis_vector.X * axis_vector.Z;
      float p10 = p01;
      float p11 = 1 - (float)Math.Pow(axis_vector.Z, 2);

      //计算圆的最大最小透影
      min = new Vector3(0, 0, 0);
      max = new Vector3(0, 0, 0);
      Vector3 center = world_center_pos_;
      Vector3 center_pass = new Vector3(0, 0, 0);
      center_pass.X = (float)(center.X * p00 + center.Z * p10);
      center_pass.Z = (float)(center.X * p01 + center.Z * p11);
      float half_x = radius_ * Math.Abs(axis_vector.Z);
      min.X = (float)(center_pass.X - half_x);
      max.X = (float)(center_pass.X + half_x);
      float half_y = radius_ * Math.Abs(axis_vector.X);
      min.Z = (float)(center_pass.Z - half_y);
      max.Z = (float)(center_pass.Z + half_y);
    }

    public override object Clone()
    {
      return new Circle(relate_center_pos_, radius_);
    }

    public void set_relate_center_pos(Vector3 pos) { relate_center_pos_ = pos; }
    public Vector3 relate_center_pos() { return relate_center_pos_; }
    public void set_world_center_pos(Vector3 pos) { world_center_pos_ = pos; }
    public Vector3 world_center_pos() { return world_center_pos_; }
    public float radius() { return radius_; }

    private Vector3 relate_center_pos_;    //相对的圆心位置
    private Vector3 world_center_pos_;     //世界圆心位置
    private float radius_ = 0;
  }
}
