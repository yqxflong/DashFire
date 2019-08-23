using UnityEngine;
using System.Collections;
using DashFire;

public class LoadingProgressBar : MonoBehaviour
{

  // Use this for initialization
  void Start()
  {
  }

  // Update is called once per frame
  void Update()
  {
    UISlider us = this.GetComponent<UISlider>();
    float progressvalue = DashFire.LogicSystem.GetLoadingProgress();
    if (us != null) {
      us.value = progressvalue;
    }
    if (progressvalue >= 0.9999f) {
      Transform tf = gameObject.transform.Find("Panel");
      if (tf != null) {
        UIPanel up = tf.gameObject.GetComponent<UIPanel>();
        if (up != null) {
          up.alpha = 0.0f;
        }
      }
    }
  }
  void EndLoading()
  {
    NGUITools.DestroyImmediate(this.transform.parent.gameObject);
  }
}
