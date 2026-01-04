using DCCPanelController.Helpers.Logging;
using DCCPanelController.View.Components;
using Indiko.Maui.Controls.Markdown;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls.Shapes;

namespace DCCPanelController.View.Properties.DynamicProperties.Renderers;

public abstract class BaseRenderer {
    protected static readonly ILogger Logger = LogHelper.CreateLogger("PropertyRenderer");

    protected static readonly Color LabelColor      = Colors.Black;
    protected static readonly Color FieldColor      = Colors.Black;
    protected static readonly Color ErrorColor      = Colors.Red;
    protected static readonly Color DescColor       = Colors.LightSteelBlue;
    protected static readonly Color ModifiedColor   = Colors.Green;
    protected static readonly Color MixedValueColor = Colors.Blue;
    
    protected virtual int LabelWidth => 125; // Allow this to be overriden
    protected virtual int FieldHeight => 35;
    protected virtual int FieldWidth => -1;

    protected virtual int LabelFontSize => 12;
    protected virtual int FieldFontSize => 12;
    protected virtual int ErrorFontSize => 10;
    protected virtual int DescFontSize => 10;
    protected virtual int ColumnSpacing => 12;
    protected virtual int RowSpacing => 2;

    protected bool IsModified(PropertyRow row) => row.IsTouched || !DefaultEquality.AreEqual(row.CurrentValue, row.OriginalValue);
    protected string MixedPlaceholder(PropertyRow row) => row.HasMixedValues ? "— mixed —" : string.Empty;
    protected string MixedPlaceholderInt(PropertyRow row) => row.HasMixedValues ? "---" : string.Empty;

    protected event Action<PropertyRow>? RowValueChanged;

    protected void SetValue(PropertyRow row, object? newVal) {
        try {
            row.IsTouched = true;
            row.CurrentValue = newVal;
            RowValueChanged?.Invoke(row); // <— notify UI wrappers to refresh visuals
        } catch (Exception ex) {
            Logger.LogError(ex, "Error setting value for property: {Label}", row?.Field?.Meta?.Label);
        }
    }

    protected int GetFieldWidth(PropertyContext ctx) {
        try {
            // Need to account for iPhone style displays so set a maximum width that we can
            // adjust the field to. If we had the field as 300 + 150 label then 450 is > iPhone width
            // ---------------------------------------------------------------------------------------
            var maxWidth = (int)(ctx.Width - (LabelWidth + ColumnSpacing + 50));
            var fieldWidth = ctx?.Row?.Field?.Meta?.Width ?? -1;
            return fieldWidth switch {
                -1                      => maxWidth,
                0 when FieldWidth is-1  => maxWidth,
                0 when FieldWidth is> 0 => Math.Min(FieldWidth, maxWidth),
                > 0                     => Math.Min(fieldWidth, maxWidth),
                _                       => Math.Min(FieldWidth, maxWidth),
            };
        } catch (Exception ex) {
            Logger.LogError(ex, "Error calculating field width");
            return 200; // Return reasonable default
        }
    }

    protected Microsoft.Maui.Controls.View WrapPicker(PropertyContext ctx, Picker picker, int width) {
        try {
            var row = ctx?.Row;
            if (row == null) {
                Logger.LogError("WrapPicker called with null row");
                return new Label { Text = "Error: Invalid context" };
            }

            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(28) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(28) });
            grid.WidthRequest = width;
            grid.Margin = new Thickness(5, 0, 0, 5);

            var clearButton = new ImageButton {
                BackgroundColor = Colors.White,
                CornerRadius = 21,
                Margin = new Thickness(0, 4, 8, 0),
                Scale = 0.5,
                Source = "x_circle.png",
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
            };

            var findButton = new ImageButton {
                BackgroundColor = Colors.White,
                CornerRadius = 21,
                Margin = new Thickness(0, 4, 0, 0),
                Scale = 0.5,
                Source = "search.png",
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
            };

            var isReadOnly = row.Field?.Meta?.IsReadOnlyInRunMode ?? false;
            clearButton.IsEnabled = !isReadOnly && row.CurrentValue != null && !string.IsNullOrEmpty(row.CurrentValue as string);
            findButton.IsEnabled = !isReadOnly;

            clearButton.Clicked += (s, e) => {
                try {
                    picker.SelectedIndex = -1;
                    picker.SelectedItem = null;
                    SetValue(row, null);
                } catch (Exception ex) {
                    Logger.LogError(ex, "Error clearing picker value");
                }
            };

            findButton.Clicked += (sender, args) => {
                try {
                    picker.Focus();
                } catch (Exception ex) {
                    Logger.LogError(ex, "Error focusing picker");
                }
            };

