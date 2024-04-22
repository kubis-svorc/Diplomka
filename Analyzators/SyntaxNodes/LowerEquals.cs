namespace Diplomka.Analyzators.SyntaxNodes
{
    using Runtime;

    public class LowerEquals : BinaryOperation
    {
        public LowerEquals(Syntax left, Syntax right) : base(left, right)
        {
        }

        public override void Generate()
        {
            _left.Generate();
            _right.Generate();
            VirtualMachine.Poke((int)Instruction.LrEq);
        }
    }

}
