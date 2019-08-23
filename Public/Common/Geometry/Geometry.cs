﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ScriptRuntime;
using DashFire;

namespace DashFire
{
  public partial class Geometry
  {
    public const float c_FloatPrecision = 0.0001f;
    public const float c_DoublePrecision = 0.0000001f;
    public static bool IsInvalid(float v)
    {
      return float.IsNaN(v) || float.IsInfinity(v);
    }
    public static bool IsInvalid(Vector3 pos)
    {
      return IsInvalid(pos.X) || IsInvalid(pos.Y) || IsInvalid(pos.Z);
    }
    public static float Max(float a, float b)
    {
      return (a > b ? a : b);
    }
    public static float Min(float a, float b)
    {
      return (a < b ? a : b);
    }
    public static float DistanceSquare(Vector3 p1, Vector3 p2)
    {
      return (p1.X - p2.X) * (p1.X - p2.X) + (p1.Z - p2.Z) * (p1.Z - p2.Z);
    }
    public static float Distance(Vector3 p1, Vector3 p2)
    {
      return (float)Math.Sqrt(DistanceSquare(p1, p2));
    }
    public static bool IsSameFloat(float v1, float v2)
    {
      return Math.Abs(v1 - v2) < c_FloatPrecision;
    }
    public static bool IsSameDouble(float v1, float v2)
    {
      return Math.Abs(v1 - v2) < c_DoublePrecision;
    }
    public static bool IsSamePoint(Vector3 p1, Vector3 p2)
    {
      return IsSameFloat(p1.X, p2.X) && IsSameFloat(p1.Z, p2.Z);
    }
    /// <summary>
    /// 计算向量叉乘
    /// </summary>
    /// <param name="sp"></param>
    /// <param name="ep"></param>
    /// <param name="op"></param>
    /// <returns>
    /// ret>0 ep在opsp矢量逆时针方向
    /// ret=0 共线
    /// ret<0 ep在opsp矢量顺时针方向
    /// </returns>
    public static float Multiply(Vector3 sp, Vector3 ep, Vector3 op)
    {
      return ((sp.X - op.X) * (ep.Z - op.Z) - (ep.X - op.X) * (sp.Z - op.Z));
    }
    /// <summary>
    /// r=DotMultiply(p1,p2,p0),得到矢量(p1-p0)和(p2-p0)的点积。
    /// 注：两个矢量必须是非零矢量
    /// </summary>
    /// <param name="p1"></param>
    /// <param name="p2"></param>
    /// <param name="p0"></param>
    /// <returns>
    /// r>0:两矢量夹角为锐角；
    /// r=0：两矢量夹角为直角；
    /// r<0:两矢量夹角为钝角
    /// </returns>
    public static float DotMultiply(Vector3 p1, Vector3 p2, Vector3 p0)
    {
      return ((p1.X - p0.X) * (p2.X - p0.X) + (p1.Z - p0.Z) * (p2.Z - p0.Z));
    }
    /// <summary>
    /// 判断p与线段p1p2的关系
    /// </summary>
    /// <param name="p"></param>
    /// <param name="p1"></param>
    /// <param name="p2"></param>
    /// <returns>
    /// r=0 p = p1
    /// r=1 p = p2
    /// r<0 p is on the backward extension of p1p2
    /// r>1 p is on the forward extension of p1p2
    /// 0<r<1 p is interior to p1p2
    /// </returns>
    public static float Relation(Vector3 p, Vector3 p1, Vector3 p2)
    {
      return DotMultiply(p, p2, p1) / DistanceSquare(p1, p2);
    }
    /// <summary>
    /// 求垂足
    /// </summary>
    /// <param name="p"></param>
    /// <param name="p1"></param>
    /// <param name="p2"></param>
    /// <returns></returns>
    public static Vector3 Perpendicular(Vector3 p, Vector3 p1, Vector3 p2)
    {
      float r = Relation(p, p1, p2);
      Vector3 tp=new Vector3();
      tp.X = p1.X + r * (p2.X - p1.X);
      tp.Z = p1.Z + r * (p2.Z - p1.Z);
      return tp;
    }
    /// <summary>
    /// 求点到线段的最小距离并返回距离最近的点。
    /// </summary>
    /// <param name="p"></param>
    /// <param name="p1"></param>
    /// <param name="p2"></param>
    /// <param name="np"></param>
    /// <returns></returns>
    public static float PointToLineSegmentDistance(Vector3 p, Vector3 p1, Vector3 p2,out Vector3 np)
    {
      float r=Relation(p,p1,p2);
      if(r<0)
      {
        np=p1;
        return Distance(p,p1);
      }
      if(r>1)
      {
        np=p2;
        return Distance(p,p2);
      }
      np=Perpendicular(p,p1,p2);
      return Distance(p,np);
    }
    /// <summary>
    /// 求点到直线的距离
    /// </summary>
    /// <param name="p"></param>
    /// <param name="p1"></param>
    /// <param name="p2"></param>
    /// <returns></returns>
    public static float PointToLineDistance(Vector3 p,Vector3 p1,Vector3 p2)
    {
      return Math.Abs(Multiply(p,p2,p1))/Distance(p1,p2);
    }   
    /// <summary>
    /// 求点到折线的最小距离并返回最小距离点。
    /// 注：如果给定点不足以够成折线，则不会修改q的值
    /// </summary>
    /// <param name="p"></param>
    /// <param name="pts"></param>
    /// <param name="start"></param>
    /// <param name="len"></param>
    /// <param name="q"></param>
    /// <returns></returns>
    public static float PointToPolylineDistance(Vector3 p,IList<Vector3> pts, int start,int len,ref Vector3 q)
    {
      float ret = float.MaxValue;
      for (int i = start; i < start + len - 1; ++i) {
        Vector3 tq;
        float d = PointToLineSegmentDistance(p, pts[i], pts[i + 1], out tq);
        if (d < ret) {
          ret = d;
          q = tq;
        }
      }
      return ret;
    }
    public static bool Intersect(Vector3 us, Vector3 ue, Vector3 vs, Vector3 ve)
    {
      return ((Max(us.X, ue.X) >= Min(vs.X, ve.X)) &&
      (Max(vs.X, ve.X) >= Min(us.X, ue.X)) &&
      (Max(us.Z, ue.Z) >= Min(vs.Z, ve.Z)) &&
      (Max(vs.Z, ve.Z) >= Min(us.Z, ue.Z)) &&
      (Multiply(vs, ue, us) * Multiply(ue, ve, us) >= 0) &&
      (Multiply(us, ve, vs) * Multiply(ve, ue, vs) >= 0));
    }
    public static bool LineIntersectRectangle(Vector3 lines, Vector3 linee, float left, float top, float right, float bottom)
    {
      if (Max(lines.X, linee.X) >= left && right >= Min(lines.X, linee.X) && Max(lines.Z, linee.Z) >= top && bottom >= Min(lines.Z, linee.Z)) {
        Vector3 lt = new Vector3(left, 0, top);
        Vector3 rt = new Vector3(right, 0, top);
        Vector3 rb = new Vector3(right, 0, bottom);
        Vector3 lb = new Vector3(left, 0, bottom);
        if (Multiply(lines, lt, linee) * Multiply(lines, rt, linee) <= 0)
          return true;
        if (Multiply(lines, lt, linee) * Multiply(lines, lb, linee) <= 0)
          return true;
        if (Multiply(lines, rt, linee) * Multiply(lines, rb, linee) <= 0)
          return true;
        if (Multiply(lines, lb, linee) * Multiply(lines, rb, linee) <= 0)
          return true;
        /*
        if (Intersect(lines, linee, lt, rt))
          return true;
        if (Intersect(lines, linee, rt, rb))
          return true;
        if (Intersect(lines, linee, rb, lb))
          return true;
        if (Intersect(lines, linee, lt, lb))
          return true;
        */
      }
      return false;
    }
    public static bool PointOnLine(Vector3 pt, Vector3 pt1, Vector3 pt2)
    {
      float dx, dy, dx1, dy1;
      bool retVal;
      dx = pt2.X - pt1.X;
      dy = pt2.Z - pt1.Z;
      dx1 = pt.X - pt1.X;
      dy1 = pt.Z - pt1.Z;
      if (pt.X == pt1.X && pt.Z == pt1.Z || pt.X == pt2.X && pt.Z == pt2.Z) {//在顶点上
        retVal = true;
      } else {
        if (Math.Abs(dx * dy1 - dy * dx1) < c_FloatPrecision) {//斜率相等//考虑计算误差
          //等于零在顶点上
          if (dx1 * (dx1 - dx) < 0 | dy1 * (dy1 - dy) < 0) {
            retVal = true;
          } else {
            retVal = false;
          }
        } else {
          retVal = false;
        }
      }
      return retVal;
    }
    /// <summary>
    /// 判断a是否在bc直线左侧
    /// </summary>
    public static bool PointIsLeft(Vector3 a, Vector3 b, Vector3 c)
    {
      return Multiply(c, a, b) > 0;
    }
    /// <summary>
    /// 判断a是否在bc直线左侧或线上
    /// </summary>
    public static bool PointIsLeftOn(Vector3 a, Vector3 b, Vector3 c)
    {
      return Multiply(c, a, b) >= 0;
    }
    /// <summary>
    /// 判断a、b、c三点是否共线
    /// </summary>
    public static bool PointIsCollinear(Vector3 a, Vector3 b, Vector3 c)
    {
      return Multiply(c, a, b) == 0;
    }
    public static bool IsCounterClockwise(IList<Vector3> pts, int start, int len)
    {
      bool ret = false;
      if (len >= 3) {
        float maxx = pts[start].X;
        float maxz = pts[start].Z;
        float minx = maxx;
        float minz = maxz;
        int maxxi = 0;
        int maxzi = 0;
        int minxi = 0;
        int minzi = 0;
        for (int i = 1; i < len; ++i) {
          int ix = start + i;
          float x = pts[ix].X;
          float z = pts[ix].Z;
          if (maxx < x) {
            maxx = x;
            maxxi = i;
          } else if (minx > x) {
            minx = x;
            minxi = i;
          }
          if (maxz < z) {
            maxz = z;
            maxzi = i;
          } else if (minz > z) {
            minz = z;
            minzi = i;
          }
        }
        int val = 0;
        for (int delt = 0; delt < len; delt++) {
          int ix0 = start + (len * 2 + maxxi - delt - 1) % len;
          int ix1 = start + maxxi;
          int ix2 = start + (maxxi + delt + 1) % len;

          float r = Multiply(pts[ix1], pts[ix2], pts[ix0]);
          if (r > 0) {
            val++;
            break;
          } else if (r < 0) {
            val--;
            break;
          }
        }
        for (int delt = 0; delt < len; delt++) {
          int ix0 = start + (len * 2 + maxzi - delt - 1) % len;
          int ix1 = start + maxzi;
          int ix2 = start + (maxzi + delt + 1) % len;

          float r = Multiply(pts[ix1], pts[ix2], pts[ix0]);
          if (r > 0) {
            val++;
            break;
          } else if (r < 0) {
            val--;
            break;
          }
        }
        for (int delt = 0; delt < len; delt++) {
          int ix0 = start + (len * 2 + minxi - delt - 1) % len;
          int ix1 = start + minxi;
          int ix2 = start + (minxi + delt + 1) % len;

          float r = Multiply(pts[ix1], pts[ix2], pts[ix0]);
          if (r > 0) {
            val++;
            break;
          } else if (r < 0) {
            val--;
            break;
          }
        }
        for (int delt = 0; delt < len; delt++) {
          int ix0 = start + (len * 2 + minzi - delt - 1) % len;
          int ix1 = start + minzi;
          int ix2 = start + (minzi + delt + 1) % len;

          float r = Multiply(pts[ix1], pts[ix2], pts[ix0]);
          if (r > 0) {
            val++;
            break;
          } else if (r < 0) {
            val--;
            break;
          }
        }
        if (val > 0)
          ret=true;
        else
          ret=false;
      }
      return ret;
    }
    /// <summary>
    /// 计算多边形形心与半径
    /// </summary>
    /// <param name="pts"></param>
    /// <param name="start"></param>
    /// <param name="len"></param>
    /// <param name="centroid"></param>
    /// <returns></returns>
    public static float CalcPolygonCentroidAndRadius(IList<Vector3> pts, int start, int len, out Vector3 centroid)
    {
      float ret = 0;
      if (len > 0) {
        float x = 0;
        float z = 0;
        for (int i = 1; i < len; ++i) {
          int ix = start + i;
          x += pts[ix].X;
          z += pts[ix].Z;
        }
        x /= len;
        z /= len;
        centroid = new Vector3(x, 0, z);
        float distSqr = 0;
        for (int i = 1; i < len; ++i) {
          int ix = start + i;
          float tmp = Geometry.DistanceSquare(centroid, pts[ix]);
          if (distSqr < tmp)
            distSqr = tmp;
        }
        ret = (float)Math.Sqrt(distSqr);
      } else {
        centroid = new Vector3();
      }
      return ret;
    }
    /// <summary>
    /// 计算多边形轴对齐矩形包围盒
    /// </summary>
    /// <param name="pts"></param>
    /// <param name="start"></param>
    /// <param name="len"></param>
    /// <param name="minXval"></param>
    /// <param name="maxXval"></param>
    /// <param name="minZval"></param>
    /// <param name="maxZval"></param>
    public static void CalcPolygonBound(IList<Vector3> pts, int start, int len, out float minXval, out float maxXval, out float minZval, out float maxZval)
    {
      maxXval = pts[start].X;
      minXval = pts[start].X;
      maxZval = pts[start].Z;
      minZval = pts[start].Z;
      for (int i = 1; i < len; ++i) {
        int ix = start + i;
        float xv = pts[ix].X;
        float zv = pts[ix].Z;
        if (maxXval < xv)
          maxXval = xv;
        else if (minXval > xv)
          minXval = xv;
        if (maxZval < zv)
          maxZval = zv;
        else if (minZval > zv)
          minZval = zv;
      }
    }
    /// <summary>
    /// 判断点是否在多边形内
    /// </summary>
    /// <typeparam name="PointT"></typeparam>
    /// <param name="pt"></param>
    /// <param name="pts"></param>
    /// <param name="start"></param>
    /// <param name="len"></param>
    /// <returns>
    /// 1  -- 在多边形内
    /// 0  -- 在多边形边上
    /// -1 -- 在多边形外 
    /// </returns>
    public static int PointInPolygon(Vector3 pt, IList<Vector3> pts, int start, int len)
    {
      float maxXval;
      float minXval;
      float maxZval;
      float minZval;
      CalcPolygonBound(pts, start, len, out minXval, out maxXval, out minZval, out maxZval);
      return PointInPolygon(pt, pts, start, len, minXval, maxXval, minZval, maxZval);
    }