            row.CurrentChanged += (sender, o) => {
                try {
                    var readOnly = row.Field?.Meta?.IsReadOnlyInRunMode ?? false;
                    clearButton.IsEnabled = !readOnly && o is { };
                } catch (Exception ex) {
                    Logger.LogError(ex, "Error handling picker CurrentChanged event");
                }
            };

            picker.VerticalOptions = LayoutOptions.Center;
            grid.Add(picker, 0);
            grid.Add(findButton, 1);
            grid.Add(clearButton, 2);

            return grid;
        } catch (Exception ex) {
            Logger.LogError(ex, "Error wrapping picker");
            return new Label { Text = "Error creating picker" };
        }
    }

    protected Microsoft.Maui.Controls.View AddBorder(Microsoft.Maui.Controls.View view) {
        var border = new Border {
            Stroke = new SolidColorBrush(Colors.Gray),
            StrokeThickness = 1,
            Background = new SolidColorBrush(Colors.White),
            StrokeShape = new RoundRectangle {
                CornerRadius = new CornerRadius(10), // uniform radius
            },
            Content = view,
        };
        return border;
    }

    protected Microsoft.Maui.Controls.View WrapWithLabel(PropertyContext ctx,
        Microsoft.Maui.Controls.View control) {
        try {
            var row = ctx.Row;
            var labelWidth = LabelWidth;
            var fieldHeight = FieldHeight;
            var fieldWidth = GetFieldWidth(ctx);

            var label = new Label {
                Text = row.Field?.Meta?.Label ?? "Unknown",
                FontSize = LabelFontSize,
                TextColor = LabelColor,
                VerticalOptions = LayoutOptions.Center,
                HorizontalTextAlignment = TextAlignment.Start,
                HorizontalOptions = LayoutOptions.Fill,
            };

            var descriptionKey = row.Field?.Meta?.Description ?? string.Empty;
            var description = string.IsNullOrWhiteSpace(descriptionKey)
                ? null
                : new MarkdownLabel {
                    Text = descriptionKey,
                    Opacity = 0.7,
                    FontSize = DescFontSize,
                    FontColor = DescColor,
                    HorizontalOptions = LayoutOptions.Fill,
                    VerticalOptions = LayoutOptions.Center,
                };

            var errorLabel = new Label {
                FontSize = ErrorFontSize,
                TextColor = ErrorColor,
                IsVisible = false,
            };

            // Create a 2 Column grid, first fixed to 150 and second Auto
            // -----------------------------------------------------------------
            var grid = new Grid {
                ColumnDefinitions = {
                    new ColumnDefinition(labelWidth >= 0 ? labelWidth : 0),
                    new ColumnDefinition(fieldWidth >= 0 ? GetFieldWidth(ctx) : GridLength.Star),
                    new ColumnDefinition(GridLength.Star),
                },
                RowDefinitions = {
                    new RowDefinition(fieldHeight >= 0 ? fieldHeight : GridLength.Auto), // label + control
                    new RowDefinition(GridLength.Auto),
                },
                ColumnSpacing = ColumnSpacing,
                RowSpacing = RowSpacing,
            };

            // Row 0: label + control
            // -----------------------------------------------------------------
            if (LabelWidth > 0) {
                grid.Add(label, 0);
                grid.Add(control, 1);
            } else {
                grid.Add(control, 0);
                grid.SetColumnSpan(control, 2);
            }

            // Row 1: description/error, indented under the control (col 1)
            // -----------------------------------------------------------------
            var infoStack = new VerticalStackLayout { Spacing = 2 };
            if (description != null) infoStack.Add(description);
            infoStack.Add(errorLabel);
            grid.Add(infoStack, 1, 1);
            grid.SetColumnSpan(infoStack, 2);

            RowValueChanged += OnRowValueChanged;
            grid.Unloaded += (_, __) => RowValueChanged -= OnRowValueChanged;

            RefreshVisuals();
            return grid;

            // Listen for value changes on this row (so label updates as you type/click)
            void OnRowValueChanged(PropertyRow changed) {
                try {
                    if (ReferenceEquals(changed, row)) RefreshVisuals();
                } catch (Exception ex) {
                    Logger.LogError(ex, "Error in OnRowValueChanged");
                }
            }

            void RefreshVisuals() {
                try {
                    var modified = IsModified(row);
                    label.TextColor = modified ? ModifiedColor
                        : row.HasMixedValues   ? MixedValueColor : LabelColor;

                    var err = row.Issues?.FirstOrDefault(i => i.Severity == Severity.Error);
                    errorLabel.IsVisible = err != null;
                    errorLabel.Text = err?.Message ?? string.Empty;
                } catch (Exception ex) {
                    Logger.LogError(ex, "Error refreshing visual state for property: {Label}", row?.Field?.Meta?.Label);
                }
            }
        } catch (Exception ex) {
            Logger.LogError(ex, "Error wrapping control with label");
            return new Label { Text = "Error creating property control" };
        }
    }
}