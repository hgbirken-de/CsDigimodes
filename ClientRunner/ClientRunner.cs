
using DigitalVoice.AmbeSupport;
using DigitalVoice.Common;
using DigitalVoice.Config;
using DigitalVoice.Dmr;
using DigitalVoice.DStar.Dcs;
using DigitalVoice.DStar.Ref;
using DigitalVoice.DStar.Xrf;
using DigitalVoice.Fusion;
using DigitalVoice.Nxdn;
using NLog;

namespace YsfClientRunner;

/// <summary>
/// Helper class to run the DCS-, DMR-, FCS-, and YSF-Clients.
/// </summary>
class ClientRunner
{
    static readonly Logger logger = LogManager.GetCurrentClassLogger();

    static DcsClient? dcsClient;
    static DmrClient1? dmrClient1;
    static DmrClient2? dmrClient2;
    static FcsClient? fcsClient;
    static NxdnClient? nxdnClient;
    static RefClient? refClient;
    static XrfClient? xrfClient;
    static YsfClient? ysfClient;

    static readonly ManualResetEventSlim quitEvent = new(false);

    static void Main(string[] args)
    {
        DmrProtocol dmrProtocol = DmrProtocol.MmdvmHost;
        Mode clientType = Mode.Dmr;         // <====  S E L E C T  C L I E N T  H E R E

        AppDomain.CurrentDomain.ProcessExit += (sender, e) =>
        {
            Console.WriteLine("Process exit event triggered...");
        };

        Console.WriteLine($"Running {clientType}Client. Press Ctrl+C to stop.");

        string dataDir = "data";
        if (!Directory.Exists(dataDir))
        {
            Directory.CreateDirectory(dataDir);
        }

        switch (clientType)
        {
            case Mode.Dcs:
                RunDcsClient();
                break;
            case Mode.Dmr:
                RunDmrClient(dmrProtocol);
                break;
            case Mode.Fcs:
                RunFcsClient();
                break;
            case Mode.Nxdn:
                RunNxdnClient();
                break;
            case Mode.Ref:
                RunRefClient();
                break;
            case Mode.Xrf:
                RunXrfClient();
                break;
            case Mode.Ysf:
                RunYsfClient();
                break;
        }

        Console.CancelKeyPress += (sender, e) =>
        {
            Console.WriteLine("Ctrl+C pressed, exiting...");   
            quitEvent.Set();
            e.Cancel = true; // prevents immediate termination, lets us clean up
        };

        // Wait here until Ctrl+C pressed
        quitEvent.Wait();

        switch (clientType)
        {
            case Mode.Dcs:
                dcsClient?.Stop();
                break;
            case Mode.Dmr:
                if (dmrProtocol == DmrProtocol.Homebrew)
                    dmrClient1?.Stop();
                else
                    dmrClient2?.Stop();
                break;
            case Mode.Fcs:
                fcsClient?.StopClient();
                break;
            case Mode.Nxdn:
                nxdnClient?.StopClient();
                break;
            case Mode.Ref:
                refClient?.Stop();
                break;
            case Mode.Xrf:
                xrfClient?.Stop();
                break;
            case Mode.Ysf:
                ysfClient?.StopClient();
                break;
        }
        Thread.Sleep(2000); // wait for client to clean up
        Console.WriteLine("Cleanup done. Bye!");
    }

   
    /// <summary>
    /// Run the DCS client.
    /// </summary>
    static void RunDcsClient()
    {  
        string refName = "DCS001";
        string? refAddr = DcsHosts.GetAddress(refName);
        if (refAddr == null) ArgumentException.ThrowIfNullOrEmpty(refAddr, nameof(refAddr));
        DcsClientConfig cfg = new()
        {
            RefAddress = refAddr,
            RefPort = 30051,
            RefName = refName,
            Module = 'C',
            Callsign = "DL1HGB",
            AudioPlayer = new(),
            RecordRcvdUdpPackets = true,
            //SimulationFile = "./data/DcsClient_packets_20250912100514.bin",
            SimulationFile = "C:/Users/hgbir/AppData/Roaming/DigitalVoiceControl/Dcs/Data/Dcs_packets_20250919153621_gps.bin",
            SimulationMode = true,
        };
        dcsClient = new(cfg);
        dcsClient.Start();
    }

