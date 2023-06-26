using UnityEngine;

public class SVGRect : SVGEntity
{
  private Vector2 m_start;
  private Vector2 m_size;
  private string fill_color;
  private string stroke_color;
  private float stroke_width;

  public SVGRect(
    Vector2 i_start,
    Vector2 i_size,
    string i_fill_color = "#ffffff",
    string i_stroke_color = "#000000",
    float i_stroke_width = 1
  )
  {
    m_start = i_start;
    m_size = i_size;

    fill_color = i_fill_color;
    stroke_color = i_stroke_color;
    stroke_width = i_stroke_width;
  }

  public override string GetXML()
  {
    System.Globalization.CultureInfo.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;
    return $"<rect " +
      $"x=\"{m_start.x}\" y=\"{m_start.y}\" " +
      $"width=\"{m_size.x}\" height=\"{m_size.y}\" " +
      $"fill=\"{fill_color}\" " +
      $"stroke-width=\"{stroke_width}\" stroke=\"{stroke_color}\"" +
    $"/>";
  }
}