using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DCCPanelController.Models.DataModel;
using DCCPanelController.Models.DataModel.Entities;
using DCCPanelController.Models.ViewModel.Tiles;
using DCCPanelController.Helpers;
using DCCPanelController.Models.ViewModel.Helpers;
using DCCPanelController.Models.ViewModel.Interfaces;
using DCCPanelController.Resources.Styles;
using DCCPanelController.View.Base;
using DCCPanelController.View.Helpers;
using SkiaSharp;
using Syncfusion.Maui.Toolkit.SegmentedControl;

namespace DCCPanelController.View.TileSelectors;

public partial class PillSelectorPanelViewModel : TileSelectorViewModel {

    [ObservableProperty] 
    private string _selectedCategory = string.Empty;
    public ObservableCollection<SfSegmentItem> SfCategories { get; } = [];

    public PillSelectorPanelViewModel() : base() {
        SfCategories.Clear();
        SfCategories.Add(new SfSegmentItem { Text = "Loading" });
    }

    partial void OnSelectedCategoryChanged(string value) => OnPropertyChanged(nameof(TilesForSelectedCategory));
    public ObservableCollection<ITile> TilesForSelectedCategory {
        get {
            if (string.IsNullOrEmpty(SelectedCategory) || !ByCategory.ContainsKey(SelectedCategory)) return [];
            return ByCategory[SelectedCategory];
        }
    }
    
    protected override void AfterBuildAllTiles() {
        SfCategories.Clear();
        foreach (var category in Categories) {
            SfCategories.Add(new SfSegmentItem {
                Text = category,
                SelectedSegmentBackground = new SolidColorBrush(StyleHelper.FromStyle("Primary")),
            });
        }
    }
}