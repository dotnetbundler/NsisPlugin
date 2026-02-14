namespace NsisPlugin;

/// <summary>
/// NSIS 插件回调消息
///<seealso href="https://github.com/NSIS-Dev/nsis/blob/691211035c2aaaebe8fbca48ee02d4de93594a52/Source/exehead/api.h#L27-L32">NSPIM Source</seealso>
/// </summary>
public enum Nspim
{
    /// <summary>
    /// 这是插件收到的最后一条消息，请进行最终清理
    /// </summary>
    NspimUnload,

    /// <summary>
    /// 在.onGUIEnd之后调用
    /// </summary>
    NspimGuiunload
}

/// <summary>
/// NSIS 插件变量
/// <seealso href="https://github.com/NSIS-Dev/nsis/blob/691211035c2aaaebe8fbca48ee02d4de93594a52/Contrib/ExDLL/pluginapi.h#L29-L57">NsVariable Source</seealso>
/// </summary>
public enum NsVariable
{
    /// <summary>$0</summary>
    Inst0,

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

    /// <summary>最后一个变量，用于边界检查</summary>
    InstLast
}
