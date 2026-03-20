using NsisPlugin.Test.Helper;
using Xunit;

namespace NsisPlugin.Test;

public unsafe class ExtraParametersTest
{
    [Fact]
    public void ShouldExposeExecFlagsByReference()
    {
        ExtraParametersTestHelper.ResetStubState();
        ExtraParametersTestHelper.ResetPluginCallbackCache();
        var extra = ExtraParametersTestHelper.Create();

        try
        {
            NsPlugin.Init(IntPtr.Zero, 64, IntPtr.Zero, IntPtr.Zero, (IntPtr)extra);

            NsPlugin.ExtraParameters.ExecFlags.ExecError = 9;
            Assert.Equal(9, extra->exec_flags->ExecError);

            NsPlugin.ExtraParameters.ExecFlags.ExecError = 0;
            Assert.Equal(0, extra->exec_flags->ExecError);
        }
        finally
        {
            ExtraParametersTestHelper.Free(extra);
        }
    }

    [Fact]
    public void ShouldForwardExecuteCodeSegment()
    {
        ExtraParametersTestHelper.ResetStubState();
        ExtraParametersTestHelper.ResetPluginCallbackCache();
        var extra = ExtraParametersTestHelper.Create();

        try
        {
            var hwndParent = (IntPtr)0x4567;
            NsPlugin.Init(hwndParent, 64, IntPtr.Zero, IntPtr.Zero, (IntPtr)extra);

            var result = NsPlugin.ExtraParameters.ExecuteCodeSegment(7);

            Assert.Equal(17, result);
            Assert.Equal(7, ExtraParametersTestHelper.CodeSegmentStub);
            Assert.Equal(hwndParent, ExtraParametersTestHelper.HwndParentStub);
        }
        finally
        {
            ExtraParametersTestHelper.Free(extra);
        }
    }

    [Theory]
    [InlineData(NsEncoding.Ansi)]
    [InlineData(NsEncoding.Unicode)]
    public void ShouldValidateFilename(NsEncoding encoding)
    {
        ExtraParametersTestHelper.ResetStubState();
        ExtraParametersTestHelper.ResetPluginCallbackCache();
        var extra = ExtraParametersTestHelper.Create();

        try
        {
            using var _ = NsPluginEnc.CreateEncScope(encoding);
            NsPlugin.Init(IntPtr.Zero, 64, IntPtr.Zero, IntPtr.Zero, (IntPtr)extra);

            var filename = "input";
            NsPlugin.ExtraParameters.ValidateFilename(ref filename);

            Assert.Equal("input_Verified", filename);
        }
        finally
        {
            ExtraParametersTestHelper.Free(extra);
        }
    }

    [Fact]
    public void ShouldRegisterPluginCallbackOnce()
    {
        ExtraParametersTestHelper.ResetStubState();
        ExtraParametersTestHelper.ResetPluginCallbackCache();
        var extra = ExtraParametersTestHelper.Create();

        try
        {
            var moduleHandle = (IntPtr)0x9988;
            NsPlugin.ModuleHandle = moduleHandle;
            NsPlugin.Init(IntPtr.Zero, 64, IntPtr.Zero, IntPtr.Zero, (IntPtr)extra);

            // ReSharper disable once ConvertToLocalFunction
            NsPluginCallback callback1 = _ => IntPtr.Zero;

            var first = NsPlugin.ExtraParameters.RegisterPluginCallback(callback1);
            var second = NsPlugin.ExtraParameters.RegisterPluginCallback(_ => (IntPtr)1);

            Assert.Equal(0, first);
            Assert.Equal(1, second);
            Assert.Equal(1, ExtraParametersTestHelper.RegisterCountStub);
            Assert.Equal(moduleHandle, ExtraParametersTestHelper.ModuleHandleStub);
            Assert.NotEqual(IntPtr.Zero, ExtraParametersTestHelper.RegisterCallbackStub);
        }
        finally
        {
            ExtraParametersTestHelper.Free(extra);
        }
    }
}
