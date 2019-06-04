using System;

namespace ExploringChannelsAndDataflow.Dataflow.BackgroundWork
{
    public class AnotherThing
    {
        public AnotherThing(DateTime someDateTime)
        {
            SomeDateTime = someDateTime;
        }

        public DateTime SomeDateTime { get; }
    }
}