    public static int PointInPolygon(Vector3 pt, IList<Vector3> pts, int start, int len, float minXval, float maxXval, float minZval, float maxZval)
    {
      if ((pt.X > maxXval) | (pt.X < minXval) | pt.Z > maxZval | pt.Z < minZval) {
        return -1;//在多边形外
      } else {
        int cnt = 0;
        int lastStatus;
        Vector3 pt0 = pts[start];
        if (pt0.Z - pt.Z > c_FloatPrecision)
          lastStatus = 1;
        else if (pt0.Z - pt.Z < c_FloatPrecision)
          lastStatus = -1;
        else
          lastStatus = 0;

        for (int i = 0; i < len; ++i) {
          int ix1 = start + i;
          int ix2 = start + (i + 1) % len;
          Vector3 pt1 = pts[ix1];
          Vector3 pt2 = pts[ix2];

          if (PointOnLine(pt, pt1, pt2))
            return 0;

          int status;
          if (pt2.Z - pt.Z > c_FloatPrecision)
            status = 1;
          else if (pt2.Z - pt.Z < c_FloatPrecision)
            status = -1;
          else
            status = 0;

          int temp = status - lastStatus;
          lastStatus = status;
          if (temp > 0) {
            if (!PointIsLeftOn(pt, pt1, pt2))
              cnt += temp;
          } else if (temp < 0) {
            if (PointIsLeft(pt, pt1, pt2))
              cnt += temp;
          }
        }
        if (cnt == 0)
          return -1;
        else
          return 1;
      }
    }
  }
  //坐标系为左手坐标系，0度角为z轴正方向，旋转方向为z轴正方向向x轴正向旋转（沿y轴正向看逆时针旋转）
  public partial class Geometry
  {
    public static float GetYAngle(ScriptRuntime.Vector2 fvPos1, ScriptRuntime.Vector2 fvPos2)
    {
      float dDistance = (float)ScriptRuntime.Vector2.Distance(fvPos1, fvPos2);
      if (dDistance <= 0.0f)
        return 0.0f;

      float fACos = (fvPos2.Y - fvPos1.Y) / dDistance;
      if (fACos > 1.0)
        fACos = 0.0f;
      else if (fACos < -1.0)
        fACos = (float)Math.PI;
      else
        fACos = (float)Math.Acos(fACos);

      // [0~180]
      if (fvPos2.X >= fvPos1.X)
        return (float)fACos;
      //(180, 360)
      else
        return (float)(Math.PI * 2 - fACos);
    }
    public static ScriptRuntime.Vector2 GetReflect(ScriptRuntime.Vector2 fvPos1, ScriptRuntime.Vector2 fvPos2, ScriptRuntime.Vector2 v1)
    {
      // pos1 -> pos2
      ScriptRuntime.Vector2 fvNormal = fvPos2 - fvPos1;
      fvNormal.Normalize();
      // pos1 -> v1
      ScriptRuntime.Vector2 fvLine1 = v1 - fvPos1;
      // pos1 -> v2
      ScriptRuntime.Vector2 fvLine2 = fvNormal * (2 * ScriptRuntime.Vector2.Dot(fvLine1, fvNormal)) - fvLine1;
      //return v2
      return fvLine2 + fvPos1;
    }
    public static ScriptRuntime.Vector2 GetRotate(ScriptRuntime.Vector2 fvPos1, float angle)
    {
      // pos1 -> pos2
      ScriptRuntime.Vector2 retV = new ScriptRuntime.Vector2(
          (float)(fvPos1.X * Math.Cos(angle) - fvPos1.Y * Math.Sin(angle)),
          (float)(fvPos1.X * Math.Sin(angle) + fvPos1.Y * Math.Cos(angle))
          );
      return retV;
    }
    // 获取两向量方向性夹角
    public static float GetIntersectionAngle(ScriptRuntime.Vector2 fromV, ScriptRuntime.Vector2 toV)
    {
      return GetRotateAngle(fromV.X, fromV.Y, toV.X, toV.Y);
    }
    // 获取两向量方向性夹角
    public static float GetRotateAngle(float x1, float y1, float x2, float y2)
    {
      const float epsilon = 1.0e-6f;
      float nyPI = (float)Math.Acos(-1.0);
      float dist, dot, angle;

      // normalize
      dist = (float)Math.Sqrt(x1 * x1 + y1 * y1);
      x1 /= dist;
      y1 /= dist;
      dist = (float)Math.Sqrt(x2 * x2 + y2 * y2);
      x2 /= dist;
      y2 /= dist;
      // dot product
      dot = x1 * x2 + y1 * y2;
      if (Math.Abs(dot - 1.0) <= epsilon)
        angle = 0.0f;
      else if (Math.Abs(dot + 1.0) <= epsilon)
        angle = nyPI;
      else {
        float cross;

        angle = (float)Math.Acos(dot);
        //cross product
        cross = x1 * y2 - x2 * y1;
        // vector p2 is clockwise from vector p1 
        // with respect to the origin (0.0)
        if (cross < 0) {
          angle = 2 * nyPI - angle;
        }
      }
      //degree = angle *  180.0 / nyPI;
      return angle;
    }

