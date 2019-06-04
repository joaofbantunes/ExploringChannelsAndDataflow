using System.Threading.Tasks;

namespace ExploringChannelsAndDataflow.Common
{
    public interface IBackgroundWorkerManager
    {
        Task StartAsync();

        Task StopAsync();
    }
}
