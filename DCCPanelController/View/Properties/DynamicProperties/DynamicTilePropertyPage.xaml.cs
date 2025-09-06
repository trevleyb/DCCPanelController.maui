using CommunityToolkit.Maui.Behaviors;
using CommunityToolkit.Maui.Markup;
using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DCCPanelController.Models.ViewModel.Interfaces;
using DCCPanelController.Resources.Styles;
using DCCPanelController.Services;
using DCCPanelController.View.Converters;

namespace DCCPanelController.View.Properties.DynamicProperties;

[ObservableObject]
public partial class DynamicTilePropertyPopupContent {
    public DynamicTilePropertyPopupContent() {
        InitializeComponent();
        BindingContext = this;
    }

    public static readonly BindableProperty TilesSourceProperty = BindableProperty.Create(nameof(TilesSource), typeof(IEnumerable<ITile>), typeof(DynamicTilePropertyPopupContent), defaultValue: null, propertyChanged: OnTilesSourceChanged);
    public IEnumerable<ITile>? TilesSource {
        get => (IEnumerable<ITile>?)GetValue(TilesSourceProperty);
        set => SetValue(TilesSourceProperty, value);
    }

    public string Title { get; set; } = "Properties";
    public DynamicTilePropertyForm? Form => _form;
    public event EventHandler? Applied;
    public event EventHandler? Cancelled;

    private DynamicTilePropertyForm? _form;
    private IUndoService _undo = new DefaultUndoService(); // from MauiAdapters

    private static async void OnTilesSourceChanged(BindableObject bindable, object oldValue, object newValue) {
        var view = (DynamicTilePropertyPopupContent)bindable;
        await view.RebuildAsync();
    }

    private async Task RebuildAsync() {
        PropertyHost.Children.Clear();
        var tiles = TilesSource?.ToList();
        if (tiles == null || tiles.Count == 0) return;

        var selection = tiles.Select(object (t) => t.Entity);
        _form = DynamicTilePropertyForm.CreateForm(selection);
        await _form.ValidateAsync();

        bool isFirst = true;
        foreach (var group in _form.Groups) {
            var header = CreateExpanderGroup(group.Name, isFirst);
            isFirst = false;

            // Group body: stack of labeled controls
            //var body = new VerticalStackLayout { Spacing = 8 };
            foreach (var row in group.Rows) {
                if (_form!.GetRendererView(row) is Microsoft.Maui.Controls.View v) {
                    header.children?.Add(v);
                }
            }
            PropertyHost.Children.Add(header.expander);
        }
    }

    public async Task Validate() {
        if (_form == null) return;
        var summary = await _form.ValidateAsync();
        if (summary.HasErrors) {
            await DisplayAlertHelper.DisplayOkAlertAsync("Validation Error", "Please fix errors.");
        } else {
            await DisplayAlertHelper.DisplayOkAlertAsync("Validation Passed", "All Validation Rules Passed.");
        }
    }

    [RelayCommand]
    private async Task ApplyAsync() {
        if (_form == null) return;

        // Important: ensure PropertyChange uses row.Field (already fixed in your FormContext)
        var ok = await _form.ApplyAsync(requireAtomic: false);
        if (!ok) {
            await DisplayAlertHelper.DisplayOkAlertAsync("Apply", "Nothing to apply or failed.");
            return;
        }
        Applied?.Invoke(this, EventArgs.Empty);
    }

    [RelayCommand]
    private async Task CancelAsync() {
        await _undo.UndoAsync();
        await RebuildAsync();
        Cancelled?.Invoke(this, EventArgs.Empty);
    }

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
}