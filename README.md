# NsisPlugin

![NsisPlugin](https://raw.githubusercontent.com/dotnetbundler/NsisPlugin/refs/heads/main/docs/icon/icon-128x128.png)

[![GitHub repo size](https://img.shields.io/github/repo-size/dotnetbundler/NsisPlugin)](https://github.com/dotnetbundler/NsisPlugin)
[![GitHub License](https://img.shields.io/github/license/dotnetbundler/NsisPlugin)](https://github.com/dotnetbundler/NsisPlugin/blob/master/LICENSE)
[![NuGet Version](https://img.shields.io/nuget/v/NsisPlugin?label=NsisPlugin)](https://www.nuget.org/packages/NsisPlugin)
[![NuGet Downloads](https://img.shields.io/nuget/dt/NsisPlugin?label=NsisPlugin)](https://www.nuget.org/packages/NsisPlugin)

[NsisPlugin](https://github.com/dotnetbundler/NsisPlugin) 是一个面向 C# 的现代化 [NSIS](https://nsis.sourceforge.io/)（Nullsoft Scriptable Install System）插件开发框架。  
它通过特性标注（Attribute）和 Roslyn 源生成器，将繁琐的非托管互操作样板代码完全自动化，让开发者专注于业务逻辑本身。

## 文档导航

- [API 参考](docs/api-reference.md)
  - [特性（Attributes）](docs/api-reference.md#特性attributes)
  - [核心类型](docs/api-reference.md#核心类型)
- 示例与实战
  - [使用 NsisPlugin 的示例项目](samples/Plugins/UseNsisPlugin)
  - [插件开发、发布、打包全流程](samples/nsisplugin-usage-sample-release-and-packaging.md)
  - [Native AOT 发布体积分析](samples/aot-publish-volume-analysis.md)
- [源生成器约束与诊断](docs/source-generator-diagnostics.md)

## 特性

- **特性驱动**：使用 [`[NsisAction]`](docs/api-reference.md#nsisactionattribute) 声明导出函数，源生成器自动生成非托管导出包装代码，无需手动编写样板
- **自动参数绑定**：自动从 NSIS 栈弹出参数并转换为目标类型，将返回值压回栈
- **变量绑定**：通过 [`[FromVariable]`](docs/api-reference.md#fromvariableattribute) / [`[ToVariable]`](docs/api-reference.md#tovariableattribute) 直接读写 NSIS 变量
- **ANSI / Unicode 双编码支持**：全局与方法级编码均可独立控制
- **编译期诊断**：不满足导出规则的方法会在编译时产生诊断信息，快速定位配置错误

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

> **先说明一件事**：下面的示例展示的是“如何声明插件函数”。要让 NSIS 真正加载你的插件，仍需将消费项目发布为原生共享库；仅仅 `dotnet build` 一个普通托管类库通常还不够。

### 1. 最简单的插件函数

以下代码将 `Add` 导出为 NSIS 插件入口点。源生成器自动生成包装函数，从 NSIS 栈读取参数并将返回值压回 NSIS 栈：

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

方法名与导出名称可以分离。`entryPointFormat` 支持 `{0}` 占位符（表示方法名）：

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

同一方法可附加任意数量的 [`[NsisAction]`](docs/api-reference.md#nsisactionattribute)，分别指定不同的入口点名称和编码。不指定 `Encoding` 时使用项目全局编码（由 `NSISUnicode` 属性决定）：

```csharp
[NsisAction("ToUpper")]                                  // 使用全局编码（由 NSISUnicode 决定）
[NsisAction("ToUpper_A", Encoding = NsEncoding.Ansi)]    // 使用 ANSI 编码
[NsisAction("ToUpper_U", Encoding = NsEncoding.Unicode)] // 使用 Unicode 编码
public static string ToUpper(string input) => input.ToUpper();
```

### 6. 特殊参数（[StackT](docs/api-reference.md#stackt) / [Variables](docs/api-reference.md#variables) / [ExtraParameters](docs/api-reference.md#extraparameters)）

将 [StackT](docs/api-reference.md#stackt)、[Variables](docs/api-reference.md#variables) 或 [ExtraParameters](docs/api-reference.md#extraparameters) 声明为方法参数时，源生成器会自动传入当前调用的上下文对象，**不会**从 NSIS 栈弹出。这三种类型可自由组合，顺序任意，与普通参数混用：

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

消费项目中使用 ANSI 默认编码时通常什么都不需要设置，使用 Unicode 作为默认编码时将以下内容复制到 `csproj` 文件中：

```xml
<PropertyGroup>
    <NSISUnicode>true</NSISUnicode>
</PropertyGroup>
```

## 注意事项

- **编码一致性**：插件函数必须与调用它们的 NSIS 脚本使用相同的编码（ANSI 或 Unicode）。
- **发布方式**：生成器只负责生成导出包装代码；如果消费项目没有按原生共享库方式发布，NSIS 仍然无法加载该插件。
- **字符串长度限制**：写入栈或变量的字符串受 NSIS `string_size` 约束（由 `NsPlugin.StringSize` 反映）。超出长度时内容将被截断，不会抛出异常。
- **类型转换失败**：泛型扩展方法（`Pop<T>`、`Get<T>` 等）在转换失败时返回 `false` 而非抛出异常，调用方需检查返回值。
- **返回值与变量**：普通返回值会压回 NSIS 栈；使用 [`[ToVariable]`](docs/api-reference.md#tovariableattribute) 时结果写入指定变量，不再压栈。两者不可同时生效。
- **手动初始化**：当 `AutoGenerateNsisPluginInitializer` 设为 `false` 时，必须在插件加载时手动设置 `NsPlugin.ModuleHandle` 与 `NsPluginEnc.UseUnicode`。
- **Visual Studio中生成器诊断不实时**：
  - 2026 （若高级配置未迁移，需要打开旧的选项框）
    - 工具 -> 选项 -> 语言 -> C# -> 高级 -> 源生成器（执行） -> 修改为：自动。在任意更改后运行生成器
  - 2022
    - 工具 -> 选项 -> 文本编辑器 -> C# -> 高级 -> 源生成器（执行） -> 修改为：自动。在任意更改后运行生成器

## License

本项目使用 [MIT](LICENSE) 协议。
