namespace Diplomka.Analyzators
{
    using Runtime;

    public class SyntaxAnalyzer
	{
        
    }

	public abstract class Syntax
    {
        public abstract void Generate();
    }

    public class Const : Syntax
    {
        private int _value;

        public Const(int value) : base()
        {
            _value = value;
        }

        public override void Generate()
        {
            //VirtualMachine.Poke((int)Instruction.Push);
            VirtualMachine.Poke(_value);
        }

    }

    public abstract class MidiCommand : Syntax 
    {
        protected int _param;

        public MidiCommand(int param)
        {
            _param = param;
        }
    }

    public class Tone : MidiCommand
    {
        private int _duration, _volume;

        public Tone(int toneCode, int duration, int volume)
            : base(toneCode)
        {
            _duration = duration;
            _volume = volume;
        }

        public override void Generate()
        {
            VirtualMachine.Poke((int)Instruction.Duration);
            VirtualMachine.Poke(_duration);

            VirtualMachine.Poke((int)Instruction.Volume);
            VirtualMachine.Poke(_volume);

            VirtualMachine.Poke((int)Instruction.Sound);
            VirtualMachine.Poke(_param);
            
        }
    }

    public class Instrument : MidiCommand
    {

        public Instrument(int instrumentCode) : base(instrumentCode)
        {
        }

        public override void Generate()
        {
            VirtualMachine.Poke((int)Instruction.Insturment);
            VirtualMachine.Poke(_param);
        }
    }

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
            System.Array.Resize<Syntax>(ref _items, _items.Length + 1);
            _items[_items.Length - 1] = item;
        }
    }

    public class Cycle : Syntax
    {
        private Const _count;

        private Block _body;

        public Cycle(Const count, Block body)
        {
            _count = count;
            _body = body;
        }

        public override void Generate()
        {
            // set counter
            VirtualMachine.Poke((int)Instruction.Set);
            VirtualMachine.Poke(VirtualMachine.CounterAddress);
            _count.Generate();
            VirtualMachine.CounterAddress--;

            // remember where body starts
            int bodyLoop = VirtualMachine.adr;
            _body.Generate();
            VirtualMachine.CounterAddress++;

            // set looping jumps
            VirtualMachine.Poke((int)Instruction.Loop);
            VirtualMachine.Poke(VirtualMachine.CounterAddress);
            VirtualMachine.Poke(bodyLoop);
        }
    }

    public abstract class BinaryOperation : Syntax
    {
        protected Syntax _left, _right;

        public BinaryOperation(Syntax left, Syntax right)
        {
            this._left = left;
            this._right = right;
        }
       
    }

    public abstract class UnaryOperation : Syntax
    {
        protected Syntax _expression;

        public UnaryOperation(Syntax expression)
        {
            this._expression = expression;
        }

    }

    public class Add : BinaryOperation
    {
        public Add(Syntax left, Syntax right) : base(left, right)
        {
        }

        public override void Generate()
        {
            _left.Generate();
            _right.Generate();
            VirtualMachine.Poke((int)Instruction.Add);
        }
    }

    public class Sub : BinaryOperation
    {
        public Sub(Syntax left, Syntax right) : base(left, right)
        {
        }

        public override void Generate()
        {
            _left.Generate();
            _right.Generate();
            VirtualMachine.Poke((int)Instruction.Sub);
        }
    }
   
    public class Mul : BinaryOperation
    {
        public Mul(Syntax left, Syntax right) : base(left, right)
        {
        }

        public override void Generate()
        {
            _left.Generate();
            _right.Generate();
            VirtualMachine.Poke((int)Instruction.Mul);
        }
    }

    public class Div : BinaryOperation
    {
        public Div(Syntax left, Syntax right) : base(left, right)
        {
        }

        public override void Generate()
        {
            _left.Generate();
            _right.Generate();
            VirtualMachine.Poke((int)Instruction.Div);
        }
    }

    public class Equals : BinaryOperation
    {
        public Equals(Syntax left, Syntax right) : base(left, right)
        {

        }

        public override void Generate()
        {
            _left.Generate();
            _right.Generate();
            VirtualMachine.Poke((int)Instruction.Compare);
        }
    }

    public class Minus : UnaryOperation
    {
        public Minus(Syntax expression) : base(expression)
        {
        }

        public override void Generate()
        {
            _expression.Generate();
            VirtualMachine.Poke((int)Instruction.Minus);
        }
    }

    public class Variable : Syntax
    {
        private string _name;

        public Variable(string name)
        {
            _name = name;
        }

        public override void Generate()
        {
            VirtualMachine.Poke((int)Instruction.GetVar);
            VirtualMachine.Poke(VirtualMachine.Variables[_name]);
        }
    
        public void GenerateSet()
        {
            VirtualMachine.Poke((int)Instruction.SetVar);
            VirtualMachine.Poke(VirtualMachine.Variables[_name]);
        }
    }

    public class Assign : Syntax
    {
        private Variable _variable;

        private Syntax _expression;

        public Assign(Variable variable, Syntax expr)
        {
            _expression = expr;
            _variable = variable;
        }

        public override void Generate()
        {
            _expression.Generate();
            _variable.GenerateSet();
        }
    }

    public class Print : Syntax
    {
        private Syntax _expression;

        public Print(Syntax expr)
        {
            _expression = expr;
        }

        public override void Generate()
        {
            _expression.Generate();
            VirtualMachine.Poke((int)Instruction.Print);
        }

    }

}
