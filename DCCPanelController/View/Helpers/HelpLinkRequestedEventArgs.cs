namespace DCCPanelController.View.Helpers;

public sealed class HelpLinkRequestedEventArgs(string topicId, string? anchor = null) : EventArgs {
    public string TopicId { get; } = topicId;
    public string? Anchor { get; } = anchor;
}