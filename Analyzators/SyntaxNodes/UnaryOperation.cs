namespace Diplomka.Analyzators.SyntaxNodes
{
    public abstract class UnaryOperation : Syntax
    {
        protected Syntax _expression;

        public UnaryOperation(Syntax expression)
        {
            _expression = expression;
        }

    }

}
