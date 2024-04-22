namespace Diplomka.Analyzators.SyntaxNodes
{
    using Runtime;

    public class Const : Syntax
    {
        private int _value;

        public Const(int value) : base()
        {
            _value = value;
        }

        public override void Generate()
        {
            VirtualMachine.Poke((int)Instruction.Push);
            VirtualMachine.Poke(_value);
        }

    }

}