    public static Vector3 GetProjection(Vector3 v, Vector3 n)
    {
      Vector3 vll = new Vector3();
      float distance_power = DistanceSquare(new Vector3(), n);
      if (distance_power <= 0) { 
        return vll; 
      }
      float dot_value = Dot(v, n) / distance_power;
      vll = Multy(n, dot_value);
      return vll;
    }

    //取得v在n的垂直方向上的投影，n为单伙向量
    public static Vector3 GetVtProjection(Vector3 v, Vector3 n)
    {
      Vector3 vt = new Vector3();
      float v_mag = (float)Math.Sqrt(v.X * v.X + v.Z * v.Z);
      float power_n_mag = n.X * n.X + n.Z * n.Z;
      float k = (v.X * n.X + v.Z * n.Z) / (n.X * n.X + n.Z * n.Z);
      vt.X = v.X - n.X * k;
      vt.Z = v.Z - n.Z * k;
      return vt;
    }

    public static Vector3 Multy(Vector3 vect, float value)
    {
      return new Vector3(vect.X * value, vect.Y * value, vect.Z * value);
    }

    public static float GetVectorAngle(Vector3 from, Vector3 to)
    {
      float from_mod = GetMod(from);
      float to_mod = GetMod(to);
      if (from_mod * to_mod < 0.001) {
        return 0;
      }
      float cos_value = Dot(from, to) / (from_mod * to_mod);
      if (cos_value > 1) {
        cos_value = 1;
      }
      float degree = (float)Math.Acos(cos_value);
      return degree;
    }
    public static float GetMod(Vector3 v)
    {
      return (float)Math.Sqrt(v.X * v.X + v.Z * v.Z);
    }

