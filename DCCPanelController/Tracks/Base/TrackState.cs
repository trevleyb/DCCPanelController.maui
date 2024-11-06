namespace DCCPanelController.Tracks.Base;

public class TrackState {
    public string _state = TrackPiece.UnknownState;
    public List<string> States = [];

    /// <summary>
    /// Add a State to the collection of available States
    /// </summary>
    /// <param name="state">The State to Add</param>
    public void AddState(string state) {
        if (!States.Contains(state)) States.Add(state);
    }

    /// <summary>
    /// Step to the next state in the list, and if it was the last, cycle back to the first state
    /// </summary>
    public void Next() {
        try {
            State = GetNextValidState(1);
        } catch {
            State = TrackPiece.UnknownState;
        }
    }

    public void Prev() {
        try {
            State = GetNextValidState(-1);
        } catch {
            State = TrackPiece.UnknownState;
        }
    }

    private string GetNextValidState(int direction) {
        var nextState = GetNextState(direction);
        if (nextState.Equals(TrackPiece.UnknownState) && States.Count > 1) {
            nextState = GetNextStateFrom(nextState, direction);
        }

        return nextState;
    }

    private string GetNextState(int direction) {
        return GetNextStateFrom(_state, direction);
    }

    private string GetNextStateFrom(string currentState, int direction) {
        var currentIndex = States.FindIndex(item => item == currentState);

        if (currentIndex == -1) return TrackPiece.UnknownState;

        var nextIndex = (currentIndex + direction + States.Count) % States.Count;
        return States[nextIndex];
    }

    public void First() {
        _state = States.Count > 0 ? States[0] : TrackPiece.UnknownState;
    }

    /// <summary>
    /// Set the State but ensure it is one of the valid states 
    /// </summary>
    public string State {
        get => _state ?? TrackPiece.UnknownState;
        set => _state = States.Contains(value) ? value : TrackPiece.UnknownState;
    }
}