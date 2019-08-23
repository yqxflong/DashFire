using System;
using System.Collections.Generic;

public delegate void SectionEvent();
public class SectionInfo {
  public int SectionId { get; set; }
  public SectionEvent SectionEvent { get; set; }
}

public class SkillSectionCom {
  private Dictionary<int, SectionInfo> m_Sections = new Dictionary<int, SectionInfo>();
  public int CurSectionId { get; set; }
  public bool IsCurrentSection(object sectionId) {
    return CurSectionId == (int)sectionId;
  }
  public void ChangeSection(int nextSectionId, bool isImmidiately = false) {
    SectionInfo sInfo = GetSectionInfo(nextSectionId);
    if (sInfo != null) {
      CurSectionId = nextSectionId;
      if (isImmidiately && sInfo.SectionEvent != null) {
        sInfo.SectionEvent();
      }
    }
  }
  public void ChangeNextSection(bool isImmidiately = false) {
    CurSectionId++;
    ChangeSection(CurSectionId, isImmidiately);
  }
  public void RegisterSection(int id, SectionEvent section) {
    SectionInfo info = new SectionInfo();
    info.SectionId = id;
    info.SectionEvent = section;
    m_Sections.Add(id, info);
  }
  public SectionInfo GetSectionInfo(int id) {
    if (m_Sections.ContainsKey(id)) {
      return m_Sections[id];
    }
    return null;
  }
  public void ExecuteSection() {
    SectionInfo sectionInfo = GetSectionInfo(CurSectionId);
    if (sectionInfo != null && sectionInfo.SectionEvent != null) {
      sectionInfo.SectionEvent();
    }
  }
}

