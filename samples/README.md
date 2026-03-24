# NsisPlugin 样品

该目录包含以下内容

- NsisPlugin 的使用示例
- 插件的发布示例
- NSIS 脚本使用插件示例


## 目录结构

```text
samples/
├── scripts/
│   ├── addplugin/                  # 通过 PublishPlugin.cmd 发布的插件目录
│   ├── NSIS/                       # NSIS 打包程序目录
│   ├── NSISPackaging.cmd           # 用于将 NSIS 脚本打包成可执行文件的脚本
│   ├── PublishPlugin.cmd           # 用于发布插件的脚本
│   ├── UseLocalNsisPlugin.exe      # UseLocalNsisPlugin NSIS 脚本打包的程序（需要自己打包）
│   ├── UseLocalNsisPlugin.nsi      # UseLocalNsisPlugin NSIS 脚本
│   ├── UseNsisPlugin.exe           # UseNsisPlugin NSIS 脚本打包的程序（需要自己打包）
│   └── UseNsisPlugin.nsi           # UseNsisPlugin NSIS 脚本
├── UseNsisPlugin/                  # NsisPlugin Nuegt 包使用示例（学习只看这个即可）
├── UseLocalUsisPlugin/             # NsisPlugin 本地程序使用示例
└── README.md
```

## 使用
> ps. 需要在 Windows 环境下使用  
> 工作目录需要切换到 samples 目录下

### 1. 发布插件
> 发布后的脚本文件会被放在 `scripts/addplugin` 目录下

```bash
# 发布样品中的所有插件
.\scripts\PublishPlugin.cmd

# 发布 UseNsisPlugin 插件
.\scripts\PublishPlugin.cmd UseNsisPlugin

# 发布 UseLocalNsisPlugin 插件
.\scripts\PublishPlugin.cmd UseLocalNsisPlugin
```

### 2. 打包可执行文件
> 打包后的可执行文件会被放在 `scripts` 目录下

```bash
# 打包所有 NSIS 脚本
.\scripts\NSISPackaging.cmd

# 打包 UseNsisPlugin.nsi 脚本
.\scripts\NSISPackaging.cmd UseNsisPlugin.nsi

# 打包 UseLocalNsisPlugin.nsi 脚本
.\scripts\NSISPackaging.cmd UseLocalNsisPlugin.nsi
```

#### 发布插件并打包可执行文件

```bash
# 发布 UseLocalNsisPlugin 插件并打包 UseLocalNsisPlugin.nsi
.\scripts\PublishPlugin.cmd UseLocalNsisPlugin && .\scripts\NSISPackaging.cmd UseLocalNsisPlugin.nsi
```

### 3. 执行看看效果

双击运行 `UseNsisPlugin.exe` 和 `UseLocalNsisPlugin.exe`，看看效果吧！
