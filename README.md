# NsisPlugin

![NsisPlugin](https://raw.githubusercontent.com/dotnetbundler/NsisPlugin/refs/heads/main/docs/icon/icon-128x128.png)

[![GitHub repo size](https://img.shields.io/github/repo-size/dotnetbundler/NsisPlugin)](https://github.com/dotnetbundler/NsisPlugin)
[![GitHub License](https://img.shields.io/github/license/dotnetbundler/NsisPlugin)](https://github.com/dotnetbundler/NsisPlugin/blob/master/LICENSE)
[![NuGet Version](https://img.shields.io/nuget/v/NsisPlugin?label=NsisPlugin)](https://www.nuget.org/packages/NsisPlugin)
[![NuGet Downloads](https://img.shields.io/nuget/dt/NsisPlugin?label=NsisPlugin)](https://www.nuget.org/packages/NsisPlugin)

[NsisPlugin](https://github.com/dotnetbundler/NsisPlugin) 是一个面向 C# 的现代化 [NSIS](https://nsis.sourceforge.io/)（Nullsoft Scriptable Install System）插件开发框架。  
它通过特性标注（Attribute）和 Roslyn 源生成器，将繁琐的非托管互操作样板代码完全自动化，让开发者专注于业务逻辑本身。

## 特性

- **特性驱动**：使用 `[NsisAction]` 声明导出函数，源生成器自动生成非托管导出包装代码，无需手动编写样板
- **自动参数绑定**：自动从 NSIS 栈弹出参数并转换为目标类型，将返回值压回栈
- **变量绑定**：通过 `[FromVariable]` / `[ToVariable]` 直接读写 NSIS 变量
- **ANSI / Unicode 双编码支持**：全局与方法级编码均可独立控制
- **编译期诊断**：不满足导出规则的方法会在编译时产生诊断信息，快速定位配置错误

## 前提条件

- **使用源生成器**：需要 C# 9.0 或更高版本
    - [**配置**](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/configure-language-version)：`<LangVersion>9.0</LangVersion>` 或更高
    - 不满足时无法生成模块初始化器与导出包装代码，但仍可使用 NsisPlugin 的运行时库部分，手动编写导出函数和互操作代码
- **发布为 NSIS 插件**：最终产物必须是原生共享库（Windows 下为 `.dll`）
    - NsisPlugin 会生成带 `UnmanagedCallersOnly` 的导出包装代码，但**不会自动为消费项目开启 Native AOT / 原生库发布**
    - 因此消费项目仍需显式配置原生发布流程，例如使用 [Native AOT class library 发布](https://learn.microsoft.com/en-us/dotnet/core/deploying/native-aot/libraries)

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

使用 `[FromVariable]` 从指定 NSIS 变量读取参数，使用 `[ToVariable]` 将返回值写入指定变量（而非压栈）：

```csharp
// 从 $R0 读取路径，将结果写入 $R1
[NsisAction]
[return: ToVariable(NsVariable.InstR1)]
public static string NormalizePath([FromVariable(NsVariable.InstR0)] string path) => path.Replace('/', '\\');
```

### 5. 同一方法多个入口点

同一方法可附加任意数量的 `[NsisAction]`，分别指定不同的入口点名称和编码。不指定 `Encoding` 时使用项目全局编码（由 `NSISUnicode` 属性决定）：

```csharp
[NsisAction("ToUpper")]                                  // 使用全局编码（由 NSISUnicode 决定）
[NsisAction("ToUpper_A", Encoding = NsEncoding.Ansi)]    // 使用 ANSI 编码
[NsisAction("ToUpper_U", Encoding = NsEncoding.Unicode)] // 使用 Unicode 编码
public static string ToUpper(string input) => input.ToUpper();
```

### 6. 特殊参数（StackT / Variables / ExtraParameters）

将 `StackT`、`Variables` 或 `ExtraParameters` 声明为方法参数时，源生成器会自动传入当前调用的上下文对象，**不会**从 NSIS 栈弹出。这三种类型可自由组合，顺序任意，与普通参数混用：

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

| 属性                                  | 默认值     | 说明                                                                                                             |
|-------------------------------------|---------|----------------------------------------------------------------------------------------------------------------|
| `NSISUnicode`                       | `false` | 设置默认编码，`false` 为 ANSI，`true` 为 Unicode。方法级 `Encoding` 可覆盖此设置。                                                  |
| `AutoGenerateNsisPluginInitializer` | `true`  | 自动生成模块初始化器，负责设置 `NsPluginEnc.UseUnicode` 和 `NsPlugin.ModuleHandle`。为 `false` 时 `NSISUnicode` 配置无效。大多数场景保持默认即可。 |

> **编码说明**：`NSISUnicode=true` 会在编译时定义 `NSIS_UNICODE` 预处理符号，源生成器生成的初始化器在编译时以此来设置默认编码。

消费项目中使用 ANSI 默认编码时通常什么都不需要设置，使用 Unicode 作为默认编码时将以下内容复制到 `csproj` 文件中：

```xml
<PropertyGroup>
    <NSISUnicode>true</NSISUnicode>
</PropertyGroup>
```

## API 文档

### 特性（Attributes）

- `NsisActionAttribute`
    - 目标：方法
    - 说明：将方法声明为 NSIS 插件函数，源生成器会为其生成非托管导出包装代码
    - 参数：
        - `entryPointFormat`：可选，指定导出函数名称的格式，默认为 `{0}`（方法名）。例如 `"Plugin_{0}"` 会将方法 `Greet` 导出为 `Plugin_Greet`
        - `Encoding`：可选，指定此方法使用的编码（ANSI / Unicode）。
- `FromVariableAttribute`
    - 目标：参数
    - 说明：从指定 NSIS 变量读取参数值，而非从栈弹出。
    - 参数：
        - `NsVariable`：要绑定的 NSIS 变量枚举值，例如 `NsVariable.InstR0`
- `ToVariableAttribute`
    - 目标：返回值
    - 说明：将返回值写入指定 NSIS 变量，而非压回栈。
    - 参数：
        - `NsVariable`：要绑定的 NSIS 变量枚举值，例如 `NsVariable.InstR0`

### 核心类型

#### `NsPlugin`（静态类）

| 成员                | 类型                | 说明                                      |
|-------------------|-------------------|-----------------------------------------|
| `ModuleHandle`    | `IntPtr`          | DLL 模块句柄，由模块初始化器自动填充。                   |
| `HwndParent`      | `IntPtr`          | NSIS 安装器父窗口句柄。                          |
| `StringSize`      | `int`             | NSIS 字符串缓冲区大小（字符数）。                     |
| `MaxStringBytes`  | `int`             | 字符串缓冲区最大字节数（= `StringSize × CharSize`）。 |
| `Variables`       | `Variables`       | NSIS 变量封装。                              |
| `StackTop`        | `StackT`          | NSIS 栈封装。                               |
| `ExtraParameters` | `ExtraParameters` | NSIS 额外参数与执行标志封装。                       |

#### `Variables`（类）

| 成员                                            | 说明                                    |
|-----------------------------------------------|---------------------------------------|
| `Get(NsVariable variable, out string? value)` | 读取指定 NSIS 变量的字符串值，成功返回 `true`。        |
| `Get<T>(NsVariable variable, out T? value)`   | 读取指定 NSIS 变量的值并尝试转换为指定类型，成功返回 `true`。 |
| `Set(NsVariable variable, string value)`      | 写入指定 NSIS 变量的字符串值，成功返回 `true`。        |
| `Set<T>(NsVariable variable, T value)`        | 将指定值转换为字符串并写入 NSIS 变量，成功返回 `true`。    |
| `Raw`                                         | 底层变量缓冲区指针，供高级场景使用。                    |

#### `StackT`（类）

| 成员                       | 说明                                     |
|--------------------------|----------------------------------------|
| `Pop(out string? value)` | 从 NSIS 栈顶弹出一个字符串参数，成功返回 `true`。        |
| `Pop<T>(out T? value)`   | 从 NSIS 栈顶弹出一个参数并尝试转换为指定类型，成功返回 `true`。 |
| `Push(string value)`     | 将一个字符串参数压回 NSIS 栈。                     |
| `Push<T>(T value)`       | 将一个值转换为字符串并压回 NSIS 栈。                  |
| `Raw`                    | `stack_t**` 指针，供高级场景使用。                |

#### `ExtraParameters`（类）

| 成员                                                  | 说明                              |
|-----------------------------------------------------|---------------------------------|
| `ExecFlags`                                         | NSIS 执行标志结构体。                   |
| `ExecuteCodeSegment(int code)`                      | 执行 NSIS 代码段，成功返回 0。             |
| `ValidateFilename(ref string filename)`             | 通过 NSIS 内置验证函数规范化文件名。           |
| `RegisterPluginCallback(NsPluginCallback callback)` | 注册插件回调函数，成功返回 0，已经注册返回 1。       |
| `Raw`                                               | `extra_parameters*` 指针，供高级场景使用。 |

#### `NsPluginEnc`（静态类）

| 成员                                    | 说明                                                  |
|---------------------------------------|-----------------------------------------------------|
| `UseUnicode`                          | 全局编码开关；`true` 为 Unicode，`false` 为 ANSI。由模块初始化器自动设置。 |
| `ScopeUseUnicode`                     | 线程本地编码覆盖；为 `null` 时使用全局设置。                          |
| `IsUnicode`                           | 当前线程是否使用 Unicode 编码（综合全局与作用域设置）。                    |
| `CharSize`                            | 当前编码每个字符的字节数（Unicode = 2，ANSI = 1）。                 |
| `Encoding`                            | 当前编码对应的 `System.Text.Encoding` 对象。                  |
| `PtrToString(IntPtr ptr)`             | 按当前编码将非托管指针转换为 `string`。                            |
| `CreateEncScope(NsEncoding encoding)` | 创建编码作用域，在 `using` 块内临时切换编码，离开后自动恢复。                 |

#### `ExecFlags`（结构体）

映射 NSIS `exec_flags_t` 结构体；
字段类型均为 `int`，被当作布尔值标志时，遵循0表示false，非0表示true的约定

| 字段                 | 说明                   |
|--------------------|----------------------|
| `Autoclose`        | 自动关闭标志               |
| `AllUserVar`       | 变量作用域（0 = 用户，1 = 机器） |
| `ExecError`        | 执行错误标志               |
| `Abort`            | 中止标志                 |
| `ExecReboot`       | 是否需要重启               |
| `RebootCalled`     | 是否已调用重启              |
| `XxxCurInsttype`   | 已弃用；保留以支持向后兼容的ABI/布局 |
| `PluginApiVersion` | 插件 API/ABI 版本        |
| `Silent`           | 静默模式标志               |
| `InstdirError`     | 安装目录错误标志             |
| `Rtl`              | 语言是否为从右到左（RTL）       |
| `Errlvl`           | 错误级别                 |
| `AlterRegView`     | 注册表视图设置              |
| `StatusUpdate`     | 状态更新 / 详细信息打印        |

### 枚举

#### `NsVariable`

对应 NSIS 脚本中的变量，可用于 `[FromVariable]`、`[ToVariable]` 以及 `Variables.Get` / `Variables.Set`：

| 值                   | 对应 NSIS 变量    |
|---------------------|---------------|
| `Inst0` ~ `Inst9`   | `$0` ~ `$9`   |
| `InstR0` ~ `InstR9` | `$R0` ~ `$R9` |
| `InstCmdline`       | `$CMDLINE`    |
| `InstInstdir`       | `$INSTDIR`    |
| `InstOutdir`        | `$OUTDIR`     |
| `InstExedir`        | `$EXEDIR`     |
| `InstLang`          | `$LANGUAGE`   |

#### `NsEncoding`

| 值           | 说明                   |
|-------------|----------------------|
| `Undefined` | 未定义，使用全局设置           |
| `Ansi`      | ANSI 编码              |
| `Unicode`   | Unicode（UTF-16 LE）编码 |

#### `Nspim`

插件回调消息类型，用于 `NsPluginCallback` 委托：

| 值                | 说明                         |
|------------------|----------------------------|
| `NspimUnload`    | 插件卸载，执行最终清理                |
| `NspimGuiunload` | GUI 卸载（在 `.onGUIEnd` 之后触发） |

### 委托

```csharp
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate IntPtr NsPluginCallback(Nspim message);
```

插件回调委托，通过 `ExtraParameters.RegisterPluginCallback` 注册后，由 NSIS 在卸载时调用。

## 导出函数约束与诊断

### 初始化器诊断

| 诊断 ID       | 级别      | 说明                                                  |
|-------------|---------|-----------------------------------------------------|
| `NSPGEN001` | Warning | C# 语言版本低于 9.0，无法生成模块初始化器                            |
| `NSPGEN002` | Warning | 找不到 `ModuleInitializerAttribute`，无法生成模块初始化器         |
| `NSPGEN003` | Info    | `AutoGenerateNsisPluginInitializer` 未设为 `true`，跳过生成 |

### 导出函数诊断

| 诊断 ID       | 级别      | 说明                                                                                                              |
|-------------|---------|-----------------------------------------------------------------------------------------------------------------|
| `NSPGEN101` | Warning | 方法不满足导出条件，例如：不是 `static`、是 `abstract`、是泛型方法、方法或其包含类型不是 `public/internal` 可访问、参数包含 `ref/out/in`、不在命名类型中、或包含类型是泛型 |
| `NSPGEN102` | Warning | 带 `[ToVariable]` 的方法不能返回 `void`                                                                                 |
| `NSPGEN121` | Error   | 入口点与其他导出冲突                                                                                                      |
| `NSPGEN122` | Error   | 入口点格式字符串无效                                                                                                      |
| `NSPGEN123` | Error   | 入口点名称无效，不是合法标识符或包含不支持导出的字符                                                                                      |

## 注意事项

- **编码一致性**：插件函数必须与调用它们的 NSIS 脚本使用相同的编码（ANSI 或 Unicode）。
- **发布方式**：生成器只负责生成导出包装代码；如果消费项目没有按原生共享库方式发布，NSIS 仍然无法加载该插件。
- **字符串长度限制**：写入栈或变量的字符串受 NSIS `string_size` 约束（由 `NsPlugin.StringSize` 反映）。超出长度时内容将被截断，不会抛出异常。
- **类型转换失败**：泛型扩展方法（`Pop<T>`、`Get<T>` 等）在转换失败时返回 `false` 而非抛出异常，调用方需检查返回值。
- **返回值与变量**：普通返回值会压回 NSIS 栈；使用 `[ToVariable]` 时结果写入指定变量，不再压栈。两者不可同时生效。
- **手动初始化**：当 `AutoGenerateNsisPluginInitializer` 设为 `false` 时，必须在插件加载时手动设置 `NsPlugin.ModuleHandle` 与 `NsPluginEnc.UseUnicode`。

## License

本项目使用 [MIT](LICENSE) 协议。
