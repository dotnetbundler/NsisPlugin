using System.Reflection;
using System.Runtime.InteropServices;
using NsisPlugin.Compatibility;
using NsisPlugin.NsisApi;

namespace NsisPlugin.Test.Helper;

public static unsafe class ExtraParametersTestHelper
{
    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private delegate int ExecuteCodeSegmentDelegate(int code, IntPtr hwndParent);

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private delegate void ValidateFilenameDelegate(IntPtr fileNameBuffer);

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private delegate int RegisterPluginCallbackDelegate(IntPtr moduleHandle, IntPtr callback);

    [ThreadStatic] private static ExecuteCodeSegmentDelegate? _executeCodeSegmentDelegate;
    [ThreadStatic] private static ValidateFilenameDelegate? _validateFilenameDelegate;
    [ThreadStatic] private static RegisterPluginCallbackDelegate? _registerPluginCallbackDelegate;

    // 这些值在模拟函数中处理，用于验证调用是否正确传递了参数
    [field: ThreadStatic] public static IntPtr ModuleHandleStub { get; set; }
    [field: ThreadStatic] public static IntPtr HwndParentStub { get; set; }
    [field: ThreadStatic] public static int CodeSegmentStub { get; set; }
    [field: ThreadStatic] public static IntPtr RegisterCallbackStub { get; set; }
    [field: ThreadStatic] public static int RegisterCountStub { get; set; }

    public static extra_parameters* Create()
    {
        var extraPtr = MemoryManager.AllocZeroed((nuint)sizeof(extra_parameters));

        var extra = (extra_parameters*)extraPtr;
        extra->exec_flags = (ExecFlags*)MemoryManager.AllocZeroed((nuint)sizeof(ExecFlags));

        _executeCodeSegmentDelegate = ExecuteCodeSegmentStub;
        _validateFilenameDelegate = ValidateFilenameStub;
        _registerPluginCallbackDelegate = RegisterPluginCallbackStub;

        extra->ExecuteCodeSegment = (delegate* unmanaged[Stdcall]<int, IntPtr, int>)Marshal.GetFunctionPointerForDelegate(_executeCodeSegmentDelegate);
        extra->validate_filename = (delegate* unmanaged[Stdcall]<IntPtr, void>)Marshal.GetFunctionPointerForDelegate(_validateFilenameDelegate);
        extra->RegisterPluginCallback = (delegate* unmanaged[Stdcall]<IntPtr, IntPtr, int>)Marshal.GetFunctionPointerForDelegate(_registerPluginCallbackDelegate);

        return extra;

        // 模拟执行代码段
        static int ExecuteCodeSegmentStub(int code, IntPtr hwndParent)
        {
            HwndParentStub = hwndParent;
            CodeSegmentStub = code;
            return code + 10;
        }

        // 模拟验证文件名
        static void ValidateFilenameStub(IntPtr fileNameBuffer)
        {
            var filename = NsPluginEnc.PtrToString(fileNameBuffer)!;

            var resultFileName = $"{filename}_Verified";
            var bytes = NsPluginEnc.Encoding.GetBytes(resultFileName);
            Marshal.Copy(bytes, 0, fileNameBuffer, bytes.Length);
            Marshal.WriteByte(fileNameBuffer, bytes.Length, 0);
            if (NsPluginEnc.IsUnicode) Marshal.WriteByte(fileNameBuffer, bytes.Length + 1, 0);
        }

        // 模拟注册插件回调
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
        if (extra is not null)
        {
            if (extra->exec_flags != null) MemoryManager.Free(extra->exec_flags);
            MemoryManager.Free(extra);
        }

        _executeCodeSegmentDelegate = null;
        _validateFilenameDelegate = null;
        _registerPluginCallbackDelegate = null;
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
