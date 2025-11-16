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

public abstract partial class AccessoryViewModel<T> : ConnectionViewModel where T : Accessory
{
    protected readonly ProfileService _profileService;

    // Sorting state shared by all tables
    private string _sortKey;
    private bool _isAscending;

    [field: AllowNull, MaybeNull]
    public ObservableCollection<T> Items {
        get;
        protected set => SetProperty(ref field, value);
    }

    // Common “supported” flags
    [NotifyPropertyChangedFor(nameof(IsNotSupported))]
    [ObservableProperty] private bool _isSupported;
    public bool IsNotSupported => !IsSupported;

    protected AccessoryViewModel(ProfileService profileService, ConnectionService connectionService)
        : base(profileService, connectionService)
    {
        _profileService = profileService;
        _isAscending = true;
        _sortKey = DefaultSortKey;

        _profileService.ActiveProfileChanged += (_, __) => RebindToActiveProfile();
        RebindToActiveProfile(); // initial bind
    }

    // --- Hooks each derived VM must provide ---
    protected abstract string DefaultSortKey { get; }
    protected abstract ObservableCollection<T> ResolveCollection(Profile profile);
    /// <summary>Map column keys -> selector used for sorting. Keys should match your ColumnLabelX bindings.</summary>
    protected abstract IReadOnlyDictionary<string, Func<T, IComparable>> Sorters { get; }
    /// <summary>Update ColumnLabel* properties in derived VMs.</summary>
    protected abstract void UpdateColumnLabels();
    /// <summary>Raise OnPropertyChanged for the derived alias property (e.g., "Turnouts") when Items is rebound.</summary>
    protected virtual void OnItemsRebound() { }
    // ------------------------------------------------

    private void RebindToActiveProfile()
    {
        var profile = _profileService.ActiveProfile ?? throw new ArgumentNullException(nameof(_profileService), "Active profile is not defined.");
        Items = ResolveCollection(profile);
        _sortKey = DefaultSortKey;
        _isAscending = true;
        UpdateColumnLabels();
        OnItemsRebound();
    }

    protected string LabelWithArrow(string key, string label)
        => label + (string.Equals(_sortKey, key, StringComparison.Ordinal) ? _isAscending.GetSortDirection() : "");

    [RelayCommand]
    public Task SortByColumnAsync(string key)
    {
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
    public static class ObservableCollectionSortExtensions
    {
        public static void ApplyOrder(ObservableCollection<T> source, IEnumerable<T> ordered)
        {
            var target = ordered.ToList();
            for (int targetIndex = 0; targetIndex < target.Count; targetIndex++)
            {
                var item = target[targetIndex];
                var currentIndex = source.IndexOf(item);
                if (currentIndex >= 0 && currentIndex != targetIndex)
                    source.Move(currentIndex, targetIndex);
            }
        }
    }
}
