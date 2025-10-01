using System.IO.Compression;
using System.Text;
using DCCPanelController.Models.DataModel;
using DCCPanelController.Models.DataModel.Repository;
using DCCPanelController.Resources.Strings;

namespace DCCPanelController.Services.ProfileService;

public class ProfileService {
    private readonly SemaphoreSlim   _gate = new(1, 1);
    private          ProfileCatalog? _catalog;

    public Profile? ActiveProfile { get; private set; }

    #region Ctor
    public async Task InitializeAsync() {
        _catalog = ProfileCatalog.Load();
        if (_catalog is null) throw new ApplicationException("Failed to load profile catalog.");

        var activeFile = _catalog.ActiveFileName;
        ActiveProfile = string.IsNullOrWhiteSpace(activeFile)
            ? await CreateAsync("Default")
            : await LoadAsync(activeFile);

        ActiveProfile ??= await CreateAsync("Default");
        ActiveProfileChanged?.Invoke(this, new ProfileChangedEventArgs(null, ActiveProfile));
    }
    #endregion

    #region Events
    public event EventHandler<ProfileChangedEventArgs>? ActiveProfileChanged;
    public event EventHandler<ProfileDataChangedEventArgs>? ActiveProfileDataChanged;
    private void RaiseActiveProfileChanged(Profile? oldP, Profile? newP) => ActiveProfileChanged?.Invoke(this, new ProfileChangedEventArgs(oldP, newP));
    private void RaiseDataChanged(ProfileDataChangeType t, Profile profile, object? obj = null) => ActiveProfileDataChanged?.Invoke(this, new ProfileDataChangedEventArgs(t, profile, obj));
    #endregion

    #region Queries
    public IReadOnlyList<string> GetProfileNames() => _catalog?.Profiles.Select(p => p.ProfileName).ToList() ?? [];
    public IReadOnlyList<string> GetProfileFileNames() => _catalog?.Profiles.Select(p => p.FileName).ToList() ?? [];

    public IReadOnlyList<string> GetProfileNamesWithDefault() {
        ArgumentNullException.ThrowIfNull(_catalog);
        var active = ActiveProfile?.Filename;
        return _catalog.Profiles.Select(p => {
                            var name = p.ProfileName;
                            if (p.IsDefault) name += " (default)";
                            if (!string.IsNullOrWhiteSpace(active) && p.FileName == active) name += " (active)";
                            return name;
                        })
                       .ToList();
    }

    public int NumberOfProfiles => _catalog?.Profiles.Count ?? 0;

    public void MarkAsDefault(Profile profile) {
        ArgumentNullException.ThrowIfNull(profile);
        ArgumentNullException.ThrowIfNull(_catalog);
        _catalog.SetDefault(profile.Filename);
    }

    public bool IsDefault(Profile profile) {
        ArgumentNullException.ThrowIfNull(profile);
        ArgumentNullException.ThrowIfNull(_catalog);
        return _catalog.IsDefault(profile.Filename);
    }
    #endregion

    #region Switch Profile Helpers
    public async Task SwitchProfileByIndexAsync(int index) {
        var items = GetProfileFileNames();
        if (items.Count > 0 && index >= 0 && index < items.Count) {
            await SwitchProfileByFilenameAsync(items[index]);
        }
    }

    public async Task SwitchProfileByFilenameAsync(string fileName) {
        var profile = await LoadAsync(fileName);
        if (profile is null) throw new ApplicationException($"Failed to load profile: {fileName}");
        ActiveProfile = profile;
    }
    #endregion

    #region Upload / Download (JSON & ZIP)
    public string DownloadProfile(Profile? profile = null) {
        var p = profile ?? ActiveProfile ?? throw new ApplicationException("No active profile to download.");
        return JsonRepository.DownloadProfile(p);
    }

    public async Task<string?> DownloadProfileAsync(Profile? profile = null) {
        var p = profile ?? ActiveProfile ?? throw new ApplicationException("No active profile to download.");
        return JsonRepository.DownloadProfile(p);
    }

