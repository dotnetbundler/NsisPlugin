using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;

namespace NsisPlugin.SourceGeneration.Tests.Helper;

public static class AssertHelper
{
    /// <summary>
    /// 验证快照
    /// </summary>
    /// <param name="target">目标</param>
    /// <param name="snapshotsDirectory">快照目录</param>
    /// <param name="methodName">调用方法名（auto）</param>
    /// <param name="filePath">调用文件名（auto）</param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static Task VerifySnapshot<T>(T target, string snapshotsDirectory, [CallerMemberName] string methodName = "", [CallerFilePath] string filePath = "")
    {
        // 如果快照目录不是绝对路径
        // 则将其视为相对于调用测试方法的源代码文件所在目录的路径
        if (!Path.IsPathRooted(snapshotsDirectory))
        {
            var directory = Path.GetDirectoryName(filePath);
            Assert.True(Directory.Exists(directory), $"{directory} does not exist");
            snapshotsDirectory = Path.Combine(directory, snapshotsDirectory);
        }

        // 配置 Verify 以使用特定的目录和文件名存储快照
        var settings = new VerifySettings();
        settings.UseDirectory(snapshotsDirectory);
        settings.UseFileName(methodName);
        settings.DisableRequireUniquePrefix();

        // 验证生成了代码快照
        return Verify(target, settings);
    }

    /// <summary>
    /// 验证诊断信息的 ID 是否与预期的 ID 列表完全匹配，并且顺序一致
    /// </summary>
    /// <param name="diagnostics">诊断列表</param>
    /// <param name="expectedIds">预期的诊断 ID 列表</param>
    public static void AssertDiagnosticIdsInOrder(IEnumerable<Diagnostic> diagnostics, params IEnumerable<string> expectedIds)
    {
        var actual = diagnostics.Select(d => d.Id).ToArray();
        var expected = expectedIds.ToArray();
        Assert.Equal(expected, actual);
    }

    /// <summary>
    /// 验证诊断信息的 ID 是否与预期的 ID 列表完全匹配，并且顺序一致
    /// </summary>
    /// <param name="diagnostics">诊断列表</param>
    /// <param name="expectedIds">预期的诊断 ID 列表</param>
    public static void AssertDiagnosticIds(IEnumerable<Diagnostic> diagnostics, params IEnumerable<string> expectedIds)
    {
        AssertDiagnosticIdsInOrder(diagnostics.OrderBy(d => d.Id), expectedIds.OrderBy(id => id));
    }
}
