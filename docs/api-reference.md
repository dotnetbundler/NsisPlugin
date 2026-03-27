# NsisPlugin API 参考

本文档描述 NsisPlugin 的公开 API，包括类型职责、成员签名、线程语义、返回值约定以及与 NSIS 主机的互操作边界。

## 目录

- [文档约定](#文档约定)
- [特性（Attributes）](#特性attributes)
- [核心类型](#核心类型)
- [扩展方法](#扩展方法)
- [枚举与委托](#枚举与委托)
- [底层互操作类型（高级）](#底层互操作类型高级)

## 文档约定

- 命名空间：主 API 位于 `NsisPlugin`，底层互操作结构位于 `NsisPlugin.NsisApi`。
- 返回值：`bool` 一般表示操作是否成功；失败通常不会抛出异常。
- 编码策略：由 `NsPluginEnc` 的全局配置与线程作用域配置共同决定。
- 线程语义：`NsPlugin` 的上下文字段基于 `[ThreadStatic]` 实现，必须在当前线程完成初始化后再访问。

## 特性（Attributes）

### NsisActionAttribute

声明可导出的插件入口。该特性由源生成器消费，用于生成 NSIS 导出包装逻辑。

```csharp
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public class NsisActionAttribute(string entryPointFormat = "{0}") : Attribute
{
    public NsEncoding Encoding { get; set; }
}
```

| 成员               | 类型         | 说明                                                        |
| ------------------ | ------------ | ----------------------------------------------------------- |
| `entryPointFormat` | `string`     | 导出入口格式，默认 `"{0}"`（方法名）。例如 `"Plugin_{0}"`。 |
| `Encoding`         | `NsEncoding` | 当前入口编码。`Undefined` 时跟随全局编码。                  |

使用说明：

- 可重复标注同一方法（`AllowMultiple = true`），用于生成多个导出入口名。
- 该特性仅描述导出元数据，不直接处理运行时参数绑定。

### FromVariableAttribute

声明参数来自 NSIS 变量区，而不是来自栈弹出。

```csharp
[AttributeUsage(AttributeTargets.Parameter)]
public class FromVariableAttribute(NsVariable variable) : Attribute;
```

| 参数       | 类型         | 说明             |
| ---------- | ------------ | ---------------- |
| `variable` | `NsVariable` | 绑定的变量槽位。 |

### ToVariableAttribute

声明返回值写入 NSIS 变量，而不是压回栈。

```csharp
[AttributeUsage(AttributeTargets.ReturnValue)]
public class ToVariableAttribute(NsVariable variable) : Attribute;
```

| 参数       | 类型         | 说明               |
| ---------- | ------------ | ------------------ |
| `variable` | `NsVariable` | 写入目标变量槽位。 |

## 核心类型

### NsPlugin

NSIS 运行时上下文入口（静态类）。

```csharp
public static class NsPlugin
```

| 成员                                                                                       | 类型              | 说明                                                    |
| ------------------------------------------------------------------------------------------ | ----------------- | ------------------------------------------------------- |
| `ModuleHandle`                                                                             | `IntPtr`          | 当前插件模块句柄。通常由生成初始化器设置。              |
| `HwndParent`                                                                               | `IntPtr`          | NSIS 父窗口句柄（线程静态）。                           |
| `StringSize`                                                                               | `int`             | NSIS 字符缓冲区长度（线程静态）。                       |
| `Variables`                                                                                | `Variables`       | 变量访问器（线程静态）。                                |
| `StackTop`                                                                                 | `StackT`          | 栈访问器（线程静态）。                                  |
| `ExtraParameters`                                                                          | `ExtraParameters` | 额外能力访问器（线程静态）。                            |
| `MaxStringBytes`                                                                           | `int`             | 最大字符串字节数：`StringSize * NsPluginEnc.CharSize`。 |
| `Init(IntPtr hwndParent, int stringSize, IntPtr variables, IntPtr stacktop, IntPtr extra)` | `void`            | 绑定当前线程上下文。                                    |

线程与生命周期：

- `Init` 应在每个调用线程进入插件逻辑之前执行。
- 未初始化时访问线程静态上下文可能导致失败或未定义行为。

### Variables

封装 NSIS 变量缓冲区访问。

```csharp
public class Variables(IntPtr variables)
```

| 成员                                          | 返回值   | 说明                             |
| --------------------------------------------- | -------- | -------------------------------- |
| `Raw`                                         | `IntPtr` | 原始变量缓冲区指针。             |
| `Get(NsVariable variable, out string? value)` | `bool`   | 读取变量；成功时输出非空字符串。 |
| `Set(NsVariable variable, string value)`      | `bool`   | 写入变量字符串。                 |

行为约定：

- 变量索引越界（`< Inst0` 或 `>= InstLast`）返回 `false`。
- 原始指针为 `IntPtr.Zero` 返回 `false`。
- 写入超长字符串会截断到 NSIS 可容纳长度。

### StackT

封装 NSIS 栈操作。

```csharp
public unsafe class StackT(IntPtr stackTop)
```

| 成员                   | 返回值      | 说明               |
| ---------------------- | ----------- | ------------------ |
| `Raw`                  | `stack_t**` | 栈顶二级指针。     |
| `Pop(out string? str)` | `bool`      | 弹出栈顶字符串。   |
| `Push(string str)`     | `bool`      | 压入字符串到栈顶。 |

行为约定：

- `Pop` 成功后会释放被弹出节点内存。
- `Push` 使用当前编码写入并执行长度截断。
- 当 `Raw` 或 `*Raw` 不可用时，`Pop` 返回 `false`。

### ExtraParameters

封装 NSIS `extra_parameters` 能力，包括执行代码段、文件名校验与卸载回调。

```csharp
public unsafe class ExtraParameters(IntPtr extraPtr)
```

| 成员                                                | 返回值              | 说明                                                     |
| --------------------------------------------------- | ------------------- | -------------------------------------------------------- |
| `Raw`                                               | `extra_parameters*` | 原始互操作结构指针。                                     |
| `ExecFlags`                                         | `ref ExecFlags`     | 执行标志结构体引用。                                     |
| `ExecuteCodeSegment(int code)`                      | `int`               | 执行 NSIS 代码段。成功返回 `0`。                         |
| `ValidateFilename(ref string filename)`             | `void`              | 调用 NSIS 逻辑规范化文件名，原地更新参数。               |
| `RegisterPluginCallback(NsPluginCallback callback)` | `int`               | 注册插件回调：`0` 成功，`1` 已注册，其它值为主机返回码。 |

行为约定：

- 回调注册成功后会持有委托引用，以防止被 GC 回收。
- 当前实现为单次注册模型，重复注册会直接返回 `1`。

### NsPluginEnc

统一控制 ANSI/Unicode 编码选择。

```csharp
public static class NsPluginEnc
```

| 成员                                  | 类型                   | 说明                                         |
| ------------------------------------- | ---------------------- | -------------------------------------------- |
| `UseUnicode`                          | `bool`                 | 全局编码开关。`false`=ANSI，`true`=Unicode。 |
| `ScopeUseUnicode`                     | `bool?`                | 线程作用域覆盖（`null` 表示跟随全局）。      |
| `IsUnicode`                           | `bool`                 | 当前线程最终编码选择。                       |
| `CharSize`                            | `int`                  | 当前编码字符宽度（ANSI=1，Unicode=2）。      |
| `Encoding`                            | `System.Text.Encoding` | 当前编码对象。                               |
| `PtrToString(IntPtr ptr)`             | `string?`              | 按当前编码将非托管字符串指针转为托管字符串。 |
| `CreateEncScope(NsEncoding encoding)` | `IDisposable`          | 创建临时编码作用域。                         |

建议用法：

```csharp
using (NsPluginEnc.CreateEncScope(NsEncoding.Unicode))
{
    // 作用域内临时使用 Unicode
}
```

### NsPluginEncScope

`NsPluginEnc.CreateEncScope` 的具体返回类型，用于作用域切换并自动恢复。

```csharp
public sealed class NsPluginEncScope : IDisposable
```

| 成员                                    | 返回值   | 说明                         |
| --------------------------------------- | -------- | ---------------------------- |
| `NsPluginEncScope(NsEncoding encoding)` | 构造函数 | 设置当前线程编码作用域。     |
| `Dispose()`                             | `void`   | 恢复进入作用域前的编码设置。 |

### ExecFlags

映射 NSIS `exec_flags_t`。字段为 `int`，作为布尔使用时遵循 `0=false`、非 `0=true`。

```csharp
[StructLayout(LayoutKind.Sequential)]
public struct ExecFlags
```

| 字段               | 说明                             |
| ------------------ | -------------------------------- |
| `Autoclose`        | 自动关闭标志。                   |
| `AllUserVar`       | 变量作用域，`0` 用户、`1` 机器。 |
| `ExecError`        | 执行错误标志。                   |
| `Abort`            | 中止标志。                       |
| `ExecReboot`       | 需要重启标志。                   |
| `RebootCalled`     | 已调用重启标志。                 |
| `XxxCurInsttype`   | 已弃用，保留 ABI 布局兼容。      |
| `PluginApiVersion` | 插件 API/ABI 版本。              |
| `Silent`           | 静默模式标志。                   |
| `InstdirError`     | 安装目录错误标志。               |
| `Rtl`              | RTL 语言标志。                   |
| `Errlvl`           | 错误级别。                       |
| `AlterRegView`     | 注册表视图设置。                 |
| `StatusUpdate`     | 状态更新/详细输出标志。          |

## 扩展方法

`NsPluginExtensions` 提供基于字符串转换的泛型便捷 API，用于减少手动解析与格式化逻辑。

| 扩展目标    | 成员                                      | 返回值 | 说明                          |
| ----------- | ----------------------------------------- | ------ | ----------------------------- |
| `StackT`    | `Pop<T>(out T? val)`                      | `bool` | 弹出字符串并尝试转换为 `T`。  |
| `StackT`    | `Push<T>(T val)`                          | `bool` | 将 `T` 转为字符串后压栈。     |
| `Variables` | `Get<T>(NsVariable variable, out T? val)` | `bool` | 读取变量并转换为 `T`。        |
| `Variables` | `Set<T>(NsVariable variable, T val)`      | `bool` | 将 `T` 转为字符串后写入变量。 |

转换规则：

- 基于 `Convert.ChangeType`，支持可空类型的基础转换。
- 任一转换失败均返回 `false`，不会抛出异常。

## 枚举与委托

### NsVariable

NSIS 变量索引枚举。

| 值                  | 对应 NSIS 变量               |
| ------------------- | ---------------------------- |
| `Inst0` ~ `Inst9`   | `$0` ~ `$9`                  |
| `InstR0` ~ `InstR9` | `$R0` ~ `$R9`                |
| `InstCmdline`       | `$CMDLINE`                   |
| `InstInstdir`       | `$INSTDIR`                   |
| `InstOutdir`        | `$OUTDIR`                    |
| `InstExedir`        | `$EXEDIR`                    |
| `InstLang`          | `$LANGUAGE`                  |
| `InstLast`          | 边界标记（不用于业务读写）。 |

### NsEncoding

字符串编码枚举。

| 值          | 说明                                  |
| ----------- | ------------------------------------- |
| `Undefined` | 未显式指定，使用全局/作用域计算结果。 |
| `Ansi`      | ANSI（系统默认代码页）。              |
| `Unicode`   | Unicode（UTF-16 LE）。                |

### Nspim

插件卸载相关消息。

| 值               | 说明                               |
| ---------------- | ---------------------------------- |
| `NspimUnload`    | 插件卸载前最后一条消息。           |
| `NspimGuiunload` | GUI 卸载消息（`.onGUIEnd` 之后）。 |

### NsPluginCallback

插件回调委托。

```csharp
[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
public delegate IntPtr NsPluginCallback(Nspim message);
```

该委托通过 `ExtraParameters.RegisterPluginCallback` 注册，并由 NSIS 主机在卸载阶段调用。

## 底层互操作类型（高级）

以下类型用于与 NSIS 原生 ABI 对齐。通常通过 `Raw` 指针间接访问，不建议在业务代码中直接操作。

### NsisPlugin.NsisApi.stack_t

```csharp
[StructLayout(LayoutKind.Sequential)]
public unsafe struct stack_t
```

| 字段   | 类型       | 说明                                 |
| ------ | ---------- | ------------------------------------ |
| `next` | `stack_t*` | 下一个栈节点。                       |
| `text` | `byte`     | 内联字符串首字节，后续字节连续存储。 |

### NsisPlugin.NsisApi.extra_parameters

```csharp
[StructLayout(LayoutKind.Sequential)]
public unsafe struct extra_parameters
```

| 字段                     | 类型                                                | 说明                   |
| ------------------------ | --------------------------------------------------- | ---------------------- |
| `exec_flags`             | `ExecFlags*`                                        | 指向执行标志。         |
| `ExecuteCodeSegment`     | `delegate* unmanaged[Stdcall]<int, IntPtr, int>`    | 执行代码段函数指针。   |
| `validate_filename`      | `delegate* unmanaged[Stdcall]<IntPtr, void>`        | 文件名规范化函数指针。 |
| `RegisterPluginCallback` | `delegate* unmanaged[Stdcall]<IntPtr, IntPtr, int>` | 回调注册函数指针。     |

## 相关文档

- [项目主页](../README.md)
- [源生成器约束与诊断](source-generator-diagnostics.md)
- [示例总览](../samples/README.md)
