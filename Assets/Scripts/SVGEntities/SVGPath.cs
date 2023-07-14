using System.Collections.Generic;
using UnityEngine;

public class SVGPath : SVGEntity
{
  public string fill_color;
  public SVGStrokeProps stroke_props;
  private List<string> m_commands;

  public SVGPath()
  {
    m_commands = new List<string>();
  }

  public void MoveTo(Vector2 point)
  {
    System.Globalization.CultureInfo.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;
    m_commands.Add($"M {point[0]} {point[1]}");
  }

  public void LineTo(Vector2 point)
  {
    System.Globalization.CultureInfo.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;
    m_commands.Add($"L {point[0]} {point[1]}");
  }

  public void ArcTo(float rounding_radius, bool arc_direction, Vector2 point)
  {
    System.Globalization.CultureInfo.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;
    int arc_direction_value = arc_direction ? 0 : 1;
    m_commands.Add($"A {rounding_radius} {rounding_radius} 0 0 {arc_direction_value} {point[0]} {point[1]}");
  }

  public void Close()
  {
    m_commands.Add("z");
  }

  public override string GetXML()
  {
    System.Globalization.CultureInfo.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;
    string path = string.Join(" ", m_commands);
    return $"<path d=\"{path}\" fill=\"{fill_color}\" {stroke_props.GetXML()}/>";
  }
}
