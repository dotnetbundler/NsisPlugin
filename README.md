# NsisPlugin

`NsisPlugin` 是一个面向 C# 的 NSIS 插件开发库。

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

### 导出一个最简单的插件函数

- 导出 `Add` 为插件入口点
- 使用默认编码（由 MSBuild 属性 `NSISUnicode` 控制）
- 依次从 NSIS 栈弹出参数 `a`、`b`
- 将返回值压回 NSIS 栈

```csharp
using NsisPlugin;

public static class MyPlugin
{
    [NsisAction]
    public static int Add(int a, int b) => a + b;
}
```

## 配置

包会自动导入以下 MSBuild 属性：

```xml
<PropertyGroup>
  <NSISUnicode>false</NSISUnicode>
  <AutoGenerateNsisPluginInitializer>true</AutoGenerateNsisPluginInitializer>
</PropertyGroup>
```

| 配置项                                 | 默认值     | 说明                                      |
|-------------------------------------|---------|-----------------------------------------|
| `NSISUnicode`                       | `false` | 设置默认编码。`false` 为 ANSI，`true` 为 Unicode。 |
| `AutoGenerateNsisPluginInitializer` | `true`  | 自动生成插件初始化代码。大多数场景建议保持默认值。               |

### 方法级编码覆盖

如果某个导出入口需要和默认编码不同，可以单独指定：

```csharp
[NsisAction("DoWorkA", Encoding = NsEncoding.Ansi)]
[NsisAction("DoWorkU", Encoding = NsEncoding.Unicode)]
```

## API 概览

### 特性

| API                     | 作用               |
|-------------------------|------------------|
| `NsisActionAttribute`   | 声明插件入口点          |
| `FromVariableAttribute` | 将参数绑定到指定 NSIS 变量 |
| `ToVariableAttribute`   | 将返回值写入指定 NSIS 变量 |

### 核心类型

| API               | 用途         | 常用成员                                                                         |
|-------------------|------------|------------------------------------------------------------------------------|
| `NsPlugin`        | 当前插件调用上下文  | `HwndParent`、`StringSize`、`Variables`、`StackTop`、`ExtraParameters`           |
| `StackT`          | 读写 NSIS 栈  | `Pop`、`Push`                                                                 |
| `Variables`       | 读写 NSIS 变量 | `Get`、`Set`                                                                  |
| `ExtraParameters` | 使用额外参数能力   | `ExecFlags`、`ExecuteCodeSegment`、`ValidateFilename`、`RegisterPluginCallback` |
| `NsPluginEnc`     | 控制当前编码     | `IsUnicode`、`Encoding`、`CreateEncScope`                                      |

### 扩展方法

`StackT` 和 `Variables` 还提供了泛型扩展方法，适合直接读写常见基础类型：

- `StackT.Pop<T>(out T value)`
- `StackT.Push<T>(T value)`
- `Variables.Get<T>(NsVariable variable, out T value)`
- `Variables.Set<T>(NsVariable variable, T value)`

### 常用枚举

| API          | 说明                                                 |
|--------------|----------------------------------------------------|
| `NsVariable` | NSIS 变量枚举，例如 `Inst0` ~ `Inst9`、`InstR0` ~ `InstR9` |
| `NsEncoding` | 编码枚举：`Undefined`、`Ansi`、`Unicode`                  |
| `Nspim`      | 插件回调消息类型                                           |

## 导出函数规则

带 `[NsisAction]` 的方法建议遵循以下规则：

- 使用 `static` 方法
- 使用 `public` 或 `internal` 可见性
- 不使用泛型方法
- 不使用 `ref`、`out`、`in` 参数
- 保证入口点名称合法且唯一

如果不满足这些条件，项目会给出对应诊断，提示该方法无法正常导出。

## 注意事项

- **先确认编码**：安装器是 ANSI 还是 Unicode，必须和插件侧配置一致；编码不一致通常会直接表现为字符串乱码。
- **了解返回值规则**：普通返回值会压回栈；如果使用 `[ToVariable]`，结果会写入指定变量，而不是压栈。
- **注意字符串长度**：写入栈或变量的字符串会受到 NSIS `string_size` 限制，超出长度时会被截断。
- **注意类型转换**：普通参数和变量绑定参数最终都需要转换成目标类型；如果字符串内容不能转换，调用会失败。
- **关闭自动初始化时要自行处理**：如果把 `AutoGenerateNsisPluginInitializer` 设为 `false`，请确保你已经正确完成初始化和编码设置。
- **带 `[ToVariable]` 的方法必须返回值**：如果方法返回 `void`，结果无法写入变量。

如需查看更完整的行为边界和用例，可参考：

- `tests/NsisPlugin.Test/`
- `tests/NsisPlugin.SourceGeneration.Tests/`

## License

本项目使用 [MIT](LICENSE) 协议。
