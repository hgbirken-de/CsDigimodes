using NLog;
using System.IO.Ports;
using System.Text;

namespace AmbeServer;

public static  class Serial
{
    static readonly Logger logger = LogManager.GetCurrentClassLogger();

    public static string PortName { get; set; } = "COM9";

    public static int BaudRate { get; set; } = 420300;


    private static bool _ambe6000R_found = false;


    private static SerialPort? _serialPort1;

    private static bool _running = false;

    private static int ser3_in_buf_lenP = 330;

    private static byte[] serial3_bufferP = new byte[Serial.ser3_in_buf_lenP + 1];

    private static int serial3_buffer_input_pointerP = 0;

    public static string StatusText { get; private set; }

    public static event EventHandler StatusTextChanged;

    public static bool IsOpen()
    {
        return _serialPort1 != null && _serialPort1.IsOpen;
    }

    // Token: 0x0600001D RID: 29 RVA: 0x000030D0 File Offset: 0x000012D0
    public static bool Open()
    {
        Serial._serialPort1 ??= new SerialPort();

        if (string.IsNullOrEmpty(PortName))
        {
            Serial._serialPort1.Close();
            return false;
        }
        try
        {
            if (!Serial._serialPort1.IsOpen)
            {
                Serial._serialPort1.PortName = PortName;
                Serial._serialPort1.BaudRate = BaudRate;
                Serial._serialPort1.DataBits = 8;
                Serial._serialPort1.Parity = Parity.None;
                Serial._serialPort1.StopBits = StopBits.One;
                Serial._serialPort1.Handshake = Handshake.None;
                Serial._serialPort1.RtsEnable = false;
                Serial._serialPort1.DtrEnable = true;
                Serial._serialPort1.ReadTimeout = 500;
                Serial._serialPort1.WriteTimeout = -1;
                Serial._serialPort1.WriteBufferSize = 512;
                Serial._serialPort1.ReadBufferSize = 512;
                Serial._serialPort1.Open();
                Serial._running = true;
                new Thread(new ThreadStart(Serial.DataReceived)).Start();
            }
            return true;
        }
        catch (Exception)
        {
        }
        return true;
    }

    private static void DataReceived()
    {
        while (Serial._running)
        {
            if (IsOpen())
            {
                try
                {
                    int bytesToRead = _serialPort1!.BytesToRead;
                    if (bytesToRead > 0)
                    {
                        byte[] array = new byte[bytesToRead];
                        try
                        {
                            _serialPort1.Read(array, 0, bytesToRead);
                            for (int i = 0; i < array.Length; i++)
                            {
                                DecodeAmbe(array[i]);
                            }
                        }
                        catch (TimeoutException) {}
                    }
                }
                catch (Exception)
                {
                }
            }
            Thread.Sleep(1);
        }
    }

    public static void Write(byte[] data)
    {
        if (IsOpen())
        {
            try
            {
                logger.Debug($"To AMBE: {BitConverter.ToString(data)}");
                _serialPort1!.Write(data, 0, data.Length);
            }
            catch (TimeoutException) { }
            catch (Exception) { }
        }
    }

    public static void Close()
    {
        if (IsOpen())
        {
            _running = false;
            _serialPort1!.Close();
        }
    }

    private static void ChangeAmbeStatusText(string text)
    {
        Serial.StatusText = text;
        Serial.StatusTextChanged?.Invoke(null, EventArgs.Empty);
    }

    public static void GetAmbeProductId()
    {
        Write([0x61, 0, 1, 0, 0x30]);
    }

    public static void SetParityOff()
    {
        Write([0x61, 0, 4, 0, 0x3F, 0, 0x2F, 0x14]);
    }

    public static void SetAmbePacketMode()
    {
        Write([0x61, 0, 7, 0, 0x34, 5, 0, 0, 7, 0, 0]);
    }

