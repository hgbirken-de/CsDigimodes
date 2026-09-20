namespace YsfReflector;

public static class GpsDecoder
{
    public static double Latitude { get; private set; } = 999.0;
    public static double Longitude { get; private set; } = 999.0;
    public static int RadioCode { get; private set; } = 0x0;

    public static void Reset()
    {
        Latitude = 999.0;
        Longitude = 999.0;
        RadioCode = 0x0;
    }

    public static byte AddCRC(byte[] input, int length)
    {
        int crc = 0;

        for (int i = 0; i < length && i < input.Length; i++)
        {
            crc = (crc + input[i]) & 0xFF;
        }

        return (byte)crc;
    }

    public static bool Decode(byte[] data, int ft)
    {
        Reset();

        int i = (ft - 5) * 10 - 2;
        bool valid = false;

        while (i >= 0)
        {
            if (data[i] == 0x03)
            {
                byte crcd = AddCRC(data, i + 1);
                if (crcd == data[i + 1])
                {
                    valid = true;
                    break;
                }
            }
            i--;
        }

        if (valid)
        {
            RadioCode = data[4];

            if ((data[1] == 0x22 && data[2] == 0x62) ||
                (data[1] == 0x47 && data[2] == 0x64))
            {
                ParseGpsString(data);
            }
        }

        return valid;
    }

    private static bool ParseGpsString(byte[] data)
    {
        for (int i = 0; i < 6; i++)
        {
            int b = data[i + 5] & 0xF0;
            if (b != 0x50 && b != 0x30)
                return false;
        }

        int tens = data[5] & 0x0F;
        int units = data[6] & 0x0F;
        int latDeg = tens * 10 + units;
        if (tens > 9 || units > 9 || latDeg > 89)
            return false;

        tens = data[7] & 0x0F;
        units = data[8] & 0x0F;
        int latMin = tens * 10 + units;
        if (tens > 9 || units > 9 || latMin > 59)
            return false;

        tens = data[9] & 0x0F;
        units = data[10] & 0x0F;
        int latMinFrac = tens * 10 + units;
        if (tens > 9 || units > 10 || latMinFrac > 99)
            return false;

        int bdir = data[8] & 0xF0;
        int latDir = bdir switch
        {
            0x50 => 1,
            0x30 => -1,
            _ => 0
        };
        if (latDir == 0)
            return false;

        bdir = data[9] & 0xF0;
        int lonDeg;
        if (bdir == 0x50)
        {
            int b = data[11];
            if (b >= 0x76 && b <= 0x7F)
                lonDeg = b - 0x76;
            else if (b >= 0x6C && b <= 0x75)
                lonDeg = 100 + (b - 0x6C);
            else if (b >= 0x26 && b <= 0x6B)
                lonDeg = 110 + (b - 0x26);
            else
                return false;
        }
        else if (bdir == 0x30)
        {
            int b = data[11];
            if (b >= 0x26 && b <= 0x7F)
                lonDeg = 10 + (b - 0x26);
            else
                return false;
        }
        else
        {
            return false;
        }

        int lonMin;
        int val = data[12];
        if (val >= 0x58 && val <= 0x61)
            lonMin = val - 0x58;
        else if (val >= 0x26 && val <= 0x57)
            lonMin = 10 + (val - 0x26);
        else
            return false;

        int lonMinFrac;
        val = data[13];
        if (val >= 0x1C && val <= 0x7F)
            lonMinFrac = val - 0x1C;
        else
            return false;

        int lonDirBits = data[10] & 0xF0;
        int lonDir = lonDirBits switch
        {
            0x30 => 1,
            0x50 => -1,
            _ => 0
        };
        if (lonDir == 0)
            return false;

        Latitude = latDeg + ((latMin + (latMinFrac * 0.01)) / 60.0);
        Latitude *= latDir;

        Longitude = lonDeg + ((lonMin + (lonMinFrac * 0.01)) / 60.0);
        Longitude *= lonDir;

        return true;
    }
}