using NsisPlugin;

namespace UseNsisPlugin;

internal class Plugin5
{
    /// <summary>
    /// 调用NSIS函数（在插件中调用NSIS脚本中的函数）
    /// </summary>
    /// <param name="extra"></param>
    /// <param name="funcAddress">函数地址</param>
    /// <returns>执行结果</returns>
    [NsisAction]
    public static int CallNsisFunction(StackT stack, string message, ExtraParameters extra, int funcAddress)
    {
        stack.Push(message);
        // 注意：调用时要对 funcAddress - 1
        return extra.ExecuteCodeSegment(funcAddress - 1);
    }
}
