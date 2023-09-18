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
			VirtualMachine.Poke((int)Instruction.Push);
			VirtualMachine.Poke(_value);
        }

    }

    public class RandConst : Syntax	
    {
		private int _minVal, _maxVal;		

        public RandConst(int minVal, int maxVal) : base()
        {
            this._minVal = minVal;
            this._maxVal = maxVal;
        }

        public override void Generate()
        {
			int generated = new System.Random().Next(_minVal, _maxVal + 1);
			VirtualMachine.Poke((int)Instruction.Random);
			VirtualMachine.Poke(_minVal);
			VirtualMachine.Poke(_maxVal);
		}
    }

    public abstract class MidiCommand : Syntax 
	{
		public MidiCommand()
		{
			
		}
	}

	public class Tone : MidiCommand
	{
		private Const _toneCode, _duration, _volume;

		public Tone(Const toneCode, Const duration, Const volume): base()
		{
			_toneCode = toneCode;
			_duration = duration;
			_volume = volume;
		}

		public override void Generate()
		{
			_duration.Generate();
			_volume.Generate();
			_toneCode.Generate();
			VirtualMachine.Poke((int)Instruction.Sound);
		}
	}

	public class Instrument : MidiCommand
	{
		private Syntax _instrumentCode;

		public Instrument(Const instrumentCode) : base()
		{
			_instrumentCode = instrumentCode;
		}

		public override void Generate()
		{
			_instrumentCode.Generate();
			VirtualMachine.Poke((int)Instruction.Insturment);
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
			int bodyLoop = VirtualMachine.adr;
			_body.Generate();
			VirtualMachine.Poke((int)Instruction.Loop);
			VirtualMachine.Poke(bodyLoop);
		}
	}

	public class WhileTrueLoop : Syntax
	{
		private Syntax _body;

		public WhileTrueLoop(Syntax body)
		{
			_body = body;
		}

		public override void Generate()
		{
			int bodyAddress = VirtualMachine.adr;
			_body.Generate();
			VirtualMachine.Poke((int)Instruction.Jmp);
			VirtualMachine.Poke(bodyAddress);
        }
    }

    public class WhileLoop : Syntax
    {
        public WhileLoop()
        {

        }

        public override void Generate()
        {
            
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
			VirtualMachine.Poke((int)Instruction.Eql);
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
			VirtualMachine.Poke((int)Instruction.Get);
			VirtualMachine.Poke(VirtualMachine.Variables[_name]);
		}
	
		public void GenerateSet()
		{
			VirtualMachine.Poke((int)Instruction.Set);
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

	public class Subroutine: Syntax
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
			VirtualMachine.adr++;
			bodyAdr = VirtualMachine.adr;
			body.Generate();
			VirtualMachine.Poke((int)Instruction.Return);
			VirtualMachine.mem[bodyAdr - 1] = VirtualMachine.adr;
			bodyEndAdr = VirtualMachine.adr;
		}
	}

	public class IfElse : Syntax
	{
		private Syntax _test, _bodyTrue, _bodyFalse;
		
		public IfElse(Syntax test, Syntax bodyT, Syntax bodyF=null)
		{
			_test = test;
			_bodyTrue = bodyT;
			_bodyFalse = bodyF;
		}

		public override void Generate()
		{
			_test.Generate();
			VirtualMachine.Poke((int)Instruction.JmpIfFalse);
			int jmpAddress = VirtualMachine.adr;
			VirtualMachine.adr++; // ????
			_bodyTrue.Generate();
			if (_bodyFalse == null)
			{
				VirtualMachine.mem[jmpAddress] = VirtualMachine.adr;
			}
			else
			{
				VirtualMachine.Poke((int)Instruction.Jmp);
				int jmpFalseAddress = VirtualMachine.adr;
				VirtualMachine.adr++; // ????
				VirtualMachine.mem[jmpAddress] = VirtualMachine.adr;
				_bodyFalse.Generate();
				VirtualMachine.mem[jmpFalseAddress] = VirtualMachine.adr;
			}
		}
	}

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

    public class Greater : BinaryOperation
    {
        public Greater(Syntax left, Syntax right) : base(left, right)
        {
        }

        public override void Generate()
        {
			_left.Generate();
			_right.Generate();
			VirtualMachine.Poke((int)Instruction.Grt);
        }
    }

    public class Lower : BinaryOperation
    {
        public Lower(Syntax left, Syntax right) : base(left, right)
        {
        }

        public override void Generate()
        {
			_left.Generate();
			_right.Generate();
			VirtualMachine.Poke((int)Instruction.Lwr);
        }
    }

    public class GreaterEquals : BinaryOperation
    {
        public GreaterEquals(Syntax left, Syntax right) : base(left, right)
        {
        }

        public override void Generate()
        {
			_left.Generate();
			_right.Generate();
			VirtualMachine.Poke((int)Instruction.GrEq);
        }
    }

    public class LowerEquals : BinaryOperation
    {
        public LowerEquals(Syntax left, Syntax right) : base(left, right)
        {
        }

        public override void Generate()
        {
			_left.Generate();
			_right.Generate();
			VirtualMachine.Poke((int)Instruction.LrEq);
        }
    }

    public class Diff : BinaryOperation
    {
        public Diff(Syntax left, Syntax right) : base(left, right)
        {
        }

        public override void Generate()
        {
			_left.Generate();
			_right.Generate();
			VirtualMachine.Poke((int)Instruction.Diff);
        }
    }

    public class ThreadCommand : Syntax
    {
        public override void Generate()
        {
			VirtualMachine.Poke((int)Instruction.Thrd);
        }
    }

}
