using UnityEngine;
using System.Collections;

public class EXVisible : MonoBehaviour
{

  // Use this for initialization
  void Start()
  {
    time = 0.0f;
    DashFire.LogicSystem.EventChannelForGfx.Subscribe<string, bool>("ge_ex_skill", "ui", Ex);
    SetPosition();
    Come(0.0f);
    NGUITools.SetActive(gameObject, false);
  }

  // Update is called once per frame
  void Update()
  {
    time += RealTime.deltaTime;
    if (time < cometime) {
      Come(time);
    }
    if (time >= (staytime - gotime)) {
      Go(time - (staytime - gotime));
    }
    if (time > staytime) {
      Come(0.0f);
      time = 0.0f;
      NGUITools.SetActive(gameObject, false);
    }
  }
  public void SetOkShow(bool show) 
  {
    OkShow = show;
  }
  void Ex(string hero, bool isstart)
  {
    if (!OkShow) return;
    if (isstart) {
      Transform tf = gameObject.transform.Find("SpriteHero");
      if (tf != null) {
        UISprite us = tf.gameObject.GetComponent<UISprite>();
        if (us != null) {
          us.spriteName = hero;
        }
      }
      time = 0.0f;
      NGUITools.SetActive(gameObject, true);
    }
    GameObject dfmUIRootGO = null;
    if (isstart) {
      dfmUIRootGO = this.transform.parent.gameObject;
      if (dfmUIRootGO != null) {
        dfmUIRootGO.GetComponent<DFMUiRoot>().SetUIVisible(false);
      }
    } else {
      dfmUIRootGO = this.transform.parent.gameObject;
      if (dfmUIRootGO != null) {
        dfmUIRootGO.GetComponent<DFMUiRoot>().SetUIVisible(true);
      }
    }
  }
  void SetPosition()
  {
    float h = Screen.height;
    float w = Screen.width;
    Transform tf = gameObject.transform.Find("SpriteBack");
    if (tf != null) {
      tf.position = UICamera.mainCamera.ScreenToWorldPoint(new Vector3(0, h, 0));
    }
    tf = gameObject.transform.Find("SpriteHero");
    if (tf != null) {
      tf.position = UICamera.mainCamera.ScreenToWorldPoint(new Vector3(0, h, 0));
    }
    tf = gameObject.transform.Find("SpriteLight");
    if (tf != null) {
      tf.position = UICamera.mainCamera.ScreenToWorldPoint(new Vector3(0, h, 0));
    }
  }
  void Come(float come)
  {
    float scale = 1.0f - come / cometime;
    this.transform.localPosition = new Vector3(-1688 * scale, 0, 0);
  }
  void Go(float go)
  {
    float scale = go / gotime;
    this.transform.localPosition = new Vector3(-1688 * scale, 0, 0);
  }
  private float time = 0.0f;
  private float cometime = 0.1f;
  private float gotime = 0.1f;
  private float staytime = 1.2f;
  private bool OkShow = true;
}
