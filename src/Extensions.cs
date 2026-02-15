using System.Diagnostics.CodeAnalysis;

namespace NsisPlugin;

public static class Extensions
{
    extension(string self)
    {
        internal bool TryTo<T>([NotNullWhen(true)] out T? val)
        {
            try
            {
                // 获取 T 的非空类型
                var type = Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T);
                // TypeConverter 比 ChangeType 更强大，但是 aot 后比较大
                // var converter = TypeDescriptor.GetConverter(type);
                // if (converter.CanConvertFrom(typeof(string)))
                // {
                //     val = (T)converter.ConvertFromString(self)!;
                //     return true;
                // }

                // 备用方案
                val = (T)Convert.ChangeType(self, type);
                return true;
            }
            catch
            {
                val = default;
                return false;
            }
        }
    }

    extension(StackT self)
    {
        public bool Pop<T>([NotNullWhen(true)] out T? val)
        {
            if (self.Pop(out var str) && str.TryTo(out T? res))
            {
                val = res;
                return true;
            }
            val = default;
            return false;
        }

        public bool Push<T>(T val) => self.Push(val?.ToString() ?? string.Empty);
    }

    extension(Variables self)
    {
        public bool Get<T>(NsVariable variable, [NotNullWhen(true)] out T? val)
        {
            if (self.Get(variable, out var str) && str.TryTo(out T? res))
            {
                val = res;
                return true;
            }
            val = default;
            return false;
        }

        public bool Set<T>(NsVariable variable, T val) => self.Set(variable, val?.ToString() ?? string.Empty);
    }
}
