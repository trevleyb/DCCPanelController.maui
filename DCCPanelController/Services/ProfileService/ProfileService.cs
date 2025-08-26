using System.IO.Compression;
using System.Text;
using DCCPanelController.Models.DataModel;
using DCCPanelController.Models.DataModel.Repository;

namespace DCCPanelController.Services.ProfileService;

public class ProfileService {
    protected ProfileIndexFile ProfilesIndex = new ProfileIndexFile();
    private readonly SemaphoreSlim _gate = new(1, 1);
    public Profile? ActiveProfile { get; set; }

    public event EventHandler<ProfileChangedEventArgs>? ActiveProfileChanged;
    public event EventHandler<ProfileDataChangedEventArgs>? ActiveProfileDataChanged;

    public ProfileService() {
        LoadProfiles();

        // Fire initial event once an active profile is established
        if (ActiveProfile != null)
            ActiveProfileChanged?.Invoke(this, new ProfileChangedEventArgs(null, ActiveProfile));
    }

    // =====================
    // Access Helpers
    // =====================
    public List<string> GetProfileNames() => ProfilesIndex?.Profiles.Select(x => x.ProfileName).ToList() ?? new List<string>();
    public List<string> GetProfileFileNames() => ProfilesIndex?.Profiles.Select(x => x.FileName).ToList() ?? new List<string>();
    public List<string> GetProfileNamesWithDefault() {
        var profileList = new List<string>();
        foreach (var profile in ProfilesIndex.Profiles) {
            var profileName = profile.ProfileName;
            if (profile.IsDefault) profileName += " (default)";
            if (profile.FileName == ActiveProfile?.Filename)  profileName += " (active)";
            profileList.Add(profileName);
        }
        return profileList;
    }

    public void MarkAsDefault(Profile profile) => ProfilesIndex.SetAsDefault(profile);
    public void MarkAsDefault() => ProfilesIndex.SetAsDefault(ActiveProfile ?? throw new ArgumentNullException(nameof(ActiveProfile), "Active profile is not defined."));
    public bool IsDefault(Profile profile) => ProfilesIndex.IsDefault(profile);
    public bool IsDefault() => ProfilesIndex.IsDefault(ActiveProfile ?? throw new ArgumentNullException(nameof(ActiveProfile), "Active profile is not defined."));

    // =====================
    // Event helpers
    // =====================
    private void RaiseActiveProfileChanged(Profile? oldP, Profile? newP) => ActiveProfileChanged?.Invoke(this, new ProfileChangedEventArgs(oldP, newP));
    private void RaiseDataChanged(ProfileDataChangeType t, object? obj = null) => ActiveProfileDataChanged?.Invoke(this, new ProfileDataChangedEventArgs(t, obj));

    // =====================
    // SYNC operations (UI threads that cannot await)
    // =====================
    public void SetActive(Profile profile, bool markAsDefault = false) {
        var old = ActiveProfile;
        ActiveProfile = profile;
        if (markAsDefault) ProfilesIndex.SetAsDefault(profile); // persists default selection
        RaiseActiveProfileChanged(old, profile);
    }

    public Profile Create(string? profileName = null, bool setActive = true) {
        _gate.Wait();
        try {
            profileName ??= ProfilesIndex.GetUniqueProfileName("Profile");
            var fileName = $"DCCPanelController.{Guid.NewGuid()}"; // keep current convention

            var profile = new Profile(profileName, fileName);
            JsonRepository.Save(profile);
            ProfilesIndex.AddOrUpdate(profile);

            if (setActive) SetActive(profile, markAsDefault: true);
            return profile;
        } finally {
            _gate.Release();
        }
    }

    public void Delete(Profile profile) {
        _gate.Wait();
        try {
            JsonRepository.Delete(profile);
            ProfilesIndex.Delete(profile);

            if (ActiveProfile?.Filename == profile.Filename) {
                if (ProfilesIndex.Profiles.Count > 0) {
                    var next = ProfilesIndex.Profiles[0];
                    var loaded = JsonRepository.Load(next.FileName);
                    if (loaded != null)
                        SetActive(loaded, markAsDefault: true);
                } else {
                    // Ensure there is always at least one active profile
                    // SetActive already raised the event
                    var _ = Create("Default", setActive: true);
                }
            }
        } finally {
            _gate.Release();
        }
    }

