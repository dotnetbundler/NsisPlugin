namespace NsisPlugin;

/// <summary>
/// NSIS 插件方法特性
/// </summary>
/// <param name="entryPointFormat">
/// 方法入口点格式字符串，默认为 "{0}" 表示使用方法名称作为入口点
/// </param>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public class NsisActionAttribute(string entryPointFormat = "{0}") : Attribute
{
    public Encodings Encoding { get; set; }
}

/// <summary>
/// NSIS 插件方法参数特性
/// 表示该参数应该从 NSIS 变量中获取
/// </summary>
/// <param name="variable">要获取的变量</param>
[AttributeUsage(AttributeTargets.Parameter)]
public class FromVariableAttribute(NsVariable variable) : Attribute { }

/// <summary>
/// NSIS 插件方法返回值特性
/// 表示该参数应该被设置到 NSIS 变量中
/// </summary>
/// <param name="variable">要设置的变量</param>
[AttributeUsage(AttributeTargets.ReturnValue)]
public class ToVariableAttribute(NsVariable variable) : Attribute { }
