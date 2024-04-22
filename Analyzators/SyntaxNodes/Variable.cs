namespace Diplomka.Analyzators.SyntaxNodes
{
    using Runtime;

    public class Variable : Syntax
    {
        private string _name;
        public string Name { get { return _name; } }
        public Variable(string name)
        {
            _name = name;
        }

        public override void Generate()
        {
            VirtualMachine.Poke((int)Instruction.Get);
            VirtualMachine.Poke(VirtualMachine.Variables[_name]);
        }

        public void GenerateSet()
        {
            VirtualMachine.Poke((int)Instruction.Set);
            VirtualMachine.Poke(VirtualMachine.Variables[_name]);
        }
    }

}
