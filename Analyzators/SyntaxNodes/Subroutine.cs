namespace Diplomka.Analyzators.SyntaxNodes
{
    using Runtime;

    public class Subroutine : Syntax
    {
        public Syntax body;

        private string _name;

        public int bodyAdr, bodyEndAdr;

        public Subroutine(string name, Syntax body)
        {
            _name = name;
            this.body = body;
        }

        public override void Generate()
        {
            VirtualMachine.Poke((int)Instruction.Jmp);
            VirtualMachine.ADR++;
            bodyAdr = VirtualMachine.ADR;
            body.Generate();
            VirtualMachine.Poke((int)Instruction.Return);
            VirtualMachine.MEM[bodyAdr - 1] = VirtualMachine.ADR;
            bodyEndAdr = VirtualMachine.ADR;
        }
    }

}
