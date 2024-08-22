namespace DCCPanelController.Components.TrackPieces;

public class TrackState {

    private const string DefaultState = "Unknown";
    private string _state = DefaultState;
    private List<(string state, int offset)> _states = [(DefaultState, 1)];    

    public List<string> States => _states.Select(s => s.state).ToList();
    public void SetState(string state) => state = state ?? DefaultState;
    public void SetStates(params (string state, int offset)[] states) => _states = states.ToList();
    
    public void Next() {
        var currentIndex = _states.FindIndex(item => item.state == _state);
        if (currentIndex == -1) _state = DefaultState; 
        var nextIndex = (currentIndex + 1) % _states.Count;
        _state = _states[nextIndex].state;
    }

    public void Prev() {
        var currentIndex = _states.FindIndex(item => item.state == _state);
        if (currentIndex == -1) _state = DefaultState;
        var prevIndex = (currentIndex - 1 + _states.Count) % _states.Count;
        _state = _states[prevIndex].state;
    }
    
    /// <summary>
    /// Set the State but ensure it is one of the valid states 
    /// </summary>
    public string State {
        get => _state ?? DefaultState;
        set => _state = States.Contains(value, StringComparer.InvariantCultureIgnoreCase) ? value : DefaultState; 
    }
    public int GetOffset => _states.FirstOrDefault(s => s.state.Equals(_state)).offset;
}