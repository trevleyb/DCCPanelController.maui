using CommunityToolkit.Maui.Behaviors;
using CommunityToolkit.Maui.Markup;
using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using DCCPanelController.Models.DataModel.Entities;
using DCCPanelController.Resources.Styles;
using DCCPanelController.View.Converters;
using Microsoft.Maui.Controls.Shapes;
using Syncfusion.Maui.Toolkit.Popup;

namespace DCCPanelController.View.Properties.DynamicProperties;

[ObservableObject]
public partial class DynamicTilePropertyPage {
    public enum FormState { Normal, Invalid, NoSelectedTiles, NoCommonProperties }

    private readonly             IUndoService _undo                = new DefaultUndoService();
    [ObservableProperty] private string       _errorMessages       = "";
    [ObservableProperty] private bool         _hasCommonProperties = true;
    [ObservableProperty] private double       _height;

    [NotifyPropertyChangedFor(nameof(ShowInformation))]
    [ObservableProperty] private string? _information;

    [ObservableProperty] private bool _noCommonProperties;
    [ObservableProperty] private bool _noSelectedProperties;

    [ObservableProperty] private FormState _state = FormState.Normal;
    [ObservableProperty] private string    _title = "Properties";
    [ObservableProperty] private double    _width;
    public                       SfPopup?  Popup;

    private DynamicTilePropertyPage(double width, double height) {
        InitializeComponent();
        BindingContext = this;
        Width = width;
        Height = height;
    }

    public bool ShowInformation => !string.IsNullOrWhiteSpace(Information);

    public DynamicTilePropertyForm? Form { get; private set; }

    public event EventHandler? Applied;
    public event EventHandler? Cancelled;

