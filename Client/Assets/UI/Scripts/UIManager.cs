using System;
using UnityEngine;
using System.Collections.Generic;

public enum UIType : int
{
  DontLoad = -1,
  NoneActive = 0,
  Active = 1
}
public class UIManager
{
  public void Init(TextAsset config)
  {
    LoadWindowsInfo(config);
  }

  public void Clear()
  {
    m_WindowsInfoDic.Clear();
  }
  public void LoadWindowByName(GameObject father, string name, Camera cam)
  {
    if (null == father)
      return;
    if (m_WindowsInfoDic.ContainsKey(name)) {
      WindowInfo info = m_WindowsInfoDic[name];
      GameObject go = DashFire.ResourceSystem.GetSharedResource(info.windowPath) as GameObject;
      if (null == go) {
        Debug.Log("Err can not find:" + info.windowPath);
        return;
      }
      GameObject child = NGUITools.AddChild(father, go);
      if (null != child && cam != null)
        child.transform.position = cam.ScreenToWorldPoint(info.windowPos);
    }
  }

  public void UnloadWindowByName(GameObject father, string name)
  {

  }

  public void LoadAllWindows(GameObject father, int sceneId, Camera cam)
  {
    Debug.Log("Load all windows");
    if (null == father)
      return;
    foreach (WindowInfo info in m_WindowsInfoDic.Values) {
      if (info.windowType != UIType.DontLoad && sceneId == info.sceneId) {
        
        GameObject go = DashFire.ResourceSystem.GetSharedResource(info.windowPath) as GameObject;
        GameObject child = NGUITools.AddChild(father, go);
        if (info.windowType == UIType.NoneActive) {
          NGUITools.SetActive(child, true);
        }

        info.windowPos.z = 0;

        if (null != child && cam != null)
          child.transform.position = cam.ScreenToWorldPoint(info.windowPos);
      }
    }
  }

  //设置UI是否Visible
  public void SetUiVisibleByConfig(GameObject father,int sceneId, bool visible)
  {
    if (null == father)
      return;

    foreach (WindowInfo info in m_WindowsInfoDic.Values) {
      if (info.windowType == UIType.Active && sceneId == info.sceneId) {
        GameObject go = null;
        Transform trans = father.transform.Find(info.windowName + "(Clone)");
        if (trans != null) {
          go = trans.gameObject;
          if (go != null) {
            NGUITools.SetActive(go, visible);
          }
        }
      } 
    }
  }
  public void SetAllUiVisible(GameObject father, bool visible)
  {
    if(father ==null)
      return;
    for (int index = 0; index < father.transform.childCount; ++index) {
      Transform trans = father.transform.GetChild(index);
      if (trans != null) {
        GameObject go = trans.gameObject;
        NGUITools.SetActive(go, visible);
      }
    }
  }

  //删除配置文件中场景ID为sceneId的UI
  public void UnLoadAllWindow(GameObject parent, int sceneId)
  {
    if (parent == null)
      return;
    //用于记录info.sceneId != sceneId的UI数量
    int count = 0;
    while (parent.transform.childCount > count) {
      Transform trans = parent.transform.GetChild(count);
      if (trans == null)
        continue;
      bool canContinue = false;
      foreach(WindowInfo info in m_WindowsInfoDic.Values) {
        if ((info.windowName + "(Clone)").Equals(trans.name)) {
          if (info.sceneId != sceneId) {
            canContinue = true;
            count++;
            break;
          }
        }
      }
      
      if (canContinue)
        continue;
      NGUITools.Destroy(trans.gameObject);
 
    }
   
  }

  private void LoadWindowsInfo(TextAsset config)
  {
    string[] head = null;
    string[] lines = null;
    if (null != config)
      lines = config.text.Split('\n');
    if (lines.Length > 0) {
      head = lines[0].Split('\t');

    }
    for (int index = 1; index < lines.Length; ++index) {
      string[] data = lines[index].Split('\t');
      WindowInfo wndInfo = new WindowInfo();
      for (int i = 0; i < data.Length; ++i) {
        if (i < head.Length) {
          SetWndInfo(wndInfo, head[i], data[i]);
        }
      }
      if (null != wndInfo) {
        if (!m_WindowsInfoDic.ContainsKey(wndInfo.windowName)) {
          m_WindowsInfoDic.Add(wndInfo.windowName, wndInfo);
        }
      }
    }

  }
  private float CalculatePercent(string fraction)
  {
    string[] numbers = fraction.Split('/');
    float ret = 0.0f;
    if (numbers.Length < 2)
      return ret;
    int member = Convert.ToInt32(numbers[0]);
    int deno = Convert.ToInt32(numbers[1]);
    if (member >= 0 && deno > 0) {
      ret = member / (float)deno;
    }
    return ret;
  }

  private void SetWndInfo(WindowInfo wndInfo, string property, string value)
  {
    switch (property) {
      case "SceneId":
        wndInfo.sceneId = Convert.ToInt32(value);break;
      case "Type":
        int type = Convert.ToInt32(value);
        wndInfo.windowType = (UIType)type; break;
      case "WindowName":
        wndInfo.windowName = value; break;
      case "WindowPath":
        wndInfo.windowPath = value; break;
      case "OffsetL":
        float offsetL = (float)CalculatePercent(value);
        if (offsetL != -1f) {
          wndInfo.windowPos.x = offsetL * Screen.width;
        } break;
      case "OffsetC":
        if (value == "1")
          wndInfo.isCenter = true; break;
      case "OffsetB":
        float offsetB = (float)CalculatePercent(value);
        if (offsetB != -1f) { wndInfo.windowPos.y = offsetB * Screen.height; } break;
      default: break;

    }
  }
  
  static private UIManager m_Instance = new UIManager();
  static public UIManager Instance
  {
    get { return m_Instance; }
  }
  static public UnityEngine.Color SkillDrectorColor = new UnityEngine.Color(255, 255, 255);
  public Dictionary<string, WindowInfo> m_WindowsInfoDic = new Dictionary<string, WindowInfo>();
}