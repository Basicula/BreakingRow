using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameElement : MonoBehaviour {
  public enum State {
    Waiting,
    Creating,
    Destroying,
    Moving,
    Selected,
    Highlighted,
    Undefined
  }

  private class AnimationDetails {
    public List<Vector3> position_control_points;
    public List<Vector3> scale_control_points;
    public List<Vector3> rotation_control_points;
    public float start_time;
    public bool endless;

    public AnimationDetails() {
      position_control_points = new();
      scale_control_points = new();
      rotation_control_points = new();
    }

    public void Reset(bool i_endless) {
      position_control_points.Clear();
      scale_control_points.Clear();
      rotation_control_points.Clear();
      endless = i_endless;
      start_time = Time.time;
    }
  }

  [SerializeReference] private float m_animation_duration;
  private State m_state;
  private AnimationDetails m_animation_details;
  private static List<Vector3> m_shake_animation_control_rotations = new List<Vector3>()
    {
      Vector3.zero,
      new Vector3(0, 0, -15),
      Vector3.zero,
      new Vector3(0, 0, 15),
      Vector3.zero,
    };
  private static List<Vector3> m_shake_animation_control_scales = new List<Vector3>()
    {
      Vector3.one,
      new Vector3(1.1f, 1.1f, 1.1f),
      Vector3.one,
      new Vector3(0.9f, 0.9f, 0.9f),
      Vector3.one
    };
  public GameElement() {
    m_state = State.Undefined;
    m_animation_details = new AnimationDetails();
  }

  void Start() {
    transform.localScale = Vector3.zero;
  }

  void Update() {
    var elapsed_time = Time.time - m_animation_details.start_time;
    if (!m_animation_details.endless && elapsed_time > m_animation_duration) {
      m_state = State.Waiting;
      if (m_animation_details.scale_control_points.Count > 0)
        transform.localScale = m_animation_details.scale_control_points[^1];
      if (m_animation_details.position_control_points.Count > 0)
        transform.localPosition = m_animation_details.position_control_points[^1];
      if (m_animation_details.rotation_control_points.Count > 0)
        transform.eulerAngles = m_animation_details.rotation_control_points[^1];
    } else {
      if (m_animation_details.scale_control_points.Count > 0)
        transform.localScale = VectorUtilities.Lerp(m_animation_details.scale_control_points, m_animation_duration, elapsed_time);
      if (m_animation_details.position_control_points.Count > 0)
        transform.localPosition = VectorUtilities.Lerp(m_animation_details.position_control_points, m_animation_duration, elapsed_time);
      if (m_animation_details.rotation_control_points.Count > 0)
        transform.eulerAngles = VectorUtilities.Lerp(m_animation_details.rotation_control_points, m_animation_duration, elapsed_time);
    }
  }

  public void Destroy() {
    m_state = State.Destroying;
    m_animation_details.Reset(false);
    m_animation_details.scale_control_points.Add(Vector3.one);
    m_animation_details.scale_control_points.Add(Vector3.zero);
  }

  public void Create(ElementStyleProvider.ElementProps i_element_props, bool i_is_animated = true) {
    var sprite_handler_gameobject = transform.GetChild(0).gameObject;
    var text_canvas_handler_gameobject = transform.GetChild(1).gameObject;
    var tmp_handler_gameobject = text_canvas_handler_gameobject.transform.GetChild(0).gameObject;
    var tmp_rect = tmp_handler_gameobject.GetComponent<RectTransform>();
    tmp_rect.sizeDelta = new Vector2(i_element_props.text_zone_size, i_element_props.text_zone_size);
    var tmp_text = tmp_handler_gameobject.GetComponent<TMP_Text>();
    tmp_text.text = i_element_props.number;
    var sprite_renderer = sprite_handler_gameobject.GetComponent<SpriteRenderer>();
    sprite_renderer.sprite = i_element_props.sprite;

    m_animation_details.Reset(false);
    if (i_is_animated) {
      m_state = State.Creating;
      m_animation_details.scale_control_points.Add(Vector3.zero);
      m_animation_details.scale_control_points.Add(Vector3.one);
    } else {
      m_state = State.Waiting;
      transform.eulerAngles = Vector3.zero;
      transform.localScale = Vector3.one;
    }
  }

  public void MoveTo(Vector3 position) {
    m_state = State.Moving;
    m_animation_details.Reset(false);
    m_animation_details.position_control_points.Add(transform.position);
    m_animation_details.position_control_points.Add(position);
  }

  public bool IsAvailable() {
    return m_state == State.Waiting || m_state == State.Highlighted ||
      m_state == State.Selected;
  }

  public void UpdateSelection(bool is_selected) {
    if (!IsAvailable())
      return;
    m_animation_details.Reset(true);
    if (is_selected) {
      m_state = State.Selected;
      m_animation_details.scale_control_points = new List<Vector3>(m_shake_animation_control_scales);
      m_animation_details.rotation_control_points = new List<Vector3>(m_shake_animation_control_rotations);
    } else {
      m_state = State.Waiting;
      transform.eulerAngles = Vector3.zero;
      transform.localScale = Vector3.one;
    }
  }

  public void UpdateHighlighting(bool is_highlighted) {
    if (!IsAvailable())
      return;
    m_animation_details.Reset(true);
    if (is_highlighted) {
      m_state = State.Highlighted;
      m_animation_details.rotation_control_points = new List<Vector3>(m_shake_animation_control_rotations);
    } else {
      m_state = State.Waiting;
      transform.eulerAngles = Vector3.zero;
    }
  }
}
