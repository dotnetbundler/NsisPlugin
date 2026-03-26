# NsisPlugin 样品

该目录包含以下内容

- NsisPlugin 的使用示例 [Plugins](./Plugins/)
- 插件的[发布以及使用](./nsis-script-use.md)
- [Aot 发布体积分析](./print-aot-plugin-size.md)

## 目录结构

```text
samples/
├── AotOutputSize/                  # AOT 发布体积分析示例
├── NsisScripts/                    # NSIS 脚本使用示例
├── Plugins/                        # 插件示例
│   ├── NotUseNsisPlugin/               # 不使用 NsisPlugin 的 NSIS 脚本示例
│   ├── UseNsisPlugin/                  # NsisPlugin Nuegt 包使用示例（学习只看这个即可）
│   └── UseLocalUsisPlugin/             # NsisPlugin 本地程序使用示例
├── nsis-script-use.md                # NSIS 脚本使用说明
├── print-aot-plugin-size.md           # 打印 AOT 发布体积说明
└── README.md
```