    /// <summary>
    /// Run the DMR client.
    /// </summary>
    /// <param name="dmrProtocol"Defines which DMR protocol to use.</param>
    static void RunDmrClient(DmrProtocol dmrProtocol)
    {
        var us = UserSettings.Instance();
        DmrClientConfig cfg = new()
        {
            AmbeServiceType = AmbeServiceType.Stick,
            AmbeServerAddr = "127.0.0.1",
            AmbeServerPort = 2460,
            AmbeStickComport = "COM13",
            BmServerAddress = "master1.bm262.de",
            BmServerPort = 62030,
            Password = us.Dmr.Password,
            MyDmrId = 2622363,
            EssId = 15,
            RecordAudio = false,
            RecordDmrPackets = false,
            RecordDmrPacketsFile = Path.Combine("data", $"DmrClient1_packets_{DateTime.Now:yyyyMMddHHmmss}.bin"),
            SimulationMode = true,
            AudioPlayer = new(),
        };
        switch (dmrProtocol)
        {
            case DmrProtocol.Homebrew:
                cfg.BmServerAddress = "master1.bm262.de";
                cfg.BmServerPort = 62030;

                cfg.SimulationModeFile = "C:\\Users\\hgbir\\AppData\\Roaming\\PyDigimodes\\Dmr\\Data\\dmr1_recorded_packets_20251116194218.bin";
                dmrClient1 = new(cfg);
                dmrClient1.Start();
                break;
            case DmrProtocol.MmdvmHost:
                string designator = "BM_2621_Germany";
                var (_, Host, Port, _) = DmrHosts.GetHostInfo(designator);
                cfg.BmServerAddress = Host;
                cfg.BmServerPort = Port;
                cfg.SimulationModeFile = "C:\\Users\\hgbir\\AppData\\Roaming\\DigitalVoiceControl\\Dmr\\Data\\DmrClient2_packets_20250815100925.bin";

                dmrClient2 = new(cfg);
                dmrClient2.Start();
                break;
        }
    }

    /// <summary>
    /// Run the FCS client
    /// </summary>
    static void RunFcsClient()
    {
        string? simulationFile = @"C:\Users\hgbir\AppData\Roaming\DigitalVoiceControl\Fcs\Data\fcs_packets_20260108125139.bin";

        FcsClientConfig cfg = new()
        {
            ReflectorAddress = "fcs004.xreflector.net",
            ReflectorPort = 62500,
            ReflectorId = "FCS00428",
            Callsign = "DL1HGB",
            Locator = "JO54OL",
            HotspotType = "MMDVM",
            RxFrequency = 434300000,
            TxFrequency = 434300000,
            AudioPlayer = new(),

            RecordAudio = false,
            RecordAudioFile = Path.Combine("data", $"fcs_audio_{DateTime.Now:yyyyMMddHHmmss}.wav"),

            RecordFcsPackets = true,
            RecordFcsPacketsFile = Path.Combine("data", $"fcs_packets_{DateTime.Now:yyyyMMddHHmmss}.bin"),

            SimulationFile = simulationFile,
            SimulationMode = true,
        };

        fcsClient = new(cfg);
        fcsClient.StartClient();
    }

    /// <summary>
    /// Run the NXDN client
    /// </summary>
    static void RunNxdnClient()
    {
        string? simulationFile = @"C:\Users\hgbir\AppData\Roaming\DigitalVoiceControl\Fcs\Data\fcs_packets_20260108125139.bin";

        int id = 20421;
        var (Host, Port) = NxdnHosts.GetHostInfo(id);
        if (string.IsNullOrEmpty(Host))
            throw new ArgumentException($"Unable to find host info for id={id}");

        NxdnClientConfig cfg = new()
        {
            NxdnReflectorAddr = Host,
            NxdnReflectorPort = Port,
            NxdnReflectorId = id,
            Callsign = "DL1HGB",
            MyNxdnId = 39251,
            AudioPlayer = new(),

            RecordAudio = false,
            RecordAudioFile = Path.Combine("data", $"nxdn_audio_{DateTime.Now:yyyyMMddHHmmss}.wav"),

            RecordRxPackets = true,
            RecordRxPacketsFile = Path.Combine("data", $"nxdn_packets_{DateTime.Now:yyyyMMddHHmmss}.bin"),

            SimulationModeFile = simulationFile,
            SimulationMode = true,
        };

        nxdnClient = new(cfg);
        nxdnClient.StartClient();
    }

