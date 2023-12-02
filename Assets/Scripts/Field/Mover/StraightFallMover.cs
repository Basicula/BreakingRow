using System.Collections.Generic;

public class StraightFallMover : IFieldElementsMover {
  public StraightFallMover(FieldData i_field_data) : base(i_field_data) {
  }

  public override FieldChanges Move() {
    var changes = new FieldChanges();
    var field_configuration = m_field_data.configuration;
    var empty_element = (-1, -1);
    var it = new FieldDataIterator(field_configuration.move_direction, field_configuration.height, field_configuration.width);
    while (!it.Finished()) {
      if (!it.IsValid()) {
        it.Validate();
        empty_element = (-1, -1);
      }
      var curr_element = it.current;
      if (m_field_data[curr_element].type == FieldElement.Type.Empty) {
        m_field_data[curr_element] = FieldElementsFactory.empty_element;
        if (empty_element == (-1, -1))
          empty_element = curr_element;
      } else if (m_field_data[curr_element].movable && empty_element != (-1, -1)) {
        m_field_data.SwapCells(curr_element, empty_element);
        changes.moved.Add((curr_element, new List<(int, int)> { curr_element, empty_element }));
        it.current = empty_element;
        empty_element = (-1, -1);
      } else if (!m_field_data[curr_element].movable && m_field_data[curr_element] != FieldElementsFactory.hole_element) {
        if (empty_element != (-1, -1)) {
          var stay_empty_element = empty_element;
          while (stay_empty_element != curr_element) {
            m_field_data[stay_empty_element] = FieldElementsFactory.CreateElement(FieldElement.Type.Empty, 0);
            stay_empty_element.Item1 += it.direction.Item1;
            stay_empty_element.Item2 += it.direction.Item2;
          }
        }
        empty_element = (-1, -1);
      }
      it.Increment(false);
    }
    return changes;
  }
}