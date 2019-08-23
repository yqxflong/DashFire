using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DashFire;

public class ClimbEnter : MonoBehaviour {

  public GameObject m_MonsterPrefab;
  public float m_ClimbDistance = 10.0f;
  public float m_ClimbStopDistance = 1.0f;
  public float m_BornInterval = 21.0f;
  public float m_ClimbSpeed = 2.0f;
  public Transform m_MatchTransform;

  private float m_LastBornTime = 0.0f;

  private List<GameObject> m_Monsters = new List<GameObject>();
  private Vector3 m_Direction;
  private Vector3 m_MatchPoint;
	// Use this for initialization
	void Start () {
    m_LastBornTime = Time.time;
	}
	
	// Update is called once per frame
	void Update () {
    if (Time.time > m_LastBornTime + m_BornInterval) {
      m_LastBornTime = Time.time;
      if (null != m_MonsterPrefab) {
        GameObject monster = ResourceSystem.NewObject(m_MonsterPrefab) as GameObject;
        if (null != monster) {
          monster.transform.position = this.transform.position;
          monster.transform.rotation = m_MatchTransform.rotation;
          m_Monsters.Add(monster);
          monster.SendMessage("SetMatchTransform", m_MatchTransform);
        }
      }
    }
    /*
    foreach (GameObject monster in m_Monsters) {
      Animator animator = monster.GetComponent<Animator>();
      if (null != animator) {
        if (Vector3.Distance(this.transform.position, monster.transform.position) > m_ClimbStopDistance) {
          animator.SetBool("StartClimp", true);
          monster.transform.position += m_ClimbSpeed * this.transform.up * Time.deltaTime;
        } else {
          animator.SetBool("StartClimp", false);
          animator.SetBool("ClimpEnd", true);
        }
      }
    }
     */
	}
}
