using System;

public class FieldDataIterator {
  private int m_height;
  private int m_width;
  private (int, int) m_current_element;
  private (int, int) m_start_element;
  private (int, int) m_end_element;
  public readonly (int, int) direction;

  public FieldDataIterator(FieldConfiguration.MoveDirection i_move_direction, int i_height, int i_width) {
    m_width = i_width;
    m_height = i_height;
    switch (i_move_direction) {
      case FieldConfiguration.MoveDirection.TopToBottom:
        m_start_element = (m_height - 1, 0);
        m_end_element = (-1, m_width - 1);
        direction = (-1, 0);
        break;
      case FieldConfiguration.MoveDirection.RightToLeft:
        m_start_element = (0, 0);
        m_end_element = (m_height - 1, m_width);
        direction = (0, 1);
        break;
      case FieldConfiguration.MoveDirection.BottomToTop:
        m_start_element = (0, m_width - 1);
        m_end_element = (m_height, 0);
        direction = (1, 0);
        break;
      case FieldConfiguration.MoveDirection.LeftToRight:
        m_start_element = (m_height - 1, m_width - 1);
        m_end_element = (0, -1);
        direction = (0, -1);
        break;
      default:
        throw new NotImplementedException();
    }
    m_current_element = m_start_element;
  }

  public (int, int) current { get => m_current_element; set => m_current_element = value; }

  public bool Finished() {
    return m_current_element.Item1 == m_end_element.Item1 && m_current_element.Item2 == m_end_element.Item2;
  }

  public bool IsValid() {
    return m_current_element.Item1 >= 0 && m_current_element.Item2 >= 0 &&
      m_current_element.Item1 < m_height && m_current_element.Item2 < m_width;
  }

  public void Validate() {
    if (Finished())
      return;
    var orthogonal_direction = (direction.Item2, -direction.Item1);
    var offset_from_start = (Math.Abs(m_current_element.Item1 - m_start_element.Item1) + 1, Math.Abs(m_current_element.Item2 - m_start_element.Item2) + 1);
    m_current_element = (m_start_element.Item1 + orthogonal_direction.Item1 * offset_from_start.Item1,
      m_start_element.Item2 + orthogonal_direction.Item2 * offset_from_start.Item2);
  }

  public void Increment(bool i_with_validation) {
    if (Finished())
      return;
    m_current_element.Item1 += direction.Item1;
    m_current_element.Item2 += direction.Item2;
    if (i_with_validation && !IsValid())
      Validate();
  }
}