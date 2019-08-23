using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DashFire
{
  internal class FilePathDefine
  {
    public const string C_SceneConfig = @"Public/Scenes/SceneConfig.txt";

    public const string C_ActionConfig = @"Public/ActionConfig.txt";
    public const string C_NpcConfig = @"Public/NpcConfig.txt";
    public const string C_NpcLevelupConfig = @"Public/NpcLevelupConfig.txt";
    public const string C_PlayerConfig = @"Public/PlayerConfig.txt";
    public const string C_PlayerLevelupConfig = @"Public/PlayerLevelupConfig.txt";
    public const string C_PlayerLevelupExpConfig = @"Public/PlayerLevelupExpConfig.txt";
    public const string C_CriticalConfig = @"Public/CriticalConfig.txt";
    public const string C_DynamicSceneConfig = @"Public/DynamicSceneConfig.txt";

    //物品装备
    public const string C_ItemConfig = @"Public/ItemConfig.txt";
    public const string C_EquipmentConfig = @"Public/EquipmentConfig.txt";
    public const string C_EquipmentLevelupConfig = @"Public/EquipmentLevelupConfig.txt";
    
    // 武器系统配置
    public const string C_WeaponSystemConfig = @"Public/Weapon/WeaponData.txt";
    public const string C_GlobalSoundConfig = @"Public/SoundConfig.txt";

    public const string C_PlayerMuzzleConfig = @"Public/PlayerMuzzlePos.txt";
    public const string C_NpcMuzzleConfig = @"Public/NpcMuzzlePos.txt";

    // 技能系统配置
    public const string C_SkillSystemConfig = @"Public/Skill/SkillData.txt";
    public const string C_ImpactSystemConfig = @"Public/Skill/ImpactData.txt";
    public const string C_SoundConfig = @"Public/Skill/SoundData.txt";
    public const string C_BuffConfig = @"Public/Skill/BuffData.txt";
    public const string C_SkillSoundConfig = @"Public/Skill/SoundData.txt";

    //动作系统配置
    public const string C_ActionSystemConfig = @"Public/ActionData.txt";

    //技能Dsl文件根
    public const string C_SkillDslPath = @"Public/SkillDsl/";
    
    //客户端专用
    public const string C_CommandsConfig = @"Client/Commands.xml";
    public const string C_EventsConfig = @"Client/Events.xml";
    public const string C_PveClientConfig = @"Client/ClientConfig.ini";
    public const string C_PvpClientConfig = @"Client/PvpClientConfig.ini";
    public const string C_ResPoolConfig = @"Client/ResPoolConfig.txt";
    public const string C_StrDictionary = @"Client/StrDictionary.txt";

    //服务端专用

  }
  public class FilePathDefine_Server
  {
    public const string C_RootPath = "data/";

    public const string C_SceneConfig = C_RootPath+FilePathDefine.C_SceneConfig;

    public const string C_ActionConfig = C_RootPath + FilePathDefine.C_ActionConfig;
    public const string C_NpcConfig = C_RootPath + FilePathDefine.C_NpcConfig;
    public const string C_NpcLevelupConfig = C_RootPath + FilePathDefine.C_NpcLevelupConfig;
    public const string C_PlayerConfig = C_RootPath + FilePathDefine.C_PlayerConfig;
    public const string C_PlayerLevelupConfig = C_RootPath + FilePathDefine.C_PlayerLevelupConfig;
    public const string C_PlayerLevelupExpConfig = C_RootPath + FilePathDefine.C_PlayerLevelupExpConfig;
    public const string C_CriticalConfig = C_RootPath + FilePathDefine.C_CriticalConfig;
    public const string C_BuffConfig = C_RootPath + FilePathDefine.C_BuffConfig;

    //物品装备
    public const string C_ItemConfig = C_RootPath + FilePathDefine.C_ItemConfig;
    public const string C_EquipmentConfig = C_RootPath + FilePathDefine.C_EquipmentConfig;
    public const string C_EquipmentLevelupConfig = C_RootPath + FilePathDefine.C_EquipmentLevelupConfig;

    // 武器系统配置
    public const string C_WeaponSystemConfig = C_RootPath + FilePathDefine.C_WeaponSystemConfig;

    public const string C_PlayerMuzzleConfig = C_RootPath + FilePathDefine.C_PlayerMuzzleConfig;
    public const string C_NpcMuzzleConfig = C_RootPath + FilePathDefine.C_NpcMuzzleConfig;
    
    // 技能系统配置
    public const string C_SkillSystemConfig = C_RootPath + FilePathDefine.C_SkillSystemConfig;
    public const string C_ImpactSystemConfig = C_RootPath + FilePathDefine.C_ImpactSystemConfig;
    public const string C_ActionSystemConfig = C_RootPath + FilePathDefine.C_ActionSystemConfig;
    public const string C_SkillSoundConfig = C_RootPath + FilePathDefine.C_SkillSoundConfig;

    public const string C_SkillDslPath = C_RootPath + FilePathDefine.C_SkillDslPath;
  }
  public class FilePathDefine_Client
  {
    public const string C_RootPath = "";

    public const string C_SceneConfig = C_RootPath + FilePathDefine.C_SceneConfig;

    public const string C_ActionConfig = C_RootPath + FilePathDefine.C_ActionConfig;
    public const string C_NpcConfig = C_RootPath + FilePathDefine.C_NpcConfig;
    public const string C_NpcLevelupConfig = C_RootPath + FilePathDefine.C_NpcLevelupConfig;
    public const string C_PlayerConfig = C_RootPath + FilePathDefine.C_PlayerConfig;
    public const string C_PlayerLevelupConfig = C_RootPath + FilePathDefine.C_PlayerLevelupConfig;
    public const string C_PlayerLevelupExpConfig = C_RootPath + FilePathDefine.C_PlayerLevelupExpConfig;
    public const string C_CriticalConfig = C_RootPath + FilePathDefine.C_CriticalConfig;

    public const string C_DynamicSceneConfig = C_RootPath + FilePathDefine.C_DynamicSceneConfig;

    //物品装备
    public const string C_ItemConfig = C_RootPath + FilePathDefine.C_ItemConfig;
    public const string C_EquipmentConfig = C_RootPath + FilePathDefine.C_EquipmentConfig;
    public const string C_EquipmentLevelupConfig = C_RootPath + FilePathDefine.C_EquipmentLevelupConfig;
    
    // 武器系统配置
    public const string C_WeaponSystemConfig = C_RootPath + FilePathDefine.C_WeaponSystemConfig;
    public const string C_GlobalSoundConfig = C_RootPath + FilePathDefine.C_GlobalSoundConfig;
    
    public const string C_PlayerMuzzleConfig = C_RootPath + FilePathDefine.C_PlayerMuzzleConfig;
    public const string C_NpcMuzzleConfig = C_RootPath + FilePathDefine.C_NpcMuzzleConfig;

    // 技能系统配置
    public const string C_SkillSystemConfig = C_RootPath + FilePathDefine.C_SkillSystemConfig;
    public const string C_ImpactSystemConfig = C_RootPath + FilePathDefine.C_ImpactSystemConfig;
    public const string C_ActionSystemConfig = C_RootPath + FilePathDefine.C_ActionSystemConfig;
    public const string C_BuffConfig = C_RootPath + FilePathDefine.C_BuffConfig;
    public const string C_SoundConfig = C_RootPath + FilePathDefine.C_SoundConfig;

    public const string C_SkillDslPath = C_RootPath + FilePathDefine.C_SkillDslPath;

    public const string C_CommandsConfig = C_RootPath + FilePathDefine.C_CommandsConfig;
    public const string C_EventsConfig = C_RootPath + FilePathDefine.C_EventsConfig;
    public const string C_PveClientConfig = C_RootPath + FilePathDefine.C_PveClientConfig;
    public const string C_PvpClientConfig = C_RootPath + FilePathDefine.C_PvpClientConfig;
    public const string C_ResPoolConfig = C_RootPath + FilePathDefine.C_ResPoolConfig;

    public const string C_StrDictionary = C_RootPath + FilePathDefine.C_StrDictionary;
  }
}
