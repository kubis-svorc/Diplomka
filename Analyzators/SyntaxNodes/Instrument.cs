namespace Diplomka.Analyzators.SyntaxNodes
{
    using Runtime;

    public class Instrument : MidiCommand
    {
        private Syntax _instrumentCode;

        public Instrument(Const instrumentCode) : base()
        {
            _instrumentCode = instrumentCode;
        }

        public override void Generate()
        {
            _instrumentCode.Generate();
            VirtualMachine.Poke((int)Instruction.Insturment);
        }
    }

}
