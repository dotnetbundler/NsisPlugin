// using System.Text;
// using Microsoft.CodeAnalysis;
// using Microsoft.CodeAnalysis.CSharp.Syntax;
// using Microsoft.CodeAnalysis.Text;
//
// namespace NsisPlugin.SourceGeneration;
//
// /// <summary>
// /// NSIS插件 源代码生成器
// /// </summary>
// [Generator]
// public class NsisPluginSourceGenerator : IIncrementalGenerator
// {
//     public void Initialize(IncrementalGeneratorInitializationContext context)
//     {
//         context.RegisterPostInitializationOutput(ctx => ctx.AddSource(
//             "HelloGenerated.g.cs",
//             SourceText.From("namespace Generated { public static class Hello { public static void Say() => System.Console.WriteLine(\"H2el1lo from Gen!\"); } }", Encoding.UTF8)
//         ));
//     }
//
//     /// <summary>
//     /// 语法节点过滤
//     /// </summary>
//     /// <param name="syntaxNode">语法节点</param>
//     /// <param name="token">取消令牌，通常编译器在大工程、IDE 停止编译时会触发取消</param>
//     /// <returns>是否需要</returns>
//     private static bool SyntaxNodeFilter(SyntaxNode syntaxNode, CancellationToken token)
//     {
//         // 不是类
//         if (syntaxNode is not ClassDeclarationSyntax cds) return false;
//         // 没有注解
//         if (cds.AttributeLists.Count == 0) return false;
//         return true;
//     }
//
//     /// <summary>
//     /// 转换器
//     /// </summary>
//     /// <param name="context">生成器语法上下文</param>
//     /// <param name="token">取消令牌，通常编译器在大工程、IDE 停止编译时会触发取消</param>
//     /// <returns></returns>
//     private static INamedTypeSymbol? Transform(GeneratorSyntaxContext context, CancellationToken token)
//     {
//         var symbol = context.SemanticModel.GetDeclaredSymbol(context.Node, token) as INamedTypeSymbol;
//         return symbol;
//     }
//
//     /// <summary>
//     /// 生成代码
//     /// </summary>
//     /// <param name="context">来源生产上下文</param>
//     /// <param name="classSymbol">类符号</param>
//     private static void GenerateCode(SourceProductionContext context, INamedTypeSymbol? classSymbol)
//     {
//     }
//
//     private void GenerateModuleInitialization(IncrementalGeneratorInitializationContext context)
//     {
//     }
// }


