using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using NsisPlugin.NsisApi;

namespace NsisPlugin.Test.Helper;

public static unsafe class ExtraParametersTestHelper
{
    // 这些值在模拟函数中处理，用于验证调用是否正确传递了参数
    [field: ThreadStatic] public static IntPtr ModuleHandleStub { get; set; }
    [field: ThreadStatic] public static IntPtr HwndParentStub { get; set; }
    [field: ThreadStatic] public static int CodeSegmentStub { get; set; }
    [field: ThreadStatic] public static IntPtr RegisterCallbackStub { get; set; }
    [field: ThreadStatic] public static int RegisterCountStub { get; set; }

    public static extra_parameters* Create()
    {
        var extraPtr = TestUnmanagedMemory.Zeroed<extra_parameters>();

        var extra = (extra_parameters*)extraPtr;
        extra->exec_flags = (ExecFlags*)TestUnmanagedMemory.Zeroed<ExecFlags>();
        extra->ExecuteCodeSegment = &ExecuteCodeSegmentStub;
        extra->validate_filename = &ValidateFilenameStub;
        extra->RegisterPluginCallback = &RegisterPluginCallbackStub;

        return extra;

        [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
        static int ExecuteCodeSegmentStub(int code, IntPtr hwndParent)
        {
            HwndParentStub = hwndParent;
            CodeSegmentStub = code;
            return code + 10;
        }

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

        [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
        static int RegisterPluginCallbackStub(IntPtr moduleHandle, IntPtr callback)
        {
            ModuleHandleStub = moduleHandle;
            RegisterCallbackStub = callback;
            RegisterCountStub++;
            return 0;
        }
    }

    public static void Free(extra_parameters* extra)
    {
        if (extra is null) return;

        if (extra->exec_flags != null) TestUnmanagedMemory.Free((IntPtr)extra->exec_flags);
        TestUnmanagedMemory.Free((IntPtr)extra);
    }

    public static void ResetStubState()
    {
        ModuleHandleStub = IntPtr.Zero;
        HwndParentStub = IntPtr.Zero;
        CodeSegmentStub = 0;
        RegisterCallbackStub = IntPtr.Zero;
        RegisterCountStub = 0;
    }

    public static void ResetPluginCallbackCache()
    {
        var callbackField = typeof(ExtraParameters).GetField("_callback", BindingFlags.Static | BindingFlags.NonPublic);
        var callbackPtrField = typeof(ExtraParameters).GetField("_callbackPtr", BindingFlags.Static | BindingFlags.NonPublic);

        callbackField?.SetValue(null, null);
        callbackPtrField?.SetValue(null, IntPtr.Zero);
    }
}
