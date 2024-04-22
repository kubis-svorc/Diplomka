namespace Diplomka.Analyzators.SyntaxNodes
{
    using Runtime;

    public class Minus : UnaryOperation
    {
        public Minus(Syntax expression) : base(expression)
        {
        }

        public override void Generate()
        {
            _expression.Generate();
            VirtualMachine.Poke((int)Instruction.Minus);
        }
    }

}
