# NsisPlugin

![NsisPlugin](https://raw.githubusercontent.com/dotnetbundler/NsisPlugin/refs/heads/main/docs/icon/icon-128x128.png)

[![GitHub repo size](https://img.shields.io/github/repo-size/dotnetbundler/NsisPlugin)](https://github.com/dotnetbundler/NsisPlugin)
[![GitHub License](https://img.shields.io/github/license/dotnetbundler/NsisPlugin)](https://github.com/dotnetbundler/NsisPlugin/blob/master/LICENSE)
[![NuGet Version](https://img.shields.io/nuget/v/NsisPlugin?label=NsisPlugin)](https://www.nuget.org/packages/NsisPlugin)
[![NuGet Downloads](https://img.shields.io/nuget/dt/NsisPlugin?label=NsisPlugin)](https://www.nuget.org/packages/NsisPlugin)

[NsisPlugin](https://github.com/dotnetbundler/NsisPlugin) 是一个面向 C# 的现代化 [NSIS](https://nsis.sourceforge.io/)（Nullsoft Scriptable Install System）插件开发框架。  
它基于特性标注与 Roslyn 源生成器自动生成导出包装代码和初始化逻辑，用于减少 NSIS 插件开发中的非托管互操作样板代码，使开发者能够专注于插件业务本身。

## 文档导航

- [API 参考](docs/api-reference.md)
  - [特性（Attributes）](docs/api-reference.md#特性attributes)
  - [核心类型](docs/api-reference.md#核心类型)
- 示例与实践
  - [使用 NsisPlugin 的示例项目](samples/Plugins/UseNsisPlugin)
  - [插件开发、发布、打包全流程](samples/nsisplugin-usage-sample-release-and-packaging.md)
  - [Native AOT 发布体积分析](samples/aot-publish-volume-analysis.md)
- [源生成器约束与诊断](docs/source-generator-diagnostics.md)

## 特性

- **特性驱动导出**：使用 [`[NsisAction]`](docs/api-reference.md#nsisactionattribute) 声明插件入口，由源生成器自动生成导出包装代码。
- **自动参数绑定**：自动从 NSIS 栈弹出参数并转换为目标类型，同时将返回值压回栈。
- **变量绑定**：通过 [`[FromVariable]`](docs/api-reference.md#fromvariableattribute) / [`[ToVariable]`](docs/api-reference.md#tovariableattribute) 直接读取和写入 NSIS 变量。
- **ANSI / Unicode 双编码支持**：支持全局编码配置以及方法级编码覆盖。
- **编译期诊断**：在不满足导出约束时提供诊断信息，便于尽早发现配置与声明错误。

## 前提条件

- **.NET 9.0+**
  - **源生成器**： 项目依赖 Roslyn 源生成器生成模块初始化器与导出包装代码。（.NET 5.0+）
  - **支持 win-x86 Native AOT**
    - NSIS 插件需以原生共享库（Windows 下为 `.dll`）交付
    - NSIS 只支持 32 位插件，因此在通过 AOT 发布时需指定 `win-x86` 作为运行时标识符。
  - 参考文档：
    - [Native AOT 平台与架构限制（.NET 9+）](https://learn.microsoft.com/en-us/dotnet/core/deploying/native-aot/?tabs=windows%2Cnet9plus#platformarchitecture-restrictions)
    - [Native AOT class library 发布](https://learn.microsoft.com/en-us/dotnet/core/deploying/native-aot/libraries)

## 安装

### dotnet CLI

```bash
dotnet add package NsisPlugin
```

### Package Manager

```powershell
Install-Package NsisPlugin
```

## 快速开始

> **说明**：以下示例仅演示插件入口的声明方式。要使 NSIS 能够实际加载插件，消费项目仍需发布为原生共享库；普通托管类库的 `dotnet build` 产物通常不能直接作为 NSIS 插件使用。

### 1. 最简单的插件函数

以下代码将 `Add` 导出为 NSIS 插件入口点。源生成器会自动生成包装函数，并负责从 NSIS 栈读取参数以及将返回值压回栈：

```csharp
using NsisPlugin;

public class MyPlugin
{
    [NsisAction]
    public static int Add(int a, int b) => a + b;
}
```

在 NSIS 脚本中调用：

```nsis
; 假设编译生成的插件名为 MyPlugin.dll

MyPlugin::Add 3 5
Pop $0 ; $0 = "8"
```

### 2. 多种参数类型

NSIS 传递的参数均为字符串，源生成器会自动将栈中的字符串转换为方法声明的参数类型。支持 `string`、`int`、`double`、`bool` 等所有可由 `Convert.ChangeType` 转换的类型：

```csharp
[NsisAction]
public static string Greet(string name) => $"Hello, {name}!";

[NsisAction]
public static double Power(double baseValue, int exponent) => Math.Pow(baseValue, exponent);

[NsisAction]
public static string IsEven(int number) => (number % 2 == 0) ? "true" : "false";
```

### 3. 自定义入口点名称

方法名与导出名称可以分离。`entryPointFormat` 支持 `{0}` 占位符，用于表示方法名：

```csharp
[NsisAction("GetVersion")]
public static string Version() => "1.0.0";

// 也可使用格式字符串，入口点名称为 "Plugin_Greet"
[NsisAction("Plugin_{0}")]
public static string Greet(string name) => $"Hello, {name}!";
```

### 4. 读写 NSIS 变量

使用 [`[FromVariable]`](docs/api-reference.md#fromvariableattribute) 从指定 NSIS 变量读取参数，使用 [`[ToVariable]`](docs/api-reference.md#tovariableattribute) 将返回值写入指定变量（而非压栈）：

```csharp
// 从 $R0 读取路径，将结果写入 $R1
[NsisAction]
[return: ToVariable(NsVariable.InstR1)]
public static string NormalizePath([FromVariable(NsVariable.InstR0)] string path) => path.Replace('/', '\\');
```

### 5. 同一方法多个入口点

同一方法可附加任意数量的 [`[NsisAction]`](docs/api-reference.md#nsisactionattribute)，分别定义不同的入口点名称和编码。不指定 `Encoding` 时，将使用项目级默认编码（由 `NSISUnicode` 属性决定）：

```csharp
[NsisAction("ToUpper")]                                  // 使用全局编码（由 NSISUnicode 决定）
[NsisAction("ToUpper_A", Encoding = NsEncoding.Ansi)]    // 使用 ANSI 编码
[NsisAction("ToUpper_U", Encoding = NsEncoding.Unicode)] // 使用 Unicode 编码
public static string ToUpper(string input) => input.ToUpper();
```

### 6. 特殊参数（[StackT](docs/api-reference.md#stackt) / [Variables](docs/api-reference.md#variables) / [ExtraParameters](docs/api-reference.md#extraparameters)）

将 [StackT](docs/api-reference.md#stackt)、[Variables](docs/api-reference.md#variables) 或 [ExtraParameters](docs/api-reference.md#extraparameters) 声明为方法参数时，源生成器会自动传入当前调用的上下文对象，**不会**从 NSIS 栈弹出。这三种类型可以自由组合，顺序不限，也可与普通参数混用：

```csharp
[NsisAction]
public static void Summarize(string input, StackT stack, Variables vars, ExtraParameters extra)
{
    // 手动向栈压入多个值
    stack.Push(input.ToUpper());
    stack.Push(input.Length.ToString());

    // 将统计结果写入 NSIS 变量
    vars.Set(NsVariable.InstR0, input.Length.ToString());

    // 根据执行标志决定是否输出额外信息
    if (extra.ExecFlags.Silent == 0) stack.Push("verbose");
}
```

### 7. 使用插件回调

通过 [ExtraParameters](docs/api-reference.md#extraparameters) 的 `RegisterPluginCallback` 方法注册 [NsPluginCallback](docs/api-reference.md#nsplugincallback)，回调参数类型为 [Nspim](docs/api-reference.md#nspim)：

```csharp
[NsisAction]
public static void RegisterCallback()
{
    NsPlugin.ExtraParameters.RegisterPluginCallback(OnMessage);
}

private static IntPtr OnMessage(Nspim message)
{
    if(message == Nspim.NspimGuiunload)
    {
        // NSIS GUI 卸载时的清理逻辑
    }
    else if(message == Nspim.NspimUnload)
    {
        // 插件即将卸载
    }

    return IntPtr.Zero;
}
```

## 配置

| 属性                                | 默认值  | 说明                                                                                                                                               |
| ----------------------------------- | ------- | -------------------------------------------------------------------------------------------------------------------------------------------------- |
| `NSISUnicode`                       | `false` | 设置默认编码，`false` 为 ANSI，`true` 为 Unicode。方法级 `Encoding` 可覆盖此设置。                                                                 |
| `AutoGenerateNsisPluginInitializer` | `true`  | 自动生成模块初始化器，负责设置 `NsPluginEnc.UseUnicode` 和 `NsPlugin.ModuleHandle`。为 `false` 时 `NSISUnicode` 配置无效。大多数场景保持默认即可。 |

> **编码说明**：`NSISUnicode=true` 会在编译时定义 `NSIS_UNICODE` 预处理符号，源生成器生成的初始化器在编译时以此来设置默认编码。

消费项目在使用 ANSI 作为默认编码时通常无需额外配置；如需使用 Unicode 作为默认编码，可在 `csproj` 中添加以下配置：

```xml
<PropertyGroup>
    <NSISUnicode>true</NSISUnicode>
</PropertyGroup>
```

## 注意事项

- **编码一致性**：插件函数必须与调用它们的 NSIS 脚本使用相同编码（ANSI 或 Unicode）。
- **发布方式**：源生成器仅负责生成导出包装代码；若消费项目未以原生共享库方式发布，NSIS 仍无法加载该插件。
- **字符串长度限制**：写入栈或变量的字符串受 NSIS `string_size` 限制（由 `NsPlugin.StringSize` 反映）。超出长度时内容会被截断，且不会抛出异常。
- **类型转换失败**：泛型扩展方法（`Pop<T>`、`Get<T>` 等）在转换失败时返回 `false`，调用方应显式检查结果。
- **返回值与变量**：普通返回值会压回 NSIS 栈；使用 [`[ToVariable]`](docs/api-reference.md#tovariableattribute) 时，结果写入指定变量且不再压栈。
- **手动初始化**：当 `AutoGenerateNsisPluginInitializer` 设为 `false` 时，必须在插件加载阶段手动设置 `NsPlugin.ModuleHandle` 与 `NsPluginEnc.UseUnicode`。
- **Visual Studio 中生成器诊断不实时**：
  - 2026 （若高级配置未迁移，需要打开旧的选项框）
    - 工具 -> 选项 -> 语言 -> C# -> 高级 -> 源生成器（执行） -> 修改为：自动。在任意更改后运行生成器
  - 2022
    - 工具 -> 选项 -> 文本编辑器 -> C# -> 高级 -> 源生成器（执行） -> 修改为：自动。在任意更改后运行生成器

## License

本项目使用 [MIT](LICENSE) 协议。
