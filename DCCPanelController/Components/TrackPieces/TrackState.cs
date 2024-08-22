namespace DCCPanelController.Components.TrackPieces;

public class TrackState {

    private const string DefaultState = "Unknown";
    private string _state = DefaultState;
    private Dictionary<string, int> _states = new Dictionary<string, int>();    

    public List<string> States => _states.Keys.ToList();
    
    /// <summary>
    /// Sets the active and current state
    /// </summary>
    /// <param name="state">The state to set the current state to</param>
    public void SetState(string state) => _state = _states.ContainsKey(state) ? state : DefaultState;

    /// <summary>
    /// Set up the available states using a default dictionary construct
    /// </summary>
    /// <param name="initialState">The initial or starting state to use. </param>
    /// <param name="states">States and Offsets as a dictionary</param>
    public void SetStates(Dictionary<string, int> states) => SetStates(states.First().Key, states);
    public void SetStates(string initialState, Dictionary<string, int> states) {
        _states = states;
        _states.TryAdd(DefaultState, 1);
        SetState(initialState);
    }

    /// <summary>
    /// Set up the available states using a list of parameters
    /// </summary>
    /// <param name="initialState">The initial or starting state to use. </param>
    /// <param name="states">Tuple(s) of State and Offsets</param>
    public void SetStates(params (string state, int offset)[] states) => SetStates(states[0].state, states);
    public void SetStates(string initialState, params (string state, int offset)[] states) {
        SetStates(initialState, states.Select(x => new KeyValuePair<string, int>(x.state, x.offset)).ToDictionary());
    }

    /// <summary>
    /// Step to the next state in the list, and if it was the last, cycle back to the first state
    /// </summary>
    public void Next() {
        try {
            var currentIndex = _states.ToList().FindIndex(item => item.Key == _state);
            if (currentIndex == -1) _state = DefaultState;
            var nextIndex = (currentIndex + 1) % _states.Count;
            _state = _states.ToList()[nextIndex].Key;
        } catch {
            _state = DefaultState;
        }
    }

    /// <summary>
    /// Step to the previous state in the list. If this was the first state, cycle to the last
    /// </summary>
    public void Prev() {
        try {
            var currentIndex = _states.ToList().FindIndex(item => item.Key == _state);
            if (currentIndex == -1) _state = DefaultState;
            var prevIndex = (currentIndex - 1 + _states.Count) % _states.Count;
            _state = _states.ToList()[prevIndex].Key;
        } catch {
            _state = DefaultState;
        }
    }
    
    /// <summary>
    /// Set the State but ensure it is one of the valid states 
    /// </summary>
    public string State {
        get => _state ?? DefaultState;
        set => SetState(value);
    }
    
    /// <summary>
    /// Given the current state, returns the offset to use for images based on this state
    /// </summary>
    public int Offset => GetOffset(_state);
    public int GetOffset(string state) => _states.ContainsKey(state) ? _states[state] : 1;
}