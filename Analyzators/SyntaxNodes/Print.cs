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
            if (_expression is Variable)
            {
                VirtualMachine.Poke((int)Instruction.Push);
                VirtualMachine.Poke(VirtualMachine.Variables[_name]);
                VirtualMachine.Poke((int)Instruction.Print);
            }
            else             
            {
                _expression.Generate();
                VirtualMachine.Poke((int)Instruction.Print);
            }            
        }

    }

}
