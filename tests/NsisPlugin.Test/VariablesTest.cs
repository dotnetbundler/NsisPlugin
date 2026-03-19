using NsisPlugin.Test.Helper;
using Xunit;

namespace NsisPlugin.Test;

public class VariablesTest
{
    [Theory]
    [InlineData(Encodings.Ansi, "ansi")]
    [InlineData(Encodings.Unicode, "你好")]
    public void Variables_ShouldReadAndWriteValue(Encodings encoding, string expectedValue)
    {
        const int stringSize = 64;
        var variablesPtr = CreateVariables(encoding, stringSize);

        try
        {
            using var _ = NsPluginEnc.CreateEncScope(encoding);
            NsPlugin.Init(IntPtr.Zero, stringSize, variablesPtr, IntPtr.Zero, IntPtr.Zero);

            for (var i = NsVariable.Inst0; i < NsVariable.InstLast; i++)
            {
                // 初始空
                Assert.True(NsPlugin.Variables.Get(i, out var actualValue));
                Assert.Empty(actualValue);

                // 写入并读取验证
                var setValue = $"{expectedValue}_{i}";
                Assert.True(NsPlugin.Variables.Set(i, setValue));
                Assert.True(NsPlugin.Variables.Get(i, out actualValue));
                Assert.Equal(setValue, actualValue);
            }
        }
        finally
        {
            TestUnmanagedMemory.Free(variablesPtr);
        }
    }

    [Theory]
    [InlineData(Encodings.Ansi)]
    [InlineData(Encodings.Unicode)]
    public void Variables_ShouldNotWriteOrReadOutOfBounds(Encodings encoding)
    {
        const int stringSize = 16;
        var variablesPtr = CreateVariables(encoding, stringSize);

        try
        {
            using var _ = NsPluginEnc.CreateEncScope(encoding);
            NsPlugin.Init(IntPtr.Zero, stringSize, variablesPtr, IntPtr.Zero, IntPtr.Zero);

            // 计算写入和预期值
            var overflowChar = encoding == Encodings.Unicode ? '你' : 'x';
            var tooLongValue = new string(overflowChar, stringSize + 16);
            var expectedTruncated = new string(overflowChar, stringSize - 1);

            for (var i = NsVariable.Inst0; i < NsVariable.InstLast; i++)
            {
                // 初始空
                Assert.True(NsPlugin.Variables.Get(i, out var initialValue));
                Assert.Empty(initialValue);

                // 写入长值应被截断
                Assert.True(NsPlugin.Variables.Set(i, tooLongValue));
                Assert.True(NsPlugin.Variables.Get(i, out var actualValue));
                Assert.Equal(expectedTruncated, actualValue);
            }

            // 变量边界外访问应失败
            const NsVariable belowInst0 = (NsVariable)(-1);
            Assert.False(NsPlugin.Variables.Set(belowInst0, "value"));
            Assert.False(NsPlugin.Variables.Get(belowInst0, out var belowRangeValue));
            Assert.Null(belowRangeValue);

            Assert.False(NsPlugin.Variables.Set(NsVariable.InstLast, "value"));
            Assert.False(NsPlugin.Variables.Get(NsVariable.InstLast, out var outOfRangeValue));
            Assert.Null(outOfRangeValue);
        }
        finally
        {
            TestUnmanagedMemory.Free(variablesPtr);
        }
    }

    /// <summary>
    /// 创建用于测试的 Variables 内存块，大小根据字符串长度和变量数量计算。
    /// </summary>
    /// <param name="encoding">编码</param>
    /// <param name="stringSize">字符串长度</param>
    /// <returns>variables 指针，需要手动释放</returns>
    private static IntPtr CreateVariables(Encodings encoding, int stringSize)
    {
        if (encoding == Encodings.Undefined) throw new NotSupportedException("Undefined encoding");
        var charSize = encoding == Encodings.Unicode ? 2 : 1;
        var variablesBytes = (nuint)(stringSize * charSize * (int)NsVariable.InstLast);
        var variablesPtr = TestUnmanagedMemory.Zeroed(variablesBytes);
        return variablesPtr;
    }
}
