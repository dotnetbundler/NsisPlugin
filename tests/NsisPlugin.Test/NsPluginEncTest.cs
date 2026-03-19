using System.Collections.Concurrent;
using Xunit;

namespace NsisPlugin.Test;

public class NsPluginEncTest
{
    [Fact]
    public void CreateEncScope_ShouldApplyAndRestoreNestedEncoding()
    {
        using (NsPluginEnc.CreateEncScope(Encodings.Unicode))
        {
            Assert.True(NsPluginEnc.IsUnicode);
            Assert.Equal(2, NsPluginEnc.CharSize);

            using (NsPluginEnc.CreateEncScope(Encodings.Ansi))
            {
                Assert.False(NsPluginEnc.IsUnicode);
                Assert.Equal(1, NsPluginEnc.CharSize);
            }

            Assert.True(NsPluginEnc.IsUnicode);
            Assert.Equal(2, NsPluginEnc.CharSize);
        }
    }

    [Fact]
    public void ScopeUseUnicode_InMultiThreadedEnvironment_ShouldBeIsolated()
    {
        const int workerCount = 8;
        const int iterationsPerWorker = 200;
        var start = new ManualResetEventSlim(false);
        var errors = new ConcurrentQueue<Exception>();
        var threads = new Thread[workerCount];

        for (var worker = 0; worker < workerCount; worker++)
        {
            var workerIndex = worker;
            threads[worker] = new Thread(() =>
            {
                try
                {
                    var random = new Random(unchecked((Environment.TickCount * 31) + workerIndex));
                    start.Wait();

                    for (var i = 0; i < iterationsPerWorker; i++)
                    {
                        Thread.Sleep(random.Next(0, 5));
                        var outerEncoding = (Encodings)random.Next(0, 3);

                        AssertScopeEnc(Encodings.Undefined);
                        using (NsPluginEnc.CreateEncScope(outerEncoding))
                        {
                            AssertScopeEnc(outerEncoding);
                            Thread.Sleep(random.Next(0, 5));
                            var innerEncoding = (Encodings)random.Next(0, 3);

                            AssertScopeEnc(outerEncoding);
                            using (NsPluginEnc.CreateEncScope(innerEncoding)) { AssertScopeEnc(innerEncoding); }
                            AssertScopeEnc(outerEncoding);
                        }
                        AssertScopeEnc(Encodings.Undefined);
                    }
                }
                catch (Exception ex)
                {
                    errors.Enqueue(ex);
                }
            });

            threads[worker].IsBackground = true;
            threads[worker].Name = $"NsPluginEncTestWorker-{workerIndex}";
            threads[worker].Start();
        }

        // 开始并等待结束
        start.Set();
        foreach (var thread in threads) thread.Join();

        Assert.Empty(errors);

        // 断言当前编码状态与预期一致
        void AssertScopeEnc(Encodings encoding)
        {
            if (encoding == Encodings.Undefined)
            {
                Assert.False(NsPluginEnc.ScopeUseUnicode.HasValue);
                return;
            }

            Assert.True(NsPluginEnc.ScopeUseUnicode.HasValue);
            Assert.Equal(encoding == Encodings.Unicode, NsPluginEnc.ScopeUseUnicode.Value);
            Assert.Equal(encoding == Encodings.Unicode, NsPluginEnc.IsUnicode);
            Assert.Equal(encoding == Encodings.Unicode ? 2 : 1, NsPluginEnc.CharSize);
        }
    }

    [Fact]
    public void MaxStringBytes_ShouldTrackCurrentEncoding()
    {
        NsPlugin.Init(IntPtr.Zero, 10, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);

        using (NsPluginEnc.CreateEncScope(Encodings.Ansi))
        {
            Assert.Equal(10, NsPlugin.MaxStringBytes);
        }

        using (NsPluginEnc.CreateEncScope(Encodings.Unicode))
        {
            Assert.Equal(20, NsPlugin.MaxStringBytes);
        }
    }
}
