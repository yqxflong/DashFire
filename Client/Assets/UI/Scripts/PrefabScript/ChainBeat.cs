using UnityEngine;
using System.Collections;

public class ChainBeat : MonoBehaviour
{

  // Use this for initialization
  void Start()
  {
    nowscale = 1.0f;
    time = 0.0f;
  }

  // Update is called once per frame
  void Update()
  {
    time += RealTime.deltaTime;
    int multiple = (int)System.Math.Round(time / 0.03f);
    switch (multiple) {
      case 0: Zoom(1.4f); break;
      case 1: Zoom(1.8f); break;
      case 2: Zoom(1.4f); break;
      case 3: Zoom(1.2f); break;
      case 4: Zoom(1.0f); break;
      default: break;
    }
    if (time > 0.12f) {
      Zoom(1.0f);
      Destroy(this.GetComponent("ChainBeat"));
    }
  }
  void Zoom(float wantscale)
  {
    if (wantscale == nowscale) return;
    float scale = wantscale / nowscale;

    for (int j = 0; j < 2; ++j) {
      Transform tf = this.gameObject.transform.Find("Number" + j);
      if (tf != null) {
        UISprite us = tf.gameObject.GetComponent<UISprite>();

        //         Vector3 lp = us.transform.localPosition;
        //         us.transform.localPosition = new Vector3(lp.x * scale, lp.y * scale, lp.z);
        if (us != null) {
          us.width = (int)System.Math.Round(us.width * scale);
          us.height = (int)System.Math.Round(us.height * scale);
        }
      }
    }

    Transform tf2 = this.gameObject.transform.Find("Chain");
    if (tf2 != null) {
      UISprite us2 = tf2.gameObject.GetComponent<UISprite>();
      if (us2 != null) {
        us2.width = (int)System.Math.Round(us2.width * scale);
        us2.height = (int)System.Math.Round(us2.height * scale);
      }
    }

    Transform tf3 = this.gameObject.transform.Find("Back");
    if (tf3 != null) {
      UISprite us3 = tf3.gameObject.GetComponent<UISprite>();
      if (us3 != null) {
        us3.width = (int)System.Math.Round(us3.width * scale);
        us3.height = (int)System.Math.Round(us3.height * scale);
      }
    }

    nowscale = wantscale;
  }
  private float nowscale = 1.0f;
  private float time = 0.0f;
}
