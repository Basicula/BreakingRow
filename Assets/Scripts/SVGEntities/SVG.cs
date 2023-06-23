using System.Collections.Generic;
using UnityEngine;

public class SVG
{
  public float width;
  public float height;
  public Vector2 viewbox_min;
  public Vector2 viewbox_size;
  private List<SVGEntity> m_entities;

  public SVG()
  {
    m_entities = new List<SVGEntity>();
  }

  public void Add(SVGEntity entity)
  {
    m_entities.Add(entity);
  }

  public string GetXML()
  {
    System.Globalization.CultureInfo.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;
    string svg = $"<svg " +
      $"width=\"{width}px\" " +
      $"height=\"{height}px\" " +
      $"viewBox=\"{viewbox_min.x} {viewbox_min.y} {viewbox_size.x} {viewbox_size.y}\">";
    foreach (SVGEntity entity in m_entities)
      svg += entity.GetXML();
    svg += "</svg>";
    return svg;
  }
}
