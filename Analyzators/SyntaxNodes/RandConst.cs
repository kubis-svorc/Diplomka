namespace Diplomka.Analyzators.SyntaxNodes
{
    using Runtime;

    public class RandConst : Syntax
    {
        private int _minVal, _maxVal;

        public RandConst(int minVal, int maxVal) : base()
        {
            _minVal = minVal;
            _maxVal = maxVal;
        }

        public override void Generate()
        {
            int generated = new System.Random().Next(_minVal, _maxVal + 1);
            VirtualMachine.Poke((int)Instruction.Random);
            VirtualMachine.Poke(_minVal);
            VirtualMachine.Poke(_maxVal);
        }
    }

}
