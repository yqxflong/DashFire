using UnityEngine;
using System.Collections;

public class SkillTeach : MonoBehaviour {

	// Use this for initialization
	void Start () {
    NGUITools.SetActive(gameObject, false);
	}
	
	// Update is called once per frame
	void Update () {
	
	}
  public void OnStopBtnClicked()
  {
    NGUITools.SetActive(gameObject,false);
  }
}
