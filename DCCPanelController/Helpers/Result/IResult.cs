namespace DCCPanelController.Helpers.Result;

public interface IResult {
    bool       Success   { get; }
    bool       Failed    { get; }
    string     Message   { get; }
    Exception? Exception { get; }
}