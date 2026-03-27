# NsisPlugin 使用示例、发布及打包

本文档主要介绍以下内容：

- NsisPlugin 的[使用示例](./Plugins/UseNsisPlugin/)
- 如何发布 NSIS 插件
- 如何在 NSIS 脚本中使用发布后的插件

## 前提条件

- Windows 操作系统
- 已安装 .NET SDK 10

## 示例介绍

示例位于 [Plugins](./Plugins/) 目录下，包含以下内容：

- [UseNsisPlugin](./Plugins/UseNsisPlugin/)：使用 NsisPlugin 开发插件示例。（学习只看这个）
- [UseLocalNsisPlugin](./Plugins/UseLocalNsisPlugin/)：使用本地 NsisPlugin 开发插件示例。(用于验证本地 NsisPlugin)
- [NotUseNsisPlugin](./Plugins/NotUseNsisPlugin/)：手写 NSIS 交互的方式开发插件。

## 发布及打包
>
> ps. 以下命令默认在 `NsisScript` 目录下执行

### 发布插件并打包可执行文件

```bash
# 发布所有插件并打包所有 NSIS 脚本
.\PublishPlugin.cmd && .\NSISPackaging.cmd

# 发布 UseNsisPlugin 插件并打包 UseNsisPlugin.nsi 脚本
.\PublishPlugin.cmd UseNsisPlugin && .\NSISPackaging.cmd UseNsisPlugin.nsi
```

### 发布插件
>
> ps. 发布的都是 [Plugins](./Plugins/) 目录下的插件项目
> 发布后的插件在 `NsisScript/addplugin` 目录下

```bash
# 发布所有插件
.\PublishPlugin.cmd

# 发布 UseNsisPlugin 插件
.\PublishPlugin.cmd UseNsisPlugin
```

#### 命令说明

命令：`dotnet publish <项目> -c Release -o <输出目录> /p:GenerateDocumentationFile=false /p:DebugType=none /p:DebugSymbols=false`
这里使用这个命令是为了让发布结果保持干净，避免生成不必要的 XML 文档文件和 PDB 调试文件。参数说明如下：

- `-o`：输出到指定目录
- `/p:GenerateDocumentationFile`：避免生成 XML 文档文件
- `/p:DebugType` 和 `/p:DebugSymbols`：避免生成 PDB 调试文件

### 打包 NSIS 可执行程序
>
> ps. 打包的都是 [NsisScripts](./NsisScripts/) 目录下的 NSIS 脚本
> 打包后的可执行文件在 `NsisScript` 目录下与脚本同名

```bash
# 打包所有 NSIS 脚本
.\NSISPackaging.cmd

# 打包 UseNsisPlugin.nsi 脚本
.\NSISPackaging.cmd UseNsisPlugin.nsi
```
