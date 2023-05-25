using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SVGText : SVGEntity
{
  public string text;
  public string font_family;
  public int font_size;
  public int font_weight;
  public string fill_color;
  public string stroke_color;
  public int stroke_width;
  public int x;
  public int y;

  public override string GetXML()
  {
    return $"<text " +
      $"x=\"{x}\" " +
      $"y=\"{y}\" " +
      $"fill=\"{fill_color}\" " +
      $"stroke=\"{stroke_color}\" " +
      $"font-size=\"{font_size}\" " +
      $"stroke-width=\"{stroke_width}\" " +
      $"text-anchor=\"middle\" " +
      $"alignment-baseline=\"middle\">" +
      $"{text}" +
      $"</text>";
  }
}