using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace ESFA.DC.JobScheduler.Console.Tests
{
    public sealed class AutoFacTests
    {
#if DEBUG
        [Fact]
#endif
        public async Task TestRegistrations()
        {
            try
            {
                CancellationTokenSource cts = new CancellationTokenSource();
                CancellationToken cancellationToken = cts.Token;

                Program.CancellationToken = cancellationToken;

                cts.Cancel();
                await Program.Main(null);
            }
            catch (OperationCanceledException oce)
            {
                Assert.True(true);
            }
            catch (Exception ex)
            {
                Assert.Null(ex);
            }
        }
    }
}
