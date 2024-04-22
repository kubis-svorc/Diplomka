namespace Diplomka.Analyzators.SyntaxNodes
{
    using Runtime;

    public class RandomTone : MidiCommand
    {
        private int _vol, _dur;

        public RandomTone(int volume, int duration)
        {
            _vol = volume;
            _dur = duration;
        }

        public override void Generate()
        {
            VirtualMachine.Poke((int)Instruction.RandomTone);
            VirtualMachine.Poke(_vol);
            VirtualMachine.Poke(_dur);
        }
    }

}
