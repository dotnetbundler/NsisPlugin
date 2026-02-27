namespace NsisPlugin;

/// <summary>
/// NSIS 插件方法特性
/// </summary>
/// <param name="entryPoint">NSIS 脚本中调用插件方法的名称，默认为方法名</param>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public class NsisActionAttribute(string? entryPoint = null) : Attribute
{
    public string? EntryPoint { get; } = entryPoint;
    public Encodings Encoding { get; set; } = Encodings.Undefined;
}