    public byte[] DownloadProfileZip(Profile? profile = null) {
        var p = profile ?? ActiveProfile ?? throw new ApplicationException("No active profile to download.");
        var json = JsonRepository.DownloadProfile(p);
        var entryName = string.IsNullOrWhiteSpace(p.Filename) ? "Profile.json" : p.Filename;
        using var mem = new MemoryStream();
        using (var zip = new ZipArchive(mem, ZipArchiveMode.Create, true)) {
            var entry = zip.CreateEntry(entryName, CompressionLevel.Optimal);
            using var entryStream = entry.Open();
            using var writer = new StreamWriter(entryStream, Encoding.UTF8, leaveOpen: true);
            writer.Write(json);
        }
        return mem.ToArray();
    }

    public async Task<byte[]> DownloadProfileZipAsync(Profile? profile = null) {
        var p = profile ?? ActiveProfile ?? throw new ApplicationException("No active profile to download.");
        var json = JsonRepository.DownloadProfile(p);
        var entryName = string.IsNullOrWhiteSpace(p.Filename) ? "Profile.json" : p.Filename;
        using var mem = new MemoryStream();
        using (var zip = new ZipArchive(mem, ZipArchiveMode.Create, true)) {
            var entry = zip.CreateEntry(entryName, CompressionLevel.Optimal);
            await using var entryStream = entry.Open();
            await using var writer = new StreamWriter(entryStream, Encoding.UTF8, leaveOpen: true);
            await writer.WriteAsync(json);
        }
        return mem.ToArray();
    }

    public void ExportProfileToFile(string filePath, Profile? profile = null) => File.WriteAllText(filePath, DownloadProfile(profile));
    public void ExportProfileZipToFile(string filePath, Profile? profile = null) => File.WriteAllBytes(filePath, DownloadProfileZip(profile));

    public Task<Profile?> UploadProfileAsync(byte[] content, bool setActive = true, string? displayName = null) {
        if (content == null || content.Length == 0) throw new ArgumentException("Empty content.", nameof(content));
        var json = IsZip(content) ? ExtractJsonFromZipBytes(content) : Encoding.UTF8.GetString(content);
        return UploadProfileAsync(json, setActive, displayName);
    }

    public async Task<Profile?> UploadProfileAsync(string json, bool setActive = true, string? displayName = null) {
        ArgumentNullException.ThrowIfNull(_catalog);
        await _gate.WaitAsync();
        try {
            var uploaded = await JsonRepository.UploadProfile(json);
            if (uploaded is null) return null;

            if (!string.IsNullOrWhiteSpace(displayName)) {
                uploaded.ProfileName = displayName;
            }

            if (string.IsNullOrWhiteSpace(uploaded.ProfileName)) {
                uploaded.ProfileName = _catalog.GetUniqueProfileName("Profile");
            }

            if (string.IsNullOrWhiteSpace(uploaded.Filename)) {
                uploaded.Filename = $"DCCPanelController.{Guid.NewGuid()}.json";
            }

            await JsonRepository.SaveAsync(uploaded);
            _catalog.Upsert(uploaded);

            if (setActive) {
                SetActive(uploaded, true);
            } else {
                RaiseDataChanged(ProfileDataChangeType.ProfileSaved, uploaded);
            }
            return uploaded;
        } finally {
            _gate.Release();
        }
    }

    public async Task<Profile?> UploadProfileFromFileAsync(string filePath, bool setActive = true, string? displayName = null) {
        var bytes = await File.ReadAllBytesAsync(filePath);
        return await UploadProfileAsync(bytes, setActive, displayName);
    }

    private static bool IsZip(byte[] data) => data.Length > 3 && data[0] == 0x50 && data[1] == 0x4B && (data[2] == 0x03 || data[2] == 0x05 || data[2] == 0x07);

