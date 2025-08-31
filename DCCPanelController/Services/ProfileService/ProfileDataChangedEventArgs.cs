namespace DCCPanelController.Services.ProfileService;

public class ProfileDataChangedEventArgs : EventArgs {
    public ProfileDataChangedEventArgs(ProfileDataChangeType changeType, object? changedObject = null) {
        ChangeType = changeType;
        ChangedObject = changedObject;
    }

    public ProfileDataChangeType ChangeType { get; }
    public object? ChangedObject { get; }
}