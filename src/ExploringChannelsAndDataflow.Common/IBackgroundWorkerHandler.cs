using System.Threading.Tasks;

namespace ExploringChannelsAndDataflow.Common
{
    public interface IBackgroundWorkerHandler<T>
    {
        /// <summary>
        /// Allows for work to be submitted for execution in the background.
        /// </summary>
        /// <param name="payload">The payload to work with.</param>
        /// <returns><code>true</code> if the work is submitted successfully.</returns>
        Task<bool> SubmitAsync(T payload);
    }
}
