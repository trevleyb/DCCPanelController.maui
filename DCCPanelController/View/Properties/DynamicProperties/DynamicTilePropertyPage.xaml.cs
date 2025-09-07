using CommunityToolkit.Maui.Behaviors;
using CommunityToolkit.Maui.Markup;
using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DCCCommon;
using DCCPanelController.Models.ViewModel.Interfaces;
using DCCPanelController.Resources.Styles;
using DCCPanelController.View.Converters;

namespace DCCPanelController.View.Properties.DynamicProperties;

[ObservableObject]
public partial class DynamicTilePropertyPopupContent {
    public DynamicTilePropertyPopupContent() {
        InitializeComponent();
        BindingContext = this;
    }
    
    public enum FormState {Normal, Invalid, NoSelectedTiles, NoCommonProperties}

    [ObservableProperty] private FormState _state = FormState.Normal;
    [ObservableProperty] private string _title = "Properties";
    [ObservableProperty] private bool _noCommonProperties = false;
    [ObservableProperty] private bool _noSelectedProperties = false;
    [ObservableProperty] private bool _hasCommonProperties = true;
    
    public DynamicTilePropertyForm? Form { get; private set; }
    public event EventHandler? Applied;
    public event EventHandler? Cancelled;

    private IUndoService _undo = new DefaultUndoService(); // from MauiAdapters

    #region Binadable Collection of Tiles
    public static readonly BindableProperty TilesSourceProperty = BindableProperty.Create(nameof(TilesSource), typeof(IEnumerable<ITile>), typeof(DynamicTilePropertyPopupContent), defaultValue: null, propertyChanged: OnTilesSourceChanged);
    public IEnumerable<ITile>? TilesSource {
        get => (IEnumerable<ITile>?)GetValue(TilesSourceProperty);
        set => SetValue(TilesSourceProperty, value);
    }

    private static async void OnTilesSourceChanged(BindableObject bindable, object oldValue, object newValue) {
        try {
            var view = (DynamicTilePropertyPopupContent)bindable;
            await view.RebuildAsync();
        } catch (Exception ex) {
            Console.WriteLine($"Error rebuilding DynamicTilePropertyPopupContent: {ex.Message}");
        }
    }
    #endregion

    #region Build the Properties form 
    /// <summary>
    /// Rebuild the Form if the collection of Tiles changes
    /// </summary>
    private async Task RebuildAsync() {
        PropertyHost.Children.Clear();
        State = FormState.Normal;
        
        // Lets make sure we have some valid Tiles to work with
        // ---------------------------------------------------------------
        var tiles = TilesSource?.ToList();
        if (tiles == null || tiles.Count == 0) {
            State = FormState.NoSelectedTiles;
            return;
        }

        // Valid the for and ensure we have some common properties
        // ---------------------------------------------------------------
        var selection = tiles.Select(object (t) => t.Entity);
        Form = DynamicTilePropertyForm.CreateForm(selection);
        await Form.ValidateAsync();

        if (!Form.HasCommonProperties) {
            State = FormState.NoCommonProperties;;
            return;
        }

        // Build up the Properties by Group and Sort order. 
        // Each Group is in its own Expander View so can be collaposed
        // ---------------------------------------------------------------
        var isFirst = true;
        foreach (var group in Form.Groups) {
            var header = CreateExpanderGroup(group.Name, isFirst);
            isFirst = false;
            foreach (var row in group.Rows) {
                Console.WriteLine($"Rendering: {row?.Field?.Meta?.EditorKind ?? "Unknown kind"}");
                if (row is not null) {
                    if (Form!.GetRendererView(row) is Microsoft.Maui.Controls.View v) {
                        header.children?.Add(v);
                    }
                }
            }
            PropertyHost.Children.Add(header.expander);
        }
    }

