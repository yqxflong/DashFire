using UnityEngine;
using System.Collections;

public enum FingerMotionPhase 
{
  None = 0,
  Started,
  Updated,
  Ended,
}

public class FingerMotionEvent : FingerEvent 
{
  FingerMotionPhase phase = FingerMotionPhase.None;
  Vector2 position = Vector2.zero;
  float starttime = 0;

  public float StartTime
  {
    get
    {
      return starttime;
    }
    set
    {
      starttime = value;
    }
  }

  public override Vector2 Position
  {
    get
    {
      return position;
    }
    set
    {
      position = value;
    }
  }

  /// ��ʾ�¼��Ľ׶�
  public FingerMotionPhase Phase
  {
    get
    {
      return phase;
    }
    set
    {
      phase = value;
    }
  }

  /// �ӿ�ʼ�׶ε����ڻ��ѵ�ʱ��
  public float ElapsedTime
  {
    get
    {
      return Mathf.Max(0, Time.time - StartTime);
    }
  }
}

/// ׷��һ����ָ���ƶ���ֹ״̬
public class FingerMotion : FingerEventDetector<FingerMotionEvent> 
{
  enum EventType 
  {
    Move,
    Stationary
  }

  public FingerEventHandler OnFingerMove;
  public FingerEventHandler OnFingerStationary;

  public string MoveMessageName = "OnFingerMove";
  public string StationaryMessageName = "OnFingerStationary";
  public bool TrackMove = true;
  public bool TrackStationary = true;

  bool FireEvent(FingerMotionEvent e, EventType eventType, FingerMotionPhase phase, Vector2 position, bool updateSelection)
  {
    if ((!TrackMove && eventType == EventType.Move) || (!TrackStationary && eventType == EventType.Stationary)) {
      return false;
    }

    e.Phase = phase;
    e.Position = position;

    if (e.Phase == FingerMotionPhase.Started) {
      e.StartTime = Time.time;
    }

    if (eventType == EventType.Move) {
      e.Name = MoveMessageName;
      if (OnFingerMove != null)
        OnFingerMove(e);
      TrySendMessage(e);
    } else if (eventType == EventType.Stationary) {
      e.Name = StationaryMessageName;
      if (OnFingerStationary != null)
        OnFingerStationary(e);
      TrySendMessage(e);
    } else {
      Debug.Log("Invalid event type: " + eventType);
      return false;
    }

    return true;
  }

  protected override void ProcessFinger(TouchManager.Finger finger)
  {
    FingerMotionEvent e = GetEvent(finger);
    bool selectionUpdated = false;

    // ��ָ״̬�ı�
    if (finger.Phase != finger.PreviousPhase) {
      switch (finger.PreviousPhase) {
        case TouchManager.FingerPhase.Moving:
          selectionUpdated |= FireEvent(e, EventType.Move, FingerMotionPhase.Ended, finger.Position, !selectionUpdated);
          break;
        case TouchManager.FingerPhase.Stationary:
          selectionUpdated |= FireEvent(e, EventType.Stationary, FingerMotionPhase.Ended, finger.PreviousPosition, !selectionUpdated);
          break;
      }

      switch (finger.Phase) {
        case TouchManager.FingerPhase.Moving:
          selectionUpdated |= FireEvent(e, EventType.Move, FingerMotionPhase.Started, finger.PreviousPosition, !selectionUpdated);
          selectionUpdated |= FireEvent(e, EventType.Move, FingerMotionPhase.Updated, finger.Position, !selectionUpdated);
          break;
        case TouchManager.FingerPhase.Stationary:
          selectionUpdated |= FireEvent(e, EventType.Stationary, FingerMotionPhase.Started, finger.Position, !selectionUpdated);
          selectionUpdated |= FireEvent(e, EventType.Stationary, FingerMotionPhase.Updated, finger.Position, !selectionUpdated);
          break;
      }
    } else {
      // ��ָ״̬һֱһ��
      switch (finger.Phase) {
        case TouchManager.FingerPhase.Moving:
          selectionUpdated |= FireEvent(e, EventType.Move, FingerMotionPhase.Updated, finger.Position, !selectionUpdated);
          break;
        case TouchManager.FingerPhase.Stationary:
          selectionUpdated |= FireEvent(e, EventType.Stationary, FingerMotionPhase.Updated, finger.Position, !selectionUpdated);
          break;
      }
    }
  }
}