    public void Save() {
        if (ActiveProfile is null) return;
        JsonRepository.Save(ActiveProfile);
        ProfilesIndex.AddOrUpdate(ActiveProfile);
        RaiseDataChanged(ProfileDataChangeType.ProfileSaved, ActiveProfile);
    }

    // =====================
    // Sharing helpers (Upload / Download)
    // =====================
    /// <summary>
    /// Return the JSON for the provided profile (or the active profile if null) so it can be shared.
    /// </summary>
    public string DownloadProfile(Profile? profile = null) {
        var p = profile ?? ActiveProfile ?? throw new ApplicationException("No active profile to download.");
        return JsonRepository.DownloadProfile(p);
    }

    /// <summary>
    /// Write the provided (or active) profile JSON to a file path.
    /// </summary>
    public void ExportProfileToFile(string filePath, Profile? profile = null) {
        var json = DownloadProfile(profile);
        File.WriteAllText(filePath, json);
    }

    /// <summary>
    /// Load a profile from a JSON string, persist it, index it, and optionally set it active.
    /// Returns the new Profile instance.
    /// </summary>
    public Profile? UploadProfile(string json, bool setActive = true, string? displayName = null) {
        Profile? uploaded;
        bool raiseActiveChanged = false;
        _gate.Wait();
        try {
            uploaded = JsonRepository.UploadProfile(json).GetAwaiter().GetResult();
            if (uploaded is null) return null;

            // Optional display name, or ensure a unique one
            if (!string.IsNullOrWhiteSpace(displayName))
                uploaded.ProfileName = displayName;
            if (string.IsNullOrWhiteSpace(uploaded.ProfileName))
                uploaded.ProfileName = ProfilesIndex.GetUniqueProfileName("Profile");

            // Ensure a concrete filename
            if (string.IsNullOrWhiteSpace(uploaded.Filename))
                uploaded.Filename = $"DCCPanelController.{Guid.NewGuid()}.json";

            JsonRepository.Save(uploaded);
            ProfilesIndex.AddOrUpdate(uploaded);

            if (setActive) {
                ActiveProfile = uploaded;
                ProfilesIndex.SetAsDefault(uploaded);
                raiseActiveChanged = true;
            }
        } finally {
            _gate.Release();
        }

        // Raise outside the lock to avoid deadlocks/reentrancy
        RaiseDataChanged(ProfileDataChangeType.ProfileSaved, uploaded);
        if (raiseActiveChanged) RaiseActiveProfileChanged(null, uploaded);
        return uploaded;
    }

    /// <summary>
    /// Async variant of UploadProfile.
    /// </summary>
    public async Task<Profile?> UploadProfileAsync(string json, bool setActive = true, string? displayName = null) {
        Profile? uploaded;
        var raiseActiveChanged = false;
        await _gate.WaitAsync();
        try {
            uploaded = await JsonRepository.UploadProfile(json);
            if (uploaded is null) return null;

            if (!string.IsNullOrWhiteSpace(displayName))
                uploaded.ProfileName = displayName;
            if (string.IsNullOrWhiteSpace(uploaded.ProfileName))
                uploaded.ProfileName = ProfilesIndex.GetUniqueProfileName("Profile");
            if (string.IsNullOrWhiteSpace(uploaded.Filename))
                uploaded.Filename = $"DCCPanelController.{Guid.NewGuid()}.json";

            await JsonRepository.SaveAsync(uploaded);
            ProfilesIndex.AddOrUpdate(uploaded);

            if (setActive) {
                ActiveProfile = uploaded;
                ProfilesIndex.SetAsDefault(uploaded);
                raiseActiveChanged = true;
            }
        } finally {
            _gate.Release();
        }

        RaiseDataChanged(ProfileDataChangeType.ProfileSaved, uploaded);
        if (raiseActiveChanged) RaiseActiveProfileChanged(null, uploaded);
        return uploaded;
    }

