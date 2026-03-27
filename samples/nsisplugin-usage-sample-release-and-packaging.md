# NsisPlugin 使用示例、发布及打包

本文档说明示例项目的使用方式，以及插件发布与 NSIS 打包的标准流程。

- NsisPlugin 的[使用示例](./Plugins/UseNsisPlugin/)
- 如何发布 NSIS 插件
- 如何在 NSIS 脚本中使用发布后的插件

## 前提条件

- Windows 操作系统
- 已安装 .NET SDK 10

## 示例介绍

示例位于 [Plugins](./Plugins/) 目录下，包含以下项目：

- [UseNsisPlugin](./Plugins/UseNsisPlugin/)：使用 NsisPlugin 开发插件的标准示例，建议优先阅读。
- [UseLocalNsisPlugin](./Plugins/UseLocalNsisPlugin/)：引用本地源码版本的示例，用于验证仓库中的本地改动。
- [NotUseNsisPlugin](./Plugins/NotUseNsisPlugin/)：手写 NSIS 互操作逻辑的对照实现。

## 发布及打包

说明：以下命令默认在 `NsisScripts` 目录下执行。

### 发布插件并打包可执行文件

```bash
# 发布所有插件并打包所有 NSIS 脚本
.\PublishPlugin.cmd && .\NSISPackaging.cmd

# 发布 UseNsisPlugin 插件并打包 UseNsisPlugin.nsi 脚本
.\PublishPlugin.cmd UseNsisPlugin && .\NSISPackaging.cmd UseNsisPlugin.nsi
```

### 发布插件

说明：发布对象为 [Plugins](./Plugins/) 目录下的插件项目，输出位于 `NsisScripts/addplugin`。

```bash
# 发布所有插件
.\PublishPlugin.cmd

# 发布 UseNsisPlugin 插件
.\PublishPlugin.cmd UseNsisPlugin
```

#### 命令说明

命令：`dotnet publish <项目> -c Release -o <输出目录> /p:GenerateDocumentationFile=false /p:DebugType=none /p:DebugSymbols=false`

该命令用于保持发布结果尽可能精简，避免生成不必要的 XML 文档文件和 PDB 调试文件。参数说明如下：

- `-o`：输出到指定目录
- `/p:GenerateDocumentationFile`：避免生成 XML 文档文件
- `/p:DebugType` 和 `/p:DebugSymbols`：避免生成 PDB 调试文件

### 打包 NSIS 可执行程序

说明：打包对象为 [NsisScripts](./NsisScripts/) 目录下的脚本，输出文件位于 `NsisScripts`，且与脚本同名。

```bash
# 打包所有 NSIS 脚本
.\NSISPackaging.cmd

# 打包 UseNsisPlugin.nsi 脚本
.\NSISPackaging.cmd UseNsisPlugin.nsi
```

## 相关文档

- [示例总览](./README.md)
- [项目主页](../README.md)
- [API 参考](../docs/api-reference.md)
- [源生成器约束与诊断](../docs/source-generator-diagnostics.md)
