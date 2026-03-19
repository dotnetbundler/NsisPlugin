using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using NsisPlugin.NsisApi;
using NsisPlugin.Test.Helper;
using Xunit;

namespace NsisPlugin.Test;

public unsafe class ExtraParametersTest
{
    // 这些值在模拟函数中处理，用于验证
    [field: ThreadStatic] public static IntPtr ModuleHandleStub { get; set; }
    [field: ThreadStatic] public static IntPtr HwndParentStub { get; set; }
    [field: ThreadStatic] public static int CodeSegmentStub { get; set; }
    [field: ThreadStatic] public static IntPtr RegisterCallbackStub { get; set; }
    [field: ThreadStatic] public static int RegisterCountStub { get; set; }

    [Fact]
    public void ShouldExposeExecFlagsByReference()
    {
        var extra = CreateExtraParameters();

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
            FreeExtraParameters(extra);
        }
    }

    [Fact]
    public void ShouldForwardExecuteCodeSegment()
    {
        var extra = CreateExtraParameters();

        try
        {
            const IntPtr hwndParent = 0x4567;
            NsPlugin.Init(hwndParent, 64, IntPtr.Zero, IntPtr.Zero, (IntPtr)extra);

            var result = NsPlugin.ExtraParameters.ExecuteCodeSegment(7);

            Assert.Equal(17, result);
            Assert.Equal(7, CodeSegmentStub);
            Assert.Equal(hwndParent, HwndParentStub);
        }
        finally
        {
            FreeExtraParameters(extra);
        }
    }

    [Theory]
    [InlineData(Encodings.Ansi)]
    [InlineData(Encodings.Unicode)]
    public void ShouldValidateFilename(Encodings encoding)
    {
        var extra = CreateExtraParameters();

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
            FreeExtraParameters(extra);
        }
    }

    [Fact]
    public void ShouldRegisterPluginCallbackOnce()
    {
        var extra = CreateExtraParameters();

        try
        {
            const IntPtr moduleHandle = 0x9988;
            NsPlugin.ModuleHandle = moduleHandle;
            NsPlugin.Init(IntPtr.Zero, 64, IntPtr.Zero, IntPtr.Zero, (IntPtr)extra);

            // ReSharper disable once ConvertToLocalFunction
            NsPluginCallback callback1 = _ => IntPtr.Zero;

            var first = NsPlugin.ExtraParameters.RegisterPluginCallback(callback1);
            var second = NsPlugin.ExtraParameters.RegisterPluginCallback(_ => 1);

            Assert.Equal(0, first);
            Assert.Equal(1, second);
            Assert.Equal(1, RegisterCountStub);
            Assert.Equal(moduleHandle, ModuleHandleStub);
            Assert.NotEqual(IntPtr.Zero, RegisterCallbackStub);
        }
        finally
        {
            FreeExtraParameters(extra);
        }
    }

    private static extra_parameters* CreateExtraParameters()
    {
        var extraPtr = TestUnmanagedMemory.Zeroed<extra_parameters>();

        var extra = (extra_parameters*)extraPtr;
        extra->exec_flags = (ExecFlags*)TestUnmanagedMemory.Zeroed<ExecFlags>();
        extra->ExecuteCodeSegment = &ExecuteCodeSegmentStub;
        extra->validate_filename = &ValidateFilenameStub;
        extra->RegisterPluginCallback = &RegisterPluginCallbackStub;

        return extra;

        // 模拟执行代码段
        [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
        static int ExecuteCodeSegmentStub(int code, IntPtr hwndParent)
        {
            HwndParentStub = hwndParent;
            CodeSegmentStub = code;
            return code + 10;
        }

        // 模拟验证文件名
        [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
        static void ValidateFilenameStub(IntPtr fileNameBuffer)
        {
            var filename = NsPluginEnc.PtrToString(fileNameBuffer)!;

            var resultFileName = $"{filename}_Verified";
            var bytes = NsPluginEnc.Encoding.GetBytes(resultFileName);
            if (NsPluginEnc.IsUnicode) bytes = [..bytes, 0, 0];
            else bytes = [..bytes, 0];

            Marshal.Copy(bytes, 0, fileNameBuffer, bytes.Length);
        }

        // 模拟回调用函数注册
        [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
        static int RegisterPluginCallbackStub(IntPtr moduleHandle, IntPtr callback)
        {
            ModuleHandleStub = moduleHandle;
            RegisterCallbackStub = callback;
            RegisterCountStub++;
            return 0;
        }
    }
    private static void FreeExtraParameters(extra_parameters* extra)
    {
        if (extra is null) return;

        if (extra->exec_flags != null) TestUnmanagedMemory.Free((IntPtr)extra->exec_flags);
        TestUnmanagedMemory.Free((IntPtr)extra);
    }
}