    /// <summary>
    /// Run the REF client.
    /// </summary>
    static void RunRefClient()
    {
        string refName = "REF001";
        string? refIpAddr = DPlusHostRepository.GetAddress(refName);
        if (refIpAddr == null) ArgumentException.ThrowIfNullOrEmpty(refIpAddr, nameof(refIpAddr));
        RefClientConfig cfg = new()
        {
            RefAddress = refIpAddr,
            RefPort = 20001,
            RefName = refName,
            Module = 'C',
            Callsign = "DL1HGB",
            AudioPlayer = new(),
            RecordRefPackets = true,
            SimulationFile = null,
            SimulationMode = false,
        };
        refClient = new(cfg);
        refClient.Start();
    }

    /// <summary>
    /// Run the YSF client
    /// </summary>
    static void RunYsfClient()
    {
        List<string> ysf_designators = [
        "local" //  0 local test
        ,"26200" //  1 DE-Deutschland    (7)
        ,"07588" //  2 DE-DL-Hamburg     (9)
        ,"54919" //  3 DE-DL-NORDWEST    (10)
        ,"26538" //  4 DE-Nordharz       (21)
        ,"26201" //  5 DE-C4FM-Germany   (6)
        ,"62829" //  6 DE-DE-Germany     (?)
        ,"00260" //  7 DE-AFu-Kiel       (2)
        ,"13100" //  8 DE-Lausitz        (19)
        ,"26285" //  9 DE-Oberbayern     (24)
        ,"74154" // 10 Pegasus           (26)
        ,"26444" // 11 DE-Inselfreunde   (17)
        ,"32592" // 12 US-America-Link
        ];
        int ysf_designator_selector = 2; // set to correct index to select a YSF designator

        (string _, string _, string Host, int Port, string _) = YsfHosts.GetHostInfo(ysf_designators[ysf_designator_selector]);
        ArgumentException.ThrowIfNullOrEmpty(Host, nameof(Host));

        //string? simulationFile = @"C:\Users\hgbir\VisualStudio_Source\repos\YSF\YsfClientRunner\bin\Debug\net8.0\data\YsfClient_packets_20250815153315.bin";
        string? simulationFile = "C:/Users/hgbir/AppData/Roaming/DigitalVoiceControl/Ysf/Data/ysf_packets_20251002074928.bin";


        YsfClientConfig cfg = new()
        {
            ReflectorAddress = Host,
            ReflectorPort = Port,
            Callsign = "DL1HGB",
            Town = "Todendorf, Fehmarn",
            Locator = "JO54OL",
            HotspotType = "BY DL1HGB",
            RxFrequency = 434300000,
            TxFrequency = 434300000,

            RecordAudio = true,
            RecordAudioFile = Path.Combine("data", $"ysf_audio_{DateTime.Now:yyyyMMddHHmmss}.wav"),
            
            RecordYsfPackets = true,
            RecordYsfPacketsFile = Path.Combine("data", $"ysf_packets_{DateTime.Now:yyyyMMddHHmmss}.bin"),

            SimulationFile = simulationFile,
            SimulationMode = true,

            AudioPlayer = new(),
        };

        ysfClient = new(cfg);
        ysfClient.StartClient();
    }

    /// <summary>
    /// Run the XRF client.
    /// </summary>
    static void RunXrfClient()
    {
        string refName = "XRF021";
        string? refIpAddr = XrfHosts.GetAddress(refName);
        if (refIpAddr == null) ArgumentException.ThrowIfNullOrEmpty(refIpAddr, nameof(refIpAddr));
        XrfClientConfig cfg = new()
        {
            RefAddress = refIpAddr,
            RefPort = 30001,
            RefName = refName,
            Module = 'B',
            Callsign = "DL1HGB",
            AudioPlayer = new(),
            RecordRefPackets = true,
            SimulationFile = null,
            SimulationMode = false,
        };
        xrfClient = new(cfg);
        xrfClient.Start();
    }

    static async void GetFcsReflectorInfo()
    {
        List<FcsReflector> li = await FcsReflectorListFetcher.FetchFcsReflectorsAsync();
    }
}