using NLog;

namespace DigitalVoiceControlApp.Maui;

/// <summary>Ein Eintrag der Auswahlliste. <see cref="IsCurrent"/> hebt den aktuell gewählten hervor.</summary>
public sealed record SelectionItem(string Text, bool IsCurrent);

/// <summary>
/// Auswahlseite für lange Listen (Talkgroups, Reflektoren): ein Suchfeld filtert die Liste beim Tippen, der aktuelle Eintrag
/// ist hervorgehoben und wird beim Öffnen in die Mitte gescrollt. Ein Tipp auf einen Eintrag meldet ihn über den Rückruf und
/// schließt die Seite. Ersetzt den Picker von MAUI, der unter Android die Auswahl nicht anzeigt (und keine Suche hat).
/// </summary>
public partial class SelectionPage : ContentPage
{
    private static readonly Logger Log = LogManager.GetCurrentClassLogger();

    private readonly List<string> _all;
    private readonly string? _current;
    private readonly Action<string> _onSelected;

    private List<SelectionItem> _shown = [];
    private bool _closing;

    /// <param name="title">Titel der Seite.</param>
    /// <param name="items">Alle wählbaren Einträge.</param>
    /// <param name="current">Der aktuell gewählte Eintrag (oder <c>null</c>).</param>
    /// <param name="onSelected">Wird mit dem gewählten Eintrag aufgerufen (UI-Thread), bevor die Seite schließt.</param>
    public SelectionPage(string title, IEnumerable<string> items, string? current, Action<string> onSelected)
    {
        InitializeComponent();

        Title = title;
        _all = items.ToList();
        _current = string.IsNullOrEmpty(current) ? null : current;
        _onSelected = onSelected;

        ShowItems(_all);
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        // Die Liste ist beim Öffnen noch nicht fertig gelegt: ScrollTo zu früh hat keine Wirkung. Deshalb zweimal, kurz
        // verzögert (der zweite Versuch fängt langsame Geräte ab; ein erneutes Scrollen an dieselbe Stelle schadet nicht).
        Dispatcher.DispatchDelayed(TimeSpan.FromMilliseconds(150), ScrollToCurrent);
        Dispatcher.DispatchDelayed(TimeSpan.FromMilliseconds(500), ScrollToCurrent);
    }

    private void ShowItems(List<string> names)
    {
        _shown = names.Select(n => new SelectionItem(n, n == _current)).ToList();
        ItemsList.ItemsSource = _shown;
        CountLabel.Text = names.Count == _all.Count ? $"{_all.Count} entries" : $"{names.Count} of {_all.Count} entries";
    }

    /// <summary>Scrollt den aktuellen Eintrag in die Mitte (nur, wenn er in der gezeigten Liste vorkommt).</summary>
    private void ScrollToCurrent()
    {
        if (_closing || _current == null)
            return;

        int index = _shown.FindIndex(i => i.IsCurrent);
        if (index < 0)
            return;

        try
        {
            ItemsList.ScrollTo(index, position: ScrollToPosition.Center, animate: false);
        }
        catch (Exception ex)
        {
            Log.Warn(ex, "Scrolling to the current entry failed.");
        }
    }

    private void OnSearchTextChanged(object? sender, TextChangedEventArgs e)
    {
        string[] words = (e.NewTextValue ?? "").Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        if (words.Length == 0)
        {
            ShowItems(_all);
            Dispatcher.DispatchDelayed(TimeSpan.FromMilliseconds(100), ScrollToCurrent); // wieder zum aktuellen Eintrag
            return;
        }

        ShowItems(_all.Where(n => words.All(w => n.Contains(w, StringComparison.OrdinalIgnoreCase))).ToList());
    }

    private async void OnItemTapped(object? sender, TappedEventArgs e)
    {
        if (_closing || sender is not BindableObject { BindingContext: SelectionItem item })
            return;

        _closing = true; // ein Doppeltipp darf nicht zweimal auswählen
        try
        {
            _onSelected(item.Text);
            await Navigation.PopAsync();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Closing the selection page failed.");
        }
    }
}
