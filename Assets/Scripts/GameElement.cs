using System.Collections.Generic;
using UnityEngine;

public class GameElement : MonoBehaviour
{
  public enum State
  {
    Waiting,
    Creating,
    Destroying,
    Moving,
    Selected,
    Highlighted,
    Undefined
  }

  [SerializeReference] private float m_animation_duration;
  private State m_state;
  private float m_creation_start_time;
  private float m_destroy_start_time;
  private float m_moving_start_time;
  private float m_select_start_time;
  private float m_highlight_start_time;
  private Vector3 m_move_target_position;
  private Vector3 m_move_start_position;
  private List<Vector3> m_shake_animation_control_rotations;
  private List<Vector3> m_shake_animation_control_scales;
  public GameElement()
  {
    m_state = State.Undefined;
    m_shake_animation_control_rotations = new List<Vector3>()
    {
      new Vector3(0, 0, 0),
      new Vector3(0, 0, -15),
      new Vector3(0, 0, 0),
      new Vector3(0, 0, 15)
    };
    m_shake_animation_control_scales = new List<Vector3>()
    {
      new Vector3(1, 1, 1),
      new Vector3(1.1f, 1.1f, 1.1f),
      new Vector3(1, 1, 1),
      new Vector3(0.9f, 0.9f, 0.9f)
    };
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
        if (Time.time - m_creation_start_time > m_animation_duration)
        {
          m_state = State.Waiting;
          transform.localScale = new Vector3(1, 1, 1);
        }
        else
          transform.localScale = Vector3.Lerp(new Vector3(0, 0, 0), new Vector3(1, 1, 1), (Time.time - m_creation_start_time) / m_animation_duration);
        break;
      case State.Destroying:
        if (Time.time - m_destroy_start_time > m_animation_duration)
        {
          m_state = State.Waiting;
          transform.localScale = new Vector3(0, 0, 0);
        }
        else
          transform.localScale = Vector3.Lerp(new Vector3(1, 1, 1), new Vector3(0, 0, 0), (Time.time - m_destroy_start_time) / m_animation_duration);
        break;
      case State.Moving:
        if (Time.time - m_moving_start_time > m_animation_duration)
        {
          m_state = State.Waiting;
          transform.position = m_move_target_position;
        }
        else
          transform.position = Vector3.Lerp(m_move_start_position, m_move_target_position, (Time.time - m_moving_start_time) / m_animation_duration);
        break;
      case State.Selected:
        transform.eulerAngles = VectorUtilities.Lerp(
          m_shake_animation_control_rotations,
          m_animation_duration,
          Time.time - m_select_start_time
        );
        transform.localScale = VectorUtilities.Lerp(
          m_shake_animation_control_scales,
          m_animation_duration,
          Time.time - m_select_start_time
        );
        break;
      case State.Highlighted:
        transform.eulerAngles = VectorUtilities.Lerp(
          m_shake_animation_control_rotations,
          m_animation_duration,
          Time.time - m_highlight_start_time
        );
        break;
      case State.Undefined:
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

  public void Create(ElementStyleProvider.ElementProps i_element_props)
  {
    transform.localScale = new Vector3(1, 1, 1);
    var sprite_handler_gameobject = transform.GetChild(0).gameObject;
    var text_handler_gameobject = transform.GetChild(1).gameObject;
    var sprite_renderer = sprite_handler_gameobject.GetComponent<SpriteRenderer>();
    sprite_renderer.sprite = i_element_props.sprite;
    var text_mesh = text_handler_gameobject.GetComponent<TextMesh>();
    text_mesh.text = i_element_props.number;
    text_mesh.fontSize = i_element_props.font_size;
    m_state = State.Creating;
    m_creation_start_time = Time.time;
    transform.eulerAngles = new Vector3(0, 0, 0);
    transform.localScale = new Vector3(0, 0, 0);
  }

  public void MoveTo(Vector3 position)
  {
    m_state = State.Moving;
    m_move_target_position = position;
    m_move_start_position = transform.position;
    m_moving_start_time = Time.time;
  }

  public bool IsAvailable()
  {
    return m_state == State.Waiting || m_state == State.Highlighted ||
      m_state == State.Selected;
  }

  public void UpdateSelection(bool is_selected)
  {
    if (!IsAvailable())
      return;
    if (is_selected)
    {
      m_state = State.Selected;
      m_select_start_time = Time.time;
    }
    else
    {
      m_state = State.Waiting;
      transform.eulerAngles = new Vector3(0, 0, 0);
      transform.localScale = new Vector3(1, 1, 1);
    }
  }

  public void UpdateHighlighting(bool is_highlighted)
  {
    if (!IsAvailable())
      return;
    if (is_highlighted)
    {
      m_state = State.Highlighted;
      m_highlight_start_time = Time.time;
    }
    else
    {
      m_state = State.Waiting;
      transform.eulerAngles = new Vector3(0, 0, 0);
    }
  }
}
