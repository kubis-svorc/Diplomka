namespace Diplomka.Analyzators.SyntaxNodes
{
    using Runtime;

    public class Call : Syntax
    {
        private string _name;

        public Call(string name)
        {
            _name = name;
        }

        public override void Generate()
        {
            VirtualMachine.Poke((int)Instruction.Call);
            VirtualMachine.Poke(VirtualMachine.Subroutines[_name].bodyAdr);
        }
    }

}
