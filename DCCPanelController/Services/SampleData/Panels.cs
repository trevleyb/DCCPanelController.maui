using System.Collections.ObjectModel;
using DCCPanelController.Model;

namespace DCCPanelController.Services.SampleData;

public static class SamplePanels {
    public static ObservableCollection<Panel> DemoData() {
        //var panels = new ObservableCollection<Panel>();
        //panels.Add(CreateTestPanel("Test Panel #1"));
        //panels.Add(CreateTestPanel("Test Panel #2"));
        //panels.Add(CreateTestPanel("Test Panel #3"));
        //return panels;
        return new ObservableCollection<Panel>();
    }

    //private static Panel CreateTestPanel(string name) {
        //var panel = new Panel(name, 18, 12);

        // panel.Tracks.Add(new TrackStraight() { X = 1, Y = 1, ImageRotation = 0 });
        // panel.Tracks.Add(new TrackStraight() { X = 2, Y = 2, ImageRotation = 90 });
        //
        // panel.Tracks.Add(new TrackCorner() { X = 2, Y = 3, ImageRotation = 180 });
        // panel.Tracks.Add(new TrackStraight() { X = 3, Y = 3, ImageRotation = 180 });
        // panel.Tracks.Add(new TrackStraight() { X = 4, Y = 3, ImageRotation = 180 });
        // panel.Tracks.Add(new TrackStraight() { X = 5, Y = 3, ImageRotation = 180 });
        // panel.Tracks.Add(new TrackStraight() { X = 6, Y = 3, ImageRotation = 180 });
        // panel.Tracks.Add(new TrackStraight() { X = 7, Y = 3, ImageRotation = 180 });
        // panel.Tracks.Add(new TrackStraight() { X = 8, Y = 3, ImageRotation = 180 });
        // panel.Tracks.Add(new TrackCorner() { X = 9, Y = 3, ImageRotation = 0 });
        //
        // panel.Tracks.Add(new TrackStraight() { X = 4, Y = 4, ImageRotation = 270 });
        // panel.Tracks.Add(new TrackStraight() { X = 5, Y = 5, ImageRotation = 0 });
        //
        // panel.Tracks.Add(new TrackButton() { X = 10, Y = 8, ImageRotation = 0 });
        // panel.Tracks.Add(new TrackCrossing() { X = 11, Y = 9, ImageRotation = 0 });
        // panel.Tracks.Add(new TrackTerminator() { X = 12, Y = 10, ImageRotation = 0 });
        // panel.Tracks.Add(new TrackLeftTurnout() { X = 14, Y = 11, ImageRotation = 0 });
        // panel.Tracks.Add(new TrackLeftTurnout() { X = 14, Y = 12, ImageRotation = 0 });
        // panel.Tracks.Add(new TrackRightTurnout() { X = 15, Y = 8, ImageRotation = 0 });

        //panel.Tracks.Add(new TrackThreeway() { X = 13, Y = 11, ImageRotation = 0 });

        //return panel;
    //}
}