    /// <summary>
    /// Import from a JSON file path (sync helper)
    /// </summary>
    public Profile? UploadProfileFromFile(string filePath, bool setActive = true, string? displayName = null) {
        var bytes = File.ReadAllBytes(filePath);
        if (IsZip(bytes))
            return UploadProfile(bytes, setActive, displayName);
        var json = Encoding.UTF8.GetString(bytes);
        return UploadProfile(json, setActive, displayName);
    }

    /// <summary>
    /// Import from a JSON file path (async helper)
    /// </summary>
    public async Task<Profile?> UploadProfileFromFileAsync(string filePath, bool setActive = true, string? displayName = null) {
        var bytes = await File.ReadAllBytesAsync(filePath);
        if (IsZip(bytes))
            return await UploadProfileAsync(bytes, setActive, displayName);
        var json = Encoding.UTF8.GetString(bytes);
        return await UploadProfileAsync(json, setActive, displayName);
    }

    /// <summary>
    /// Upload profile from raw bytes. Detects ZIP vs JSON automatically.
    /// </summary>
    public Profile? UploadProfile(byte[] content, bool setActive = true, string? displayName = null) {
        if (content == null || content.Length == 0) throw new ArgumentException("Empty content.", nameof(content));
        string json = IsZip(content) ? ExtractJsonFromZipBytes(content) : Encoding.UTF8.GetString(content);
        return UploadProfile(json, setActive, displayName);
    }

    /// <summary>
    /// Async variant for bytes content. Detects ZIP vs JSON automatically.
    /// </summary>
    public async Task<Profile?> UploadProfileAsync(byte[] content, bool setActive = true, string? displayName = null) {
        if (content == null || content.Length == 0) throw new ArgumentException("Empty content.", nameof(content));
        var json = IsZip(content) ? ExtractJsonFromZipBytes(content) : Encoding.UTF8.GetString(content);
        return await UploadProfileAsync(json, setActive, displayName);
    }

    /// <summary>
    /// Return a ZIP (byte[]) containing the profile JSON as a single entry.
    /// </summary>
    public byte[] DownloadProfileZip(Profile? profile = null) {
        var p = profile ?? ActiveProfile ?? throw new ApplicationException("No active profile to download.");
        var json = JsonRepository.DownloadProfile(p);
        var entryName = string.IsNullOrWhiteSpace(p.Filename) ? "Profile.json" : p.Filename;
        using var mem = new MemoryStream();
        using (var zip = new ZipArchive(mem, ZipArchiveMode.Create, leaveOpen: true)) {
            var entry = zip.CreateEntry(entryName, CompressionLevel.Optimal);
            using var entryStream = entry.Open();
            using var writer = new StreamWriter(entryStream, Encoding.UTF8, leaveOpen: true);
            writer.Write(json);
        }
        return mem.ToArray();
    }

    /// <summary>
    /// Return a ZIP (byte[]) containing the profile JSON as a single entry.
    /// </summary>
    public async Task<byte[]> DownloadProfileZipAsync(Profile? profile = null) {
        var p = profile ?? ActiveProfile ?? throw new ApplicationException("No active profile to download.");
        var json = JsonRepository.DownloadProfile(p);
        var entryName = string.IsNullOrWhiteSpace(p.Filename) ? "Profile.json" : p.Filename;
        using var mem = new MemoryStream();
        using (var zip = new ZipArchive(mem, ZipArchiveMode.Create, leaveOpen: true)) {
            var entry = zip.CreateEntry(entryName, CompressionLevel.Optimal);
            await using var entryStream = entry.Open();
            await using var writer = new StreamWriter(entryStream, Encoding.UTF8, leaveOpen: true);
            await writer.WriteAsync(json);
        }
        return mem.ToArray();
    }

