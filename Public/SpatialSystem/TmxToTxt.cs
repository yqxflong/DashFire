﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using DashFire;

namespace DashFireSpatial
{
  public sealed class TmxToTxt
  {
    public bool ParseTiledData(string xml_file, float mapwidth, float mapheight, float scale, out string txt)
    {
      txt = "";
      XmlDocument xmldoc = new XmlDocument();
      System.IO.Stream ms = null;
      try {
        ms = DashFire.FileReaderProxy.ReadFileAsMemoryStream(xml_file);
        if (ms == null) { return false; }
        xmldoc.Load(ms);
      } catch (System.IO.FileNotFoundException ex) {
        LogSystem.Error("config xml file {0} not find!\n{1}", xml_file, ex.Message);
        return false;
      } catch (Exception ex) {
        LogSystem.Error("parse xml file {0} error!\n{1}", xml_file, ex.Message);
        return false;
      } finally {
        if (ms != null) {
          ms.Close();
        }
      }

      XmlNode root = xmldoc.SelectSingleNode("map");
      XmlNodeList objgroups = root.SelectNodes("objectgroup");
      StringBuilder sb = new StringBuilder();
      sb.AppendLine("类型\t名称\t是否多边形\t点XZ列表");
      foreach (XmlNode objgroup in objgroups) {
        string groupName = objgroup.Attributes["name"].Value;
        foreach (XmlNode obj in objgroup.ChildNodes) {
          string objName = obj.Attributes["name"].Value;
          sb.Append(groupName).Append("\t").Append(objName).Append("\t");
          TiledData td = new TiledData(mapwidth * scale, mapheight * scale);
          if (td.CollectDataFromXml(obj)) {
            sb.Append(td.IsPolygon).Append("\t");
            AppendPointList(td.GetPoints(), scale, sb);
            sb.AppendLine();
          }
        }
      }
      txt = sb.ToString();
      return true;
    }
    private void AppendPointList(IList<ScriptRuntime.Vector3> pts, float scale, StringBuilder sb)
    {
      bool first = true;
      foreach (ScriptRuntime.Vector3 pt in pts) {
        if (first)
          first = false;
        else
          sb.Append(" ");
        sb.AppendFormat("{0:F2} {1:F2}", pt.X / scale, pt.Z / scale);
      }
    }
  }
}
