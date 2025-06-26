namespace DCCPanelController.Services;

public class ProfileDataChangedEventArgs : EventArgs {
    public ProfileDataChangeType ChangeType { get; }
    public object? ChangedObject { get; }

    public ProfileDataChangedEventArgs(ProfileDataChangeType changeType, object? changedObject = null) {
        ChangeType = changeType;
        ChangedObject = changedObject;
    }
}
