using System;
using System.Text.RegularExpressions;
using Microsoft.Maui.Controls;

namespace DCCPanelController.Clients.Helpers;

// Convenience specializations
public class EntryIntValidationBehavior : EntryValidationBehavior<int> { }

public class EntryDoubleValidationBehavior : EntryValidationBehavior<double> { }

public class EntryValidationBehavior<T> : Behavior<Entry>
    where T : struct, IParsable<T>, IComparable<T> {
    public static readonly BindableProperty MinProperty = BindableProperty.Create(nameof(Min), typeof(T), typeof(EntryValidationBehavior<T>), default(T));
    public static readonly BindableProperty MaxProperty = BindableProperty.Create(nameof(Max), typeof(T), typeof(EntryValidationBehavior<T>), default(T));
    public static readonly BindableProperty PatternProperty = BindableProperty.Create(nameof(Pattern), typeof(string), typeof(EntryValidationBehavior<T>), null);
    public static readonly BindableProperty IsRequiredProperty = BindableProperty.Create(nameof(IsRequired), typeof(bool), typeof(EntryValidationBehavior<T>), false);

    // Flags to indicate whether Min/Max should be enforced
    // public bool HasMin => Min != null;
    // public bool HasMax => Max != null;

    public T Min {
        get => (T)GetValue(MinProperty);
        set => SetValue(MinProperty, value);
    }

    public T Max {
        get => (T)GetValue(MaxProperty);
        set => SetValue(MaxProperty, value);
    }

    public string? Pattern {
        get => (string?)GetValue(PatternProperty);
        set => SetValue(PatternProperty, value);
    }

    public bool IsRequired {
        get => (bool)GetValue(IsRequiredProperty);
        set => SetValue(IsRequiredProperty, value);
    }

    private Entry? _entry;

    protected override void OnAttachedTo(Entry bindable) {
        base.OnAttachedTo(bindable);
        _entry = bindable;
        bindable.TextChanged += OnTextChanged;

        // Ensure bindings on the behavior work:
        BindingContext = bindable.BindingContext;
        bindable.BindingContextChanged += (_, __) => BindingContext = bindable.BindingContext;
    }

    protected override void OnDetachingFrom(Entry bindable) {
        base.OnDetachingFrom(bindable);
        bindable.TextChanged -= OnTextChanged;
        _entry = null;
    }

    private void OnTextChanged(object? sender, TextChangedEventArgs e) {
        var isValid = Validate(e.NewTextValue);
        _entry!.TextColor = isValid ? Colors.Black : Colors.Red;
    }

    private bool Validate(string? text) {
        // Required check
        if (IsRequired && string.IsNullOrWhiteSpace(text)) return false;

        // Pattern check
        if (!string.IsNullOrEmpty(Pattern) && !Regex.IsMatch(text ?? string.Empty, Pattern)) return false;

        // Empty text past here is OK if not required
        if (string.IsNullOrWhiteSpace(text)) return true;

        // TryParse gate
        if (!T.TryParse(text, null, out var value)) return false;

        // Range checks only if the flags are set
        if (value.CompareTo((T)Min) < 0) return false;
        if (value.CompareTo((T)Max) > 0) return false;
        return true;
    }
}