    #region Build the Properties form
    /// <summary>
    ///     Rebuild the Form if the collection of Tiles changes
    /// </summary>
    private async Task<FormState> RebuildAsync(StackBase propertyHost) {
        propertyHost.Children.Clear();
        State = FormState.Normal;

        // Lets make sure we have some valid Tiles to work with
        // ---------------------------------------------------------------
        var entities = EntitySource?.ToList();
        if (entities == null || entities.Count == 0) return FormState.NoSelectedTiles;

        // Valid the for and ensure we have some common properties
        // ---------------------------------------------------------------
        Form = DynamicTilePropertyForm.CreateForm(entities, Width, Height);
        await Form.ValidateAsync();

        if (!Form.HasCommonProperties) return FormState.NoCommonProperties;

        // Build up the Properties by Group and Sort order. 
        // Each Group is in its own Expander View so can be collaposed
        // ---------------------------------------------------------------
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

    #region Bindable Collection of Tiles
    public static readonly BindableProperty EntitySourceProperty = BindableProperty.Create(nameof(EntitySource), typeof(IEnumerable<Entity>), typeof(DynamicTilePropertyPage), null, propertyChanged: OnTilesSourceChanged);

    public IEnumerable<Entity>? EntitySource {
        get => (IEnumerable<Entity>?)GetValue(EntitySourceProperty);
        set => SetValue(EntitySourceProperty, value);
    }

    private static async void OnTilesSourceChanged(BindableObject bindable, object oldValue, object newValue) {
        try {
            var view = (DynamicTilePropertyPage)bindable;
            view.State = await view.RebuildAsync(view.PropertyHost);
        } catch (Exception ex) {
            Console.WriteLine($"Error rebuilding DynamicTilePropertyPopupContent: {ex.Message}");
        }
    }
    #endregion

    #region Helpers for Launching the PropertyPage
    public static async Task CreatePropertyPage(List<Entity> selectedEntities, double width, double height) {
        try {
            var content = new DynamicTilePropertyPage(width, height) {
                Title = GetTitle(selectedEntities),
                Information = GetInformation(selectedEntities),
                EntitySource = selectedEntities,
            };

            content.Popup = new SfPopup {
                ContentTemplate = new DataTemplate(() => content),
                ShowHeader = false,
                ShowFooter = false,
                BackgroundColor = Colors.WhiteSmoke,
                PopupStyle = new PopupStyle {
                    CornerRadius = 10,
                    HasShadow = false,
                    BlurIntensity = PopupBlurIntensity.Light,
                    MessageBackground = Colors.WhiteSmoke,
                },
                ShowCloseButton = false,
                StaysOpen = true,
                IsFullScreen = true,
                AutoSizeMode = PopupAutoSizeMode.Both,
                AnimationMode = PopupAnimationMode.None,
                OverlayMode = PopupOverlayMode.Transparent,
            };
            content.Popup.Show();
        } catch (Exception ex) {
            Console.WriteLine($"Error Launching DynamicTilePropertyPopupContent: {ex.Message}");
        }
    }

    public void ClosePropertyPage() {
        if (Popup is { } popup) {
            popup.IsOpen = false;
            popup.Dismiss();
        }
    }

    // Get the title to display in the Header of the Property Page
    // ----------------------------------------------------------------------------
    private static string GetTitle(List<Entity> selectedEntities) {
        var title = selectedEntities.Count switch {
            0 => "Unknown Entity",
            1 => $"{selectedEntities[0].EntityName} ({selectedEntities[0].EntityDescription}) properties.",
            _ => AllEntitiesAreSame(selectedEntities) ? $"Multiple {selectedEntities[0].EntityName} ({selectedEntities[0].EntityDescription}) properties." : "Multiple Selected Entities",
        };
        return title;
    }

    /// <summary>
    ///     Returns a markdown label of information about the select Entity(s)
    /// </summary>
    private static string? GetInformation(List<Entity> selectedEntities) {
        if (selectedEntities.Count <= 0) return null;
        if (selectedEntities.Count == 1 || AllEntitiesAreSame(selectedEntities)) return selectedEntities[0].EntityInformation;
        ;
        return null;
    }

    /// <summary>
    ///     Returns true if the Entity Type (by its name) is the same for all selected Entities
    /// </summary>
    private static bool AllEntitiesAreSame(List<Entity> selectedEntities) {
        if (selectedEntities.Count <= 1) return true;
        var first = selectedEntities[0].EntityName;
        return selectedEntities.All(entity => entity.EntityName.Equals(first, StringComparison.CurrentCultureIgnoreCase));
    }
    #endregion

    #region Commands for Applying and Cancelling
    private async void ApplyButtonClicked(object? sender, EventArgs e) {
        try {
            if (sender is Button { BindingContext: DynamicTilePropertyPage { } page }) {
                if (Form != null) {
                    var summary = await Form.ValidateAsync();
                    if (summary.HasErrors) {
                        ErrorMessages = "Errors were found in the form. Please correct them before applying changes.";
                        return;
                    }
                    var ok = await Form.ApplyAsync(false);
                    if (ok) Applied?.Invoke(this, EventArgs.Empty);
                }
                page.ClosePropertyPage();
            }
        } catch (Exception ex) {
            Console.WriteLine($"Error Applying DynamicTilePropertyPopupContent: {ex.Message}");
        }
    }

    private async void CancelButtonClicked(object? sender, EventArgs e) {
        try {
            if (sender is Button { BindingContext: DynamicTilePropertyPage { } page }) {
                await _undo.UndoAsync();
                Cancelled?.Invoke(this, EventArgs.Empty);
                page.ClosePropertyPage();
            }
        } catch (Exception ex) {
            Console.WriteLine($"Error Cancelling DynamicTilePropertyPopupContent: {ex.Message}");
        }
    }

    // [RelayCommand]
    // public async Task<IResult<ValidationSummary>> ValidateAsync() {
    //     if (Form == null) return Result<ValidationSummary>.Fail("Form is null");
    //     var summary = await Form.ValidateAsync();
    //     return summary.HasErrors
    //         ? Result<ValidationSummary>.Fail("Validation Failed").WithValue(summary)
    //         : Result<ValidationSummary>.Ok();
    // }

    // [RelayCommand]
    // public async Task<IResult> ApplyAsync() {
    //     if (Form == null) return Result.Fail("Form should not be null");
    //     var ok = await Form.ApplyAsync(requireAtomic: false);
    //     if (!ok) return Result.Fail("No Changes to apply");
    //     Applied?.Invoke(this, EventArgs.Empty);
    //     return Result.Ok();
    // }

    // [RelayCommand]
    // public async Task CancelAsync() {
    //     await _undo.UndoAsync();
    //     Cancelled?.Invoke(this, EventArgs.Empty);
    // }
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
            BackgroundColor = Colors.White,
        };
        var border = new Border {
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle {
                CornerRadius = new CornerRadius(12),
                BackgroundColor = Colors.White,
            },
            BackgroundColor = Colors.White,
            Content = stackLayout,
        };
        tableExpander.Content = border;
        return(tableExpander, stackLayout.Children);
    }

    private static Image GroupChevrons(Expander expander) {
        var chevron = new Image {
            Source = "chevron_circle_down.png",
            Behaviors = {
                new IconTintColorBehavior {
                    TintColor = (Color?)Application.Current?.Resources["Primary"] ?? Colors.Black,
                },
            },
            HeightRequest = 16,
            WidthRequest = 16,
            VerticalOptions = LayoutOptions.Center,
            HorizontalOptions = LayoutOptions.Start,
            Margin = new Thickness(0, 0, 5, 0),
            Rotation = 0,
        };
        chevron.Bind(RotationProperty, nameof(expander.IsExpanded),
            converter: new ExpandRotationConverter(), source: expander);
        return chevron;
    }

    private static (IView?, IList<IView>?) CreateGroup(string groupKey) {
        var scrollGroup = new ScrollView();
        var tableGroup = new StackLayout {
            Margin = new Thickness(0, 10, 0, 0),
        };

        if (!string.IsNullOrWhiteSpace(groupKey)) {
            tableGroup.Add(GroupHeading(groupKey));
            tableGroup.Add(GroupDivider());
        }
        scrollGroup.Content = tableGroup;
        return(scrollGroup, tableGroup.Children);
    }

    private static Label GroupHeading(string groupKey) => new() {
        Text = FormatLabel(groupKey),
        TextColor = StyleHelper.FromStyle("Primary"),
        FontSize = 18,
        Margin = new Thickness(0, 0, 0, 0),
    };

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
            Margin = new Thickness(5, 5, 5, 5),
        };
    }

    private static BoxView GroupDivider(Color? color = null) {
        color ??= StyleHelper.FromStyle("Primary");
        return new BoxView {
            BackgroundColor = color,
            HeightRequest = 1,
            HorizontalOptions = LayoutOptions.Fill,
        };
    }
    #endregion
}