    [RelayCommand]
    private async Task<IResult<ValidationSummary>> ValidateAsync() {
        if (Form == null) return Result<ValidationSummary>.Fail("Form is null");
        var summary = await Form.ValidateAsync();
        return summary.HasErrors 
            ? Result<ValidationSummary>.Fail("Validation Failed").WithValue(summary)
            : Result<ValidationSummary>.Ok();
    }

    [RelayCommand]
    private async Task<IResult> ApplyAsync() {
        if (Form == null) return Result.Fail("Form should not be null");
        var ok = await Form.ApplyAsync(requireAtomic: false);
        if (!ok) return Result.Fail("No Changes to apply");
        Applied?.Invoke(this, EventArgs.Empty);
        return Result.Ok();
    }
    
    [RelayCommand]
    private async Task CancelAsync() {
        await _undo.UndoAsync();
        await RebuildAsync();
        Cancelled?.Invoke(this, EventArgs.Empty);
    }
    #endregion 
    
    #region Group Helpers
    private static (IView? expander, IList<IView>? children) CreateExpanderGroup(string groupKey, bool isFirst) {
        if (string.IsNullOrWhiteSpace(groupKey)) return CreateGroup(groupKey);

        var tableExpander = new Expander();
        var expanderHeading = new StackLayout();
        var expanderTitle = new HorizontalStackLayout();
        expanderTitle.Children.Add(GroupChevrons(tableExpander));
        expanderTitle.Children.Add(GroupHeading(groupKey));
        expanderHeading.Children.Add(expanderTitle);
        expanderHeading.Children.Add(GroupDivider());
        tableExpander.Margin = new Thickness(0, isFirst ? 10 : 20, 10, 10);
        tableExpander.Header = expanderHeading;
        tableExpander.IsExpanded = true;
        tableExpander.BackgroundColor = Colors.WhiteSmoke;

        var stackLayout = new StackLayout {
            Margin = new Thickness(0, 10, 1, 0),
            BackgroundColor = Colors.WhiteSmoke
        };
        tableExpander.Content = stackLayout;
        return (tableExpander, stackLayout.Children);
    }

    private static Image GroupChevrons(Expander expander) {
        var chevron = new Image {
            Source = "chevron_circle_down.png",
            Behaviors = {
                new IconTintColorBehavior {
                    TintColor = (Color?)Application.Current?.Resources["Primary"] ?? Colors.Black
                }
            },
            HeightRequest = 16,
            WidthRequest = 16,
            VerticalOptions = LayoutOptions.Center,
            HorizontalOptions = LayoutOptions.Start,
            Margin = new Thickness(0, 0, 5, 0),
            Rotation = 0
        };
        chevron.Bind(VisualElement.RotationProperty, nameof(expander.IsExpanded),
                     converter: new ExpandRotationConverter(), source: expander);
        return chevron;
    }

    private static (IView?, IList<IView>?) CreateGroup(string groupKey) {
        var scrollGroup = new ScrollView();
        var tableGroup = new StackLayout {
            Margin = new Thickness(0, 10, 0, 0)
        };

        if (!string.IsNullOrWhiteSpace(groupKey)) {
            tableGroup.Add(GroupHeading(groupKey));
            tableGroup.Add(GroupDivider());
        }
        scrollGroup.Content = tableGroup;
        return (scrollGroup, tableGroup.Children);
    }

    private static Label GroupHeading(string groupKey) {
        return new Label {
            Text = FormatLabel(groupKey),
            TextColor = StyleHelper.FromStyle("Primary"),
            FontSize = 18,
            Margin = new Thickness(0, 0, 0, 0)
        };
    }

    private static string FormatLabel(string groupKey) {
        if (groupKey.EndsWith("s")) return groupKey;
        return string.IsNullOrEmpty(groupKey) ? "Properties" : $"{groupKey}";
    }

    private static BoxView GroupDivider(Color? color = null) {
        color ??= StyleHelper.FromStyle("Primary");
        return new BoxView {
            BackgroundColor = color,
            HeightRequest = 1,
            HorizontalOptions = LayoutOptions.Fill
        };
    }
    #endregion
}