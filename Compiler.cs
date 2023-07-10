namespace Diplomka.Runtime
{
    using Sanford.Multimedia.Midi;
	using Diplomka.Analyzators;

    public class Compiler
    {
        private LexicalAnalyzer analyzer;

        delegate void Scanner();    // to use shortcut Scan() for analyzer.Scan()

		delegate void Poker(int i); // to use shortcut Poke(code) for VirtualMachine.Poke(code)

		private Scanner Scan;	// procedure reference

        private Poker Poke;     // procedure reference

		public Compiler()
        {
            analyzer = new LexicalAnalyzer();
            Scan = analyzer.Scan;
			Poke = VirtualMachine.Poke;
        }

        public Compiler(string programText) : this()
        {
            analyzer.Init(programText);
        }

        /// <summary>
        /// Procedure fills memory of virtual machine with instructions.
		/// Consider using Parse() to create Syntax tree
        /// </summary>
        public void Compile(int counterAddress)
        {
            while (analyzer.kind != Kind.NOTHING)
            {
                if ("nastroj" == analyzer.ToString())
                {
                    Scan();
                    Poke((int)Instruction.Insturment);
                    Poke(GetInstrumentCode(analyzer.ToString()));
                    Scan();
                }

                else if ("hraj" == analyzer.ToString())
                {
					Scan(); //preskoc hraj
					int toneCode = GetToneCode(analyzer.ToString()); // ton <c, d, e, f, g, ek2, ... >
					Scan();	//preskoc ton
					string parameters = analyzer.ToString();

					while(parameters.IndexOf(":") > -1)
                    {
						if ("h:" == parameters)
                        {
							Scan();	 //preskoc h:
							analyzer.Check(Kind.NUMBER);
							Poke((int)Instruction.Volume);
							Poke(System.Convert.ToInt32(analyzer.ToString()));
							Scan();	// preskoc cislo
                        }
						else if ("s:" == parameters)
                        {
							Scan();  //preskoc s:
							analyzer.Check(Kind.NUMBER);
							Poke((int)Instruction.Direction);
							Poke(System.Convert.ToInt32(analyzer.ToString()));
							Scan(); // preskoc cislo
						}
						else if ("d:" == parameters)
                        {
							Scan();  //preskoc d:
							analyzer.Check(Kind.NUMBER);
							Poke((int)Instruction.Duration);
							Poke(System.Convert.ToInt32(analyzer.ToString()));
							Scan(); // preskoc cislo
						}
						parameters = analyzer.ToString();
                    }
					
					Poke((int)Instruction.Sound);
					Poke(toneCode);
				}											

				else if ("opakuj" == analyzer.ToString())
                {
					Scan();
					Poke((int)Instruction.Set);
					Poke(counterAddress);
					analyzer.Check(Kind.NUMBER);
					Poke(System.Convert.ToInt32(analyzer.ToString()));
					Scan();
					Scan();
					int bodyAddress = VirtualMachine.adr;
					Compile(counterAddress - 1);
					Poke((int)Instruction.Loop);
					Poke(counterAddress);
					Poke(bodyAddress);
					Scan();
                }
				
				else
                {
					return;
                }
            }
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
			var result = new Block();
			while (Kind.WORD == analyzer.kind)
			{
				if ("nastroj" == analyzer.ToString())
				{
					Scan();
					int instrumentCode = GetInstrumentCode(analyzer.ToString());
					result.Add(new Instrument(instrumentCode));
					Scan();
				}

				else if ("hraj" == analyzer.ToString())
				{
					Scan(); //preskoc hraj
					string ton = analyzer.ToString();
					Scan();
					if (Kind.NUMBER == analyzer.kind)	// c2, c3, c1, ...
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

					result.Add(new Analyzators.Tone(toneCode, duration, volume));
				}

				else if ("opakuj" == analyzer.ToString())
				{
					Scan();
					Syntax count;
					if (analyzer.kind == Kind.NUMBER)
                    {
						count = new Const(System.Convert.ToInt32(analyzer.ToString()));
                    }
					else
                    {
						analyzer.Check(Kind.WORD);
						if (!VirtualMachine.Variables.ContainsKey(analyzer.ToString()))
                        {
							throw new System.Collections.Generic.KeyNotFoundException(
								$"{analyzer} je neznama premenna");
                        }

						string name = analyzer.ToString();
						count = new Variable(name);
                    }

					Scan();
					result.Add(new Cycle(count, Parse()));
					//analyzer.Check(Kind.WORD, "koniec");
					Scan();
				}

				else if ("losuj" == analyzer.ToString() || "los" == analyzer.ToString())
                {
					Scan();
					analyzer.Check(Kind.SYMBOL, "(");
					Scan();
					analyzer.Check(Kind.NUMBER);
                }
					
				else
                {
					string name = analyzer.ToString();
					Scan();

					if ("=" != analyzer.ToString())
                    {
						// podprogram
                    }
					else
                    {
						Scan();
						result.Add(new Assign(new Variable(name), AddSub()));
						if (!VirtualMachine.Variables.ContainsKey(name))
                        {
							VirtualMachine.Variables.Add(name, VirtualMachine.Variables.Count + 2);
                        }
                    }

                }
			}
			return result;
        }	
	
		public void JumpToProgramBody()
        {
			int offset = VirtualMachine.Variables.Count;
			Poke((int)Instruction.Jmp);
			Poke(2 + offset);
			VirtualMachine.adr += offset;
		}

		public Syntax Operand()
        {
			Syntax result;
			if (analyzer.kind == Kind.WORD)
            {
				string name = analyzer.ToString();
				if (!VirtualMachine.Variables.ContainsKey(name))
                {
					throw new System.Collections.Generic.KeyNotFoundException($"{name} nie je zadeklarovane");
                }
				result = new Variable(name);
            }
			else
            {
				analyzer.Check(Kind.NUMBER);
				result = new Const(System.Convert.ToInt32(analyzer.ToString()));
            }
			Scan();
			return result;
        }

		public Syntax MulDiv()
		{
			var result = Operand();
            while ("*" == analyzer.ToString() || "/" == analyzer.ToString())
            {
				if ("*" == analyzer.ToString())
                {
					Scan();
					result = new Mul(result, AddSub());
                }
				else if ("/" == analyzer.ToString())
                {
					Scan();
					result = new Div(result, AddSub());
				}
            }	
			return result;
		}

		public Syntax AddSub()
        {
			var result = MulDiv();
			while ("+" == analyzer.ToString() || "-" == analyzer.ToString())
			{
				if ("+" == analyzer.ToString())
				{
					Scan();
					result = new Add(result, AddSub());
				}
				else if ("-" == analyzer.ToString())
				{
					Scan();
					result = new Sub(result, AddSub());
				}
			}
			return result;
		}

	}
}
