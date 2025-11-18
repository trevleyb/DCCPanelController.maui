using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DCCPanelController.Helpers; // for GetSortDirection()
using DCCPanelController.Models.DataModel;
using DCCPanelController.Models.DataModel.Accessories;
using DCCPanelController.Services;
using DCCPanelController.Services.ProfileService;

namespace DCCPanelController.View.Base;

public abstract partial class AccessoryViewModel<T> : ConnectionViewModel where T : Accessory {
    protected readonly ProfileService _profileService;

    private string _sortKey =  string.Empty;
    private bool   _isAscending = true;

    [field: AllowNull, MaybeNull]
    public ObservableCollection<T> Items {
        get;
        protected set => SetProperty(ref field, value);
    }

    [NotifyPropertyChangedFor(nameof(IsNotSupported))]
    [ObservableProperty] private bool _isSupported;

    public bool IsNotSupported => !IsSupported;

    [ObservableProperty] private bool _canAddNewAccessory;

    protected AccessoryViewModel(ProfileService profileService, ConnectionService connectionService)
        : base(profileService, connectionService) {
        _profileService = profileService;
        _profileService.ActiveProfileChanged += (_, __) => RebindToActiveProfile();
        RebindToActiveProfile();
    }

    // --- Hooks each derived VM must provide ---
    protected abstract string DefaultSortKey { get; }
    protected abstract void UpdateColumnLabels();
    protected virtual void OnItemsRebound() { }

    protected abstract ObservableCollection<T> ResolveCollection(Profile profile);
    protected abstract IReadOnlyDictionary<string, Func<T, IComparable>> Sorters { get; }

    // ------------------------------------------------

    private void RebindToActiveProfile() {
        var profile = _profileService.ActiveProfile ?? throw new ArgumentNullException(nameof(_profileService), "Active profile is not defined.");
        Items = ResolveCollection(profile);
        _sortKey = DefaultSortKey;
        _isAscending = true;
        CanAddNewAccessory = profile?.Settings?.ClientSettings?.SupportsManualEntries ?? true;
        UpdateColumnLabels();
        OnItemsRebound();
    }

    protected string LabelWithArrow(string key, string label) => label + (string.Equals(_sortKey, key, StringComparison.Ordinal) ? _isAscending.GetSortDirection() : "");

    [RelayCommand]
    public Task SortByColumnAsync(string key) {
        if (!Sorters.TryGetValue(key, out var selector))
            return Task.CompletedTask;

        // Toggle like your originals: show arrow for the *current* state, then flip
        var ordered = !_isAscending
            ? Items.OrderBy(selector).ToList()
            : Items.OrderByDescending(selector).ToList();

        ObservableCollectionSortExtensions.ApplyOrder(Items, ordered);

        _sortKey = key;
        _isAscending = !_isAscending;
        UpdateColumnLabels();
        return Task.CompletedTask;
    }

    // Shared in-place reorder (generic for any DccTable)
    public static class ObservableCollectionSortExtensions {
        public static void ApplyOrder(ObservableCollection<T> source, IEnumerable<T> ordered) {
            var target = ordered.ToList();
            for (int targetIndex = 0; targetIndex < target.Count; targetIndex++) {
                var item = target[targetIndex];
                var currentIndex = source.IndexOf(item);
                if (currentIndex >= 0 && currentIndex != targetIndex)
                    source.Move(currentIndex, targetIndex);
            }
        }
    }
}