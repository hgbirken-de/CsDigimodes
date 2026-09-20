using DigitalVoice.DStar.Common;
using NLog;
using System.Text;

namespace DigitalVoice.DStar.Ref;

internal static class RefCodec
{
    static readonly Logger logger = LogManager.GetCurrentClassLogger();

    static readonly byte[] userMsg = new byte[20];

    internal static byte[] Create(DStarSessionContext state, byte[] ambeData)
    {

        return [];
    }

    static bool sd_sync = true;
    static int sd_txt_seq = 1;
    static int sd_hdr_cnt = 0;
    static int sd_gps_cnt = 0;

    internal static byte[] DecodeVoiceFrame(byte[] buf, DStarSessionContext clientState)
    {
        if (buf.Length != 29)
            throw new ArgumentException($"Invalid length of argument {nameof(buf)}, expecting 29, got {buf.Length}");

        clientState.FrameNo = buf[16];

        // Sync acquisition
        if (buf[16] == 0 && buf[26] == 0x55 && buf[27] == 0x2d && buf[28] == 0x16)
        {
            sd_sync = true;
            sd_txt_seq = 1;
        }

        byte c = (byte)(buf[26] ^ 0x70);
        if (sd_sync && sd_gps_cnt == 0 && (c & 0xF0) == 0x50) // 0x50 indicates header
        {
            sd_hdr_cnt = c & 0x0F;
        }
        else if (sd_hdr_cnt > 0)
        {
            c = 0;
            sd_hdr_cnt = 0;
        }

        // GPS parsing
        //if (sd_gps_cnt == 0 && (c & 0xf0) == 0x30)
        //{
        //    HandleGpsStart(buf, ref c);
        //}
        //else if (sd_gps_cnt != 0 && buf[16] != 0)
        //{
        //    HandleGpsContinue(buf, ref c);
        //    sd_gps_cnt = 0; // reset after handling
        //}

        if (sd_sync)
        {
            // User text state machine
            switch (sd_txt_seq)
            {
                case 1:
                    if (buf[16] == 1 && buf[26] == 0x30) // 0x30 indicates user data
                    {
                        userMsg[0] = (byte)(buf[27] ^ 0x4f);
                        userMsg[1] = (byte)(buf[28] ^ 0x93);
                        sd_txt_seq++;
                    }
                    break;

                case 2:
                    if (buf[16] == 2)
                    {
                        userMsg[2] = (byte)(buf[26] ^ 0x70);
                        userMsg[3] = (byte)(buf[27] ^ 0x4f);
                        userMsg[4] = (byte)(buf[28] ^ 0x93);
                        sd_txt_seq++;
                    }
                    break;

                case 3:
                    if ((buf[16] == 3 || buf[16] == 5) && buf[26] == 0x31)
                    {
                        userMsg[5] = (byte)(buf[27] ^ 0x4f);
                        userMsg[6] = (byte)(buf[28] ^ 0x93);
                        sd_txt_seq++;
                    }
                    break;

                case 4:
                    if (buf[16] == 4 || buf[16] == 6)
                    {
                        userMsg[7] = (byte)(buf[26] ^ 0x70);
                        userMsg[8] = (byte)(buf[27] ^ 0x4f);
                        userMsg[9] = (byte)(buf[28] ^ 0x93);
                        sd_txt_seq++;
                    }
                    break;

                case 5:
                    if ((buf[16] == 5 || buf[16] == 9) && buf[26] == 0x32)
                    {
                        userMsg[10] = (byte)(buf[27] ^ 0x4f);
                        userMsg[11] = (byte)(buf[28] ^ 0x93);
                        sd_txt_seq++;
                    }
                    break;

                case 6:
                    if (buf[16] == 6 || buf[16] == 10)
                    {
                        userMsg[12] = (byte)(buf[26] ^ 0x70);
                        userMsg[13] = (byte)(buf[27] ^ 0x4f);
                        userMsg[14] = (byte)(buf[28] ^ 0x93);
                        sd_txt_seq++;
                    }
                    break;

                case 7:
                    if ((buf[16] == 7 || buf[16] == 13) && buf[26] == 0x33)
                    {
                        userMsg[15] = (byte)(buf[27] ^ 0x4f);
                        userMsg[16] = (byte)(buf[28] ^ 0x93);
                        sd_txt_seq++;
                    }
                    break;

                case 8:
                    if (buf[16] == 8 || buf[16] == 14)
                    {
                        userMsg[17] = (byte)(buf[26] ^ 0x70);
                        userMsg[18] = (byte)(buf[27] ^ 0x4f);
                        userMsg[19] = (byte)(buf[28] ^ 0x93);
                        sd_txt_seq = 0;
                        clientState.RxUsrMsg = Encoding.ASCII.GetString(userMsg, 0, 20);
                    }
                    break;
            }
        }

        return buf[17..26]; // 9 AMBE bytes
    }


    static readonly List<byte> gps_data = [];
    private static void HandleGpsStart(byte[] buf, ref char c)
    {
        sd_gps_cnt = c & 0x0f;
        c = (char)(buf[27] ^ 0x4f);
        ProcessGpsChar(c, buf, 28);
    }

    private static void HandleGpsContinue(byte[] buf, ref char c)
    {
        ProcessGpsChar(c, buf, 27);
        c = (char)(buf[28] ^ 0x93);
        ProcessGpsChar(c, buf, -1);
    }

    private static void ProcessGpsChar(char c, byte[] buf, int nextIndex)
    {
        if (c == 0x0a || c == 0x0d)
        {
            gps_data.Add(0x00);
            sd_gps_cnt = 0;
            logger.Debug("GPS: ", gps_data.ToString());
            if (gps_data[0] == '$') 
                logger.Debug($"{gps_data}");
            gps_data.Clear();
        }
        else
        {
            sd_gps_cnt--;
            gps_data.Add((byte)c);
            if (nextIndex >= 0)
            {
                char next = (char)(buf[nextIndex] ^ (nextIndex == 27 ? 0x4f : 0x93));
                ProcessGpsChar(next, buf, -1);
            }
        }
    }


}