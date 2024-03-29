﻿namespace Diplomka.Analyzators
{
	using Runtime;
    using System;
    using System.Windows.Documents;

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

	public class Accord : MidiCommand
	{

		private System.Collections.Generic.List<string> _tones;
		private Const _duration, _volume;

		public Accord(System.Collections.Generic.List<string> tones, Const duration, Const volume)
		{
			_tones = tones;
			_duration = duration;
			_volume = volume;
		}

        public override void Generate()
        {
			
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
			int bodyLoop = VirtualMachine.ADR;
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
			int bodyAddress = VirtualMachine.ADR;
			_body.Generate();
			VirtualMachine.Poke((int)Instruction.Jmp);
			VirtualMachine.Poke(bodyAddress);
        }
    }

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
			;
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
        public string Name { get { return _name; } }
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
		private string _name;

		public Print(Syntax expr)
		{
			_expression = expr;
			if (_expression is Variable)
			{
				_name = (_expression as Variable).Name;
			}
		}

		public override void Generate()
		{
			//_expression.Generate();
			VirtualMachine.Poke((int)Instruction.Print);
            VirtualMachine.Poke(VirtualMachine.Variables[_name]);
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
			VirtualMachine.ADR++;
			bodyAdr = VirtualMachine.ADR;
			body.Generate();
			VirtualMachine.Poke((int)Instruction.Return);
			VirtualMachine.MEM[bodyAdr - 1] = VirtualMachine.ADR;
			bodyEndAdr = VirtualMachine.ADR;
		}
	}

	public class IfElse : Syntax
	{
		public Syntax _test, _bodyTrue, _bodyFalse;
		
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

    public class Pause : UnaryOperation
    {
        public Pause(Syntax expression) : base(expression)
        {
        }

        public override void Generate()
        {
			_expression.Generate();
			VirtualMachine.Poke((int)Instruction.Pause);
        }
    }

}
