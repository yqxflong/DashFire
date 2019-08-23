using UnityEngine;
using System.Collections;

public class ImpactImput : MonoBehaviour {

  public ImpactInfo m_ImpactInfo;
  public GameObject m_Target;
  // 用来测试效果
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
    if(null != m_Target && null != m_ImpactInfo){
      if (Input.GetMouseButtonDown(0)) {
        //ImpactSystem.Instance.SendStiffnessImpact(m_Target, m_ImpactInfo.Clone() as ImpactInfo);
      }
      if (Input.GetMouseButtonDown(1)) {
        ImpactSystem.Instance.SendImpact(m_Target, m_ImpactInfo.Clone() as ImpactInfo);
      }
    }
	}
}
