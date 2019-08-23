using UnityEngine;
using System.Collections;
using DashFire;

public class MainCamera : MonoBehaviour
{
  public int GetTargetId()
  {
    return m_CurTargetId;
  }

  public void CameraFollow(int id)
  {
    GameObject obj = LogicSystem.GetGameObject(id);
    if(null!=obj){
      m_CurTargetId = id;
      m_Target=obj.transform;      
		  Collider collider = m_Target.collider;
      if (null != collider) {
        m_CenterOffset = collider.bounds.center - m_Target.position;
        m_HeadOffset = m_CenterOffset;
        m_HeadOffset.y = collider.bounds.max.y - m_Target.position.y;
        m_IsFollow = true;
        Cut();
      } else {
        m_IsFollow = false;
      }
    }
  }
  public void CameraFollowStop(int id)
  {
    m_Target = null;
    m_IsFollow = false;
  }
  public void CameraLookat(float[] coord)
  {
    Vector3 pos = new Vector3(coord[0], coord[1], coord[2]);
    m_Target = null;
    m_IsFollow = false;
  }
  public void CameraFixedYaw(float dir)
  {
    m_FixedYaw = LogicSystem.RadianToDegree(dir);
  }

  internal void Awake ()
  {
	  m_CameraTransform = Camera.main.transform;
	  if(!m_CameraTransform) {
		  Debug.Log("Please assign a camera to the ThirdPersonCamera script.");
		  enabled = false;	
	  }			
  }

  internal void LateUpdate()
  {
    if (m_IsFollow) {
      Apply();
    }
  }

  internal void OnLevelWasLoaded(int level)
  {
    m_CameraTransform = Camera.main.transform;
  }

  private void DebugDrawStuff ()
  {
	  Debug.DrawLine(m_Target.position, m_Target.position + m_HeadOffset);
  }

  private float AngleDistance(float a, float b)
  {
	  a = Mathf.Repeat(a, 360);
	  b = Mathf.Repeat(b, 360);
	
	  return Mathf.Abs(b - a);
  }

  private void Apply()
  {
	  // Early out if we don't have a target
    if (!m_Target)
		  return;
	
	  Vector3 targetCenter = m_Target.position + m_CenterOffset;
	  Vector3 targetHead = m_Target.position + m_HeadOffset;

  //	DebugDrawStuff();

	  // Calculate the current & target rotation angles
	  float originalTargetAngle = m_FixedYaw;//m_Target.eulerAngles.y;
    float currentAngle = m_CameraTransform.eulerAngles.y;

	  // Adjust real target angle when camera is locked
	  float targetAngle = originalTargetAngle; 
	
	  // When pressing Fire2 (alt) the camera will snap to the target direction real quick.
	  // It will stop snapping when it reaches the target
	  if (Input.GetButton("Fire2"))
		  m_Snap = true;
	
	  if (m_Snap)
	  {
		  // We are close to the target, so we can stop snapping now!
		  if (AngleDistance (currentAngle, originalTargetAngle) < 3.0)
			  m_Snap = false;
		
		  currentAngle = Mathf.SmoothDampAngle(currentAngle, targetAngle, ref m_AngleVelocity, m_SnapSmoothLag, m_SnapMaxSpeed);
	  }
	  // Normal camera motion
	  else
	  {
		  currentAngle = Mathf.SmoothDampAngle(currentAngle, targetAngle, ref m_AngleVelocity, m_AngularSmoothLag, m_AngularMaxSpeed);
	  }

    /*
	  // When jumping don't move camera upwards but only down!
	  if (false)
	  {
		  // We'd be moving the camera upwards, do that only if it's really high
		  float newTargetHeight = targetCenter.y + m_Height;
		  if (newTargetHeight < m_TargetHeight || newTargetHeight - m_TargetHeight > 5)
			  m_TargetHeight = targetCenter.y + m_Height;
	  }
	  // When walking always update the target height
	  else*/
	  {
		  m_TargetHeight = targetCenter.y + m_Height;
	  }

	  // Damp the height
	  float currentHeight = m_CameraTransform.position.y;
	  currentHeight = Mathf.SmoothDamp (currentHeight, m_TargetHeight, ref m_HeightVelocity, m_HeightSmoothLag);

	  // Convert the angle into a rotation, by which we then reposition the camera
	  Quaternion currentRotation = Quaternion.Euler (0, currentAngle, 0);
	
	  // Set the position of the camera on the x-z plane to:
	  // distance meters behind the target
    Vector3 pos = targetCenter;
	  pos += currentRotation * Vector3.back * m_Distance;

	  // Set the height of the camera
	  pos.y = currentHeight;

    m_CameraTransform.position = pos;
	
	  // Always look at the target	
	  SetUpRotation(targetCenter, targetHead);
  }

