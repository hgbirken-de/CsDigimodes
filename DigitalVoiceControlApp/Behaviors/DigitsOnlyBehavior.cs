using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using System;
using System.Linq;

namespace DigitalVoiceControlApp.Behaviors;

public static class DigitsOnlyBehavior
{
    public static readonly AttachedProperty<bool> DigitsOnlyProperty = AvaloniaProperty.RegisterAttached<TextBox, bool>("DigitsOnly", typeof(DigitsOnlyBehavior));

    // Maximum allowed value (optional)
    public static readonly AttachedProperty<int> MaxValueProperty = AvaloniaProperty.RegisterAttached<TextBox, int>("MaxValue", typeof(DigitsOnlyBehavior), defaultValue: int.MaxValue);

    public static void SetDigitsOnly(AvaloniaObject element, bool value) => element.SetValue(DigitsOnlyProperty, value);

    public static bool GetDigitsOnly(AvaloniaObject element) => element.GetValue(DigitsOnlyProperty);

    public static void SetMaxValue(AvaloniaObject element, int value) => element.SetValue(MaxValueProperty, value);

    public static int GetMaxValue(AvaloniaObject element) => element.GetValue(MaxValueProperty);

    static DigitsOnlyBehavior()
    {
        DigitsOnlyProperty.Changed.AddClassHandler<TextBox>((tb, e) =>
        {
            if (e.NewValue is true)
            {
                tb.AddHandler(TextBox.TextChangedEvent, OnTextChanged, RoutingStrategies.Bubble);
            }
            else
            {
                tb.RemoveHandler(TextBox.TextChangedEvent, OnTextChanged);
            }
        });
    }

    private static void OnTextChanged(object? sender, RoutedEventArgs e)
    {
        if (sender is not TextBox tb || string.IsNullOrEmpty(tb.Text))
            return;

        var filtered = new string([.. tb.Text.Where(char.IsDigit)]);

        // Check max value
        if (int.TryParse(filtered, out int value))
        {
            int max = GetMaxValue(tb);
            if (value > max)
            {
                value = max;
                filtered = value.ToString();
            }
        }

        if (filtered != tb.Text)
        {
            var caret = tb.CaretIndex;
            tb.Text = filtered;
            tb.CaretIndex = Math.Min(caret, filtered.Length);
        }
    }
}
