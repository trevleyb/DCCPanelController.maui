using DCCPanelController.Models.DataModel;

namespace DCCPanelController.Services.ProfileService;

public class ProfileChangedEventArgs : EventArgs {
    public ProfileChangedEventArgs(Profile? oldProfile, Profile? newProfile) {
        OldProfile = oldProfile;
        NewProfile = newProfile;
    }

    public Profile? OldProfile { get; }
    public Profile? NewProfile { get; }
}