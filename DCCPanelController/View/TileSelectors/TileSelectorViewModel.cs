using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Helpers;
using DCCPanelController.Models.DataModel;
using DCCPanelController.Models.DataModel.Entities;
using DCCPanelController.Models.ViewModel.Helpers;
using DCCPanelController.Models.ViewModel.Interfaces;
using DCCPanelController.Models.ViewModel.Tiles;
using DCCPanelController.Resources.Styles;
using DCCPanelController.View.Base;
using Syncfusion.Maui.Toolkit.SegmentedControl;

namespace DCCPanelController.View.TileSelectors;

public abstract partial class TileSelectorViewModel : BaseViewModel {
    
    private Panel? _tilePanel;       // A Dummy Panel we create holding the palette tiles
    public  ObservableCollection<string> Categories { get; } = [];
    public Dictionary<string, ObservableCollection<ITile>> ByCategory { get; init; } = [];

    public Panel Panel {
        // We add the Selector Panel to the Parent Panel collection so that when 
        // we reference things like Color, it comes from the Globally set values
        set {
            _tilePanel  = value.CloneEmptyPanel("Selector");     // Create a clone of the Panel  
            BuildAllTiles();                      
        }
    }

    protected abstract void AfterBuildAllTiles();
    
    private void BuildAllTiles() {
        if (_tilePanel is null) {
            Console.WriteLine("We should not be here with the Tile Panel null.");
            return;
        }
        using (new CodeTimer("BuildTiles", false)) {
            Categories.Clear();
            ByCategory.Clear();

            var panel = _tilePanel;

            // ——— Build categories/tiles to match Swift defaults ———
            // Categories: Actions, Track, Drawing (plus you had Branch & Shapes in the side panel)
            // (We’ll keep your existing groupings, since you already rely on them.)
            Add("Actions", [
                new TurnoutButtonEntity(panel) { State = ButtonStateEnum.On },
                new ActionButtonEntity(panel) { State = ButtonStateEnum.On },
                new RouteEntity(panel) { State = RouteStateEnum.Active },
                new SwitchEntity(panel) { SwitchStyle = SwitchStyleEnum.Light, State = ButtonStateEnum.On },
                new SwitchEntity(panel) { SwitchStyle = SwitchStyleEnum.Switch, State = ButtonStateEnum.On },
            ]);

            Add("Mainline", [
                new StraightEntity(panel) { TrackType = TrackTypeEnum.MainLine },
                new StraightContinuationEntity(panel) { TrackType = TrackTypeEnum.MainLine },
                new CornerEntity(panel) { TrackType = TrackTypeEnum.MainLine },
                new CornerContinuationEntity(panel) { TrackType = TrackTypeEnum.MainLine },
                new LeftTurnoutEntity(panel) { TrackType = TrackTypeEnum.MainLine },
                new RightTurnoutEntity(panel) { TrackType = TrackTypeEnum.MainLine },
                new TerminatorEntity(panel) { TrackType = TrackTypeEnum.MainLine },
                new CrossingEntity(panel) { TrackType = TrackTypeEnum.MainLine },
                new TunnelEntity(panel) { TrackType = TrackTypeEnum.MainLine },
                new BridgeEntity(panel) { TrackType = TrackTypeEnum.MainLine },
                new PlatformEntity(panel) { TrackType = TrackTypeEnum.MainLine }
            ]);

            Add("Branch", [
                new StraightEntity(panel) { TrackType = TrackTypeEnum.BranchLine },
                new StraightContinuationEntity(panel) { TrackType = TrackTypeEnum.BranchLine },
                new CornerEntity(panel) { TrackType = TrackTypeEnum.BranchLine },
                new CornerContinuationEntity(panel) { TrackType = TrackTypeEnum.BranchLine },
                new LeftTurnoutEntity(panel) { TrackType = TrackTypeEnum.BranchLine },
                new RightTurnoutEntity(panel) { TrackType = TrackTypeEnum.BranchLine },
                new TerminatorEntity(panel) { TrackType = TrackTypeEnum.BranchLine },
                new CrossingEntity(panel) { TrackType = TrackTypeEnum.BranchLine },
                new TunnelEntity(panel) { TrackType = TrackTypeEnum.BranchLine },
                new BridgeEntity(panel) { TrackType = TrackTypeEnum.BranchLine },
                new PlatformEntity(panel) { TrackType = TrackTypeEnum.BranchLine },
            ]);

            Add("Shapes", [
                new TextEntity(panel),
                new CircleLabelEntity(panel),
                new RectangleEntity(panel) { Height = 1, Width = 1, BackgroundColor = Colors.Silver, BorderColor = Colors.Black },
                new LineEntity(panel) { Height = 1, Width = 1, LineColor = Colors.Black, LineWidth = 3 },
                new CircleEntity(panel) { Height = 1, Width = 1, BackgroundColor = Colors.Silver, BorderColor = Colors.Black },
                new ImageEntity(panel),
            ]);
        }
        AfterBuildAllTiles();
        return;

        // Local helper to add a category
        void Add(string category, IEnumerable<Entity> entities) {
            var list = new ObservableCollection<ITile>();
            foreach (var e in entities) {
                var tile = TileFactory.CreateTile(e, 32, TileDisplayMode.Symbol);
                if (tile is not null) list.Add(tile);
            }
            ByCategory[category] = list;
            Categories.Add(category);
        }
    }
}

public class BindingProxy : BindableObject {
    public static readonly BindableProperty DataProperty =
        BindableProperty.Create(nameof(Data), typeof(object), typeof(BindingProxy), propertyChanged: OnProxyChanged);

    private static void OnProxyChanged(BindableObject bindable, object oldValue, object newValue) {
        Console.WriteLine($"BindingProxy Changed: Bindable is {bindable.GetType().Name} Value={newValue}");
    }

    public object Data {
        get => GetValue(DataProperty);
        set {
            Console.WriteLine($"BindingProxy Changed: Value={value}");
            SetValue(DataProperty, value);
        }
    }
}
