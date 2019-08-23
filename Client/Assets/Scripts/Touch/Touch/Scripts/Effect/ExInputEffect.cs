using UnityEngine;
using System.Collections;
using DashFire;

public class ExInputEffect : MonoBehaviour 
{
	internal void Start()
  {
    TouchManager.OnFingerEvent += OnFingerEvent;
	}

  internal void OnDestroy()
  {
    TouchManager.OnFingerEvent -= OnFingerEvent;
  }

	void Update () 
  {
	}

  private void OnFingerEvent(FingerEvent args)
  {
    if (null == args) {
      return;
    }
    if (TouchType.Regognizer != TouchManager.curTouchState) {
      return;
    }
    if (GestureEvent.OnFingerMove.ToString() == args.Name) {
      if (TouchManager.GestureEnable) {
        return;
      }
      if (args.Finger.IsDown && args.Finger.IsMoving) {
        if (args.Finger.DistanceFromStart > validLength) {
          Vector3 curPos = Camera.main.ScreenToWorldPoint(new Vector3(args.Position.x, args.Position.y, depth));
          if (null == obj) {
            obj = GameObject.Instantiate(original, curPos, Quaternion.identity) as GameObject;
          }
          if (null != obj) {
            obj.transform.position = curPos;
          }
        }
      }
    } else if (GestureEvent.OnFingerUp.ToString() == args.Name) {
      if (args.Finger.WasDown && null != obj) {
        GameObject.Destroy(obj, duration);
        obj = null;
      }
    }
  }

  private GameObject obj = null;
  public Object original = null;
  [SerializeField]
  public float duration = 1.0f;
  [SerializeField]
  public float depth = 1.0f;
  [SerializeField]
  public float validLength = 10f;
}