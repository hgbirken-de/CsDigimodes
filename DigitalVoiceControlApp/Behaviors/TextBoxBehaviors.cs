using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace DigitalVoiceControlApp.Behaviors;

public static class TextBoxBehaviors
{
    public static readonly AttachedProperty<bool> ToUpperProperty =
        AvaloniaProperty.RegisterAttached<TextBox, bool>(
            "ToUpper", typeof(TextBoxBehaviors));

    public static bool GetToUpper(TextBox tb) => tb.GetValue(ToUpperProperty);
    public static void SetToUpper(TextBox tb, bool value) => tb.SetValue(ToUpperProperty, value);

    static TextBoxBehaviors()
    {
        ToUpperProperty.Changed.AddClassHandler<TextBox>((tb, e) =>
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
        if (sender is TextBox tb && !string.IsNullOrEmpty(tb.Text))
        {
            var upper = tb.Text.ToUpperInvariant();
            if (upper != tb.Text)
            {
                var caret = tb.CaretIndex;
                tb.Text = upper;
                tb.CaretIndex = caret;
            }
        }
    }
}