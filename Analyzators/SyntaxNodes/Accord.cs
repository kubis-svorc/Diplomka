namespace Diplomka.Analyzators.SyntaxNodes
{
    using Runtime;

    public class Accord : MidiCommand
    {
        private Const[] _tones;
        private Const _volume, _duration;

        public Accord(Const[] tones, Const duration, Const volume)
        {
            _tones = tones;
            _duration = duration;
            _volume = volume;
        }

        public override void Generate()
        {
            _duration.Generate();
            _volume.Generate();
            int count = 0;
            foreach (var tone in _tones)
            {
                if (tone is null)
                {
                    break;
                }
                tone.Generate();
                count++;
            }
            VirtualMachine.Poke((int)Instruction.Push);
            VirtualMachine.Poke(count);
            VirtualMachine.Poke((int)Instruction.Accord);
        }
    }

}
