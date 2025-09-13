using DCCPanelController.Models.DataModel;

namespace DCCPanelController.Services.ProfileService;

public class ProfileDataChangedEventArgs : EventArgs {
    public ProfileDataChangedEventArgs(ProfileDataChangeType changeType, Profile profile, object? changedObject = null) {
        ChangeType = changeType;
        ChangedObject = changedObject;
        Profile = profile;
    }

    public ProfileDataChangeType ChangeType { get; }
    public object? ChangedObject { get; }
    public Profile Profile { get; }
}