    public static float GetDirFromVector(Vector3 vect)
    {
      return (float)Math.Atan2(vect.X, vect.Z);
    }
    public static Vector3 GetVectorFromDir(float dir)
    {
      Vector3 v = new Vector3();
      v.X = (float)Math.Sin(dir);
      v.Z = (float)Math.Cos(dir);
      return v;
    }
    public static bool GetRayWithCircleFirstHitPoint(Vector3 ray_start_pos, Vector3 ray_dir, Vector3 center_pos, float radius, out Vector3 hit_point)
    {
      hit_point = new Vector3();
      Vector3 e = new Vector3(center_pos.X - ray_start_pos.X, ray_start_pos.Y, center_pos.Z - ray_start_pos.Z);
      Vector3 d = Normalize(ray_dir);
      float a = Dot(e, d);

      float ray_center_distance = Dot(e, e) - radius * radius;
      if (ray_center_distance < 0) {
        d = new Vector3(-d.X, 0, -d.Z);
        a = Dot(e, d);
      }

      float sqrt_value = -ray_center_distance + a * a;
      // no hit point
      if (sqrt_value < 0) {
        return false;
      }

      float t = (float)(a - Math.Sqrt(sqrt_value));
      hit_point.X = ray_start_pos.X + t * d.X;
      hit_point.Z = ray_start_pos.Z + t * d.Z;
      return true;
    }

