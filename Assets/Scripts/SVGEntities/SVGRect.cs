using UnityEngine;

public class SVGRect : SVGEntity {
  private Vector2 m_start;
  private Vector2 m_size;
  private string fill_color;
  private SVGStrokeProps m_stroke_props;

  public SVGRect(
    Vector2 i_start,
    Vector2 i_size,
    string i_fill_color = "#ffffff",
    SVGStrokeProps i_stroke_props = new SVGStrokeProps()
  ) {
    m_start = i_start;
    m_size = i_size;

    fill_color = i_fill_color;
    m_stroke_props = i_stroke_props;
  }

  public override string GetXML() {
    System.Globalization.CultureInfo.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;
    return $"<rect " +
      $"x=\"{m_start.x}\" y=\"{m_start.y}\" " +
      $"width=\"{m_size.x}\" height=\"{m_size.y}\" " +
      $"fill=\"{fill_color}\" " +
      $"{m_stroke_props.GetXML()}" +
    $"/>";
  }
}