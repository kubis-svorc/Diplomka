namespace Diplomka.Analyzators.SyntaxNodes
{
    public abstract class BinaryOperation : Syntax
    {
        protected Syntax _left, _right;

        public BinaryOperation(Syntax left, Syntax right)
        {
            _left = left;
            _right = right;
        }

    }

}