  private void Cut()
  {
	  float oldHeightSmooth = m_HeightSmoothLag;
	  float oldSnapMaxSpeed = m_SnapMaxSpeed;
	  float oldSnapSmooth = m_SnapSmoothLag;
	
	  m_SnapMaxSpeed = 10000;
	  m_SnapSmoothLag = 0.001f;
	  m_HeightSmoothLag = 0.001f;
	
	  m_Snap = true;
	  Apply ();
	
	  m_HeightSmoothLag = oldHeightSmooth;
	  m_SnapMaxSpeed = oldSnapMaxSpeed;
	  m_SnapSmoothLag = oldSnapSmooth;
  }

  private void SetUpRotation(Vector3 centerPos, Vector3 headPos)
  {
	  // Now it's getting hairy. The devil is in the details here, the big issue is jumping of course.
	  // * When jumping up and down we don't want to center the guy in screen space.
	  //  This is important to give a feel for how high you jump and avoiding large camera movements.
	  //   
	  // * At the same time we dont want him to ever go out of screen and we want all rotations to be totally smooth.
	  //
	  // So here is what we will do:
	  //
	  // 1. We first find the rotation around the y axis. Thus he is always centered on the y-axis
	  // 2. When grounded we make him be centered
	  // 3. When jumping we keep the camera rotation but rotate the camera to get him back into view if his head is above some threshold
	  // 4. When landing we smoothly interpolate towards centering him on screen
	  Vector3 cameraPos = m_CameraTransform.position;
	  Vector3 offsetToCenter = centerPos - cameraPos;
	
	  // Generate base rotation only around y-axis
	  Quaternion yRotation = Quaternion.LookRotation(new Vector3(offsetToCenter.x, 0, offsetToCenter.z));

	  Vector3 relativeOffset = Vector3.forward * m_Distance + Vector3.down * m_Height;
	  m_CameraTransform.rotation = yRotation * Quaternion.LookRotation(relativeOffset);

	  // Calculate the projected center position and top position in world space
	  Ray centerRay = m_CameraTransform.camera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 1));
	  Ray topRay = m_CameraTransform.camera.ViewportPointToRay(new Vector3(0.5f, m_ClampHeadPositionScreenSpace, 1));

	  Vector3 centerRayPos = centerRay.GetPoint(m_Distance);
	  Vector3 topRayPos = topRay.GetPoint(m_Distance);
	
	  float centerToTopAngle = Vector3.Angle(centerRay.direction, topRay.direction);
	
	  float heightToAngle = centerToTopAngle / (centerRayPos.y - topRayPos.y);

	  float extraLookAngle = heightToAngle * (centerRayPos.y - centerPos.y);
	  if (extraLookAngle < centerToTopAngle)
	  {
		  extraLookAngle = 0;
	  }
	  else
	  {
      extraLookAngle = extraLookAngle - centerToTopAngle;
      m_CameraTransform.rotation *= Quaternion.Euler(-extraLookAngle, 0, 0);
    }
  }

  private Vector3 GetCenterOffset()
  {
	  return m_CenterOffset;
  }
    
  private Transform m_CameraTransform;
  private Transform m_Target;
  private bool m_IsFollow = false;
  private float m_FixedYaw = 0;

  // The distance in the x-z plane to the target
  public float m_Distance = 7.0f;
  // the height we want the camera to be above the target
  public float m_Height = 6.0f;

  private float m_AngularSmoothLag = 0.3f;
  private float m_AngularMaxSpeed = 15.0f;
  private float m_HeightSmoothLag = 0.3f;
  private float m_SnapSmoothLag = 0.2f;
  private float m_SnapMaxSpeed = 720.0f;
  private float m_ClampHeadPositionScreenSpace = 0.75f;
  private Vector3 m_HeadOffset = Vector3.zero;
  private Vector3 m_CenterOffset = Vector3.zero;
  private float m_HeightVelocity = 0.0f;
  private float m_AngleVelocity = 0.0f;
  private bool m_Snap = false;
  private float m_TargetHeight = 100000.0f;
  private int m_CurTargetId;
}
