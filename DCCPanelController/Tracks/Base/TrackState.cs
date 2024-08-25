namespace DCCPanelController.Tracks.Base;

public class TrackState {

    private const string DefaultState = "Unknown";
    public string _state = DefaultState;
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
            var currentIndex = States.FindIndex(item => item == _state);
            if (currentIndex == -1) _state = DefaultState;
            var nextIndex = (currentIndex + 1) % States.Count;
            State = States[nextIndex];
        } catch {
            _state = DefaultState;
        }
    }

    /// <summary>
    /// Step to the previous state in the list. If this was the first state, cycle to the last
    /// </summary>
    public void Prev() {
        try {
            var currentIndex = States.FindIndex(item => item == _state);
            if (currentIndex == -1) _state = DefaultState;
            var prevIndex = (currentIndex - 1 + States.Count) % States.Count;
            _state = States[prevIndex];
        } catch {
            _state = DefaultState;
        }
    }

    public void First() {
        _state = States.Count > 0 ? States[0] : DefaultState;
    }
    
    /// <summary>
    /// Set the State but ensure it is one of the valid states 
    /// </summary>
    public string State {
        get => _state ?? DefaultState;
        set => _state = States.Contains(value) ? value : DefaultState;
    }
}