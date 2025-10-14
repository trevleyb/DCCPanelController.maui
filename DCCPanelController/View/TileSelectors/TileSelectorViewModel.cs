using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Models.DataModel;
using DCCPanelController.Models.ViewModel.Interfaces;
using DCCPanelController.View.Base;

namespace DCCPanelController.View.TileSelectors;

public abstract partial class TileSelectorViewModel : BaseViewModel {
    [ObservableProperty] private Dictionary<string, ObservableCollection<ITile>> _byCategory = [];
    [ObservableProperty] private ObservableCollection<string>                    _categories = [];
    
    public Panel? Panel {
        // We add the Selector Panel to the Parent Panel collection so that when 
        // we reference things like Color, it comes from the Globally set values
        set {
            if (value is null) return;
            
            MainThread.BeginInvokeOnMainThread(() => {
                // For some reason, caching sometimes doesn't work due to UI timing issues
                var palette = PaletteCache.GetDefaultPalette(); // e.GetOrBuild(value);

                Categories = palette.Categories;
                ByCategory = palette.ByCategory;
                
                AfterBuildAllTiles();

                OnPropertyChanged(nameof(Categories));
                OnPropertyChanged(nameof(ByCategory));
            });
        }
    }

    public void ForceReDraw() {
        foreach (var tile in ByCategory.SelectMany(category => category.Value)) {
            tile.ForceRedraw();
        }
    }

    protected abstract void AfterBuildAllTiles();
}