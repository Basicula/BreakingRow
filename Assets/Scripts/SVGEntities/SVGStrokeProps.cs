public struct SVGStrokeProps
{
  public string stroke_color;
  public float stroke_width;

  public SVGStrokeProps(string i_stroke_color = "#000000", float i_stroke_width = 1)
  {
    stroke_color = i_stroke_color;
    stroke_width = i_stroke_width;
  }

  public string GetXML()
  {
    System.Globalization.CultureInfo.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;
    if (stroke_width == 0 || stroke_color == "none")
      return "";
    return $"stroke-width=\"{stroke_width}\" stroke=\"{stroke_color}\" ";
  }
}