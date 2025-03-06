using DCCPanelController.Model.Tracks.Base;
using DCCPanelController.Model.Tracks.Interfaces;
using DCCPanelController.Tracks.ImageManager;
using DCCPanelController.Tracks.StyleManager;
using DCCPanelController.View.PropertyPages.Attributes;

namespace DCCPanelController.Model.DataModel.Tracks;

public partial class TrackPoints : Track {
    public override string Name => "Connection Points";
}