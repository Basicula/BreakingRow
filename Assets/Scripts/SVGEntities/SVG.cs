using System.Collections.Generic;

public class SVG
{
  public float width;
  public float height;
  public string view_box;
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
    string svg = "<svg width=\"" + width + "px\" height=\"" + height + "px\" viewBox=\"" + view_box + "\">";
    foreach (SVGEntity entity in m_entities)
      svg += entity.GetXML();
    svg += "</svg>";
    return svg;
  }
}
