// TODO: REMOVE THIS

// using DCCPanelController.Tracks.StyleManager;
//
// namespace DCCPanelController.Tracks.Base;
//
// public class TrackState {
//     private _trackStyleSubType  _state = _trackStyleSubType.Unknown;
//     private List<_trackStyleSubType> States = [];
//
//     /// <summary>
//     /// Add a State to the collection of available States
//     /// </summary>
//     /// <param name="state">The State to Add</param>
//     public void AddState(_trackStyleSubType state) {
//         if (!States.Contains(state)) States.Add(state);
//     }
//
//     /// <summary>
//     /// Step to the next state in the list, and if it was the last, cycle back to the first state
//     /// </summary>
//     public void Next() {
//         try {
//             State = GetNextValidState(1);
//         } catch {
//             State = _trackStyleSubType.Unknown;
//         }
//     }
//
//     public void Prev() {
//         try {
//             State = GetNextValidState(-1);
//         } catch {
//             State = _trackStyleSubType.Unknown;
//         }
//     }
//
//     private _trackStyleSubType GetNextValidState(int direction) {
//         var nextState = GetNextState(direction);
//         if (nextState.Equals(_trackStyleSubType.Unknown) && States.Count > 1) {
//             nextState = GetNextStateFrom(nextState, direction);
//         }
//         return nextState;
//     }
//
//     private _trackStyleSubType GetNextState(int direction) {
//         return GetNextStateFrom(_state, direction);
//     }
//
//     private _trackStyleSubType GetNextStateFrom(_trackStyleSubType currentState, int direction) {
//         var currentIndex = States.FindIndex(item => item == currentState);
//         if (currentIndex == -1) return _trackStyleSubType.Unknown;
//         var nextIndex = (currentIndex + direction + States.Count) % States.Count;
//         return States[nextIndex];
//     }
//
//     public void First() {
//         _state = States.Count > 0 ? States[0] : _trackStyleSubType.Unknown;
//     }
//
//     /// <summary>
//     /// Set the State but ensure it is one of the valid states 
//     /// </summary>
//     public _trackStyleSubType State {
//         get => _state;
//         set => _state = States.Contains(value) ? value : _trackStyleSubType.Unknown;
//     }
// }