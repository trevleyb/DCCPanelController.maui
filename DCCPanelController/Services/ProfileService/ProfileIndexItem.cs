namespace DCCPanelController.Services.ProfileService;

[Serializable]
public class ProfileIndexItem {
    public ProfileIndexItem(string profileName, string fileName, bool isDefault) {
        ProfileName = profileName;
        FileName = fileName;
        IsDefault = isDefault;
    }

    public string ProfileName { get; set; }
    public string FileName { get; set; }
    public bool IsDefault { get; set; }
}