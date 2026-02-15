namespace NsisPlugin;

public enum Encodings { Undefined, Ansi, Unicode }

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public class NsisActionAttribute(string? name = null) : Attribute
{
    public string? Name { get; } = name;
    public Encodings Encoding { get; set; } = Encodings.Undefined;
}
