using UnityEngine;
using System.Collections;
using DashFire;

public class EffectUtil : MonoBehaviour {

  /// <summary>
  /// 播放特效，指定时间后删除
  /// </summary>
  /// <param name="args"></param>
  public void AddTemporaryEffect(TemporaryEffectArgs args) {
    if (null != args) {
      UnityEngine.Object original = Resources.Load(args.Resource);
      if(null!=original){
        Vector3 pos = new Vector3(args.X, args.Y, args.Z);
        GameObject obj = ResourceSystem.NewObject(original) as GameObject;
        if(null != obj){
          obj.transform.position = pos;
          obj.transform.rotation = Quaternion.identity;
          GameObject.Destroy(obj, args.Duration);
        }
      } else {
        LogicSystem.LogicLog("Can't load resource {0} !", args.Resource);
      }
    }
  }

}
