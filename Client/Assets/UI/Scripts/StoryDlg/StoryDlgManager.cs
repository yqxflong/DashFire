using System;
using System.Collections.Generic;
using UnityEngine;

namespace StoryDlg
{
	class StoryDlgManager
	{
    static private StoryDlgManager m_Instance = new StoryDlgManager();
    static public StoryDlgManager Instance
    {
      get { return m_Instance; }
    }
    public List<StoryDlgInfo> StoryInfos = new List<StoryDlgInfo>();

    public void Init()
    {
      string str = "StoryDlg/StoryDlgConfig";
      TextAsset config = (TextAsset)Resources.Load(str, typeof(TextAsset));
      if (null != config) {
        string[] header = null;
        string[] lines = null;
        lines = config.text.Split('\n');
        if (lines.Length > 0) {
          header = lines[0].Split('\t');
        }
        for (int i = 1; i < lines.Length; ++i) {
          string lineStr = lines[i];
          if (lineStr.Trim() != String.Empty) {
            StoryDlgInfo si = BuildStoryInfo(lineStr);
            if (si != null) {
              StoryInfos.Add(si);              
            }
          } else {
            break;
          }
        }
      }
    }
    public StoryDlgInfo GetStoryInfoByID(int id)
    {
      if(id>=1 && id<=StoryInfos.Count) {
        return StoryInfos[id-1];
      }
      return null;
    }

    private StoryDlgInfo BuildStoryInfo(string storyInfoLine)
    {
      string[] data = storyInfoLine.Split('\t');
      if (data.Length > 0) {
        StoryDlgInfo si = new StoryDlgInfo();
        si.ID = Int32.Parse(data[0]);
        si.StoryName = data[1];
        si.StoryItems = BuildStoryItems(si.StoryName);
        return si;
      } else {
        return null;
      }
    }
    private List<StoryDlgItem> BuildStoryItems(string storyName)
    {     
      string str = String.Format("StoryDlg/{0}", storyName.Trim());
      TextAsset ta = (TextAsset)Resources.Load(str, typeof(TextAsset));
      if (null != ta) {
        List<StoryDlgItem> itemList = new List<StoryDlgItem>();
        string[] lines = null;
        lines = ta.text.Split('\n');        
        for (int i = 1; i < lines.Length; ++i) {
          string lineStr = lines[i];          
          if (lineStr.Trim() != String.Empty) {
            string[] data = lines[i].Split('\t');
            StoryDlgItem item = new StoryDlgItem();
            item.Number = Int32.Parse(data[0]);
            item.SpeakerName = data[1];
            item.ImageLeft = data[2];
            item.ImageLeftBig = string.Format("{0}_big", item.ImageLeft);
            item.ImageLeftSmall = string.Format("{0}_small", item.ImageLeft);
            item.ImageRight = data[3];
            item.ImageRightBig = string.Format("{0}_big", item.ImageRight);
            item.ImageRightSmall = string.Format("{0}_small", item.ImageRight);
            item.Words = data[4];
            itemList.Add(item);
          } else {
            break;
          }
        }
        return itemList;
      }      
      return null;
    }
	}
}
