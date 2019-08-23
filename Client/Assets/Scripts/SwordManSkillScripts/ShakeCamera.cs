using UnityEngine;
using System.Collections;

public class ShakeCamera : MonoBehaviour {
  private bool is_shaking_ = false;
  private float duration;
  private float frequency;
  private float randomPercent;
  private float amplitude;
  private float cur_frequency_;
  private System.Random random;
  private Camera cur_camera;

  private bool is_controled_ = false;
  private float control_remain_time_;
  private float move_time_;
  private float back_time_;
  private float hold_time_;
  private Vector3 move_speed_;
  private Vector3 back_speed_;

  private MainCamera camera_controller = null;
	// Use this for initialization
	void Start () {
    GameObject gfx_root = GameObject.Find("GfxGameRoot");
    if (gfx_root != null) {
      camera_controller = gfx_root.GetComponent<MainCamera>();
    }
	}

	// Update is called once per frame
	void Update () {
    UpdateShake();
    UpdateCameraMove();
	}

  private void ControlCurCamera(bool iscontrol)
  {
    if (camera_controller != null) {
      if (iscontrol) {
        camera_controller.CameraFollowStop(camera_controller.GetTargetId());
      } else {
        camera_controller.CameraFollow(camera_controller.GetTargetId());
      }
    }
  }

  public void StartShakeCamera(string str_params) {
    string[] param_list = str_params.Split(' ');
    if (param_list.Length < 4) {
      return;
    }
    duration = (float)System.Convert.ToDouble(param_list[0]);
    frequency = (float)System.Convert.ToDouble(param_list[1]);
    randomPercent = (float)System.Convert.ToDouble(param_list[2]);
    amplitude = (float)System.Convert.ToDouble(param_list[3]);
    is_shaking_ = true;
    random = new System.Random();
    cur_camera = Camera.main;
    if (cur_camera == null) {
      return;
    }
    cur_frequency_ = 0;
    ControlCurCamera(true);
  }

  public void MoveCamera(string param_str) {
    string[] param_list = param_str.Split(' ');
    if (param_list.Length < 9) {
      return;
    }
    int index = 0;
    move_time_ = (float)System.Convert.ToDouble(param_list[index++]);
    move_speed_.x = (float)System.Convert.ToDouble(param_list[index++]);
    move_speed_.y = (float)System.Convert.ToDouble(param_list[index++]);
    move_speed_.z = (float)System.Convert.ToDouble(param_list[index++]);
    hold_time_ = (float)System.Convert.ToDouble(param_list[index++]);
    back_time_ = (float)System.Convert.ToDouble(param_list[index++]);
    back_speed_.x = (float)System.Convert.ToDouble(param_list[index++]);
    back_speed_.y = (float)System.Convert.ToDouble(param_list[index++]);
    back_speed_.z = (float)System.Convert.ToDouble(param_list[index++]);
    control_remain_time_ = move_time_ + hold_time_ + back_time_;
    cur_camera = Camera.main;
    if (cur_camera == null) {
      Debug.Log("move camera, cur camera is null!");
      return;
    }
    ControlCurCamera(true);
    is_controled_ = true;
  }

  private void UpdateCameraMove() {
    if (!is_controled_) {
      return;
    }
    if (cur_camera == null) {
      return;
    }
    if (move_time_ > 0) {
      Vector3 pos = cur_camera.transform.position;
      cur_camera.transform.position = pos +  move_speed_ * Time.deltaTime;
      move_time_ -= Time.deltaTime;
    } else if (hold_time_ > 0) {
      hold_time_ -= Time.deltaTime;
    } else if (back_time_ > 0) {
      Vector3 pos = cur_camera.transform.position;
      cur_camera.transform.position = pos +  back_speed_ * Time.deltaTime;
      back_time_ -= Time.deltaTime;
    }
    control_remain_time_ -= Time.deltaTime;
    if (control_remain_time_ <= 0) {
      is_controled_ = false;
      if (!is_shaking_) {
        ControlCurCamera(false);
      }
    }
  }

  private void UpdateShake() {
    if (!is_shaking_) {
      return;
    }
    if (cur_camera == null) {
      return;
    }
    duration -= Time.deltaTime;
    cur_frequency_ -= Time.deltaTime;
    if (cur_frequency_ <= 0) {
      cur_frequency_ = frequency;
      int percent = random.Next(0, 100);
      if (percent <= randomPercent) {
        float xoff = amplitude * random.Next(-100, 100) / 100.0f;
        float zoff = amplitude * random.Next(-100, 100) / 100.0f;
        float yoff = amplitude * random.Next(-100, 100) / 100.0f;
        Vector3 new_pos = cur_camera.transform.position;
        new_pos.x += xoff;
        new_pos.z += zoff;
        new_pos.y += yoff;
        cur_camera.transform.position = new_pos;
      }
    }
    if (duration <= 0) {
      is_shaking_ = false;
      if (!is_controled_) {
        ControlCurCamera(false);
      }
    }
  }
}
