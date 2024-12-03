using System.Diagnostics;

namespace DCCPanelController.Tracks.ImageManager;

[DebuggerDisplay("{Id}")]
public class SvgImageFile(string id, string svgFilename, string directions) {
    public string Id { get; init; } = id;
    public string SvgFilename { get; } = svgFilename;
    public SvgCompass Connections { get; } = new(directions);
}