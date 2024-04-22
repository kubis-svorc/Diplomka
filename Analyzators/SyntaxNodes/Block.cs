namespace Diplomka.Analyzators.SyntaxNodes
{
    public class Block : Syntax
    {
        private Syntax[] _items;

        public Block(params Syntax[] items)
        {
            _items = items;
        }

        public override void Generate()
        {
            foreach (Syntax item in _items)
            {
                item.Generate();
            }
        }

        public void Add(Syntax item)
        {
            System.Array.Resize(ref _items, _items.Length + 1);
            _items[_items.Length - 1] = item;
        }
    }

}
