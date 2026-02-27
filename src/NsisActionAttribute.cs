namespace NsisPlugin;

public enum Encodings { Undefined, Ansi, Unicode }

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public class NsisActionAttribute(string? entryPoint = null) : Attribute
{
    public string? EntryPoint { get; } = entryPoint;
    public Encodings Encoding { get; set; } = Encodings.Undefined;
}
