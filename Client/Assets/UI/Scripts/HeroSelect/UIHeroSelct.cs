using UnityEngine;
using System.Collections;
using System.Threading;
using DashFire;

public class UIHeroSelct : MonoBehaviour 
{
  public int heroId = 2;
	UISprite sp;
	// Use this for initialization
	void Start() 
  {
    this.OnZwClick();
	}	
	// Update is called once per frame
	void Update () 
  {
  }
	public void OnZwClick()
	{
		Transform trans = this.transform.FindChild ("Character");
		if(null !=trans)
		sp = trans.gameObject.GetComponent<UISprite> ();
		if(null != sp)
			sp.spriteName = "zhongwei";
		SendChooseHeroMessage((int)HeroId.ZHONGWEI);
	}
	public void OnHxyClick()
	{
		Transform trans = this.transform.FindChild ("Character");
		if(null !=trans)
			sp = trans.gameObject.GetComponent<UISprite> ();
		if(null != sp)
			sp.spriteName = "huoxiyi";
		SendChooseHeroMessage((int)HeroId.HUOYIXI);
	}
	public void OnLsnClick()
	{
		Transform trans = this.transform.FindChild ("Character");
		if(null !=trans)
			sp = trans.gameObject.GetComponent<UISprite> ();
		if(null != sp)
			sp.spriteName = "liusuannan";
		heroId = 3;
    SendChooseHeroMessage((int)HeroId.LIUSUANNAN);
	}
	public void OnStartButtonClick()
	{
    LogicSystem.PublishLogicEvent("ge_start_game", "lobby");
    NGUITools.SetActive(this.gameObject, false);
	}
  void SendChooseHeroMessage(int id)
  {
    LogicSystem.PublishLogicEvent("ge_select_hero", "lobby", id);
  }
  enum HeroId :int
  {
    ZHONGWEI = 1,
    LIUSUANNAN = 1,
    HUOYIXI = 2,
  }
}