    //public static void DecodeAmbe(byte ambeData)
    //{
    //    Serial.serial3_bufferP[Serial.serial3_buffer_input_pointerP] = ambeData;
    //    Serial.serial3_buffer_input_pointerP++;
    //    if (Serial.serial3_buffer_input_pointerP > Serial.ser3_in_buf_lenP)
    //    {
    //        Serial.serial3_buffer_input_pointerP = 0;
    //    }
    //    if ((Serial.serial3_bufferP[0] != 97) & (Serial.serial3_bufferP[0] != 0 || Serial.serial3_bufferP[0] != 1))
    //    {
    //        Serial.serial3_buffer_input_pointerP = 0;
    //    }
    //    if (Serial.serial3_bufferP[0] == 97 && Serial.serial3_buffer_input_pointerP >= 3)
    //    {
    //        int num = ((int)(byte.MaxValue & Serial.serial3_bufferP[1]) << 8) | (int)(byte.MaxValue & Serial.serial3_bufferP[2]);
    //        if (Serial.serial3_buffer_input_pointerP >= num + 4 && Serial.serial3_buffer_input_pointerP >= 2)
    //        {
    //            int num2 = ((int)(byte.MaxValue & Serial.serial3_bufferP[1]) << 8) | (int)(byte.MaxValue & Serial.serial3_bufferP[2]);
    //            byte[] array = new byte[num2 + 4];
    //            Buffer.BlockCopy(Serial.serial3_bufferP, 0, array, 0, num2 + 4);
    //            logger.Debug("From AMBE: " + BitConverter.ToString(array));
    //            switch (array[3])
    //            {
    //                case 0:
    //                    {
    //                        byte b = Serial.serial3_bufferP[4];
    //                        if (b != 48)
    //                        {
    //                            if (b != 54)
    //                            {
    //                            }
    //                        }
    //                        else
    //                        {
    //                            byte[] array2 = new byte[num2 - 2];
    //                            Buffer.BlockCopy(Serial.serial3_bufferP, 5, array2, 0, num2 - 2);
    //                            if (!information.AMBE6000R_FOUND)
    //                            {
    //                                Serial.DetAMBEpacketmode();
    //                                Thread.Sleep(150);
    //                                UDPServerTest.Start();
    //                                information.AMBE6000R_FOUND = true;
    //                                Serial.ChangeAMBEStatusText(Encoding.UTF8.GetString(array2));
    //                            }
    //                        }
    //                        UDPServerTest.SendTo(array);
    //                        break;
    //                    }
    //                case 1:
    //                    if (num2 != 8 || Serial.serial3_bufferP[4] != 1)
    //                    {
    //                        UDPServerTest.SendTo(array);
    //                    }
    //                    break;
    //                case 2:
    //                    UDPServerTest.SendTo(array);
    //                    break;
    //            }
    //            Serial.serial3_buffer_input_pointerP = 0;
    //            Serial.serial3_bufferP[0] = 0;
    //        }
    //    }
    //}

    public static void DecodeAmbe(byte incomingByte)
    {
        // Append byte into buffer
        serial3_bufferP[serial3_buffer_input_pointerP++] = incomingByte;

        // Overflow protection — reset everything
        if (serial3_buffer_input_pointerP >= ser3_in_buf_lenP)
        {
            ResetBuffer();
            return;
        }

        // Not enough bytes yet to determine header
        if (serial3_buffer_input_pointerP < 1)
            return;

        byte header = serial3_bufferP[0];

        // -------------------------------------
        // Validate header byte
        // AMBE packets may start with: 97, 0, 1
        // -------------------------------------
        if (header != 0x61 && header != 0 && header != 1)
        {
            ResetBuffer();
            return;
        }

        // -------------------------------------
        // To check packet length, need 3 bytes
        // Byte 1+2 = payload length (big endian)
        // -------------------------------------
        if (serial3_buffer_input_pointerP < 3)
            return;

        int lengthField = (serial3_bufferP[1] << 8) | serial3_bufferP[2];
        int fullPacketLength = lengthField + 4;  // header + len(2) + cmd + payload

        // Not enough data yet — wait for more bytes
        if (serial3_buffer_input_pointerP < fullPacketLength)
            return;

        // -------------------------------------
        // Complete AMBE packet received
        // -------------------------------------
        byte[] packet = new byte[fullPacketLength];
        Buffer.BlockCopy(serial3_bufferP, 0, packet, 0, fullPacketLength);

        logger.Debug($"From AMBE stick: {BitConverter.ToString(packet)}");

        // -------------------------------------
        // Dispatch based on CMD byte
        // packet[3] = commandType
        // -------------------------------------
        byte cmd = packet[3];

        switch (cmd)
        {
            case 0: // product ID / device config etc.
                HandleCommand0(packet);
                break;

            case 1:
            case 2:
                //UDPServerTest.SendTo(packet);
                break;

            default:
                // unknown command -> send anyway?
                //UDPServerTest.SendTo(packet);
                break;
        }

        // Reset buffer for next packet
        ResetBuffer();
    }

    private static void HandleCommand0(byte[] packet)
    {
        int lengthField = (packet[1] << 8) | packet[2];
        byte subCmd = packet[4];

        if (subCmd == 0x30) // product string?
        {
            byte[] textBytes = new byte[lengthField - 2];
            Buffer.BlockCopy(packet, 5, textBytes, 0, textBytes.Length);

            string text = Encoding.UTF8.GetString(textBytes);

            if (!_ambe6000R_found)
            {
                SetAmbePacketMode();
                Thread.Sleep(150);
                //UDPServerTest.Start();
                _ambe6000R_found = true;
                Serial.ChangeAmbeStatusText(text);
            }
        }

        // Forward all command-0 packets
        //UDPServerTest.SendTo(packet);
    }


    private static void ResetBuffer()
    {
        serial3_buffer_input_pointerP = 0;
        serial3_bufferP[0] = 0;
    }
}