using System.Collections.Generic;
using Unity.VectorGraphics;
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
    const string svg_xml_header = "<?xml version=\"1.0\" encoding=\"utf - 8\"?>";
    string svg = $"{svg_xml_header}<svg " +
      $"width=\"{width}px\" " +
      $"height=\"{height}px\" " +
      $"viewBox=\"{viewbox_min.x} {viewbox_min.y} {viewbox_size.x} {viewbox_size.y}\">";
    foreach (SVGEntity entity in m_entities)
      svg += entity.GetXML();
    svg += "</svg>";
    return svg;
  }

  public static Sprite BuildSprite(SVG i_svg, float i_step_distance,
    float i_sampling_step_size = 1.0f, float i_max_cord_deviation = 0.0f, float i_max_tan_angle_deviation = 0.0f)
  {
    using System.IO.StringReader textReader = new System.IO.StringReader(i_svg.GetXML());
    var sceneInfo = SVGParser.ImportSVG(textReader);
    var geometries = VectorUtils.TessellateScene(sceneInfo.Scene, new VectorUtils.TessellationOptions
    {
      StepDistance = i_step_distance,
      SamplingStepSize = i_sampling_step_size,
      MaxCordDeviation = i_max_cord_deviation,
      MaxTanAngleDeviation = i_max_tan_angle_deviation
    });
    return VectorUtils.BuildSprite(geometries, 1, VectorUtils.Alignment.Center, Vector2.zero, 128, false);
  }
}
