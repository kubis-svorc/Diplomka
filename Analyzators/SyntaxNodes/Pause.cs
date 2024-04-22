namespace Diplomka.Analyzators.SyntaxNodes
{
    using Runtime;

    public class Pause : UnaryOperation
    {
        public Pause(Syntax expression) : base(expression)
        {
        }

        public override void Generate()
        {
            _expression.Generate();
            VirtualMachine.Poke((int)Instruction.Pause);
        }
    }

}
