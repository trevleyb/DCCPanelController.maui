using System.Collections.ObjectModel;
using DCCPanelController.Model;

namespace DCCPanelController.Components.Symbols;

public class SymbolFactory {
    /// <summary>
    /// Build up the Toolbar Icons that will allow drag onto the main canvas
    /// </summary>
    /// <returns></returns>
    public static ObservableCollection<SymbolViewModel> AvailableTracks() {
        var availableTracks = new ObservableCollection<SymbolViewModel> {
            new() { TrackType = TrackTypesEnum.StraightTrack,   Name = "Straight",   Image = ImageSource.FromFile("straight.png") },
            new() { TrackType = TrackTypesEnum.Terminator,      Name = "Terminate",  Image = ImageSource.FromFile("terminate.png") },
            new() { TrackType = TrackTypesEnum.Crossing,        Name = "Crossing",   Image = ImageSource.FromFile("crossing.png") },
            new() { TrackType = TrackTypesEnum.LeftTrack,       Name = "Left",       Image = ImageSource.FromFile("angleleft.png") },
            new() { TrackType = TrackTypesEnum.RightTrack,      Name = "Right",      Image = ImageSource.FromFile("angleright.png") },
            new() { TrackType = TrackTypesEnum.LeftTurnout,     Name = "Turnout(L)", Image = ImageSource.FromFile("turnoutleft.png") },
            new() { TrackType = TrackTypesEnum.RightTurnout,    Name = "Turnout(R)", Image = ImageSource.FromFile("turnoutright.png") },
            new() { TrackType = TrackTypesEnum.WyeJunction,     Name = "Wye-Junction", Image = ImageSource.FromFile("yjunction.png") },
        };
        return availableTracks;
    }
}