using UnityEngine;
using System.Collections;

public class ShuRen_PaShan : MonoBehaviour {

	private Animator animator;
	public Transform leftHand;
	public float upspeed;
	public float checkdis = 2.0f;
	private Transform mytr;
	private Rigidbody mybody;

	private Vector3 startpostion;
	private Quaternion startraotation;


	// Use this for initialization
	void Start () {
		animator = GetComponent<Animator>();
		mytr = GetComponent<Transform> ();
		mybody = GetComponent<Rigidbody> ();
		startpostion = mytr.position;
		startraotation = mytr.rotation;
	}
	
	// Update is called once per frame
	void Update () {

        if (animator)
		{
				AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo (0);
				if (Input.GetButton ("Fire1")) {
						animator.SetBool ("BeginUp", true);
				}
				if (state.IsName ("Base Layer.Up")&&animator.GetBool("UpEnd")==false) {
					
					//mytr.position += new Vector3(0,upspeed*Time.deltaTime,0);
					RaycastHit hit = new RaycastHit();
					bool ret =Physics.Raycast(mytr.position,mytr.forward,out hit,5.0f);
					if(ret == true)
					{
						
					}
					//mybody.AddForce(leftHand.forward*100);
					//mybody.MovePosition(mytr.forward*Time.deltaTime*upspeed);
					float dis = leftHand.position.y - mytr.position.y;
					if(dis<=checkdis)
					{
					   animator.SetBool ("UpEnd", true);
					   animator.SetBool ("BeginUp", false);
					}
						
				}
				if (state.IsName ("Base Layer.UpToStand")&&animator.GetBool("UpEnd")==true) {
				    
					animator.SetBool ("UpEnd", false);
					animator.MatchTarget(leftHand.position, leftHand.rotation, AvatarTarget.LeftHand, new MatchTargetWeightMask(new Vector3(1, 1, 1), 0), 0.056f, 0.38f);

				}

				if (Input.GetButton ("Fire2")) {
					
					mytr.position =startpostion;
					mytr.rotation =startraotation;
					animator.SetBool ("BeginUp", false);
					animator.SetBool ("UpEnd", false);
				}
			
			
			
			

		}
	}

	void setUptoStandEnd()
	{
		Debug.Log("event coming");
	}
}
