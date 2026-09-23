using DigitalVoice.Dmr;
using DigitalVoiceControlApp.Config;
using System.Security.Cryptography;
using System.Text;
using Xunit.Abstractions;

namespace YsfClientTest;

public class UnitTest_DmrClient(ITestOutputHelper output)
{
    readonly ITestOutputHelper output = output;

    [Fact]
    public void Test_CreateHeaderFrame()
    {
        DmrSessionContext dmrClientState = new()
        {
            Protocol = DmrProtocol.MmdvmHost,
            TxColorCode = 1,
            TxDstId = 262997,
            TxSrcId = 2622363,
            TxRptId = 262236315,
            TxTimeSlot = 2,
        };
        byte[] dmrPkt1 = DigitalVoice.Dmr.DmrCodec.CreateHeaderFrame(dmrClientState, false);

        int SrcId = (dmrPkt1[5] << 16) | (dmrPkt1[6] << 8) | dmrPkt1[7]; // 24 bits BE -> LE
        int DstId = (dmrPkt1[8] << 16) | (dmrPkt1[9] << 8) | dmrPkt1[10]; // 24 bits BE -> LE
        int RptId = (dmrPkt1[11] << 24) | (dmrPkt1[12] << 16) | (dmrPkt1[13] << 8) | dmrPkt1[14]; // 32 bits BE -> LE

        byte flagBits = dmrPkt1[15];
        FrameType frameType = (FrameType)((flagBits & 0x30) >> 4);
        Flco flco = (flagBits & 0x40) == 0 ? Flco.GROUP : Flco.USER_USER;
        int slot = (flagBits & 0x80) != 0 ? 2 : 1;

        Assert.Equal(dmrClientState.TxSrcId, SrcId);
        Assert.Equal(dmrClientState.TxDstId, DstId);
        Assert.Equal(dmrClientState.TxRptId, RptId);

        Assert.Equal(FrameType.DataSync, frameType);
        Assert.Equal(dmrClientState.RxFlco, flco);
        Assert.Equal(dmrClientState.TxTimeSlot, slot);

        Assert.Equal(1, dmrPkt1[15] & 0x0F); // seq no

        byte[] b1 = dmrPkt1[20..]; // the payload
        byte[] b2 = new byte[12];
        DmrBptcCodec.Decode(b1, b2);
        output.WriteLine($"b2 = {Convert.ToHexString(b2)}");

        int DstId2 = (b2[3] << 16) | (b2[4] << 8) | b2[5]; // 24 bits BE -> LE
        int SrcId2 = (b2[6] << 16) | (b2[7] << 8) | b2[8]; // 24 bits BE -> LE
        Flco flco2 = (Flco)(b2[0] & 0b00111111);

        Assert.Equal(dmrClientState.TxDstId, DstId2);
        Assert.Equal(dmrClientState.TxSrcId, SrcId2);
        Assert.Equal(dmrClientState.TxFlco, flco2);

        byte[] eiData = new byte[6];
        DigitalVoice.Dmr.DmrCodec.ExtractEiFromFrame(dmrPkt1, 20, eiData);

        // the sync bytes [0x0D, 0x5D, 0x7F, 0x77, 0xFD, 0x75, 0x70]
        Assert.Equal([0xD5, 0xD7, 0xF7, 0x7F, 0xD7, 0x57], eiData);

        output.WriteLine($"eiData = {Convert.ToHexString(eiData)}");

        // Decode DMR the Slot Type
        (int colorCode, DataType dataType) = DmrCodec.DecodeSlotType();

        Assert.Equal(dmrClientState.TxColorCode, colorCode);
        Assert.Equal(DataType.DataHeader, dataType);

        byte[] dmrPkt2 = DigitalVoice.Dmr.DmrCodec.CreateHeaderFrame(dmrClientState, true);
        Assert.Equal(2, dmrPkt2[15] & 0x0F); // seq no
    }

    [Fact]
    public void Test_CreateVoiceFrame()
    {
        DmrSessionContext dmrClientState = new()
        {
            Protocol = DmrProtocol.MmdvmHost,
            TxColorCode = 1,
            TxDstId = 262997,
            TxSrcId = 2622363,
            TxRptId = 262236315,
            TxTimeSlot = 2,
            TxFlco = Flco.USER_USER,
            TxFrameCount = 0 // <- this must result an exception
        };

        var ex = Assert.Throws<InvalidOperationException>(() =>
        {
            DigitalVoice.Dmr.DmrCodec.CreateVoiceFrame(dmrClientState, []);
        });

        byte[] ambe = new byte[27]; // 3x9 ambebytes

        for (int i = 1; i <= 6; i++)
        {
            dmrClientState.TxFrameCount = i;
            byte[] dmrPkt = DigitalVoice.Dmr.DmrCodec.CreateVoiceFrame(dmrClientState, ambe);

            Assert.Equal(i, dmrPkt[4]);
            
            int SrcId = (dmrPkt[5] << 16) | (dmrPkt[6] << 8) | dmrPkt[7]; // 24 bits BE -> LE
            int DstId = (dmrPkt[8] << 16) | (dmrPkt[9] << 8) | dmrPkt[10]; // 24 bits BE -> LE
            int RptId = (dmrPkt[11] << 24) | (dmrPkt[12] << 16) | (dmrPkt[13] << 8) | dmrPkt[14]; // 32 bits BE -> LE

            byte flagBits = dmrPkt[15];
            FrameType frameType = (FrameType)((flagBits & 0x30) >> 4);
            Flco flco = (flagBits & 0x40) == 0 ? Flco.GROUP : Flco.USER_USER;
            int slot = (flagBits & 0x80) != 0 ? 2 : 1;

            Assert.Equal(dmrClientState.TxSrcId, SrcId);
            Assert.Equal(dmrClientState.TxDstId, DstId);
            Assert.Equal(dmrClientState.TxRptId, RptId);

            Assert.Equal(dmrClientState.TxFlco, flco);
            Assert.Equal(dmrClientState.TxTimeSlot, slot);

            int n = i % 6;
            Assert.Equal(n == 1 ? FrameType.VoiceSync : FrameType.Voice, frameType);
        }
    }

    [Fact]
    public void Test_Sha256()
    {
        var us = UserSettings.Instance();
        byte[] salt = { 0xf9, 0x6f, 0x4b, 0xe6 };
        string pwd = us.Dmr.Password;

        byte[] b = new byte[salt.Length + pwd.Length];
        salt.CopyTo(b, 0);
        Encoding.ASCII.GetBytes(pwd).CopyTo(b, salt.Length);
        byte[] hash = System.Security.Cryptography.SHA256.HashData(b);

        Assert.Equal(32, hash.Length);

        output.WriteLine($"hash: {Convert.ToHexString(hash)}");

        int dmrid = 262236315;
        string hex = dmrid.ToString("X8");
        output.WriteLine($"dmrid = {dmrid}, hex = {hex}");

        string salt2 = "226d7934";

        byte[] inputBytes = Encoding.UTF8.GetBytes(salt2 + pwd);
        byte[] hashBytes = SHA256.HashData(inputBytes);
        //byte[] hashBytes = new SHA256Managed().ComputeHash(inputBytes);
        //var hash = System.Security.Cryptography.SHA256.Create();
        //byte[] hData = hash.ComputeHash(data);

        StringBuilder sb = new(hashBytes.Length * 2);
        foreach (byte b1 in hashBytes)
        {
            sb.AppendFormat("{0:X2}", b1);
        }

        output.WriteLine($"hash: {sb}");
    }

}