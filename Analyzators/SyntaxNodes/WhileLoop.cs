namespace Diplomka.Analyzators.SyntaxNodes
{
    using Runtime;

    public class WhileLoop : Syntax
    {
        private Syntax _test, _body;

        public WhileLoop(Syntax test, Syntax body) : base()
        {
            _test = test;
            _body = body;
        }

        public override void Generate()
        {
            int testAdr = VirtualMachine.ADR;
            _test.Generate();
            VirtualMachine.Poke((int)Instruction.JmpIfFalse);
            int jumpLoop = VirtualMachine.ADR;
            VirtualMachine.ADR++;
            _body.Generate();
            VirtualMachine.Poke((int)Instruction.Jmp);
            VirtualMachine.Poke(testAdr);
            VirtualMachine.MEM[jumpLoop] = VirtualMachine.ADR;
        }
    }

}
