namespace Diplomka.Analyzators.SyntaxNodes
{
    using Runtime;

    public class WhileTrueLoop : Syntax
    {
        private Syntax _body;

        public WhileTrueLoop(Syntax body)
        {
            _body = body;
        }

        public override void Generate()
        {
            int bodyAddress = VirtualMachine.ADR;
            _body.Generate();
            VirtualMachine.Poke((int)Instruction.Jmp);
            VirtualMachine.Poke(bodyAddress);
        }
    }

}
