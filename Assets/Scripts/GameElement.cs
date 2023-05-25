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
    this.m_state = State.Undefined;
  }

  void Start()
  {
    this.transform.localScale = new Vector3(0, 0, 0);
  }

  void Update()
  {
    switch (this.m_state)
    {
      case State.Creating:
        if (Time.time - this.m_creation_start_time > animation_duration)
        {
          this.m_state = State.Waiting;
          transform.localScale = new Vector3(1, 1, 1);
        }
        else
          transform.localScale = Vector3.Lerp(new Vector3(0, 0, 0), new Vector3(1, 1, 1), (Time.time - this.m_creation_start_time) / animation_duration);
        break;
      case State.Destroying:
        if (Time.time - this.m_destroy_start_time > animation_duration)
        {
          this.m_state = State.Waiting;
          transform.localScale = new Vector3(0, 0, 0);
        }
        else
          transform.localScale = Vector3.Lerp(new Vector3(1, 1, 1), new Vector3(0, 0, 0), (Time.time - this.m_destroy_start_time) / animation_duration);
        break;
      case State.Moving:
        if (Time.time - this.m_moving_start_time > animation_duration)
        {
          this.m_state = State.Waiting;
          transform.position = this.m_move_target_position;
        }
        else
          transform.position = Vector3.Lerp(this.m_move_start_position, this.m_move_target_position, (Time.time - this.m_moving_start_time) / animation_duration);
        break;
      case State.Waiting:
      default:
        return;
    }
  }

  public void destroy()
  {
    this.m_state = State.Destroying;
    this.m_destroy_start_time = Time.time;
  }

  public void create(Sprite sprite)
  {
    GetComponent<SpriteRenderer>().sprite = sprite;
    this.m_state = State.Creating;
    this.m_creation_start_time = Time.time;
  }

  public void move_to(Vector3 position)
  {
    this.m_state = State.Moving;
    this.m_move_target_position = position;
    this.m_move_start_position = transform.position;
    this.m_moving_start_time = Time.time;
  }

  public State state
  {
    get => this.m_state;
  }
}