    public static Vector3 Add(Vector3 v1, Vector3 v2)
    {
      return new Vector3(v1.X + v2.X, v1.Y + v2.Y, v1.Z + v2.Z);
    }

    public static Vector3 GetClostestPointOnCircle(Vector3 q, Vector3 center, float r)
    {
      Vector3 p = new Vector3();
      Vector3 d = new Vector3(center.X - q.X, 0, center.Z - q.Z);
      if (d.X == 0 && d.Z == 0) {
        d.Z = 1;
      }
      float d_mod = (float)Math.Sqrt(d.X * d.X + d.Z * d.Z);
      float k = (d_mod - r) / d_mod;
      p.X = q.X + k * d.X;
      p.Z = q.Z + k * d.Z;
      p.Y = center.Y;
      return p;
    }

    public static float Dot(Vector3 a, Vector3 b)
    {
      return (a.X * b.X + a.Z * b.Z);
    }
    public static Vector3 Normalize(Vector3 a)
    {
      Vector3 n = new Vector3();
      float distance = (float)Math.Sqrt(a.X * a.X + a.Z * a.Z);
      if (distance == 0) {
        return a;
      }
      n.X = a.X / distance;
      n.Z = a.Z / distance;
      return n;
    }

    public static Vector3 GetVectorDistancePoint(Vector3 start, Vector3 end, float distance_with_start)
    {
      if (Math.Abs(distance_with_start) <= 0.00001) {
        return start;
      }
      Vector3 n = Normalize(end - start);
      Vector3 result = Multy(n, distance_with_start);
      result = Add(start, result);
      result.Y = start.Y;
      return result;
    }
  }
}