    /// <summary>
    /// Write a ZIP file containing the profile JSON.
    /// </summary>
    public void ExportProfileZipToFile(string filePath, Profile? profile = null) {
        var bytes = DownloadProfileZip(profile);
        File.WriteAllBytes(filePath, bytes);
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

    // =====================
    // ASYNC operations (use when you can await)
    // =====================

    public async Task<Profile> CreateAsync(string? profileName = null, bool setActive = true) {
        await _gate.WaitAsync();
        try {
            profileName ??= ProfilesIndex.GetUniqueProfileName("Profile");
            var fileName = $"DCCPanelController.{Guid.NewGuid()}";

            var profile = new Profile(profileName, fileName);
            await JsonRepository.SaveAsync(profile);
            ProfilesIndex.AddOrUpdate(profile);

            if (setActive) SetActive(profile, markAsDefault: true);

            return profile;
        } finally {
            _gate.Release();
        }
    }

    public async Task DeleteAsync(Profile profile) {
        await _gate.WaitAsync();
        try {
            JsonRepository.Delete(profile);
            ProfilesIndex.Delete(profile);

            if (ActiveProfile?.Filename == profile.Filename) {
                if (ProfilesIndex.Profiles.Count > 0) {
                    var next = ProfilesIndex.Profiles[0];
                    var loaded = await JsonRepository.LoadAsync(next.FileName);
                    if (loaded != null) SetActive(loaded, markAsDefault: true);
                } else {
                    // SetActive already raised the event
                    var _ = await CreateAsync("Default", setActive: true);
                }
            }
        } finally {
            _gate.Release();
        }
    }

    public async Task SaveAsync() {
        if (ActiveProfile is null) return;
        await JsonRepository.SaveAsync(ActiveProfile);
        ProfilesIndex.AddOrUpdate(ActiveProfile);
        RaiseDataChanged(ProfileDataChangeType.ProfileSaved, ActiveProfile);
    }

    // Load (sync) by filename or index item and make it active
    public Profile Load(string fileName, bool markAsDefault = false) {
        if (string.IsNullOrWhiteSpace(fileName)) throw new ArgumentException("Filename is required.", nameof(fileName));
        var profile = JsonRepository.Load(fileName) ?? throw new ApplicationException($"Profile not found: {fileName}");
        SetActive(profile, markAsDefault);
        return profile;
    }

    public Profile Load(ProfileIndexItem item, bool markAsDefault = false) {
        ArgumentNullException.ThrowIfNull(item);
        return Load(item.FileName, markAsDefault);
    }

    public async Task<Profile> LoadAsync(string fileName, bool markAsDefault = false) {
        if (string.IsNullOrWhiteSpace(fileName)) throw new ArgumentException("Filename is required.", nameof(fileName));
        var profile = await JsonRepository.LoadAsync(fileName) ?? throw new ApplicationException($"Profile not found: {fileName}");
        SetActive(profile, markAsDefault);
        return profile;
    }

    public async Task<Profile> LoadAsync(ProfileIndexItem item, bool markAsDefault = false) {
        ArgumentNullException.ThrowIfNull(item);
        return await LoadAsync(item.FileName, markAsDefault);
    }

    // =====================
    // Load Profiles Helpers
    // =====================
    private void LoadProfiles() {
        if (ActiveProfile != null) return;

        Console.WriteLine("Loading Profiles");

        // Load or create the index (ensures one default if none marked)
        ProfilesIndex = ProfileIndexFile.LoadMetaData();
        if (ProfilesIndex is null) throw new ApplicationException("Failed to load or create Profiles");

        // Ensure there is at least one profile
        if (ProfilesIndex.Profiles.Count == 0) {
            var created = Create("Default");
            ActiveProfile = created; // don't raise event here; ctor does it once after LoadProfiles()
            Console.WriteLine("Profiles Loaded");
            return;
        }

        // Resolve the active filename (LoadMetaData guarantees one default if items exist)
        var activeFileName = ProfilesIndex.ActiveProfileFileName;
        if (string.IsNullOrWhiteSpace(activeFileName)) {
            // Fallback: first profile in the index
            var first = ProfilesIndex.Profiles.First();
            var fallback = JsonRepository.Load(first.FileName);
            if (fallback is not null) {
                ActiveProfile = fallback; // no event here
                Console.WriteLine("Profiles Loaded: Fallback to first profile");
            } else {
                ActiveProfile = Create("Default"); // no event here
                Console.WriteLine("Profiles Loaded: Fallback to created profile");
            }
            return;
        }

        // Load the active profile from disk
        ActiveProfile = JsonRepository.Load(activeFileName); // no event here
        if (ActiveProfile is null) throw new ApplicationException("Failed to load or create Profiles");
        Console.WriteLine("Profiles Loaded");
    }
}