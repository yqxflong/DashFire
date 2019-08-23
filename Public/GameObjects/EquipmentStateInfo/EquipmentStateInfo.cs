using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DashFire
{
  public class EquipmentStateInfo
  {
    public const int c_PackageCapacity = 6;
    public bool EquipmentChanged
    {
      get { return m_EquipmentChanged; }
      set { m_EquipmentChanged = value; }
    }
    public EquipmentInfo EquipmentInfo
    {
      get { return m_EquipmentInfo; }
    }
    public void SetItemData(int index, ItemDataInfo info)
    {
      if (index >= 0 && index < c_PackageCapacity) {
        m_ItemData[index] = info;
        m_EquipmentChanged = true;
      }
    }
    public ItemDataInfo GetItemData(int index)
    {
      ItemDataInfo info = null;
      if (index >= 0 && index < c_PackageCapacity) {
        info = m_ItemData[index];
      }
      return info;
    }
    public int FindItemPos(int itemId)
    {
      return FindItemPos(itemId, 0);
    }
    public int FindItemPos(int itemId, int startIndex)
    {
      int pos = -1;
      if (startIndex >= 0 && startIndex < c_PackageCapacity) {
        for (int ix = startIndex; ix < c_PackageCapacity; ++ix) {
          ItemDataInfo info = m_ItemData[ix];
          if (null != info && info.ItemId == itemId) {
            pos = ix;
            break;
          }
        }
      }
      return pos;
    }
    public int FindEmptyPos()
    {
      int pos = -1;
      for (int ix = 0; ix < c_PackageCapacity; ++ix) {
        ItemDataInfo info = m_ItemData[ix];
        if (null == info) {
          pos = ix;
          break;
        }
      }
      return pos;
    }
    public void ResetItemData()
    {
      for (int ix = 0; ix < c_PackageCapacity; ++ix) {
        m_ItemData[ix] = null;
      }
    }
    public void Reset()
    {
      m_EquipmentInfo.Reset();
      ResetItemData();
    }

    private bool m_EquipmentChanged = false;
    private EquipmentInfo m_EquipmentInfo = new EquipmentInfo();
    private ItemDataInfo[] m_ItemData = new ItemDataInfo[c_PackageCapacity];
  }
}
