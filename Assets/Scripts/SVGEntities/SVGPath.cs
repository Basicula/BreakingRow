using System.Collections.Generic;
using UnityEngine;

public class SVGPath : SVGEntity
{
  public float stroke_width;
  public string fill_color;
  public string stroke_color;
  private List<string> m_commands;
  private bool m_is_closed;

  public SVGPath()
  {
    m_commands = new List<string>();
    m_is_closed = false;
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
    m_is_closed = true;
  }

  public override string GetXML()
  {
    System.Globalization.CultureInfo.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;
    string path = string.Join(" ", m_commands);
    var closure = m_is_closed ? " z" : "";
    return $"<path d=\"{path}{closure}\" fill=\"{fill_color}\" stroke-width=\"{stroke_width}\" stroke=\"{stroke_color}\"/>";
  }
}
