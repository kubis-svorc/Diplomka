namespace Diplomka.Runtime
{
    using Sanford.Multimedia.Midi;

    public static class MidiPlayer
    {
        public static ChannelMessage[] Thread1;
        public static int[] Durations;
        private static OutputDevice outputDevice;

        static MidiPlayer()
        {
            outputDevice = new OutputDevice(0);
        }

        public static void PlayMultipleNotes()
        {
            //outputDevice.Send(new ChannelMessage(ChannelCommand.NoteOn, 0, 60, 80)); // Play note C4 with velocity 80
            //outputDevice.Send(new ChannelMessage(ChannelCommand.ProgramChange, 0, 0)); // Change instrument for channel 0
            
            outputDevice.Send(new ChannelMessage(ChannelCommand.ProgramChange, 1, (int)GeneralMidiInstrument.VoiceOohs)); // Change instrument for channel 1
            outputDevice.Send(new ChannelMessage(ChannelCommand.NoteOn, 1, 64, 80));

            outputDevice.Send(new ChannelMessage(ChannelCommand.ProgramChange, 2, (int)GeneralMidiInstrument.Accordion)); // Change instrument for channel 2
            outputDevice.Send(new ChannelMessage(ChannelCommand.NoteOn, 2, 67, 80));            

            System.Threading.Thread.Sleep(5000);

            outputDevice.Send(new ChannelMessage(ChannelCommand.NoteOff, 0, 60, 0)); // end channel 0
            outputDevice.Send(new ChannelMessage(ChannelCommand.NoteOff, 1, 64, 0)); // end channel 1
            outputDevice.Send(new ChannelMessage(ChannelCommand.NoteOff, 2, 67, 0)); // end channel 2

            // Dispose and cleanup
            outputDevice.Dispose();
        }
    }
}
