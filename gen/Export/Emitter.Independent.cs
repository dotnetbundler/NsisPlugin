using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using SourceGenerators;

namespace NsisPlugin.SourceGeneration.Export;

/// <summary>
/// 生成独立入口初始化的导出方法
/// </summary>
internal sealed class IndependentEmitter(SourceProductionContext context) : Emitter(context)
{
    public void Emit(ImmutableEquatableArray<TypeGenerationSpec> types)
    {
        foreach (var type in types)
        {
            var hintName = GetExportFileName(type);
            var sourceText = EmitExportTypeSource(type, WriteMembers);
            Context.AddSource(hintName, sourceText);
        }

        // 写入类成员
        static void WriteMembers(SourceWriter writer, TypeGenerationSpec typeSpec)
        {
            var isFirst = true;
            foreach (var methodSpec in typeSpec.Methods) WriteMethod(writer, typeSpec.Type, methodSpec, ref isFirst);
        }
    }

    private static void WriteMethod(SourceWriter writer, TypeRef containingType, MethodGenerationSpec methodSpec, ref bool isFirst)
    {
        foreach (var actionSpec in methodSpec.Actions)
        {
            // 方法间隔
            if (!isFirst) writer.WriteLine();
            else isFirst = false;

            writer.WriteLine($"[{UnmanagedCallersOnlyAttributeRef}(EntryPoint = {SymbolDisplay.FormatLiteral(actionSpec.EntryPoint, true)}, CallConvs = new[] {{ typeof({CallConvCdeclRef}) }})]");
            writer.WriteLine($"public static void {actionSpec.EntryPoint}_Gen({IntPtrRef} hwndParent, {IntRef} string_size, {IntPtrRef} variables, {IntPtrRef} stacktop, {IntPtrRef} extra)");
            writer.WriteLine('{');
            writer.Indentation++;
            // 方法内部
            {
                // 编码和初始化
                writer.WriteLine($"using {DisposableRef} _ = {NsPluginEncRef}.CreateEncScope({NsEncodingRef}.{actionSpec.Encoding});");
                writer.WriteLine($"{NsPluginRef}.Init(hwndParent, string_size, variables, stacktop, extra);");

                writer.WriteLine("try");
                writer.WriteLine('{');
                writer.Indentation++;
                // try 内部
                {
                    WriteInvocationAndReturn(writer, containingType, methodSpec);
                }
                writer.Indentation--;
                writer.WriteLine('}');
                writer.WriteLine($"catch ({ExceptionRef} ex)");
                writer.WriteLine('{');
                writer.Indentation++;
                // catch 内部
                {
                    writer.WriteLine($"{StackTop}.Push($\"Exception in {actionSpec.EntryPoint}: {{ex}}\");");
                }
                writer.Indentation--;
                writer.WriteLine('}');
            }
            writer.Indentation--;
            writer.WriteLine('}');
        }
    }
}
