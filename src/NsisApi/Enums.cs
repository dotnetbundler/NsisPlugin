namespace NsisPlugin.NsisApi;

/// <summary>
/// NSIS Plug-In Callback Messages
///<seealso href="https://github.com/NSIS-Dev/nsis/blob/691211035c2aaaebe8fbca48ee02d4de93594a52/Source/exehead/api.h#L27-L32">NSPIM Source</seealso>
/// </summary>
public enum Nspim
{
    /// <summary>
    /// This is the last message a plugin gets, do final cleanup
    /// </summary>
    NspimUnload,

    /// <summary>
    /// Called after .onGUIEnd
    /// </summary>
    NspimGuiunload
}

/// <summary>
/// NSIS Plug-In Variables
/// <seealso href="https://github.com/NSIS-Dev/nsis/blob/691211035c2aaaebe8fbca48ee02d4de93594a52/Contrib/ExDLL/pluginapi.h#L29-L57">NsVariable Source</seealso>
/// </summary>
public enum NsVariable
{
    /// <summary>$1</summary>
    Inst1,

    /// <summary>$2</summary>
    Inst2,

    /// <summary>$3</summary>
    Inst3,

    /// <summary>$4</summary>
    Inst4,

    /// <summary>$5</summary>
    Inst5,

    /// <summary>$6</summary>
    Inst6,

    /// <summary>$7</summary>
    Inst7,

    /// <summary>$8</summary>
    Inst8,

    /// <summary>$9</summary>
    Inst9,

    /// <summary>$R0</summary>
    InstR0,

    /// <summary>$R1</summary>
    InstR1,

    /// <summary>$R2</summary>
    InstR2,

    /// <summary>$R3</summary>
    InstR3,

    /// <summary>$R4</summary>
    InstR4,

    /// <summary>$R5</summary>
    InstR5,

    /// <summary>$R6</summary>
    InstR6,

    /// <summary>$R7</summary>
    InstR7,

    /// <summary>$R8</summary>
    InstR8,

    /// <summary>$R9</summary>
    InstR9,

    /// <summary>$CMDLINE</summary>
    InstCmdline,

    /// <summary>$INSTDIR</summary>
    InstInstdir,

    /// <summary>$OUTDIR</summary>
    InstOutdir,

    /// <summary>$EXEDIR</summary>
    InstExedir,

    /// <summary>$LANGUAGE</summary>
    InstLang,

    /// <summary>last variable, used for bounds checking</summary>
    InstLast
}
