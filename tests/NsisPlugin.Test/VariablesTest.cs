using NsisPlugin.Test.Helper;
using Xunit;

namespace NsisPlugin.Test;

public class VariablesTest
{
    [Theory]
    [InlineData(NsEncoding.Ansi, "ansi")]
    [InlineData(NsEncoding.Unicode, "你好")]
    public void Variables_ShouldReadAndWriteValue(NsEncoding encoding, string expectedValue)
    {
        const int stringSize = 64;
        var variablesPtr = VariablesTestHelper.Create(encoding, stringSize);

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
            VariablesTestHelper.Free(variablesPtr);
        }
    }

    [Theory]
    [InlineData(NsEncoding.Ansi)]
    [InlineData(NsEncoding.Unicode)]
    public void Variables_ShouldNotWriteOrReadOutOfBounds(NsEncoding encoding)
    {
        const int stringSize = 16;
        var variablesPtr = VariablesTestHelper.Create(encoding, stringSize);

        try
        {
            using var _ = NsPluginEnc.CreateEncScope(encoding);
            NsPlugin.Init(IntPtr.Zero, stringSize, variablesPtr, IntPtr.Zero, IntPtr.Zero);

            // 计算写入和预期值
            var overflowChar = encoding == NsEncoding.Unicode ? '你' : 'x';
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
            VariablesTestHelper.Free(variablesPtr);
        }
    }
}
