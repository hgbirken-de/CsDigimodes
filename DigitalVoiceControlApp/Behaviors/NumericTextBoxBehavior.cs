using Avalonia.Controls;
using Avalonia.Input;

namespace DigitalVoiceControlApp.Behaviors;

public static class NumericTextBoxBehavior
{
    public static void Attach(TextBox textBox)
    {
        textBox.AddHandler(InputElement.TextInputEvent, OnTextInput, Avalonia.Interactivity.RoutingStrategies.Tunnel);
    }


    private static void OnTextInput(object? sender, TextInputEventArgs e)
    {
        if (!int.TryParse(e.Text, out _))
        {
            e.Handled = true; // Block non-numeric input
        }
    }
}
