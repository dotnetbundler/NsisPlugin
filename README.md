# NsisPlugin

[![NuGet](https://img.shields.io/nuget/v/NsisPlugin?label=NuGet)](https://www.nuget.org/packages/NsisPlugin)
[![License: MIT](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)

`NsisPlugin` 是一个面向 C# 的现代化 [NSIS](https://nsis.sourceforge.io/)（Nullsoft Scriptable Install System）插件开发框架。它通过特性标注（Attribute）和 Roslyn 源生成器，将繁琐的非托管互操作样板代码完全自动化，让开发者专注于业务逻辑本身。

## 特性

- **特性驱动**：使用 `[NsisAction]` 声明导出函数，源生成器自动生成非托管导出包装代码，无需手动编写样板
- **自动参数绑定**：自动从 NSIS 栈弹出参数并转换为目标类型，将返回值压回栈
- **变量绑定**：通过 `[FromVariable]` / `[ToVariable]` 直接读写 NSIS 变量
- **ANSI / Unicode 双编码支持**：全局与方法级编码均可独立控制
- **编译期诊断**：不满足导出规则的方法会在编译时产生诊断信息，快速定位配置错误
- **基于 NativeAOT**：插件必须以 NativeAOT 方式发布，才能生成可被 NSIS 加载的本机动态链接库

## 环境要求

使用本库的项目必须以 **NativeAOT** 方式发布，以生成可被 NSIS 加载的本机动态链接库。这要求目标框架为 .NET 8.0 或更高版本。

```xml
<PropertyGroup>
  <PublishAot>true</PublishAot>
  <AllowUnsafeBlocks>true</AllowUnsafeBlocks>
</PropertyGroup>
```

> 自动生成的模块初始化器还额外要求 C# 9.0 或更高（使用 `ModuleInitializerAttribute`）。若将 `AutoGenerateNsisPluginInitializer` 设为 `false` 并手动初始化，则无此要求。

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

### 示例一：最简单的插件函数

以下代码将 `Add` 导出为 NSIS 插件入口点。源生成器自动生成包装函数，依次从 NSIS 栈弹出参数 `a`、`b`，并将返回值压回 NSIS 栈。

```csharp
using NsisPlugin;

public static class MyPlugin
{
    [NsisAction]
    public static int Add(int a, int b) => a + b;
}
```

在 NSIS 脚本中调用（`MyPlugin` 为插件的 DLL 文件名，不含扩展名）：

```nsis
MyPlugin::Add 3 5
Pop $0   ; $0 = "8"
```

### 示例二：多种参数类型

NSIS 传递的参数均为字符串，源生成器会自动将栈中的字符串转换为方法声明的参数类型。支持 `string`、`int`、`double`、`bool` 等所有可由 `Convert.ChangeType` 转换的类型：

```csharp
[NsisAction]
public static string Greet(string name) => $"Hello, {name}!";

[NsisAction]
public static double Power(double baseValue, int exponent) => Math.Pow(baseValue, exponent);

[NsisAction]
public static string IsEven(int number) => (number % 2 == 0) ? "true" : "false";
```

### 示例三：自定义入口点名称

方法名与导出名称可以分离。`entryPointFormat` 支持 `{0}` 占位符（会被替换为方法名）：

```csharp
[NsisAction("GetVersion")]
public static string Version() => "1.0.0";

// 也可使用格式字符串，入口点名称为 "Plugin_Greet"
[NsisAction("Plugin_{0}")]
public static string Greet(string name) => $"Hello, {name}!";
```

### 示例四：读写 NSIS 变量

使用 `[FromVariable]` 从指定 NSIS 变量读取参数，使用 `[ToVariable]` 将返回值写入指定变量（而非压栈）：

```csharp
// 从 $R0 读取路径，将结果写入 $R1
[NsisAction]
[return: ToVariable(NsVariable.InstR1)]
public static string NormalizePath([FromVariable(NsVariable.InstR0)] string path)
    => path.Replace('/', '\\');
```

### 示例五：同一方法多个入口点（ANSI 与 Unicode 各一个）

```csharp
[NsisAction("DoWork_A", Encoding = NsEncoding.Ansi)]
[NsisAction("DoWork_U", Encoding = NsEncoding.Unicode)]
public static string DoWork(string input) => input.ToUpper();
```

### 示例六：使用插件回调

```csharp
[NsisAction]
public static void RegisterCallback()
{
    NsPlugin.ExtraParameters.RegisterPluginCallback(OnMessage);
}

private static IntPtr OnMessage(Nspim message)
{
    if (message == Nspim.NspimUnload)
    {
        // 插件即将卸载，执行清理操作
    }
    return IntPtr.Zero;
}
```

### 示例七：访问执行标志

```csharp
[NsisAction]
public static void SetSilent()
{
    NsPlugin.ExtraParameters.ExecFlags.Silent = 1;
}
```

## 配置

安装包会通过 MSBuild 自动导入以下属性（在 `build/NsisPlugin.props` 中定义）：

```xml
<PropertyGroup>
  <NSISUnicode>false</NSISUnicode>
  <AutoGenerateNsisPluginInitializer>true</AutoGenerateNsisPluginInitializer>
</PropertyGroup>
```

可在消费项目的 `.csproj` 中直接覆盖：

```xml
<PropertyGroup>
  <NSISUnicode>true</NSISUnicode>
</PropertyGroup>
```

| 属性 | 默认值 | 说明 |
|------|--------|------|
| `NSISUnicode` | `false` | 设置默认字符串编码。`false` 为 ANSI，`true` 为 Unicode。必须与安装器编译选项一致。 |
| `AutoGenerateNsisPluginInitializer` | `true` | 自动生成模块初始化器，负责设置 `NsPluginEnc.UseUnicode` 和 `NsPlugin.ModuleHandle`。大多数场景保持默认即可。 |

> **编码说明**：`NSISUnicode=true` 会在编译时定义 `NSIS_UNICODE` 预处理符号，源生成器以此判断初始化器中的编码设置。

## API 参考

### 特性（Attributes）

| 特性 | 目标 | 说明 |
|------|------|------|
| `[NsisAction]` | 方法 | 将方法声明为 NSIS 插件入口点。支持 `AllowMultiple`，可在同一方法上声明多个入口点。 |
| `[FromVariable(NsVariable)]` | 参数 | 从指定 NSIS 变量读取该参数，而非从栈弹出。 |
| `[ToVariable(NsVariable)]` | 返回值 | 将返回值写入指定 NSIS 变量，而非压回栈。方法返回类型不能为 `void`。 |

`NsisActionAttribute` 构造函数参数：

| 参数 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| `entryPointFormat` | `string` | `"{0}"` | 入口点名称格式字符串，`{0}` 会被替换为方法名。也可直接写固定名称。 |
| `Encoding` | `NsEncoding` | `Undefined` | 覆盖该入口点的字符串编码，`Undefined` 表示使用全局配置。 |

### 核心类型

#### `NsPlugin`（静态类）

插件调用上下文的入口，所有属性均为 ThreadStatic（线程静态）。

| 成员 | 类型 | 说明 |
|------|------|------|
| `ModuleHandle` | `IntPtr` | DLL 模块句柄，由模块初始化器自动填充。 |
| `HwndParent` | `IntPtr` | NSIS 安装器父窗口句柄。 |
| `StringSize` | `int` | NSIS 字符串缓冲区大小（字符数）。 |
| `MaxStringBytes` | `int` | 字符串缓冲区最大字节数（= `StringSize × CharSize`）。 |
| `Variables` | `Variables` | NSIS 变量访问接口。 |
| `StackTop` | `StackT` | NSIS 栈访问接口。 |
| `ExtraParameters` | `ExtraParameters` | NSIS 额外参数与执行标志接口。 |

#### `StackT`（类）

| 成员 | 说明 |
|------|------|
| `Pop(out string? str)` | 弹出栈顶字符串，成功返回 `true`。 |
| `Push(string str)` | 将字符串压入栈顶，成功返回 `true`。 |
| `Raw` | 底层 `stack_t**` 指针，供高级场景使用。 |

#### `Variables`（类）

| 成员 | 说明 |
|------|------|
| `Get(NsVariable variable, out string? value)` | 读取指定 NSIS 变量的字符串值，成功返回 `true`。 |
| `Set(NsVariable variable, string value)` | 写入指定 NSIS 变量的字符串值，成功返回 `true`。 |
| `Raw` | 底层变量缓冲区指针，供高级场景使用。 |

#### `ExtraParameters`（类）

| 成员 | 说明 |
|------|------|
| `ExecFlags` | 引用 NSIS 执行标志结构体（`ref ExecFlags`）。 |
| `ExecuteCodeSegment(int code)` | 执行指定代码段，返回执行结果。 |
| `ValidateFilename(ref string filename)` | 通过 NSIS 内置验证函数规范化文件名。 |
| `RegisterPluginCallback(NsPluginCallback callback)` | 注册插件回调，返回 0 表示成功；重复注册时返回 1。 |

#### `NsPluginEnc`（静态类）

| 成员 | 说明 |
|------|------|
| `UseUnicode` | 全局编码开关；`true` 为 Unicode，`false` 为 ANSI。由模块初始化器自动设置。 |
| `ScopeUseUnicode` | 线程本地编码覆盖；为 `null` 时使用全局设置。 |
| `IsUnicode` | 当前线程是否使用 Unicode 编码（综合全局与作用域设置）。 |
| `CharSize` | 当前编码每个字符的字节数（Unicode = 2，ANSI = 1）。 |
| `Encoding` | 当前编码对应的 `System.Text.Encoding` 对象。 |
| `PtrToString(IntPtr ptr)` | 按当前编码将非托管指针转换为 `string`。 |
| `CreateEncScope(NsEncoding encoding)` | 创建编码作用域，在 `using` 块内临时切换编码，离开后自动恢复。 |

#### `ExecFlags`（结构体）

映射 NSIS `exec_flags_t` 结构体，字段类型均为 `int`（0 = false，非 0 = true）：

| 字段 | 说明 |
|------|------|
| `Autoclose` | 自动关闭标志 |
| `AllUserVar` | 变量作用域（0 = 用户，1 = 机器） |
| `ExecError` | 执行错误标志 |
| `Abort` | 中止标志 |
| `ExecReboot` | 是否需要重启 |
| `RebootCalled` | 是否已调用重启 |
| `PluginApiVersion` | 插件 API/ABI 版本 |
| `Silent` | 静默模式标志 |
| `InstdirError` | 安装目录错误标志 |
| `Rtl` | 语言是否为从右到左（RTL） |
| `Errlvl` | 错误级别 |
| `AlterRegView` | 注册表视图设置 |
| `StatusUpdate` | 状态更新 / 详细信息打印 |

### 扩展方法

`NsPluginExtensions` 提供了泛型扩展方法，内部使用 `Convert.ChangeType` 完成类型转换，适合直接读写常见基础类型（`int`、`double`、`bool` 等）：

```csharp
// StackT 扩展
NsPlugin.StackTop.Pop<int>(out int a);
NsPlugin.StackTop.Push<double>(3.14);

// Variables 扩展
NsPlugin.Variables.Get<bool>(NsVariable.InstR0, out bool flag);
NsPlugin.Variables.Set<int>(NsVariable.InstR1, 42);
```

| 方法 | 说明 |
|------|------|
| `StackT.Pop<T>(out T? val)` | 弹出栈顶值并转换为 `T`，转换失败返回 `false`。 |
| `StackT.Push<T>(T val)` | 将 `T` 转换为字符串后压栈。 |
| `Variables.Get<T>(NsVariable, out T? val)` | 读取变量值并转换为 `T`，转换失败返回 `false`。 |
| `Variables.Set<T>(NsVariable, T val)` | 将 `T` 转换为字符串后写入变量。 |

### 枚举

#### `NsVariable`

对应 NSIS 脚本中的变量，可用于 `[FromVariable]`、`[ToVariable]` 以及 `Variables.Get` / `Variables.Set`：

| 值 | 对应 NSIS 变量 |
|-----|----------------|
| `Inst0` ~ `Inst9` | `$0` ~ `$9` |
| `InstR0` ~ `InstR9` | `$R0` ~ `$R9` |
| `InstCmdline` | `$CMDLINE` |
| `InstInstdir` | `$INSTDIR` |
| `InstOutdir` | `$OUTDIR` |
| `InstExedir` | `$EXEDIR` |
| `InstLang` | `$LANGUAGE` |

#### `NsEncoding`

| 值 | 说明 |
|----|------|
| `Undefined` | 未定义，使用全局设置 |
| `Ansi` | ANSI 编码 |
| `Unicode` | Unicode（UTF-16 LE）编码 |

#### `Nspim`

插件回调消息类型，用于 `NsPluginCallback` 委托：

| 值 | 说明 |
|----|------|
| `NspimUnload` | 插件卸载，执行最终清理 |
| `NspimGuiunload` | GUI 卸载（在 `.onGUIEnd` 之后触发） |

### 委托

```csharp
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate IntPtr NsPluginCallback(Nspim message);
```

插件回调委托，通过 `ExtraParameters.RegisterPluginCallback` 注册后，由 NSIS 在卸载时调用。

## 导出函数约束与编译诊断

源生成器会在编译时验证所有带 `[NsisAction]` 的方法。不满足条件的方法将不会生成导出包装，同时产生对应的编译诊断。

### 方法必须满足的条件

| 条件 | 诊断 ID | 级别 |
|------|---------|------|
| 必须为 `static` 方法 | `NSPGEN101` | Warning |
| 不能是 `abstract` 方法 | `NSPGEN101` | Warning |
| 不能是泛型方法 | `NSPGEN101` | Warning |
| 可见性必须为 `public` 或 `internal` | `NSPGEN101` | Warning |
| 参数不能使用 `ref`、`out`、`in` | `NSPGEN101` | Warning |
| 必须包含在一个非泛型、非隐式的类型中 | `NSPGEN101` | Warning |
| 容器类型的可见性也必须为 `public` 或 `internal` | `NSPGEN101` | Warning |

### 入口点规则

| 条件 | 诊断 ID | 级别 |
|------|---------|------|
| 入口点名称不能重复 | `NSPGEN121` | Error |
| `entryPointFormat` 格式字符串必须合法 | `NSPGEN122` | Error |
| 入口点名称必须是合法的 C# 标识符 | `NSPGEN123` | Error |

### 返回值规则

| 条件 | 诊断 ID | 级别 |
|------|---------|------|
| 带 `[ToVariable]` 的方法不能返回 `void` | `NSPGEN102` | Warning |

### 初始化器诊断

| 诊断 ID | 级别 | 说明 |
|---------|------|------|
| `NSPGEN001` | Warning | C# 语言版本低于 9.0，无法生成模块初始化器 |
| `NSPGEN002` | Warning | 找不到 `ModuleInitializerAttribute`，无法生成模块初始化器 |
| `NSPGEN003` | Info | `AutoGenerateNsisPluginInitializer` 未设为 `true`，跳过生成 |

## 注意事项

- **编码一致性**：`NSISUnicode` 属性必须与 NSIS 安装器编译时的编码选项一致，否则所有字符串读写均会出现乱码。
- **字符串长度限制**：写入栈或变量的字符串受 NSIS `string_size` 约束（由 `NsPlugin.StringSize` 反映）。超出长度时内容将被截断，不会抛出异常。
- **类型转换失败**：泛型扩展方法（`Pop<T>`、`Get<T>` 等）在转换失败时返回 `false` 而非抛出异常，调用方需检查返回值。
- **返回值与变量**：普通返回值会压回 NSIS 栈；使用 `[ToVariable]` 时结果写入指定变量，不再压栈。两者不可同时生效。
- **手动初始化**：当 `AutoGenerateNsisPluginInitializer` 设为 `false` 时，必须在插件加载时手动设置 `NsPlugin.ModuleHandle` 与 `NsPluginEnc.UseUnicode`。
- **回调引用保活**：`ExtraParameters.RegisterPluginCallback` 内部已保持对委托的引用以防止 GC，无需外部额外持有。
- **线程安全**：`NsPlugin` 的各上下文属性（`HwndParent`、`StackTop` 等）均为 `[field: ThreadStatic]`，每次 NSIS 调用都应在对应线程上通过 `NsPlugin.Init(...)` 初始化。

## 贡献 / 开发指南

### 前置要求

- [.NET 10.0 SDK](https://dotnet.microsoft.com/download) 或更高版本

### 克隆与构建

```bash
# 克隆仓库
git clone https://github.com/dotnetbundler/NsisPlugin.git
cd NsisPlugin

# 还原依赖
dotnet restore

# Debug 构建
dotnet build

# Release 构建
dotnet build -c Release
```

### 运行测试

```bash
# 运行全部测试
dotnet test

# 仅运行运行时测试
dotnet test tests/NsisPlugin.Test/NsisPlugin.Test.csproj

# 仅运行源生成器快照测试
dotnet test tests/NsisPlugin.SourceGeneration.Tests/NsisPlugin.SourceGeneration.Tests.csproj
```

> **快照测试说明**：`NsisPlugin.SourceGeneration.Tests` 使用 [Verify](https://github.com/VerifyTests/Verify) 进行快照验证。如果修改了源生成器的代码生成逻辑，需同步更新 `ExportSnapshots/` 和 `InitializerSnapshots/` 目录下的快照文件。

### 打包

```bash
dotnet pack -c Release -p:Version=<version> -o ./artifacts
```

### 仓库结构

```
NsisPlugin/
├── src/                    # 主库源码（NsisPlugin NuGet 包）
│   ├── NsisApi/            # NSIS C API 非托管结构体
│   ├── Compatibility/      # 跨框架兼容层
│   └── build/              # MSBuild props / targets（随包分发）
├── gen/                    # Roslyn 源生成器（打包为 Analyzer）
│   ├── Export/             # [NsisAction] 导出代码生成
│   └── Initializer/        # 模块初始化器代码生成
├── Common/                 # src/ 与 gen/ 共享的特性和枚举定义
├── tests/
│   ├── NsisPlugin.Test/                    # 运行时功能测试（xUnit v3）
│   └── NsisPlugin.SourceGeneration.Tests/  # 源生成器快照测试
├── eng/                    # 工程化构建脚本（包元数据、打包目标等）
├── docs/                   # 文档资源（图标等）
└── Directory.Build.props   # 解决方案级 MSBuild 属性
```

### CI / CD

本仓库使用 GitHub Actions：

| 工作流 | 触发条件 | 说明 |
|--------|----------|------|
| `push-nuget.yml` | 推送 `v*` 标签 | 构建、打包并发布至 NuGet.org |
| `release.yml` | 推送 `v*` 标签 | 创建 GitHub Release（含预发布标识检测） |

发布新版本时，推送形如 `v1.0.0` 或 `v1.1.0-preview.1` 的标签即可自动触发。

## License

本项目使用 [MIT](LICENSE) 协议。
