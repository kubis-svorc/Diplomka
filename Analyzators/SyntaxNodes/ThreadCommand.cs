namespace Diplomka.Analyzators.SyntaxNodes
{
    using Runtime;

    public class ThreadCommand : Syntax
    {
        private Block _block;
        private int _channel;

        public ThreadCommand(Block block, int channel) : base()
        {
            _block = block;
            _channel = channel;
        }

        public override void Generate()
        {
            VirtualMachine.Poke((int)Instruction.Thrd);
            VirtualMachine.Poke(_channel);
            _block.Generate();
            VirtualMachine.Poke((int)Instruction.Thrd);
            VirtualMachine.Poke(0);
        }
    }

}
