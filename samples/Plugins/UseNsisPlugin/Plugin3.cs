using NsisPlugin;

namespace UseNsisPlugin;

internal class Plugin3
{
    /// <summary>
    /// 边界测试，测试输入和输出字符串的边界情况，包括长度和内容
    /// 长度应该是1023，因为有一个结尾字符
    /// </summary>
    /// <param name="s1">来自第一栈顶</param>
    /// <param name="s2">来自第二栈顶</param>
    /// <param name="v1">来自$1</param>
    /// <param name="vr1">来自$R1</param>
    /// <param name="stackT">栈对象</param>
    /// <param name="variables">变量对象</param>
    [NsisAction]
    public static void Boundary(string s1, string s2, [FromVariable(NsVariable.Inst1)] string v1, [FromVariable(NsVariable.InstR1)] string vr1, StackT stackT, Variables variables)
    {
        stackT.Push($"{s2[0]}（{s2.Length}）");
        stackT.Push($"{s1[0]}（{s1.Length}）");
        stackT.Push(new string('3', 1024));
        stackT.Push(new string('他', 1024));

        variables.Set(NsVariable.Inst8, $"{v1[0]}（{v1.Length}）");
        variables.Set(NsVariable.InstR8, $"{vr1[0]}（{vr1.Length}）");
        variables.Set(NsVariable.Inst9, new string('4', 1024));
        variables.Set(NsVariable.InstR9, new string('她', 1024));
    }
}
