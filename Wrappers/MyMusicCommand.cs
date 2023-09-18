namespace Diplomka.Wrappers
{
    using Sanford.Multimedia.Midi;

    public class MyMusicCommand
    {
        public ChannelMessage command;
        public int duration;
        
        public MyMusicCommand(ChannelMessage command, int duration)
        {
            this.command = command;
            this.duration = duration;
        }
    }
}
