# Nsis 脚本使用

该文档介绍了如何使用 [NsisSctipt](./NsisScripts/) 中的脚本。

## 前提条件

- Windows 操作系统
- 已安装 .NET SDK（当前项目目标框架为 `net10.0`）
- 终端进入 `samples` 目录

## 使用

### 发布插件并打包可执行文件

```bash
# 发布所有插件并打包所有 NSIS 脚本
.\NsisScripts\PublishPlugin.cmd && .\NsisScripts\NSISPackaging.cmd

# 发布 UseNsisPlugin 插件并打包 UseNsisPlugin.nsi 脚本
.\NsisScripts\PublishPlugin.cmd UseNsisPlugin && .\NsisScripts\NSISPackaging.cmd UseNsisPlugin.nsi
```

### 发布插件
>
> ps. 发布插件依赖于 `Plugins` 中的项目
> 发布后的插件在 `NsisScript/addplugin` 目录下

```bash
# 发布所有插件
.\NsisScripts\PublishPlugin.cmd

# 发布 UseNsisPlugin 插件
.\NsisScripts\PublishPlugin.cmd UseNsisPlugin
```

### 通过 NSIS 脚本打包可执行文件
>
> 打包后的可执行文件在 `NsisScript` 目录下
> 与 NSIS 脚本同名

```bash
# 打包所有 NSIS 脚本
.\NsisScripts\NSISPackaging.cmd

# 打包 UseNsisPlugin.nsi 脚本
.\NsisScripts\NSISPackaging.cmd UseNsisPlugin.nsi
```
