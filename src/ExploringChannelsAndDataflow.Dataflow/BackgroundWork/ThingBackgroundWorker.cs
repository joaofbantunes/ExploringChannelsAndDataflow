using ExploringChannelsAndDataflow.Common;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace ExploringChannelsAndDataflow.Dataflow.BackgroundWork
{
    public class ThingBackgroundWorker : IBackgroundWorkerHandler<Thing>, IBackgroundWorkerManager
    {
        private readonly ITargetBlock<Thing> _processorBlock;
        private readonly ILogger<ThingBackgroundWorker> _logger;

        public ThingBackgroundWorker(ILogger<ThingBackgroundWorker> logger)
        {
            _logger = logger;
            _processorBlock = CreateProcessingPipeline();
        }

        public Task<bool> SubmitAsync(Thing payload)
        {
            return Task.FromResult(_processorBlock.Post(payload));
        }

        public Task StartAsync()
        {
            // nothing to do here...
            return Task.CompletedTask;
        }

        public async Task StopAsync()
        {
            _processorBlock.Complete();
            await _processorBlock.Completion;
        }

        private ITargetBlock<Thing> CreateProcessingPipeline()
        {
            var executionOptions = new ExecutionDataflowBlockOptions
            {
                BoundedCapacity = 10,
                MaxDegreeOfParallelism = 5
            };

            var linkOptions = new DataflowLinkOptions
            {
                PropagateCompletion = true
            };

            var groupingOptions = new GroupingDataflowBlockOptions
            {
                BoundedCapacity = executionOptions.BoundedCapacity
            };

            var entryBlock = new TransformBlock<Thing, AnotherThing>(TransformStuff, executionOptions);
            var another = new BatchBlock<AnotherThing>(2, groupingOptions);
            var theFinal = new ActionBlock<AnotherThing[]>(DoStuff, executionOptions);

            entryBlock.LinkTo(another, linkOptions);
            another.LinkTo(theFinal, linkOptions);

            return entryBlock;
        }

        private Task<AnotherThing> TransformStuff(Thing thing)
        {
            try
            {
                _logger.LogInformation("Will transform thing with date/time {dateTime}...", thing.DateTimeInTicks);
                return Task.FromResult(new AnotherThing(new DateTime(thing.DateTimeInTicks)));
            }
            finally
            {
                _logger.LogInformation("Thing with date/time {dateTime} transformed!", thing.DateTimeInTicks);
            }
        }

        private async Task DoStuff(AnotherThing[] things)
        {
            _logger.LogInformation("Will process a batch of {size} things...", things.Length);
            await Task.Delay(TimeSpan.FromSeconds(5));
            var maxDate = things.Select(t => t.SomeDateTime).Max();
            var minDate = things.Select(t => t.SomeDateTime).Min();
            _logger.LogInformation("Processed {size} things, with dates ranging from {minDate} to {maxDate}", things.Length, minDate, maxDate);
        }
    }
}
