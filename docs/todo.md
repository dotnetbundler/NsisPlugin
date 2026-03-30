## 待办

- [x] 将 README 中 API 文档部分移植到 docs 下，并且在 README 中添加链接
- [x] 将 README 中源生成器约束与诊断部分移植到 docs 下，并且在 README 中添加链接
- [x] 在 README 中添加指向 NsisPlugin 示例的链接
- [x] 在 README 中添加指向 AOT 发布体积示例的链接
- [x] NsisPlugin 包默认使用配置添加（不行，还原时不会生效，会导致发布失败）
  - PublishAot=true
  - IsAotCompatible=true
  - NativeLib=Shared
  - RuntimeIdentifier=win-x86
- [x] 在 README 中指出默认配置并说明为什么（移除）
- [x] 代码：将多目标兼容写道公共目录中去
- [x] bug：方法级的编码应该在 try 外部，否者 catch 中的是未知编码
- [x] 看导出函数（UnmanagedCallersOnlyAttribute）是否支持多标注(不支持)
- [ ] 代码：处理.net framework 下的测试
- [ ] 代码：解决范围编码释放可能无序的问题
- [ ] 代码：解决范围编码跨线程释放的问题
- [ ] 代码：使用 AsyncLocal 来替换 ThreadStatic 来存储当前插件上下文，避免在异步方法中丢失上下文
- [ ] 代码：导出函数的公共部分提取出来，通过调用执行
