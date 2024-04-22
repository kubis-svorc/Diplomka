namespace Diplomka.Analyzators.SyntaxNodes
{
    using Runtime;

    public class Print : Syntax
    {
        private Syntax _expression;
        private string _name;

        public Print(Syntax expr)
        {
            _expression = expr;
            if (_expression is Variable)
            {
                _name = (_expression as Variable).Name;
            }
        }

        public override void Generate()
        {
            //_expression.Generate();
            VirtualMachine.Poke((int)Instruction.Print);
            VirtualMachine.Poke(VirtualMachine.Variables[_name]);
        }

    }

}
