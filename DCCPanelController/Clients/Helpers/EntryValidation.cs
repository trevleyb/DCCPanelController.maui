using System;
using Microsoft.Maui.Controls;
using System.Text.RegularExpressions;

namespace DCCPanelController.Clients.Helpers;

public class EntryIntValidationBehavior : EntryValidationBehavior<int> { };
public class EntryDoubleValidationBehavior : EntryValidationBehavior<double> { };

public class EntryValidationBehavior<T> : Behavior<Entry> 
    where T : struct, IParsable<T>, IComparable<T>
{
    public static readonly BindableProperty MinProperty = BindableProperty.Create(nameof(Min), typeof(T), typeof(EntryValidationBehavior<T>), 0);
    public static readonly BindableProperty MaxProperty = BindableProperty.Create(nameof(Max), typeof(T), typeof(EntryValidationBehavior<T>), 99);
    public static readonly BindableProperty PatternProperty = BindableProperty.Create(nameof(Pattern), typeof(string), typeof(EntryValidationBehavior<T>), null);
    public static readonly BindableProperty IsRequiredProperty = BindableProperty.Create(nameof(IsRequired), typeof(bool), typeof(EntryValidationBehavior<T>), false);

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

    Entry? _entry;

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

    void OnTextChanged(object? sender, TextChangedEventArgs e) {
        var isValid = Validate(e.NewTextValue);
        _entry?.TextColor = isValid ? Colors.Black : Colors.Red;
    }

    private bool Validate(string? text) {
        var ok = !(IsRequired && string.IsNullOrWhiteSpace(text));

        // if (ok && Pattern is { Length: > 0 })
        //     ok = Regex.IsMatch(text ?? string.Empty, Pattern);
        //
        // if (ok && T.TryParse(text, null, out var value)) {
        //     if (Min is { } min && value.CompareTo(min) < 0) ok = false;
        //     if (Max is { } max && value.CompareTo(max) > 0) ok = false;
        // } else if (ok && (Min != null || Max != null) && !string.IsNullOrWhiteSpace(text)) {
        //     ok = false;
        // }
      
        return ok;
    }
}


public class FastClockValidationBehavior : Behavior<Entry> {
    protected override void OnAttachedTo(Entry entry) {
        entry.TextChanged += OnEntryTextChanged;
        base.OnAttachedTo(entry);
    }

    protected override void OnDetachingFrom(Entry entry) {
        entry.TextChanged -= OnEntryTextChanged;
        base.OnDetachingFrom(entry);
    }

    private void OnEntryTextChanged(object? sender, TextChangedEventArgs args) {
        if (sender is Entry entry) {
            if (!string.IsNullOrEmpty(args.NewTextValue)) {
                if (double.TryParse(args.NewTextValue, out var result)) {
                    entry.TextColor = result is>= 0 and<= 12 ? Colors.Black : Colors.Red;
                }
            }
        }
    }
}