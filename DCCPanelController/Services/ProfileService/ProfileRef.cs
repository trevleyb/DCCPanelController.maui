namespace DCCPanelController.Services.ProfileService;

[Serializable]
public readonly record struct ProfileRef(string ProfileName, string FileName, bool IsDefault);