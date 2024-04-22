namespace Diplomka.Analyzators.SyntaxNodes
{
    using Runtime;

    public class ForLoop : Syntax
    {
        private Syntax _count;

        private Block _body;

        public ForLoop(Syntax count, Block body)
        {
            _count = count;
            _body = body;
        }

        public override void Generate()
        {
            _count.Generate();
            int bodyLoop = VirtualMachine.ADR;
            _body.Generate();
            VirtualMachine.Poke((int)Instruction.Loop);
            VirtualMachine.Poke(bodyLoop);
        }
    }

}
