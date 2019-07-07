using ExploringChannelsAndDataflow.Common;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace ExploringChannelsAndDataflow.Channels.BackgroundWork
{
    public class ThingBackgroundWorker : IBackgroundWorkerHandler<Thing>, IBackgroundWorkerManager
    {
        private readonly Channel<Thing> _channel;
        private readonly Task[] _processors;
        private readonly ILogger<ThingBackgroundWorker> _logger;

        public ThingBackgroundWorker(ILogger<ThingBackgroundWorker> logger)
        {
            _logger = logger;
            _channel = CreateChannel();
            _processors = RegisterProcessors();
        }

        public Task<bool> SubmitAsync(Thing payload)
        {
            return Task.FromResult(_channel.Writer.TryWrite(payload));
        }

        public Task StartAsync()
        {
            // nothing to do here...
            return Task.CompletedTask;
        }

        public async Task StopAsync()
        {
            if (_channel.Writer.TryComplete())
            {
                _logger.LogInformation("Waiting for reader to complete...");
                await _channel.Reader.Completion;
                _logger.LogInformation("Waiting for processors to stop...");
                await Task.WhenAll(_processors);
            }
        }

        private Channel<Thing> CreateChannel()
        {
            return Channel.CreateBounded<Thing>(new BoundedChannelOptions(10) { FullMode = BoundedChannelFullMode.Wait });
        }

        private Task[] RegisterProcessors()
        {
            const int parallelProcessorsCount = 5;

            var processors = Enumerable
                .Range(0, parallelProcessorsCount)
                .Select(i => ReadAndProcessMessagesAsync(i))
                .ToArray();

            return processors;
        }

        private async Task ReadAndProcessMessagesAsync(int id)
        {
            _logger.LogDebug("Worker {workerId} starting...", id);
            while (await _channel.Reader.WaitToReadAsync())
            {
                if (_channel.Reader.TryRead(out var message))
                {
                    _logger.LogDebug("Worker {workerId} got a message", id);
                    await DoStuff(message);
                }
            }
            _logger.LogDebug("Worker finished!");
        }

        private async Task DoStuff(Thing thing)
        {

            _logger.LogInformation("Will process thing with date time in ticks {dateTime}...", thing.DateTimeInTicks);
            await Task.Delay(TimeSpan.FromSeconds(5));
            _logger.LogInformation("Thing with date time in ticks {dateTime} processed!", thing.DateTimeInTicks);
        }
    }
}
