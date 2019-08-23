using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DashFire;

public class SkillMovement : MonoBehaviour {
  private float m_MoveSpeed = 0;
  private bool m_IsMoving = false;
  private CharacterController m_Controller;

  private bool m_IsCurveMoving = false;
  private List<MoveSectionInfo> m_SectionList = new List<MoveSectionInfo>();
  private SkillManager m_SkillManager;
  private static float m_RayCastMaxDistance = 100;
  private static int m_TerrainLayer = 1 << 16;

	// Use this for initialization
	void Start () {
    m_Controller = gameObject.GetComponent<CharacterController>();
    m_SkillManager = gameObject.GetComponent<SkillManager>();
	}

	// Update is called once per frame
	void Update () {
    UpdateMove();
    UpdateCurveMove();
	}

  public void Move(Vector3 motion) {
    if (m_Controller != null) {
      m_Controller.Move(motion);
    }
  }

  public void StartMove(float speed)
  {
    m_MoveSpeed = speed;
    m_IsMoving = true;
  }

  public void StopMove()
  {
    m_IsMoving = false;
  }

  private void UpdateMove()
  {
    if (!m_IsMoving) {
      return;
    }

    Vector3 direction = gameObject.transform.forward;
    Vector3 motion = direction * m_MoveSpeed * Time.deltaTime;
    m_Controller.Move(motion);
  }

  private void UpdateCurveMove()
  {
    if (!m_IsCurveMoving) {
      return;
    }

    if (m_SectionList.Count == 0) {
      StopCurveMove();
      return;
    }

    float now = Time.time;
    MoveSectionInfo cur_section = m_SectionList[0];
    if (now - cur_section.startTime > cur_section.moveTime) {
      float end_time = cur_section.startTime + cur_section.moveTime;
      float used_time = end_time - cur_section.lastUpdateTime;
      cur_section.curSpeedVect = Move(cur_section.curSpeedVect, cur_section.accelVect, used_time);
      m_SectionList.RemoveAt(0);
      if (m_SectionList.Count > 0) {
        cur_section = m_SectionList[0];
        cur_section.startTime = end_time;
        cur_section.lastUpdateTime = end_time;
        cur_section.curSpeedVect = cur_section.speedVect;
      } else {
        StopCurveMove();
      }
    } else {
      cur_section.curSpeedVect = Move(cur_section.curSpeedVect, cur_section.accelVect, now - cur_section.lastUpdateTime);
      cur_section.lastUpdateTime = now;
    }
  }

  private Vector3 Move(Vector3 speed_vect, Vector3 accel_vect, float time)
  {
    Vector3 local_motion = speed_vect * time + accel_vect * time * time / 2;
    Vector3 word_target_pos = gameObject.transform.TransformPoint(local_motion);
    m_Controller.Move(word_target_pos - gameObject.transform.position);
    return (speed_vect + accel_vect * time);
  }

  public void SetFacePos(Vector3 targetPos) {
    Vector3 dir = targetPos - transform.position;
    dir.y = 0;
    transform.forward = dir;
    Vector3 rotate = gameObject.transform.rotation.eulerAngles;
    LogicSystem.NotifyGfxUpdatePosition(gameObject, transform.position.x, transform.position.y,
                                        transform.position.z, 0, rotate.y * Mathf.PI / 180, 0);

  }

  public bool IsGrounded()
  {
    if ((m_Controller.collisionFlags & CollisionFlags.Below) != 0) {
      return true;
    }
    return false;
  }

  public float GetHeightWithGround()
  {
    if (Terrain.activeTerrain != null) {
      return transform.position.y - Terrain.activeTerrain.SampleHeight(transform.position);
    } else {
      RaycastHit hit;
      Vector3 pos = transform.position;
      pos.y += 2;
      if (Physics.Raycast(pos, -Vector3.up, out hit, m_RayCastMaxDistance, m_TerrainLayer)) {
        return transform.position.y - hit.point.y;
      }
      return m_RayCastMaxDistance;
    }
  }

  public static Vector3 GetGroundPos(Vector3 pos)
  {
    Vector3 sourcePos = pos;
    RaycastHit hit;
    pos.y += 2;
    if (Physics.Raycast(pos, -Vector3.up, out hit, m_RayCastMaxDistance, m_TerrainLayer)) {
      sourcePos.y = hit.point.y;
    }
    return sourcePos;
  }


  public void StartCurveMove(string str_param)
  {
    string[] param_list = str_param.Split(' ');
    m_SectionList.Clear();
    int section_num = 0;
    while (param_list.Length >= 7 * (section_num + 1)) {
      MoveSectionInfo section = new MoveSectionInfo();
      section.moveTime = (float)System.Convert.ToDouble(param_list[(section_num * 7) + 0]);
      section.speedVect.x = (float)System.Convert.ToDouble(param_list[(section_num * 7) + 1]);
      section.speedVect.y = (float)System.Convert.ToDouble(param_list[(section_num * 7) + 2]);
      section.speedVect.z = (float)System.Convert.ToDouble(param_list[(section_num * 7) + 3]);
      section.accelVect.x = (float)System.Convert.ToDouble(param_list[(section_num * 7) + 4]);
      section.accelVect.y = (float)System.Convert.ToDouble(param_list[(section_num * 7) + 5]);
      section.accelVect.z = (float)System.Convert.ToDouble(param_list[(section_num * 7) + 6]);
      m_SectionList.Add(section);
      section_num++;
    }
    if (m_SectionList.Count == 0) {
      return;
    }
    CalNewSpeedWithTarget();
    m_SectionList[0].startTime = Time.time;
    m_SectionList[0].lastUpdateTime = Time.time;
    m_SectionList[0].curSpeedVect = m_SectionList[0].speedVect;
    m_IsCurveMoving = true;
  }

  private void CalNewSpeedWithTarget() {
    SkillScript ss = m_SkillManager.GetCurPlaySkill();
    if (ss == null) {
      return;
    }
    GameObject target = ss.MoveTarget;
    if (target == null) {
      return;
    }
    SetFacePos(target.transform.position);
    float cur_distance_z = 0;
    foreach (MoveSectionInfo section in m_SectionList) {
      cur_distance_z += (section.speedVect.z * section.moveTime +
                         section.accelVect.z * section.moveTime * section.moveTime / 2.0f);
    }
    Vector3 target_motion = (target.transform.position - transform.position);
    target_motion.y = 0;
    float target_distance_z = target_motion.magnitude;
    target_distance_z = target_distance_z * (1 + ss.ToTargetDistanceRatio) + ss.ToTargetConstDistance;
    if (target_distance_z < 0) {
      target_distance_z = 0;
    }
    float speed_ratio = 1;
    if (cur_distance_z != 0) {
      speed_ratio = target_distance_z / cur_distance_z;
    }
    foreach (MoveSectionInfo section in m_SectionList) {
      section.speedVect.z *= speed_ratio;
      section.accelVect.z *= speed_ratio;
    }
    ss.MoveTarget = null;
  }

  public void RegisterSkillCollideCurveMove(string str_param) {
    if (m_SkillManager == null) {
      return;
    }
    SkillScript cur_skill = m_SkillManager.GetCurPlaySkill();
    if (cur_skill != null) {
      cur_skill.RegisterCollideCurveMove(str_param);
    }
  }

  public void StopCurveMove() {
    m_IsCurveMoving = false;
  }
}

public class MoveSectionInfo
{
  public float moveTime;
  public Vector3 speedVect;
  public Vector3 accelVect;

  public float startTime;
  public float lastUpdateTime;
  public Vector3 curSpeedVect;
}
