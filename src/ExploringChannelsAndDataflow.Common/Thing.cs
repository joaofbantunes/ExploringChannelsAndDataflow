namespace ExploringChannelsAndDataflow.Common
{
    public class Thing
    {
        public Thing(long dateTimeInTicks)
        {
            DateTimeInTicks = dateTimeInTicks;
        }

        public long DateTimeInTicks { get; }
    }
}
