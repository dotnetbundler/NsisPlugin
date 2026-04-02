# .NET Native AOT 发布体积分析

由于 .NET Native AOT 会将运行时与应用编译为原生代码，发布体积通常较大。
本文档通过示例项目对比不同配置下的输出体积，用于评估不同实现方式与优化选项的体积成本。

本文档主要关注以下场景：

- 手写 NSIS 交互时的输出体积
- 使用 NsisPlugin 包时的输出体积
- 常见 AOT 优化参数对输出体积的影响，详情可见[.NET AOT 体积优化指南](https://linlccc.com/posts/dotnetaotconfig/)

本文档使用以下 AOT 优化参数进行对比分析：

- `OptimizationPreference`
  - `size`：优先优化体积
  - `speed`：优先优化速度
- `InvariantGlobalization`
  - `true`：使用不变文化特性，减少全球化相关代码
- `DebuggerSupport`:
  - `false`：禁用调试器支持，减少调试相关代码
- `StackTraceSupport`:
  - `false`：禁用堆栈跟踪支持，减少堆栈跟踪相关代码
- `UseSizeOptimizedLinq`:
  - `true`：使用体积优化的 LINQ 实现
- `UseSystemResourceKeys`:
  - `true`：使用系统资源键，减少资源相关代码

## 项目说明

- `Empty`：空项目基线，用于观察 AOT 最小体积。
- `Hello`：最小导出函数示例，导出 `SayHi`。
- `NotUseNsisPlugin`：手写 NSIS 栈交互逻辑，依赖 `unsafe` 和 `Marshal`
- `UseNsisPlugin`：引用 `NsisPlugin` 包，通过属性标注导出插件动作

## 运行前提

- Windows 操作系统
- 已安装 .NET SDK 10

## 打印项目 AOT 发布体积

说明：以下命令默认在 `AotOutputSize` 目录下执行。

```bash
# 打印 Empty 项目 AOT 发布体积
.\AotOutputSize.cmd .\Empty\Empty.csproj win-x86 n

# 打印 Hello 项目 AOT 发布体积
.\AotOutputSize.cmd .\Hello\Hello.csproj win-x86 n

# 打印 NotUseNsisPlugin 项目 AOT 发布体积
.\AotOutputSize.cmd .\NotUseNsisPlugin\NotUseNsisPlugin.csproj win-x86 n

# 打印 UseNsisPlugin 项目 AOT 发布体积
.\AotOutputSize.cmd .\UseNsisPlugin\UseNsisPlugin.csproj win-x86 n
```

### 脚本说明

命令：`AotOutputSize.cmd <项目路径.csproj> [RID] [是否保留日志:y/n]`

- `<项目路径.csproj>`：必填，例如 `./UseNsisPlugin/UseNsisPlugin.csproj`
- `[RID]`：可选，默认 `win-x64`
- `[是否保留日志:y/n]`：可选，默认 `n`
  - `y`：保留 `AotBuildLog.txt`
  - `n`：执行完成后删除日志

## 实测结果

### 1. Base 与 Full 数据对比（仅体积）

| Project              | Base (Bytes) | Full (Bytes) | 减少 (Bytes) |  降幅 |
| -------------------- | -----------: | -----------: | -----------: | ----: |
| Empty                |       868352 |       590336 |       278016 | 32.0% |
| Hello                |       927232 |       634368 |       292864 | 31.6% |
| NotUseNsisPlugin     |       890880 |       607744 |       283136 | 31.8% |
| UseNsisPlugin v1.0.0 |      1577472 |      1112576 |       464896 | 29.5% |
| UseNsisPlugin v1.0.1 |       894464 |       609792 |      284,672 | 31.8% |

### 2. 原始运行输出

#### Empty

```text
==============================================================================
 Project: [.\Empty\Empty.csproj] | RID: win-x86
==============================================================================
Description                    | Size (Bytes)    | Duration (s)
------------------------------------------------------------------------------
Base AOT Publish               | 868352          | 3.42
OptimizationPreference=size    | 824320          | 3.21
OptimizationPreference=speed   | 876032          | 3.33
InvariantGlobalization=true    | 714240          | 3.21
DebuggerSupport=false          | 868864          | 3.26
StackTraceSupport=false        | 779264          | 3.21
UseSizeOptimizedLinq=true      | 868352          | 3.57
UseSystemResourceKeys=true     | 860160          | 3.28
Full Optimizations             | 590336          | 3.18
------------------------------------------------------------------------------
完成。
```

#### Hello

```text
==============================================================================
 Project: [.\Hello\Hello.csproj] | RID: win-x86
==============================================================================
Description                    | Size (Bytes)    | Duration (s)
------------------------------------------------------------------------------
Base AOT Publish               | 927232          | 3.73
OptimizationPreference=size    | 876032          | 3.61
OptimizationPreference=speed   | 935424          | 3.25
InvariantGlobalization=true    | 773120          | 3.25
DebuggerSupport=false          | 927232          | 3.25
StackTraceSupport=false        | 832000          | 3.28
UseSizeOptimizedLinq=true      | 927232          | 3.46
UseSystemResourceKeys=true     | 918528          | 3.24
Full Optimizations             | 634368          | 3.69
------------------------------------------------------------------------------
完成。
```

#### NotUseNsisPlugin

```text
==============================================================================
 Project: [.\NotUseNsisPlugin\NotUseNsisPlugin.csproj] | RID: win-x86
==============================================================================
Description                    | Size (Bytes)    | Duration (s)
------------------------------------------------------------------------------
Base AOT Publish               | 890880          | 3.53
OptimizationPreference=size    | 842752          | 3.15
OptimizationPreference=speed   | 898048          | 3.31
InvariantGlobalization=true    | 736768          | 3.24
DebuggerSupport=false          | 890880          | 3.34
StackTraceSupport=false        | 799744          | 3.56
UseSizeOptimizedLinq=true      | 890880          | 3.34
UseSystemResourceKeys=true     | 881152          | 3.64
Full Optimizations             | 607744          | 3.28
------------------------------------------------------------------------------
完成。
```

#### UseNsisPlugin

NsisPlugin 版本 <= 1.0.0

```text
==============================================================================
 Project: [.\UseNsisPlugin\UseNsisPlugin.csproj] | RID: win-x86
==============================================================================
Description                    | Size (Bytes)    | Duration (s)
------------------------------------------------------------------------------
Base AOT Publish               | 1577472         | 4.05
OptimizationPreference=size    | 1486336         | 3.77
OptimizationPreference=speed   | 1394176         | 3.55
InvariantGlobalization=true    | 1331712         | 3.56
DebuggerSupport=false          | 1577472         | 3.73
StackTraceSupport=false        | 1430016         | 3.66
UseSizeOptimizedLinq=true      | 1577472         | 4.07
UseSystemResourceKeys=true     | 1568256         | 3.79
Full Optimizations             | 1112576         | 4.09
------------------------------------------------------------------------------
完成。
```

NsisPlugin 版本 >= 1.0.1

```text
==============================================================================
 Project: [.\UseNsisPlugin\UseNsisPlugin.csproj] | RID: win-x86
==============================================================================
Description                    | Size (Bytes)    | Duration (s)
------------------------------------------------------------------------------
Base AOT Publish               | 894464          | 3.44
OptimizationPreference=size    | 845824          | 3.21
OptimizationPreference=speed   | 901120          | 3.19
InvariantGlobalization=true    | 740352          | 3.19
DebuggerSupport=false          | 894464          | 3.31
StackTraceSupport=false        | 802304          | 3.27
UseSizeOptimizedLinq=true      | 894464          | 3.22
UseSystemResourceKeys=true     | 885248          | 3.20
Full Optimizations             | 609792          | 3.10
------------------------------------------------------------------------------
完成。
```

## 相关文档

- [示例总览](./README.md)
- [插件开发、发布、打包](./nsisplugin-usage-sample-release-and-packaging.md)
- [项目主页](../README.md)
- [API 参考](../docs/api-reference.md)
