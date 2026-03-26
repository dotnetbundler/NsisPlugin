# 打印 AOT 发布体积

该文档介绍了如果使用 [AotOutputSize](./AotOutputSize/) 中的脚本去打印其中几个项目的 AOT 发布体积。  
主要对比了以下两种情况下的输出体积：

- 手写 NSIS 交互逻辑时的输出体积
- 使用 NsisPlugin 包时的输出体积

同时，脚本也会对常见的 AOT 优化参数做逐项测试，便于观察不同配置对体积的影响。  
查看[.NET AOT 体积优化指南](https://linlccc.com/posts/dotnetaotconfig/)

## 项目说明

- `Empty`：空项目基线，用来观察 AOT 最小体积
- `Hello`：最小导出函数样例，导出 `SayHi`
- `NotUseNsisPlugin`：手写 NSIS 栈交互逻辑，依赖 `unsafe` 和 `Marshal`
- `UseNsisPlugin`：引用 `NsisPlugin` 包，通过属性标注导出插件动作

## 运行前提

- Windows 操作系统
- 已安装 .NET SDK（当前项目目标框架为 `net10.0`）
- 终端进入 `samples` 目录

## 快速开始

```bash
# Empty
.\AotOutputSize\AotOutputSize.cmd .\AotOutputSize\Empty\Empty.csproj win-x86 y

# Hello
.\AotOutputSize\AotOutputSize.cmd .\AotOutputSize\Hello\Hello.csproj win-x86 n

# NotUseNsisPlugin
.\AotOutputSize\AotOutputSize.cmd .\AotOutputSize\NotUseNsisPlugin\NotUseNsisPlugin.csproj win-x86 n

# UseNsisPlugin
.\AotOutputSize\AotOutputSize.cmd .\AotOutputSize\UseNsisPlugin\UseNsisPlugin.csproj win-x86 n
```

## 脚本参数

```text
AotOutputSize.cmd <项目路径.csproj> [RID] [是否保留日志:y/n]
```

- `<项目路径.csproj>`：必填，例如 `./UseNsisPlugin/UseNsisPlugin.csproj`
- `[RID]`：可选，默认 `win-x64`
- `[是否保留日志:y/n]`：可选，默认 `n`
  - `y`：保留 `AotBuildLog.txt`
  - `n`：执行完成后删除日志

## 实测结果

### 1. Base 与 Full 数据分析（仅体积）

| Project          | Base (Bytes) | Full (Bytes) | 减少 (Bytes) |  降幅 |
| ---------------- | -----------: | -----------: | -----------: | ----: |
| Empty            |       868352 |       590336 |       278016 | 32.0% |
| Hello            |       927232 |       634368 |       292864 | 31.6% |
| NotUseNsisPlugin |       890880 |       607744 |       283136 | 31.8% |
| UseNsisPlugin    |      1577472 |      1112576 |       464896 | 29.5% |

### 2. 原运行输出结果

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
