using System;
using System.Collections.Generic;
using System.Text;

namespace Diplomka
{
	class Compiler
	{
		public const int NOTHING = 0;
		public const int NUMBER = 1;
		public const int WORD = 2;
		public const int SYMBOL = 3;

		public const int INSTRUCTION_FD = 1;
		public const int INSTRUCTION_LT = 2;
		public const int INSTRUCTION_RT = 3;
		public const int INSTRUCTION_SET = 4;
		public const int INSTRUCTION_LOOP = 5;

		public string input;
		public char look;
		public StringBuilder token;
		public int index, position;
		public int kind;

		public int[] mem;
		public int pc;
		public bool terminated;
		public int adr;

		public Compiler()
		{
			token = new StringBuilder();
			mem = new int[100];
		}

		#region lexical_analyst
		// lexikalny analyzator
		public void Next()
		{
			if (index >= input.Length)
			{
				look = '\0';
			}
			else
			{
				look = input[index];
				index++;
			}
		}

		public void Scan()
		{
			while (look == ' ' || look == '\n')
			{
				Next();
			}

			token.Clear();
			position = index - 1;

			if (char.IsNumber(look))
			{
				do
				{
					token.Append(look);
					Next();

				} while (char.IsNumber(look));
				kind = NUMBER;
			}
			else if (char.IsLetter(look))
			{
				do
				{
					token.Append(look);
					Next();

				} while (char.IsNumber(look));
				kind = WORD;
			}
			else if (look != '\0')
			{
				token.Append(look);
				Next();
				kind = SYMBOL;
			}
			else
			{
				kind = NOTHING;
			}
		}
		#endregion

		#region syntax_analyst
		// syntakticky analyzator
		public void interpreter()
		{
			string tkn;
			while (kind == WORD)
			{
				tkn = token.ToString();
				if ("dopredu" == tkn)
				{
					Scan();
					// vykonaj dopredu
					Scan();
				}
				else if ("vlavo" == tkn)
				{
					Scan();
					// vykonaj vlavo
					Scan();
				}
				else if ("vpravo" == tkn)
				{
					Scan();
					// vykonaj vlavo
					Scan();
				}
				else if ("opakuj" == tkn)
				{
					Scan();
					int count = Convert.ToInt32(tkn);
					Scan();
					if ("[" == tkn)
					{
						Scan();
						int start = position;
						while (count > 0)
						{
							index = start;
							Next();
							Scan();
							interpreter();
							count--;
						}
					}
					if ("]" == tkn)
					{
						Scan();
					}
				}
				else
				{
					return;
				}
			}
		}
		#endregion

		#region virtual_machine
		public void Reset()
		{
			pc = 0;
			terminated = false;
		}

		public void Execute()
		{
			switch (mem[pc])
			{
				case INSTRUCTION_FD:
					pc++;
					//vykonaj dopredu
					pc++;
					break;

				case INSTRUCTION_LT:
					pc++;
					//vykonaj left
					pc++;
					break;

				case INSTRUCTION_RT:
					pc++;
					//vykonaj left
					pc++;
					break;

				case INSTRUCTION_LOOP:
					pc++;
					index = mem[pc];
					pc++;
					mem[index] = mem[index] - 1;
					if (mem[index] > 0)
					{
						pc = mem[pc];
					}
					else
					{
						pc++;
					}
					break;

				case INSTRUCTION_SET:
					pc++;
					index = mem[pc];
					pc++;
					mem[index] = mem[pc];
					pc++;
					break;

				default:
					terminated = true;
					break;
			}

		}

		public void Poke(int code)
		{
			mem[adr] = code;
			adr++;
		}
		#endregion

		#region compiler
		public void Compile(int counterAdr)
		{
			string tkn;
			while (kind == WORD)
			{
				tkn = token.ToString();
				switch (tkn)
				{
					case "fd":
						Scan();
						Poke(INSTRUCTION_FD);
						Poke(Convert.ToInt32(tkn));
						break;

					case "left":
						Scan();
						Poke(INSTRUCTION_LT);
						Poke(Convert.ToInt32(tkn));
						break;

					case "right":
						Scan();
						Poke(INSTRUCTION_RT);
						Poke(Convert.ToInt32(tkn));
						break;

					case "repeat":
						Scan();
						Poke(INSTRUCTION_SET);
						Poke(counterAdr);
						Poke(Convert.ToInt32(tkn));
						Scan();
						Scan();
						int bodyAdr = adr;
						Compile(counterAdr - 1);
						Poke(INSTRUCTION_LOOP);
						Poke(counterAdr);
						Poke(bodyAdr);
						Scan();
						break;

					default:
						break;
				}
			}


		}

		#endregion

		#region syntaxtree
		public abstract class Syntax
		{
			public int CounterAdress;

			protected int adr;

			public abstract void Generate();

			public virtual void Poke(int command)
			{

			}

		}

		public abstract class TurtleCommand : Syntax
		{
			public Constant param;

			public TurtleCommand(Constant param)
			{
				this.param = param;
			}

			public abstract void Execute();
		}

		public class Constant : Syntax
		{
			public int Value;
			public Constant(int value) : base()
			{
				this.Value = value;
			}

			public override void Generate()
			{
				Poke(this.Value);
			}

		}

		public class Fd : TurtleCommand
		{

			public Fd(Constant param) : base(param)
			{

			}

			public override void Execute()
			{
				//vykonaj dopredu
			}

			public override void Generate()
			{
				Poke(INSTRUCTION_FD);
				param.Generate();
			}
		}

		public class Lt : TurtleCommand
		{
			public Lt(Constant param) : base(param)
			{
			}

			public override void Execute()
			{
				//vykonaj vlavo
			}

			public override void Generate()
			{
				Poke(INSTRUCTION_LT);
				param.Generate();
			}
		}

		public class Rt : TurtleCommand
		{
			public Rt(Constant param) : base(param)
			{
			}

			public override void Execute()
			{
				//vykonaj rt
			}

			public override void Generate()
			{
				Poke(INSTRUCTION_RT);
				param.Generate();
			}
		}

		public class Block : Syntax
		{
			public Syntax[] items;

			public int counter;

			public Block(params Syntax[] items) : base()
			{
				this.items = items;
				counter = (items == null) ? 0 : items.Length;
			}

			public void Add(Syntax item)
			{
				if (items.Length == counter)
				{
					var newItems = new Syntax[items.Length + 1];
					Array.Copy(items, newItems, items.Length);
				}
				items[counter] = item;
			}

			public override void Generate()
			{
				foreach (Syntax item in items)
				{
					item.Generate();
				}
			}
		}

		public class Repeat : Syntax
		{
			Constant counter;
			Block body;

			public Repeat(Constant counter, Block body)
			{
				this.counter = counter;
				this.body = body;
			}

			public override void Generate()
			{
				Poke(INSTRUCTION_SET);
				Poke(CounterAdress);
				counter.Generate();
				CounterAdress--;
				int loopBody = adr; //adr je z compiler classy
				body.Generate();
				CounterAdress++;
				Poke(INSTRUCTION_LOOP);
				Poke(CounterAdress);
				Poke(loopBody);

			}

		}

		public void Parse()
		{
			var Result = new Block();
			while (kind == WORD)
			{
				if ("dopredu" == token.ToString())
				{
					Scan();
					Result.Add(new Fd(new Constant(Convert.ToInt32(token.ToString()))));
					Scan();
				}
				// ... lt, rt, ...
			}
		}

		#endregion syntaxtree

	}
}


