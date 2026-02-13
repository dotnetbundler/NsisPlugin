using System.Runtime.InteropServices;

namespace NsisPlugin.NsisApi;

/// <summary>
/// Linked-list node for a string stack.
/// <seealso href="https://github.com/NSIS-Dev/nsis/blob/691211035c2aaaebe8fbca48ee02d4de93594a52/Contrib/ExDLL/pluginapi.h#L20-L27">stack_t Source</seealso>
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public unsafe struct StackT
{
    /// <summary>
    /// Pointer to the next stack node (or null)
    /// </summary>
    public StackT* Next;

    /// <summary>
    /// First byte of an inline string.
    /// The remaining string bytes follow this field in memory and are typically null-terminated.
    /// Do not treat this field as a standalone byte;
    /// to read the full string, start at this address and read bytes until a null terminator is found.
    /// </summary>
    public byte Text;
}
