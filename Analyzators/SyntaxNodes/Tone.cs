namespace Diplomka.Analyzators.SyntaxNodes
{
    using Runtime;

    public class Tone : MidiCommand
    {
        private Const _toneCode, _duration, _volume;

        public Tone(Const toneCode, Const duration, Const volume) : base()
        {
            _toneCode = toneCode;
            _duration = duration;
            _volume = volume;
        }

        public override void Generate()
        {
            _duration.Generate();
            _volume.Generate();
            _toneCode.Generate();
            VirtualMachine.Poke((int)Instruction.Sound);
        }
    }

}
