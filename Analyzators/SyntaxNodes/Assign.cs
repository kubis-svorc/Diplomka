namespace Diplomka.Analyzators.SyntaxNodes
{
    public class Assign : Syntax
    {
        private Variable _variable;

        private Syntax _expression;

        public Assign(Variable variable, Syntax expr)
        {
            _expression = expr;
            _variable = variable;
        }

        public override void Generate()
        {
            _expression.Generate();
            _variable.GenerateSet();
        }
    }

}
