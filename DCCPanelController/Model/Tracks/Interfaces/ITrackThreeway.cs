namespace DCCPanelController.Model.Tracks.Interfaces;

/// <summary>
///     Like a Turnout, but with 3 diverging points. So can be Straight or diverging, but Diverging is left or right.
/// </summary>
public interface ITrackThreeway : ITrackInteractive {
    public void Clicked();
}