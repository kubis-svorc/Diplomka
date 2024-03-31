namespace Diplomka.Wrappers
{
    using Sanford.Multimedia.Midi;

    
    public interface IMyMusicCommand 
    {
        public int Duration { get; }
    }

    public class MyToneCommand : IMyMusicCommand
    {
        public ChannelMessage command;
        private int duration;
        public int Duration { get { return duration; } }

        public MyToneCommand(ChannelMessage command, int duration)
        {
            this.command = command;
            this.duration = duration;
        }
    }

    public class MyAccordCommand : IMyMusicCommand 
    {
        public ChannelMessage[] commands;
        private int duration;
        public int Duration { get { return duration; } }

        public MyAccordCommand(ChannelMessage[] commands, int duration)
        {
            this.commands = commands;
            this.duration = duration;
        }
    }
}
