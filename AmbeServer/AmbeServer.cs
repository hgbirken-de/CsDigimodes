using DigitalVoice.AmbeSupport;
using DigitalVoice.AudioSupport;
using DigitalVoice.Common;
using NLog;
using System.Net;
using System.Net.Sockets;
using YamlDotNet.Serialization;

namespace AmbeServer;

/// <summary>
/// AMBE server.
/// </summary>
public class AmbeServer : IDisposable
{
    static readonly Logger logger = LogManager.GetCurrentClassLogger();

    static readonly string _resourceFile = "AmbeServerConfig.yaml";
    
    readonly Ambe3000RController _dv3000Controller;

    readonly string _host = "127.0.0.1";
    readonly int _port = 2460;
    readonly string _serialPort = "undefined";

    readonly bool _recordPacket = false; // inbound packets
    readonly bool _recordPcm = false;

    readonly PacketRecorder? _packetRecorder;
    volatile bool _runFlag = true;

    readonly bool isLittleEndian = BitConverter.IsLittleEndian;

    readonly Socket _socket;
    EndPoint _remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);
    readonly Thread? _stickReaderThread;

    readonly WavPcmRecorder? _wavPcmRecorder;

    readonly static byte[] _receiveBuffer = new byte[512];


    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="cfg">Optional config parameter.</param>
    public AmbeServer(AmbeServerConfig? cfg)
    {
        // Configuration
        var config = cfg ?? LoadConfigFromResource();
        _host = config.Host;
        _port = config.Port;
        _recordPacket = config.RecordPacket;
        _recordPcm = config.RecordPcm;
        _serialPort = config.SerialPort;

        // Set up the packet recorder       
        if (_recordPacket)
        {
            var pktFile = Path.Combine("data", $"ambe_server_packets_{DateTime.Now:yyyyMMddHHmmss}.bin");
            logger.Info($"recording packets to: {pktFile}");
            Directory.CreateDirectory("data");
            _packetRecorder = new PacketRecorder(pktFile, FileMode.Create, FileAccess.Write);
        }

        // Test
        if (_recordPcm)
        {
            var wavFile = Path.Combine("data", $"ambe_server_packets_{DateTime.Now:yyyyMMddHHmmss}.wav");
            _wavPcmRecorder = new(wavFile);
            logger.Info($"recording PCM to: {wavFile}");
        }
        
        // Set up the DV3000 stick controller
        _dv3000Controller = new Ambe3000RController(_serialPort);
        _dv3000Controller.Open();
        logger.Info($"AMBE Product ID:  {_dv3000Controller.GetProductId()}, Version: {_dv3000Controller.GetVersion()}, Serial Port: {_serialPort}");

        // Thread to process stick responses
        _stickReaderThread = new Thread(ReadStickResponse) { IsBackground = true };
        _stickReaderThread.Start();

        // Set up the server socket
        _socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        _socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
        _socket.Bind(new IPEndPoint(IPAddress.Any, _port));

        BeginReceive();
    }

    private static bool IsChannelPacket(byte[] packet) => packet.Length > 6 && packet[0] == 0x61 && packet[3] == 0x01;
    private static bool IsSpeechPacket(byte[] packet) => packet.Length > 6 && packet[0] == 0x61 && packet[3] == 0x02;


    private void BeginReceive()
    {
        try
        {
            _socket.BeginReceiveFrom(_receiveBuffer, 0, _receiveBuffer.Length, SocketFlags.None, ref _remoteEndPoint, new AsyncCallback(ReceiveCallback), null);
        }
        catch (Exception ex)
        {
            logger.Error(ex);
        }
    }

    private void ReceiveCallback(IAsyncResult ar)
    {
        try
        {
            int bytesRcvd = _socket.EndReceiveFrom(ar, ref _remoteEndPoint);

            // TODO: here it needs a length check!
            if (_receiveBuffer[0] == 0x61 && bytesRcvd > 4)
            { 
                byte[] packet = new byte[bytesRcvd];
                Buffer.BlockCopy(_receiveBuffer, 0, packet, 0, bytesRcvd);
                if (logger.IsDebugEnabled)
                    logger.Debug($"data rcvd: {Convert.ToHexString(packet)}");
                _dv3000Controller?.SendPacket(packet);
                if (_recordPacket)
                {
                    _packetRecorder!.WritePacket(packet);
                }
            }
            else
            {
                logger.Error($"Invalid packet received: len={bytesRcvd} {Convert.ToHexString(_receiveBuffer, 0, bytesRcvd)} - packet ignored");
            }
        }
        catch (Exception ex)
        {
            logger.Error(ex);
        }
        finally 
        {
            // Continue receiving (re-arm the socket even in case of an exception)
            BeginReceive();
        }
    }

    /// <summary>
    /// Helper method to load a config file (yaml format).
    /// </summary>
    /// <param name="filePath">The path of the config file.</param>
    /// <returns></returns>
    public static AmbeServerConfig LoadConfig(string filePath)
    {
        var deserializer = new DeserializerBuilder()
            //.WithNamingConvention(CamelCaseNamingConvention.Instance)
            //.IgnoreUnmatchedProperties()
            .Build();

        var yamlContent = File.ReadAllText(filePath);
        return deserializer.Deserialize<AmbeServerConfig>(yamlContent);
    }

    /// <summary>
    /// Helper method to load a config file as resource (yaml format).
    /// </summary>
    /// <returns></returns>
    public static AmbeServerConfig LoadConfigFromResource()
    {
        var assembly = typeof(AmbeServer).Assembly;

        string? ns = typeof(AmbeServer).Namespace;
        if (string.IsNullOrEmpty(ns))
            throw new InvalidOperationException("Namespace of AmbeServer is null or empty.");

        string resourceName = $"{ns}.{_resourceFile}";

        using Stream? stream = assembly.GetManifestResourceStream(resourceName) ?? throw new FileNotFoundException($"Embedded resource not found: {resourceName}");
        using var reader = new StreamReader(stream);
        var yamlContent = reader.ReadToEnd();

        var deserializer = new DeserializerBuilder().Build();

        return deserializer.Deserialize<AmbeServerConfig>(yamlContent);
    }

    public void Dispose()
    {
        Shutdown();
    }

    /// <summary>
    /// Method that handles the DV3000 stick response. 
    /// </summary>
    private void ReadStickResponse()
    {
        while (_runFlag)
        {
            try
            {
                byte[]? data = _dv3000Controller.ReceivePacket();
                if (data == null)
                {
                    //logger.Warn("timeout");
                    continue;
                }
                if (data[0] == 0x61 && data.Length >= 5)
                {
                    if (logger.IsDebugEnabled) 
                        logger.Debug($"stick data: {Convert.ToHexString(data)}");

                    _socket.SendTo(data, _remoteEndPoint); // always send back raw data rcvd from stick
                    
                    if (_recordPcm && IsSpeechPacket(data)) // change speech packet only after sending to UPD remote client!!
                    {
                        ConvertPcmToLittleEndian(data);
                        _wavPcmRecorder?.WritePcm(data, 6, data.Length - 6);
                    }
                }
                else 
                {
                    logger.Error($"Invalid stick response: len={data.Length} {Convert.ToHexString(data)}");
                }
                
            }
            catch (TimeoutException)
            {
                // is OK, no data
            }
            catch (Exception ex)
            {
                logger.Error("Wie sind hier ...");
                logger.Error(ex, "Unhandled exception");
            }
        }
    }


    /// <summary>
    /// Converts 16-bit PCM samples in a byte array from big endian to little endian format in-place.
    /// 
    /// The conversion starts at offset 6, assuming the first 6 bytes are a protocol header.
    /// Each 16-bit sample is byte-swapped to match little endian format, which is expected
    /// by standard audio interfaces on little-endian systems.
    /// </summary>
    /// <param name="pcm">The byte array containing PCM audio data with a 6-byte header (326 bytes).</param>
    /// <exception cref="ArgumentException">Throw if argument length is not 326 bytes</exception>
    private static void ConvertPcmToLittleEndian(byte[] pcm)
    {
        if (pcm == null) return;
        if (pcm.Length != 326)
        {
            throw new ArgumentException($"Invalid PCM argument, expected 326 bytes, got {pcm.Length} bytes: {Convert.ToHexString(pcm)}");
        }
        for (int i = 6; i < pcm.Length; i += 2)
        {
            int i1 = i + 1;
            (pcm[i1], pcm[i]) = (pcm[i], pcm[i1]); // swap to little endian format
        }
    }

    /// <summary>
    /// Shut down this AMBE server instance.
    /// </summary>
    public void Shutdown()
    {
        _runFlag = false;
        _socket.Dispose();
        _dv3000Controller?.Close();
        _packetRecorder?.Close();
        _wavPcmRecorder?.Close();
    }
}