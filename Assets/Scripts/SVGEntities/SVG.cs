using System.Collections.Generic;

public class SVG
{
  public float width;
  public float height;
  public string view_box;
  private List<SVGEntity> entities;

  public SVG()
  {
    entities = new List<SVGEntity>();
  }

  public void Add(SVGEntity entity)
  {
    entities.Add(entity);
  }

  public string GetXML()
  {
    string svg = "<svg width=\"" + width + "px\" height=\"" + height + "px\" viewBox=\"" + view_box + "\">";
    foreach (SVGEntity entity in entities)
      svg += entity.GetXML();
    svg += "</svg>";
    return svg;
  }
}
