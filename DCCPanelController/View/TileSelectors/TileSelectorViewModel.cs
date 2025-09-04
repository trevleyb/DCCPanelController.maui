using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Helpers;
using DCCPanelController.Models.DataModel;
using DCCPanelController.Models.ViewModel.Interfaces;
using DCCPanelController.View.Base;

namespace DCCPanelController.View.TileSelectors;

public abstract partial class TileSelectorViewModel : BaseViewModel {
    [ObservableProperty] private Dictionary<string, ObservableCollection<ITile>> _byCategory = [];
    [ObservableProperty] private ObservableCollection<string> _categories = [];

    public Panel? Panel {
        // We add the Selector Panel to the Parent Panel collection so that when 
        // we reference things like Color, it comes from the Globally set values
        set {
            if (value is null) {
                Categories = new ObservableCollection<string>();
                ByCategory = new Dictionary<string, ObservableCollection<ITile>>();
                return;
            }

            MainThread.BeginInvokeOnMainThread(() => {
                // For some reason, caching sometimes doesn't work due to UI timing issues
                var palette = TileSelectorPaletteCache.BuildTilesForPanel(value); // e.GetOrBuild(value);
                if (palette is null) throw new InvalidOperationException("Unable to build palette");

                // Create a new dictionary and observable collections to avoid sharing the cached instances
                // -----------------------------------------------------------------------------------------
                Categories.Clear();
                foreach (var c in palette.Categories) Categories.Add(c);

                ByCategory.Clear();
                foreach (var kv in palette.ByCategory) {
                    ByCategory[kv.Key] = [];
                    foreach (var tile in kv.Value) {
                        ByCategory[kv.Key].Add(tile);
                    }
                }

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