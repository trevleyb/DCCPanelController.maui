using System;
using System.Collections.Generic;
using System.ComponentModel;
using DCCPanelController.View.Helpers;
using Microsoft.Maui.Controls;

namespace DCCPanelController.View.Components;

public partial class FontPicker : ContentView, INotifyPropertyChanged {
    public FontPicker() {
        InitializeComponent();
    }
    
    public event EventHandler<string>? AliasChanged;
    
    public string? SelectedFamily { get; set; }
    public string? SelectedStyle { get; set; }

    // -------- Bindable API (the one you persist and pass to ICanvas) --------
    public static readonly BindableProperty SelectedFontAliasProperty = BindableProperty.Create(nameof(SelectedFontAlias), typeof(string), typeof(FontPicker), null, BindingMode.TwoWay, propertyChanged: OnSelectedFontAliasChanged);
    public static readonly BindableProperty FontSizeProperty          = BindableProperty.Create(nameof(FontSize), typeof(int), typeof(FontPicker), 12);
    public static readonly BindableProperty FontColorProperty         = BindableProperty.Create(nameof(FontColor), typeof(Color), typeof(FontPicker), Colors.Black);

    public string? SelectedFontAlias {
        get => (string?)GetValue(SelectedFontAliasProperty);
        set => SetValue(SelectedFontAliasProperty, value);
    }

    public int FontSize {
        get => (int)GetValue(FontSizeProperty);
        set => SetValue(FontSizeProperty, value);
    }

    public Color FontColor {
        get => (Color)GetValue(FontColorProperty);
        set => SetValue(FontColorProperty, value);
    }

    // -------- Backing collections for the XAML Pickers --------
    public IReadOnlyList<string> Families => FontCatalog.Families;
    public IReadOnlyList<string> Styles => (string.IsNullOrWhiteSpace(SelectedFamily) ? [] : FontCatalog.StylesFor(SelectedFamily));

    // -------- Property change glue for the UI --------
    static void OnSelectedFontAliasChanged(BindableObject bindable, object oldValue, object newValue) {
        var control = (FontPicker)bindable;
        
        var alias = newValue as string;
        if (string.IsNullOrWhiteSpace(alias)) return;

        var match = FontCatalog.GetFontFace(alias);
        if (match is null) return;

        control.SelectedFamily = match.Family;
        control.SelectedStyle = match.Style;

        if (string.IsNullOrWhiteSpace(control.SelectedFamily)) control.SelectedFamily = FontCatalog.DefaultFontFamily; 
        if (string.IsNullOrWhiteSpace(control.SelectedStyle))  control.SelectedStyle = FontCatalog.DefaultStyleFor(control.SelectedFamily);
        
        control.Notify(nameof(control.Styles));
        control.Notify(nameof(control.SelectedFontAlias));
        control.Notify(nameof(control.SelectedFamily));
        control.Notify(nameof(control.SelectedStyle));
        control.AliasChanged?.Invoke(control, alias);
    }

    void UpdateAliasFromFamilyStyle() {
        if (string.IsNullOrWhiteSpace(SelectedFamily)) SelectedFamily = FontCatalog.DefaultFontFamily; 
        if (string.IsNullOrWhiteSpace(SelectedStyle))  SelectedStyle = FontCatalog.DefaultStyleFor(SelectedFamily);

        var selectedAlias = FontCatalog.GetAlias(SelectedFamily, SelectedStyle);
        if (!string.Equals(selectedAlias, SelectedFontAlias, StringComparison.OrdinalIgnoreCase)) {
            SelectedFontAlias = selectedAlias;
            OnPropertyChanged(nameof(SelectedFontAlias));
            OnPropertyChanged(nameof(SelectedFamily));
            OnPropertyChanged(nameof(SelectedStyle));
        }
    }

    void Notify(string propertyName) => OnPropertyChanged(propertyName);

    private void FamilyPicker_OnSelectedIndexChanged(object? sender, EventArgs e) {
        var index = FamilyPicker.SelectedIndex;
        if (index < 0 || index >= Families.Count) return;
        
        var fam = Families[index];
        if (string.IsNullOrWhiteSpace(fam)) return; 

        SelectedFamily = fam;
        
        var styles = FontCatalog.StylesFor(fam);
        if (string.IsNullOrWhiteSpace(SelectedStyle) || !styles.Contains(SelectedStyle, StringComparer.OrdinalIgnoreCase)) {
            var def = FontCatalog.DefaultStyleFor(fam);
            if (!string.Equals(SelectedStyle, def, StringComparison.OrdinalIgnoreCase)) SelectedStyle = def;
        }

        UpdateAliasFromFamilyStyle();
    }

    private void StylePicker_OnSelectedIndexChanged(object? sender, EventArgs e) {
        var index = StylePicker.SelectedIndex;
        if (index < 0 || index >= Styles.Count) return;
        
        var style = Styles[index];
        if (string.IsNullOrWhiteSpace(style)) return; 
        
        SelectedStyle = style;
        UpdateAliasFromFamilyStyle();
    }
}