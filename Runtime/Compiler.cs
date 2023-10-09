namespace Diplomka.Runtime
{
    using Sanford.Multimedia.Midi;
	using Diplomka.Analyzators;

    public class Compiler
    {
        private LexicalAnalyzer analyzer;
        private delegate void Scanner();    // to use shortcut Scan() for analyzer.Scan()
        private delegate void Poker(int i); // to use shortcut Poke(code) for VirtualMachine.Poke(code)
		private Scanner Scan;	// procedure reference
		
		public Compiler()
        {
            analyzer = new LexicalAnalyzer();
            Scan = analyzer.Scan;
        }

        public Compiler(string programText) : this()
        {
            analyzer.Init(programText);
        }

        public static int GetInstrumentCode(string instrument)
        {
            int instrumentCode;

            switch (instrument)
            {
				case "husle":
				case "violin":
					instrumentCode = (int)GeneralMidiInstrument.Violin;
					break;
                case "bicie":
                case "drums":
                    instrumentCode = (int)GeneralMidiInstrument.SynthDrum;
                    break;
                case "gitara":
                case "guitar":
                    instrumentCode = (int)GeneralMidiInstrument.AcousticGuitarNylon;
                    break;
                case "organ":
                    instrumentCode = (int)GeneralMidiInstrument.ChurchOrgan;
                    break;
                case "spev":
                case "hlas":
                case "voice":
                    instrumentCode = (int)GeneralMidiInstrument.VoiceOohs;
                    break;
                case "trubka":
                case "trumpet":
                    instrumentCode = (int)GeneralMidiInstrument.Trumpet;
                    break;
                case "harfa":
                case "haprh":
                    instrumentCode = (int)GeneralMidiInstrument.OrchestralHarp;
                    break;
                case "akordeon":
                case "accordion":
                    instrumentCode = (int)GeneralMidiInstrument.Accordion;
                    break;
				case "flauta":
				case "flute":
					instrumentCode = (int)GeneralMidiInstrument.Flute;
					break;
                case "klavir":
                case "piano":
                default:
                    instrumentCode = (int)GeneralMidiInstrument.AcousticGrandPiano;
                    break;
            }
            return instrumentCode;
        }

		private int GetToneCode(string tone)
		{
			int code;
			switch (tone)
			{
				case "c":
					code = (int)Tone.C;
					break;
				case "ck":
				case "db":
					code = (int)Tone.Cis;
					break;
				case "d":
					code = (int)Tone.D;
					break;
				case "dk":
				case "eb":
					code = (int)Tone.Dis;
					break;
				case "e":
				case "fb":
					code = (int)Tone.E;
					break;
				case "ek":
				case "f":
					code = (int)Tone.F;
					break;
				case "fk":
				case "gb":
					code = (int)Tone.Fis;
					break;
				case "g":
					code = (int)Tone.G;
					break;
				case "gk":
				case "ab":
					code = (int)Tone.Gis;
					break;
				case "a":
					code = (int)Tone.A;
					break;
				case "ak":
				case "b":
					code = (int)Tone.B;
					break;
				case "h":
				case "cb":
					code = (int)Tone.H;
					break;
				case "c2":
					code = (int)Tone.C2;
					break;
				case "ck2":
				case "db2":
					code = (int)Tone.Cis2;
					break;
				case "d2":
					code = (int)Tone.D2;
					break;
				case "dk2":
				case "eb2":
					code = (int)Tone.Dis2;
					break;
				case "e2":
				case "fb2":
					code = (int)Tone.E2;
					break;
				case "ek2":
				case "f2":
					code = (int)Tone.F2;
					break;
				case "fk2":
				case "gb2":
					code = (int)Tone.Fis2;
					break;
				case "g2":
					code = (int)Tone.G2;
					break;
				case "gk2":
				case "ab2":
					code = (int)Tone.Gis2;
					break;
				case "a2":
					code = (int)Tone.A2;
					break;
				case "ak2":
				case "b2":
					code = (int)Tone.B2;
					break;
				case "h2":
				case "cb2":
					code = (int)Tone.H2;
					break;
				case "c3":
					code = (int)Tone.C3;
					break;
				default:
					code = 0;
					break;
			}
			return code;
		}

		/// <summary>
		/// Function creates Syntax Tree reperesenting program
		/// </summary>
		/// <returns>Block as root of syntax tree</returns>
		public Block Parse()
		{
			Block result = new Block();
			string keyword;
			while (Kind.WORD == analyzer.kind)
			{
				keyword = analyzer.ToString();
				if ("nastroj" == keyword)
				{
					Scan();
					int instrumentCode = GetInstrumentCode(analyzer.ToString());
					result.Add(new Instrument(new Const(instrumentCode)));
					Scan();
				}

				else if ("hraj" == keyword)
				{
					Scan(); //preskoc hraj
					string ton = analyzer.ToString();
					Scan();
					if (Kind.NUMBER == analyzer.kind)   // c2, c3, c1, ...
					{
						ton += analyzer.ToString();
						Scan();
					}
					int toneCode = GetToneCode(ton); // ton <c, d, e, f, g, ek2, ... >					
					string parameters = analyzer.ToString();

					int duration = VirtualMachine.DEFAULT_DURATION,
						volume = VirtualMachine.DEFAULT_VOLUME,
						direction = 0;

					while (parameters.IndexOf(":") > -1)
					{
						if ("h:" == parameters)
						{
							Scan();  //preskoc h:
							analyzer.Check(Kind.NUMBER);
							volume = System.Convert.ToInt32(analyzer.ToString());
							Scan(); // preskoc cislo
						}
						else if ("s:" == parameters)
						{
							Scan();  //preskoc s:
							analyzer.Check(Kind.NUMBER);
							direction = System.Convert.ToInt32(analyzer.ToString());
							Scan(); // preskoc cislo
						}
						else if ("d:" == parameters)
						{
							Scan();  //preskoc d:
							analyzer.Check(Kind.NUMBER);
							duration = System.Convert.ToInt32(analyzer.ToString());
							Scan(); // preskoc cislo
						}
						parameters = analyzer.ToString();
					}
					result.Add(new Analyzators.Tone(new Const(toneCode), new Const(duration), new Const(volume)));
				}

				else if ("opakuj" == keyword || "repeat" == keyword)
				{
					Scan();
					keyword = analyzer.ToString();
					if ("stále" == keyword || "always" == keyword || "stale" == keyword)
                    {
						Scan();
						result.Add(new WhileTrueLoop(Parse()));
						Scan();
                    } 
					else
                    {
						Syntax count = Compare();
						result.Add(new ForLoop(count, Parse()));
						Scan();
					}
				}
	
				else if ("ak" == keyword || "if" == keyword)
                {
					Syntax test = null, bodyT = null, bodyF = null;
					Scan();
					test = Compare();					
					bodyT = Parse();					
					keyword = analyzer.ToString();
					analyzer.Check(Kind.WORD, "koniec");
					Scan();
					keyword = analyzer.ToString();
					if ("inak" == keyword || "else" == keyword) 
					{
						Scan();
						bodyF = Parse();
						Scan();
					}
					analyzer.Check(Kind.WORD, "koniec");
					result.Add(new IfElse(test, bodyT, bodyF));
                }

				else if ("def" == keyword || "fun" == keyword || "sub" == keyword || "function" == keyword || "urob" == keyword)
                {
					Scan();
					analyzer.Check(Kind.WORD);
					string name = analyzer.ToString();
					if (VirtualMachine.Variables.ContainsKey(name) || VirtualMachine.Subroutines.ContainsKey(name))
                    {
						//throw new Exceptions.NameException($"Name {name} is already being used");
						return result;
                    }
					Scan();
					//analyzer.Check(Kind.WORD, "zac");
					Subroutine sub = new Subroutine(name, null);
					VirtualMachine.Subroutines.Add(name, sub);
					sub.body = Parse();
					analyzer.Check(Kind.WORD, "koniec");
					Scan();
					result.Add(sub);
                }

				else if ("koniec" == keyword || "end" == keyword)
                {
					return result;
                }

				else if ("vypis" == keyword || "print" == keyword)
                {
					Scan();
					result.Add(new Print(Compare()));
                }			

				else if ("vlakno" == keyword)
				{
					// todo: vlakno == midi kanal                
					Scan();
					string name = analyzer.ToString();
					Scan();
					ThreadCommand thread = new ThreadCommand(Parse());
					result.Add(thread);
					analyzer.Check(Kind.WORD, "koniec");
					Scan();

                }
				
				else if ("losuj" == keyword)
                {
					Syntax randomConst = NumberGenerator();
					result.Add(randomConst);
                }

				else
                {
					string name = analyzer.ToString();
					Scan();
					if ("=" != analyzer.ToString())
                    {
						if (!VirtualMachine.Subroutines.ContainsKey(name))
						{
							throw new System.Collections.Generic.KeyNotFoundException($"{name} is not defined");
						}
						result.Add(new Call(name));
					}
					
					else
                    {
						if (VirtualMachine.Subroutines.ContainsKey(name))
						{
							throw new Exceptions.NameException($"Function {name} is already defined");
						}
						
						Scan();
						if ("losuj" == analyzer.ToString())
						{
							Syntax randomVal = NumberGenerator();
							result.Add(new Assign(new Variable(name), randomVal));
							if (!VirtualMachine.Variables.ContainsKey(name))
                            {
								VirtualMachine.Variables[name] = 2 + VirtualMachine.Variables.Count;
							}							
                        }

						else 
						{
							result.Add(new Assign(new Variable(name), Compare()));
							if (!VirtualMachine.Variables.ContainsKey(name))
							{
								VirtualMachine.Variables[name] = 2 + VirtualMachine.Variables.Count;
							}
						}
					}
                }
			}
			return result;	
        }	
	
		public Syntax Operand()
        {
			Syntax result;
			if (analyzer.kind == Kind.WORD)
            {
				string name = analyzer.ToString();
				if (name == "losuj")
                {
					result = NumberGenerator();
                }
				else if (!VirtualMachine.Variables.ContainsKey(name))
                {
					throw new System.Collections.Generic.KeyNotFoundException($"{name} nie je zadeklarovane");
                }
				else
                {
					result = new Variable(name);
					Scan();
				}				
            }
			else
            {
				analyzer.Check(Kind.NUMBER);
				result = new Const(System.Convert.ToInt32(analyzer.ToString()));
				Scan();
			}			
			return result;
        }

		public Syntax MulDiv()
		{
            Syntax result = Operand();
			string operation = analyzer.ToString();
			while ("*" == operation || "/" == operation)
            {
				if ("*" == operation)
                {
					Scan();
					result = new Mul(result, AddSub());
                }
				else if ("/" == operation)
                {
					Scan();
					result = new Div(result, AddSub());
				}
				operation = analyzer.ToString();
            }	
			return result;
		}

		public Syntax AddSub()
        {
            Syntax result = MulDiv();
			string operation = analyzer.ToString();
			while ("+" == operation || "-" == operation)
			{
				if ("+" == operation)
				{
					Scan();
					result = new Add(result, AddSub());
				}
				else if ("-" == operation)
				{
					Scan();
					result = new Sub(result, AddSub());
				}
				operation = analyzer.ToString();
			}
			return result;
		}

		public Syntax Compare()
        {
			Syntax result = AddSub();
			string operation = analyzer.ToString();
			if (">" == operation)
            {
				Scan();
				result = new Greater(result, AddSub());
            }
			else if ("<" == operation)
			{
				Scan();
				result = new Lower(result, AddSub());
			}
			else if (">=" == operation)
			{
				Scan();
				result = new GreaterEquals(result, AddSub());
			}
			else if ("<=" == operation)
			{
				Scan();
				result = new LowerEquals(result, AddSub());
			}
			else if ("=" == operation || "==" == operation || "je" == operation)
			{
				Scan();
				result = new Equals(result, AddSub());
			}
			else if ("!=" == operation)
			{
				Scan();
				result = new Diff(result, AddSub());
			}
			return result;
		}

		public Syntax NumberGenerator()
        {
			Scan();
			analyzer.Check(Kind.SYMBOL, "(");
			Scan();
			analyzer.Check(Kind.NUMBER);
			int minVal = System.Convert.ToInt32(analyzer.ToString());
			Scan();
			analyzer.Check(Kind.SYMBOL, ",");
			Scan();
			analyzer.Check(Kind.NUMBER);
			int maxVal = System.Convert.ToInt32(analyzer.ToString()) + 1; // zahrnieme hornu hranicu
			Scan();
			analyzer.Check(Kind.SYMBOL, ")");
			Scan();
			return new RandConst(minVal, maxVal);
		}
	}
}
