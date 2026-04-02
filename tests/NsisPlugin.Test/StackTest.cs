using NsisPlugin.Test.Helper;
using Xunit;

namespace NsisPlugin.Test;

public class StackTest
{
    [Theory]
    [InlineData(NsEncoding.Ansi, "ansi")]
    [InlineData(NsEncoding.Unicode, "你好")]
    public void StackT_ShouldPushAndPopValues(NsEncoding encoding, string expectedValue)
    {
        const int stringSize = 64;
        var stackTopPtr = StackTopTestHelper.Create();

        try
        {
            using var _ = NsPluginEnc.CreateEncScope(encoding);
            NsPlugin.Init(IntPtr.Zero, stringSize, IntPtr.Zero, stackTopPtr, IntPtr.Zero);

            // 初始状态
            Assert.False(NsPlugin.StackTop.Pop(out var emptyValue));
            Assert.Null(emptyValue);

            // 推送
            for (var i = 0; i < 8; i++)
            {
                Assert.True(NsPlugin.StackTop.Push($"{expectedValue}_{i}"));
            }
            // 弹出
            for (var i = 7; i >= 0; i--)
            {
                Assert.True(NsPlugin.StackTop.Pop(out var actualValue));
                Assert.Equal($"{expectedValue}_{i}", actualValue);
            }

            // 已经全部弹出
            Assert.False(NsPlugin.StackTop.Pop(out emptyValue));
            Assert.Null(emptyValue);
        }
        finally
        {
            StackTopTestHelper.DrainAndFree(NsPlugin.StackTop);
        }
    }

    [Theory]
    [InlineData(NsEncoding.Ansi)]
    [InlineData(NsEncoding.Unicode)]
    public void StackT_ShouldNotWriteOrReadOutOfBounds(NsEncoding encoding)
    {
        const int stringSize = 16;
        var stackTopPtr = StackTopTestHelper.Create();

        try
        {
            using var _ = NsPluginEnc.CreateEncScope(encoding);
            NsPlugin.Init(IntPtr.Zero, stringSize, IntPtr.Zero, stackTopPtr, IntPtr.Zero);

            var overflowChar = encoding == NsEncoding.Unicode ? '你' : 'x';
            var tooLongValue = new string(overflowChar, stringSize + 16);
            var expectedTruncated = new string(overflowChar, stringSize - 1);

            // 初始状态
            Assert.False(NsPlugin.StackTop.Pop(out var emptyValue));
            Assert.Null(emptyValue);

            Assert.True(NsPlugin.StackTop.Push(tooLongValue));
            Assert.True(NsPlugin.StackTop.Pop(out var actualValue));
            Assert.Equal(expectedTruncated, actualValue);

            // 已经全部弹出
            Assert.False(NsPlugin.StackTop.Pop(out emptyValue));
            Assert.Null(emptyValue);

            // 无效栈
            var invalidStack = new StackT(IntPtr.Zero);
            Assert.False(invalidStack.Push("value"));
            Assert.False(invalidStack.Pop(out var invalidValue));
            Assert.Null(invalidValue);
        }
        finally
        {
            StackTopTestHelper.DrainAndFree(NsPlugin.StackTop);
        }
    }

#if NET7_0_OR_GREATER
    [Theory]
    [InlineData(NsEncoding.Ansi)]
    [InlineData(NsEncoding.Unicode)]
    public void StackT_GenericRoundTrip_ShouldSupportCustomParsable(NsEncoding encoding)
    {
        const int stringSize = 64;
        var stackTopPtr = StackTopTestHelper.Create();

        try
        {
            using var _ = NsPluginEnc.CreateEncScope(encoding);
            NsPlugin.Init(IntPtr.Zero, stringSize, IntPtr.Zero, stackTopPtr, IntPtr.Zero);

            var expected = new HexValue(0x2A);
            Assert.True(NsPlugin.StackTop.Push(expected));
            Assert.True(NsPlugin.StackTop.Pop(out HexValue actual));
            Assert.Equal(expected, actual);
        }
        finally
        {
            StackTopTestHelper.DrainAndFree(NsPlugin.StackTop);
        }
    }
#endif
}
