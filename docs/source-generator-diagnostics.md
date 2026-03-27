# NsisPlugin 源生成器约束与诊断

本文档说明 NsisPlugin 源生成器对导出方法与初始化器生成的约束规则，以及对应的诊断 ID。

## 初始化器诊断

| 诊断 ID     | 级别    | 说明                                                          |
| ----------- | ------- | ------------------------------------------------------------- |
| `NSPGEN001` | Warning | C# 语言版本低于 9.0，无法生成模块初始化器。                   |
| `NSPGEN002` | Warning | 找不到 `ModuleInitializerAttribute`，无法生成模块初始化器。   |
| `NSPGEN003` | Info    | `AutoGenerateNsisPluginInitializer` 未设为 `true`，跳过生成。 |

## 导出函数约束

标注 `NsisActionAttribute` 的方法需要满足以下约束，以避免触发诊断：

- 必须是 `static`。
- 不能是 `abstract`。
- 不能是泛型方法。
- 方法所在类型必须是命名类型，且不能是泛型类型。
- 方法与包含类型需具备有效可访问性（`public` / `internal`）。
- 参数不能包含 `ref` / `out` / `in`。
- 使用 `ToVariableAttribute` 时，返回类型不能是 `void`。

## 导出函数诊断

以下诊断用于标识导出方法声明、入口点命名以及生成条件中的问题：

| 诊断 ID     | 级别    | 说明                                                                                                                                                                         |
| ----------- | ------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `NSPGEN101` | Warning | 方法不满足导出条件，例如：不是 `static`、是 `abstract`、是泛型方法、方法或其包含类型不是 `public/internal` 可访问、参数包含 `ref/out/in`、不在命名类型中、或包含类型是泛型。 |
| `NSPGEN102` | Warning | 带 `ToVariableAttribute` 的方法不能返回 `void`。                                                                                                                             |
| `NSPGEN121` | Error   | 入口点与其他导出冲突。                                                                                                                                                       |
| `NSPGEN122` | Error   | 入口点格式字符串无效。                                                                                                                                                       |
| `NSPGEN123` | Error   | 入口点名称无效，不是合法标识符或包含不支持导出的字符。                                                                                                                       |

## 相关文档

- [API 参考](api-reference.md)
- [示例总览](../samples/README.md)
- [项目主页](../README.md)
