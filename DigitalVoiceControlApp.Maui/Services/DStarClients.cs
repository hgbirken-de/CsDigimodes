using DigitalVoice.AmbeSupport;
using DigitalVoice.AudioSupport;
using DigitalVoice.Common;
using DigitalVoice.DStar.Common;
using DigitalVoice.DStar.Dcs;
using DigitalVoice.DStar.Ref;
using DigitalVoice.DStar.Xrf;
using DigitalVoiceControlApp.Maui.Config;

namespace DigitalVoiceControlApp.Maui.Services;

/// <summary>Das, was die App von einem D-STAR-Client (DCS, REF oder XRF) braucht: starten, stoppen, senden.</summary>
public interface IDStarClient
{
    void Start();
    void Stop();
    void StartStopTransmit(bool on);
}

/// <summary>
/// Legt den D-STAR-Client zum Mode an (Gegenstück zu <c>StartStopDcs/Ref/Xrf</c> der Avalonia-MainViewModel). DcsClient,
/// RefClient und XrfClient haben keine gemeinsame Basisklasse, deshalb stehen sie hinter <see cref="IDStarClient"/>; die
/// Namensräume der drei Clients bleiben so in dieser einen Datei und berühren das große ViewModel nicht.
/// </summary>
public static class DStarClients
{
    private sealed class DelegateClient(Action start, Action stop, Action<bool> transmit) : IDStarClient
    {
        public void Start() => start();
        public void Stop() => stop();
        public void StartStopTransmit(bool on) => transmit(on);
    }

    /// <param name="mode">DCS, REF oder XRF.</param>
    /// <param name="us">Die Einstellungen (Reflektor, Modul, Port, Nachricht, Rufzeichen).</param>
    /// <param name="onData">Der Client meldet hier seine Daten (für jeden Frame).</param>
    /// <param name="onNetMessage">Statusmeldungen des Netzwerks (Verbindungsaufbau, Fehler).</param>
    /// <param name="error">Der Grund, wenn kein Client angelegt werden konnte.</param>
    /// <returns>Der Client (noch nicht gestartet) oder <c>null</c>.</returns>
    public static IDStarClient? Create(Mode mode, UserSettings us, IAmbe3000RController ambe, IMicrophoneReader? mic,
        IAudioPlayer? player, Action<DStarSessionContext> onData, Action<string> onNetMessage, out string? error)
    {
        error = null;

        switch (mode)
        {
            case Mode.Dcs:
            {
                string reflector = us.Dcs.LastReflector;
                string? address = DcsHosts.GetAddress(reflector);
                if (string.IsNullOrEmpty(address))
                {
                    error = $"DCS reflector '{reflector}' is unknown.";
                    return null;
                }

                DcsClientConfig cfg = new()
                {
                    AmbeController = ambe,
                    RefAddress = address,
                    RefPort = us.Dcs.HostPort,
                    RefName = reflector,
                    Module = us.Dcs.LastModule,
                    Callsign = us.Common.Callsign,
                    MicrophoneReader = mic,
                    AudioPlayer = player,
                    RecordRcvdUdpPackets = false,
                    UserMessage = us.Dcs.UserMessage,
                };
                var client = new DcsClient(cfg) { ExternalDcsDataConsumer = d => onData(d), ExternalNetMsgConsumer = m => onNetMessage(m) };
                return new DelegateClient(client.Start, client.Stop, client.StartStopTransmit);
            }

            case Mode.Ref:
            {
                string reflector = us.Ref.LastReflector;
                string? address = DPlusHostRepository.GetAddress(reflector);
                if (string.IsNullOrEmpty(address))
                {
                    error = $"REF reflector '{reflector}' is unknown.";
                    return null;
                }

                RefClientConfig cfg = new()
                {
                    AmbeController = ambe,
                    RefAddress = address,
                    RefPort = us.Ref.HostPort,
                    RefName = reflector,
                    Module = us.Ref.LastModule,
                    Callsign = us.Common.Callsign,
                    MicrophoneReader = mic,
                    AudioPlayer = player,
                };
                var client = new RefClient(cfg) { ExternalRefDataConsumer = d => onData(d), ExternalNetMsgConsumer = m => onNetMessage(m) };
                return new DelegateClient(client.Start, client.Stop, client.StartStopTransmit);
            }

            case Mode.Xrf:
            {
                string reflector = us.Xrf.LastReflector;
                string? address = XrfHosts.GetAddress(reflector);
                if (string.IsNullOrEmpty(address))
                {
                    error = $"XRF reflector '{reflector}' is unknown.";
                    return null;
                }

                XrfClientConfig cfg = new()
                {
                    AmbeController = ambe,
                    RefAddress = address,
                    RefPort = us.Xrf.HostPort,
                    RefName = reflector,
                    Module = us.Xrf.LastModule,
                    Callsign = us.Common.Callsign,
                    MicrophoneReader = mic,
                    AudioPlayer = player,
                    UserMessage = us.Xrf.UserMessage,
                };
                var client = new XrfClient(cfg) { ExternalXrfDataConsumer = d => onData(d), ExternalNetMsgConsumer = m => onNetMessage(m) };
                return new DelegateClient(client.Start, client.Stop, client.StartStopTransmit);
            }

            default:
                error = $"{mode} is not a D-STAR mode.";
                return null;
        }
    }
}
