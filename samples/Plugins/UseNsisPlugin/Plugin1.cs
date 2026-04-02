using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using NsisPlugin;

namespace UseNsisPlugin;

internal class Plugin1
{
    [NsisAction]
    public static int Add(int a, int b) => a + b;

    [NsisAction("Str{0}")]
    public static string Add(string a, string b) => a + b;

    [NsisAction("Hex{0}")]
    [return: ToVariable(NsVariable.Inst0)]
    public static int Add(HexValue a, HexValue b)
    {
        var res = new HexValue(a.Value + b.Value);
        NsPlugin.StackTop.Push(res);
        return res.Value;
    }
}

internal readonly record struct HexValue(int Value) : IParsable<HexValue>
{
    public static HexValue Parse(string s, IFormatProvider? provider)
        => TryParse(s, provider, out var result) ? result : throw new FormatException();

    public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, out HexValue result)
    {
        if (!string.IsNullOrEmpty(s) && s.StartsWith("0x", StringComparison.OrdinalIgnoreCase) && int.TryParse(s.AsSpan(2), NumberStyles.HexNumber, provider, out var value))
        {
            result = new(value);
            return true;
        }

        result = default;
        return false;
    }

    public override string ToString() => $"0x{Value:X}";
}
