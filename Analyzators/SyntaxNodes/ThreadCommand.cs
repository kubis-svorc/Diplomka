namespace Diplomka.Analyzators.SyntaxNodes
{
    using Runtime;

    public class ThreadCommand : Syntax
    {
        private Block _block;

        public ThreadCommand(Block block) : base()
        {
            _block = block;
        }

        public override void Generate()
        {
            if (VirtualMachine.CHANNEL > VirtualMachine.MAX_THREAD_THRESHOLD)
            {

                new Exceptions.ThreadExceededException("Maximálny počet vlákien je 4");
            }
            VirtualMachine.Poke((int)Instruction.Thrd);
            _block.Generate();
        }
    }

}
