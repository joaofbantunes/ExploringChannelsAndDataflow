using Microsoft.Extensions.Hosting;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ExploringChannelsAndDataflow.Common
{
    public class BackgroundWorkerHost : IHostedService
    {
        private readonly IEnumerable<IBackgroundWorkerManager> _backgroundWorkerManagers;

        public BackgroundWorkerHost(IEnumerable<IBackgroundWorkerManager> backgroundWorkerManagers)
        {
            _backgroundWorkerManagers = backgroundWorkerManagers;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await Task.WhenAll(_backgroundWorkerManagers.Select(m => m.StartAsync()).ToList());
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await Task.WhenAll(_backgroundWorkerManagers.Select(m => m.StopAsync()).ToList());
        }
    }
}
