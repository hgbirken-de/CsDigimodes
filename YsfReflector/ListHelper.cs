namespace YsfReflector;

internal static class ListHelper
{
    public static bool InList(IEnumerable<string> list, string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return false;

        value = value.Trim().ToUpperInvariant();

        foreach (var entry in list)
        {
            var e = entry.Trim().ToUpperInvariant();
            if (string.IsNullOrEmpty(e))
                continue;

            // Match rule: exact or substring or prefix
            if (value.StartsWith(e) || value.Contains(e))
                return true;
        }

        return false;
    }
}

