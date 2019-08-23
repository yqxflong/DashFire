using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DashFire
{
  public class LobbyItemInfo
  {
    public int ItemId
    {
      get { return m_ItemId; }
      set { m_ItemId = value; }
    }
    public int ItemNum
    {
      get { return m_ItemNum; }
      set { m_ItemNum = value; }
    }

    private int m_ItemId = 0;
    private int m_ItemNum = 0;
  }
  public class LobbyItemBag
  {
    public const int c_MaxLobbyItemNum = 64;

    public SortedDictionary<int, LobbyItemInfo> Items
    {
      get { return m_Items; }
    }

    public void AddItem(int id, int num)
    {
      if (num >= 0 && num <= c_MaxItemNumForAdd) {
        if (m_Items.ContainsKey(id)) {
          m_Items[id].ItemNum += num;
        } else {
          LobbyItemInfo item = new LobbyItemInfo();
          item.ItemId = id;
          item.ItemNum = num;
          m_Items.Add(id, item);
        }
      }
    }

    public void Reset()
    {
      m_Items.Clear();
    }

    private SortedDictionary<int, LobbyItemInfo> m_Items = new SortedDictionary<int, LobbyItemInfo>();
    private const int c_MaxItemNumForAdd = 99;
  }
}
