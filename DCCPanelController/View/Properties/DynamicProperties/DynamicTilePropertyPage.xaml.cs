using CommunityToolkit.Maui.Behaviors;
using CommunityToolkit.Maui.Markup;
using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DCCCommon;
using DCCPanelController.Helpers;
using DCCPanelController.Models.ViewModel.Interfaces;
using DCCPanelController.Resources.Styles;
using DCCPanelController.View.Converters;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;

namespace DCCPanelController.View.Properties.DynamicProperties;

[ObservableObject]
public partial class DynamicTilePropertyPopupContent {
    public DynamicTilePropertyPopupContent() {
        InitializeComponent();
        BindingContext = this;
    }

    public enum FormState { Normal, Invalid, NoSelectedTiles, NoCommonProperties }

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
            Console.WriteLine("OnTilesSourceChanged");
            var view = (DynamicTilePropertyPopupContent)bindable;
            view.State = await view.RebuildAsync(view.PropertyHost);
        } catch (Exception ex) {
            Console.WriteLine($"Error rebuilding DynamicTilePropertyPopupContent: {ex.Message}");
        }
    }
    #endregion

    #region Commands for Applying and Cancelling
    [RelayCommand]
    public async Task<IResult<ValidationSummary>> ValidateAsync() {
        if (Form == null) return Result<ValidationSummary>.Fail("Form is null");
        var summary = await Form.ValidateAsync();
        return summary.HasErrors
            ? Result<ValidationSummary>.Fail("Validation Failed").WithValue(summary)
            : Result<ValidationSummary>.Ok();
    }

    [RelayCommand]
    public async Task<IResult> ApplyAsync() {
        if (Form == null) return Result.Fail("Form should not be null");
        var ok = await Form.ApplyAsync(requireAtomic: false);
        if (!ok) return Result.Fail("No Changes to apply");
        Applied?.Invoke(this, EventArgs.Empty);
        return Result.Ok();
    }

    [RelayCommand]
    public async Task CancelAsync() {
        await _undo.UndoAsync();
        Cancelled?.Invoke(this, EventArgs.Empty);
    }
    #endregion
    
    #region Build the Properties form
    /// <summary>
    /// Rebuild the Form if the collection of Tiles changes
    /// </summary>
    private async Task<FormState> RebuildAsync(StackBase propertyHost) {
        propertyHost.Children.Clear();
        State = FormState.Normal;

        // Lets make sure we have some valid Tiles to work with
        // ---------------------------------------------------------------
        var tiles = TilesSource?.ToList();
        if (tiles == null || tiles.Count == 0) return FormState.NoSelectedTiles;

        // Valid the for and ensure we have some common properties
        // ---------------------------------------------------------------
        var selection = tiles.Select(object (t) => t.Entity);
        Form = DynamicTilePropertyForm.CreateForm(selection);
        await Form.ValidateAsync();

        if (!Form.HasCommonProperties) return FormState.NoCommonProperties;

        // Build up the Properties by Group and Sort order. 
        // Each Group is in its own Expander View so can be collaposed
        // ---------------------------------------------------------------
        var isFirst = true;
        foreach (var group in Form.Groups) {
            if (group.Rows.Count == 0) continue;

            var expander = CreateExpanderGroup(group.Name);
            
            // Add the rows to the expander
            // -----------------------------------------------------------
            var children = 0;
            foreach (var row in group.Rows) {
                if (Form.GetRendererView(row) is Microsoft.Maui.Controls.View v) {
                    expander.children?.Add(v);
                    if (++children < group.Rows.Count) expander.children?.Add(FieldDivider());
                }
            }
            if (children > 0) PropertyHost.Children.Add(expander.expander);
        }
        return FormState.Normal;
    }
    #endregion 
    
    #region Group Helpers
    private static (IView? expander, IList<IView>? children) CreateExpanderGroup(string groupKey) {
        if (string.IsNullOrWhiteSpace(groupKey)) return CreateGroup(groupKey);

        var tableExpander = new Expander();
        var expanderHeading = new StackLayout();
        var expanderTitle = new HorizontalStackLayout();
        expanderTitle.Children.Add(GroupChevrons(tableExpander));
        expanderTitle.Children.Add(GroupHeading(groupKey));
        expanderHeading.Children.Add(expanderTitle);
        //expanderHeading.Children.Add(GroupDivider());
        tableExpander.Margin = new Thickness(0, 10, 10, 10);
        tableExpander.Header = expanderHeading;
        tableExpander.IsExpanded = true;
        tableExpander.BackgroundColor = Colors.WhiteSmoke;

        var stackLayout = new StackLayout {
            Padding = new Thickness(10, 10, 10, 10),
            BackgroundColor = Colors.White
        };
        var border = new Border() {
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle {
                CornerRadius = new CornerRadius(12),
                BackgroundColor = Colors.White,
            },
            BackgroundColor = Colors.White,
            Content = stackLayout,
        };
        tableExpander.Content = border;
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

    private static BoxView FieldDivider(Color? color = null) {
        color ??= Colors.WhiteSmoke;
        return new BoxView {
            BackgroundColor = color,
            HeightRequest = 3,
            HorizontalOptions = LayoutOptions.Fill,
            Margin = new Thickness(5, 5, 5, 5)
        };
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