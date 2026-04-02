using System.Diagnostics.CodeAnalysis;

namespace NsisPlugin;

/// <summary>
/// NSIS 插件扩展方法
/// </summary>
public static partial class NsPluginExtensions
{
    extension(string self)
    {
        internal bool TryTo<T>([NotNullWhen(true)] out T? val)
#if NET7_0_OR_GREATER
            where T : ISpanParsable<T> => T.TryParse(self, null, out val);
#else
        {
            try
            {
                // 获取 T 的非空类型
                var type = Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T);
                val = (T)Convert.ChangeType(self, type);
                return true;
            }
            catch
            {
                val = default;
                return false;
            }
        }
#endif
    }
}

public static partial class NsPluginExtensions
{
    extension(StackT self)
    {
        /// <summary>
        /// 从 NSIS 插件栈顶弹出一个字符串，并尝试转换为 T 类型的值
        /// </summary>
        /// <param name="val">弹出的值</param>
        /// <typeparam name="T">值类型</typeparam>
        /// <returns>是否成功</returns>
        public bool Pop<T>([NotNullWhen(true)] out T? val)
#if NET7_0_OR_GREATER
            where T : ISpanParsable<T>
#endif
        {
            if (self.Pop(out var str) && str.TryTo(out T? res))
            {
                val = res;
                return true;
            }
            val = default;
            return false;
        }

        /// <summary>
        /// 将一个 T 类型的值转换为字符串，并推入 NSIS 插件栈顶
        /// </summary>
        /// <param name="val">推送的值</param>
        /// <typeparam name="T">值类型</typeparam>
        /// <returns>是否成功</returns>
        public bool Push<T>(T val) => self.Push(val?.ToString() ?? string.Empty);
    }
}

public static partial class NsPluginExtensions
{
    extension(Variables self)
    {
        /// <summary>
        /// 从 NSIS 变量中获取一个字符串，并尝试转换为 T 类型的值
        /// </summary>
        /// <param name="variable">变量</param>
        /// <param name="val">获取的值</param>
        /// <typeparam name="T">值类型</typeparam>
        /// <returns>是否成功</returns>
        public bool Get<T>(NsVariable variable, [NotNullWhen(true)] out T? val)
#if NET7_0_OR_GREATER
            where T : ISpanParsable<T>
#endif
        {
            if (self.Get(variable, out var str) && str.TryTo(out T? res))
            {
                val = res;
                return true;
            }
            val = default;
            return false;
        }

        /// <summary>
        /// 将一个 T 类型的值转换为字符串，并设置到 NSIS 变量中
        /// </summary>
        /// <param name="variable">变量</param>
        /// <param name="val">设置的值</param>
        /// <typeparam name="T">值类型</typeparam>
        /// <returns>是否成功</returns>
        public bool Set<T>(NsVariable variable, T val) => self.Set(variable, val?.ToString() ?? string.Empty);
    }
}
