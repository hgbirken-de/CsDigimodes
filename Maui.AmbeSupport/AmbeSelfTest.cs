using DigitalVoice.AmbeSupport;
using System.Diagnostics;
using System.Text;

namespace Maui.AmbeSupport;

/// <summary>
/// Selbsttest für einen geöffneten <see cref="IAmbe3000RController"/> (nutzt nur das Interface, läuft also
/// auch mit dem Windows-Controller). Prüft:
/// <list type="number">
///   <item>ProductId und Version,</item>
///   <item>die DMR-Init-Sequenz (wie <c>DmrClient2.InitDV3000()</c>),</item>
///   <item>Encode: 160 PCM-Samples -> AMBE-Paket (15 Bytes),</item>
///   <item>Decode: AMBE-Paket -> PCM-Paket (326 Bytes = Antwort über mehrere USB-Pakete),</item>
///   <item>die Zeit pro Block (Soll: unter 20 ms).</item>
/// </list>
/// Nicht ausführen, solange ein DMR-Client den Chip benutzt.
/// </summary>
public static class AmbeSelfTest
{
    public static string Run(IAmbe3000RController ctrl, int rounds = 50)
    {
        var sb = new StringBuilder();
        bool ok = true;

        try
        {
            sb.AppendLine($"ProductId: {ctrl.GetProductId() ?? "null"}");
            sb.AppendLine($"Version:   {ctrl.GetVersion() ?? "null"}");

            // 1) Init wie DmrClient2.InitDV3000()
            byte[][] init =
            [
                [0x61, 0x00, 0x01, 0x00, 0x36],
                [0x61, 0x00, 0x07, 0x00, 0x34, 0x05, 0x00, 0x00, 0x07, 0x00, 0x10],
                [0x61, 0x00, 0x03, 0x00, 0x05, 0x10, 0x40],
                [0x61, 0x00, 0x0d, 0x00, 0x0a, 0x04, 0x31, 0x07, 0x54, 0x24, 0x00, 0x00, 0x00, 0x00, 0x00, 0x6f, 0x48],
            ];
            int initOk = 0;
            foreach (byte[] packet in init)
            {
                if (ctrl.SendReceivePacket(packet) != null)
                    initOk++;
            }
            sb.AppendLine($"Init: {initOk}/{init.Length} responses");
            ok &= initOk == init.Length;

            // 2) Encode: 160 Samples Stille
            byte[] speech = new byte[326];
            speech[0] = 0x61; speech[1] = 0x01; speech[2] = 0x42; speech[3] = 0x02; speech[4] = 0x00; speech[5] = 0xA0;

            byte[]? ambe = ctrl.SendReceivePacket(speech);
            bool encodeOk = AmbeHelper.IsAmbePacket(ambe) && ambe!.Length == 15;
            sb.AppendLine($"Encode: {(encodeOk ? "OK" : "ERROR")} (response: {ambe?.Length.ToString() ?? "null"} bytes, expected 15)");
            ok &= encodeOk;

            // 3) Decode: Antwort ist 326 Bytes lang, also mehrere USB-Pakete -> testet das Zusammensetzen
            bool decodeOk = false;
            byte[] channel = [0x61, 0x00, 0x0b, 0x01, 0x01, 0x48, 0, 0, 0, 0, 0, 0, 0, 0, 0];
            if (encodeOk)
            {
                Buffer.BlockCopy(ambe!, 6, channel, 6, 9);
                byte[]? pcm = ctrl.SendReceivePacket(channel);
                decodeOk = AmbeHelper.IsSpeechPacket(pcm) && pcm!.Length == 326;
                sb.AppendLine($"Decode: {(decodeOk ? "OK" : "ERROR")} (response: {pcm?.Length.ToString() ?? "null"} bytes, expected 326)");
            }
            else
            {
                sb.AppendLine("Decode: skipped (encode failed)");
            }
            ok &= decodeOk;

            // 4) Zeit pro Block
            if (encodeOk && decodeOk)
            {
                (double avg, double max, int fails) encode = Measure(rounds, () => AmbeHelper.IsAmbePacket(ctrl.SendReceivePacket(speech)));
                (double avg, double max, int fails) decode = Measure(rounds, () => AmbeHelper.IsSpeechPacket(ctrl.SendReceivePacket(channel)));

                sb.AppendLine($"Encode {rounds}x: mean {encode.avg:F1} ms, max {encode.max:F1} ms, errors {encode.fails}");
                sb.AppendLine($"Decode {rounds}x: mean {decode.avg:F1} ms, max {decode.max:F1} ms, errors {decode.fails}");
                sb.AppendLine("(Target: mean below 20 ms; Windows: approx. 16 ms)");
                ok &= encode.fails == 0 && decode.fails == 0;
            }
        }
        catch (Exception ex)
        {
            sb.AppendLine($"ERROR: {ex.GetType().Name}: {ex.Message}");
            ok = false;
        }

        sb.AppendLine(ok ? "RESULT: OK" : "RESULT: ERROR");
        return sb.ToString();
    }

    private static (double avg, double max, int fails) Measure(int rounds, Func<bool> action)
    {
        double sum = 0, max = 0;
        int fails = 0;
        for (int i = 0; i < rounds; i++)
        {
            long t0 = Stopwatch.GetTimestamp();
            if (!action())
                fails++;
            double ms = Stopwatch.GetElapsedTime(t0).TotalMilliseconds;
            sum += ms;
            max = Math.Max(max, ms);
        }
        return (sum / rounds, max, fails);
    }
}
