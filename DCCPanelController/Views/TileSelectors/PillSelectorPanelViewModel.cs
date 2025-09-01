using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Models.ViewModel.Interfaces;
using DCCPanelController.Resources.Styles;
using Syncfusion.Maui.Toolkit.SegmentedControl;

namespace DCCPanelController.Views.TileSelectors;

public partial class PillSelectorPanelViewModel : TileSelectorViewModel {
    [ObservableProperty]
    private string _selectedCategory = string.Empty;

    public PillSelectorPanelViewModel() {
        SfCategories.Clear();
        SfCategories.Add(new SfSegmentItem { Text = "Loading" });
    }

    public ObservableCollection<SfSegmentItem> SfCategories { get; } = [];

    public ObservableCollection<ITile> TilesForSelectedCategory {
        get {
            if (string.IsNullOrEmpty(SelectedCategory) || !ByCategory.ContainsKey(SelectedCategory)) return [];
            return ByCategory[SelectedCategory];
        }
    }

    partial void OnSelectedCategoryChanged(string value) {
        OnPropertyChanged(nameof(TilesForSelectedCategory));
    }

    protected override void AfterBuildAllTiles() {
        SfCategories.Clear();
        foreach (var category in Categories) {
            SfCategories.Add(new SfSegmentItem {
                Text = category,
                SelectedSegmentBackground = new SolidColorBrush(StyleHelper.FromStyle("Primary"))
            });
        }
    }
}