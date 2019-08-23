using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DashFire
{
  public class SkillConfigProvider
  {
    #region Singleton
    private static SkillConfigProvider s_instance_ = new SkillConfigProvider();
    public static SkillConfigProvider Instance
    {
      get { return s_instance_; }
    }
    #endregion

    DataTemplateMgr<SkillLogicData> skillLogicDataMgr;    // 技能逻辑数据容器
    DataTemplateMgr<ImpactLogicData> impactLogicDataMgr;  // 效果数据容器
    DataTemplateMgr<SoundLogicData> soundLogicDataMgr;  // 声音数据容器

    private SkillConfigProvider()
    {
      skillLogicDataMgr = new DataTemplateMgr<SkillLogicData>();
      impactLogicDataMgr = new DataTemplateMgr<ImpactLogicData>();
      soundLogicDataMgr = new DataTemplateMgr<SoundLogicData>();
    }

    /**
     * @brief 读取数据
     *
     * @param node
     *
     * @return 
     */
    public bool CollectData(SkillConfigType type, string file, string rootLabel)
    {
      bool result = false;
      switch (type) {
        case SkillConfigType.SCT_SKILL: {
            result = skillLogicDataMgr.CollectDataFromDBC(file, rootLabel);
          } break;
        case SkillConfigType.SCT_IMPACT: {
            result = impactLogicDataMgr.CollectDataFromDBC(file, rootLabel);
          } break;
        case SkillConfigType.SCT_SOUND: {
            result = soundLogicDataMgr.CollectDataFromDBC(file, rootLabel);
          } break;
        default: {
            LogSystem.Assert(false, "SkillConfigProvider.CollectData type error!");
          } break;
      }

      return result;
    }


    /**
     * @brief 提取数据
     *
     * @param node
     *
     * @return 
     */
    public IData ExtractData(SkillConfigType type, int id)
    {
      IData result = null;
      switch (type) {
        case SkillConfigType.SCT_SKILL: {
            result = skillLogicDataMgr.GetDataById(id);
          } break;
        case SkillConfigType.SCT_IMPACT: {
            result = impactLogicDataMgr.GetDataById(id);
          } break;
        case SkillConfigType.SCT_SOUND: {
            result = soundLogicDataMgr.GetDataById(id);
          } break;
        default: {
            result = null;
          } break;
      }
      return result;
    }
  }
}
