using UnityEngine;

public class GameElement : MonoBehaviour
{
  public enum State
  {
    Waiting,
    Creating,
    Destroying,
    Moving,
    Undefined
  }

  public float animation_duration;

  private State m_state;
  private float m_creation_start_time;
  private float m_destroy_start_time;
  private float m_moving_start_time;
  private Vector3 m_move_target_position;
  private Vector3 m_move_start_position;

  public GameElement()
  {
    m_state = State.Undefined;
  }

  void Start()
  {
    transform.localScale = new Vector3(0, 0, 0);
  }

  void Update()
  {
    switch (m_state)
    {
      case State.Creating:
        if (Time.time - m_creation_start_time > animation_duration)
        {
          m_state = State.Waiting;
          transform.localScale = new Vector3(1, 1, 1);
        }
        else
          transform.localScale = Vector3.Lerp(new Vector3(0, 0, 0), new Vector3(1, 1, 1), (Time.time - m_creation_start_time) / animation_duration);
        break;
      case State.Destroying:
        if (Time.time - m_destroy_start_time > animation_duration)
        {
          m_state = State.Waiting;
          transform.localScale = new Vector3(0, 0, 0);
        }
        else
          transform.localScale = Vector3.Lerp(new Vector3(1, 1, 1), new Vector3(0, 0, 0), (Time.time - m_destroy_start_time) / animation_duration);
        break;
      case State.Moving:
        if (Time.time - m_moving_start_time > animation_duration)
        {
          m_state = State.Waiting;
          transform.position = m_move_target_position;
        }
        else
          transform.position = Vector3.Lerp(m_move_start_position, m_move_target_position, (Time.time - m_moving_start_time) / animation_duration);
        break;
      case State.Waiting:
      default:
        return;
    }
  }

  public void Destroy()
  {
    m_state = State.Destroying;
    m_destroy_start_time = Time.time;
  }

  public void Create(Sprite sprite)
  {
    GetComponent<SpriteRenderer>().sprite = sprite;
    m_state = State.Creating;
    m_creation_start_time = Time.time;
  }

  public void MoveTo(Vector3 position)
  {
    m_state = State.Moving;
    m_move_target_position = position;
    m_move_start_position = transform.position;
    m_moving_start_time = Time.time;
  }

  public State state
  {
    get => m_state;
  }
}