    private static string ExtractJsonFromZipBytes(byte[] zipBytes) {
        using var mem = new MemoryStream(zipBytes);
        using var zip = new ZipArchive(mem, ZipArchiveMode.Read);
        var jsonEntry = zip.Entries.FirstOrDefault(e => e.FullName.EndsWith(".json", StringComparison.OrdinalIgnoreCase));
        if (jsonEntry == null) throw new ApplicationException("ZIP does not contain a .json profile.");
        using var stream = jsonEntry.Open();
        using var reader = new StreamReader(stream, Encoding.UTF8);
        return reader.ReadToEnd();
    }
    #endregion

    #region CRUD
    public void SetActive(Profile profile, bool markAsDefault = false) {
        ArgumentNullException.ThrowIfNull(_catalog);
        var old = ActiveProfile;
        ActiveProfile = profile;
        if (markAsDefault) _catalog.SetDefault(profile.Filename);
        RaiseActiveProfileChanged(old, profile);
    }

    public async Task<Profile> CreateFromTemplateAsync(string templateName) {
        var profile = templateName switch {
            "ChelseaAndBalmain" => await LoadTemplateAsync("Templates/ChelseaAndBalmain.json"),
            "PiedmontSouthern"  => await LoadTemplateAsync("Templates/PiedmontSouthern.json"),
            _                   => await CreateAsync(templateName, false)
        };
        return profile ?? await CreateAsync(templateName, false);
    }
    
    public async Task<Profile?> LoadTemplateAsync(string filename) {
        await using var stream = await FileSystem.OpenAppPackageFileAsync(filename);
        using var reader = new StreamReader(stream);
        var json = await reader.ReadToEndAsync();
        return await UploadProfileAsync(json);
    }
    
    public async Task<Profile> CreateAsync(string? profileName = null, bool setActive = true) {
        ArgumentNullException.ThrowIfNull(_catalog);
        await _gate.WaitAsync();
        try {
            profileName ??= _catalog.GetUniqueProfileName("Profile");
            var fileName = $"DCCPanelController.{Guid.NewGuid()}";
            var profile = new Profile(profileName, fileName);
            await JsonRepository.SaveAsync(profile);
            _catalog.Upsert(profile);

            if (setActive) SetActive(profile, true);
            return profile;
        } finally {
            _gate.Release();
        }
    }

    public async Task DeleteAsync(Profile profile) {
        ArgumentNullException.ThrowIfNull(_catalog);
        await _gate.WaitAsync();
        try {
            JsonRepository.Delete(profile);
            _catalog.Delete(profile);

            if (ActiveProfile?.Filename == profile.Filename) {
                if (_catalog.Profiles.Count > 0) {
                    var next = _catalog.Profiles[0];
                    var loaded = await JsonRepository.LoadAsync(next.FileName);
                    if (loaded != null) SetActive(loaded, true);
                } else {
                    var created = await CreateAsync("Default");
                    SetActive(created, true);
                }
            }
        } finally {
            _gate.Release();
        }
    }

    public async Task SaveAsync(Profile profile) {
        ArgumentNullException.ThrowIfNull(_catalog);
        await JsonRepository.SaveAsync(profile);
        _catalog.Upsert(profile);
        RaiseDataChanged(ProfileDataChangeType.ProfileSaved, profile);
    }

    public async Task SaveAsync() {
        ArgumentNullException.ThrowIfNull(_catalog);
        if (ActiveProfile is null) return;
        await SaveAsync(ActiveProfile);
    }

    public async Task<Profile> LoadAsync(string fileName, bool markAsDefault = false) {
        if (string.IsNullOrWhiteSpace(fileName)) throw new ArgumentException("Filename is required.", nameof(fileName));
        var profile = await JsonRepository.LoadAsync(fileName);
        if (profile is null) {
            _catalog?.Delete(fileName);
            profile = await CreateAsync(fileName, false);
            if (profile is null) throw new ApplicationException($"Failed to load profile: {fileName}");
        }
        SetActive(profile, markAsDefault);
        return profile;
    }

    public Task<Profile> LoadAsync(ProfileRef item, bool markAsDefault = false) => LoadAsync(item.FileName, markAsDefault);
    #endregion
}