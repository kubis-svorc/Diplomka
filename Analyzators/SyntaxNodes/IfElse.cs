namespace Diplomka.Analyzators.SyntaxNodes
{
    using Runtime;

    public class IfElse : Syntax
    {
        public Syntax _test, _bodyTrue, _bodyFalse;

        public IfElse(Syntax test, Syntax bodyT, Syntax bodyF = null)
        {
            _test = test;
            _bodyTrue = bodyT;
            _bodyFalse = bodyF;
        }

        public override void Generate()
        {
            _test.Generate();
            VirtualMachine.Poke((int)Instruction.JmpIfFalse);
            int jmpAddress = VirtualMachine.ADR;
            VirtualMachine.ADR++; // ????
            _bodyTrue.Generate();
            if (_bodyFalse == null)
            {
                VirtualMachine.MEM[jmpAddress] = VirtualMachine.ADR;
            }
            else
            {
                VirtualMachine.Poke((int)Instruction.Jmp);
                int jmpFalseAddress = VirtualMachine.ADR;
                VirtualMachine.ADR++; // ????
                VirtualMachine.MEM[jmpAddress] = VirtualMachine.ADR;
                _bodyFalse.Generate();
                VirtualMachine.MEM[jmpFalseAddress] = VirtualMachine.ADR;
            }
        }
    }

}
