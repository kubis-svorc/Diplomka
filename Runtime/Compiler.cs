using Sanford.Multimedia.Midi;
using Diplomka.Analyzators;
using System;
using System.Security.Policy;
using System.Linq;

namespace Diplomka.Runtime
{
    public class Compiler
    {
        private LexicalAnalyzer analyzer;
        private delegate void Scanner();    // to use shortcut Scan() for analyzer.Scan()
        private delegate void Poker(int i); // to use shortcut Poke(code) for VirtualMachine.Poke(code)
		private Scanner Scan;				// procedure reference
		
		public Compiler()
        {
            analyzer = new LexicalAnalyzer();
            Scan = analyzer.Scan;
        }

        public Compiler(string programText) : this()
        {
            analyzer.Init(programText);
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
				if ("nastroj" == keyword || "nástroj" == keyword)
				{
					Scan();
					int instrumentCode = GetInstrumentCode(analyzer.ToString(), analyzer.row);
					result.Add(new Instrument(new Const(instrumentCode)));
					Scan();
				}

				else if ("hraj" == keyword)
				{
                    int duration, volume, direction = 0;
					Scan(); //preskoc hraj
					string ton = analyzer.ToString();
					if ("náhodný" == ton || "nahodny" == ton)
					{
                        Scan();
                        var rnd = new Random();
                        volume = rnd.Next(25, 127);
                        duration = rnd.Next(250, 2500);
                        SetToneParameters(ref volume, ref duration, ref direction);
                        Syntax randomTone = new RandomTone(volume, duration);
                        result.Add(randomTone);
                    }
                    else 
                    {
                        if (Kind.NUMBER == analyzer.kind)   // c2, c3, c1, ...
                        {
                            ton += analyzer.ToString();
                            Scan();
                        }
                        int toneCode = GetToneCode(ton, analyzer.row); // ton <c, d, e, f, g, ek2, ... >
                        Scan();
                        volume = VirtualMachine.DEFAULT_VOLUME;
                        duration = VirtualMachine.DEFAULT_DURATION;
                        SetToneParameters(ref volume, ref duration, ref direction);
                        result.Add(new Analyzators.Tone(new Const(toneCode), new Const(duration), new Const(volume)));
                    }					
				}

				else if ("akord" == keyword)
				{
                    Scan(); //preskoc akord
					string ton;
					Const[] tones = new Const[4] { null, null, null, null };
					int i = 0, toneCode;
					while (analyzer.look != '\n' && analyzer.look != '\0' && i < 4 && analyzer.token[analyzer.token.Length - 1] != ':')
					{
                        ton = analyzer.ToString();
                        Scan();
                        if (Kind.NUMBER == analyzer.kind)   // c2, c3, c1, ...
                        {
                            ton += analyzer.ToString();
                            Scan();
                        }
                        toneCode = GetToneCode(ton, analyzer.row);
						tones[i] = new Const(toneCode);
                        ++i;
					}
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
                            volume = Convert.ToInt32(analyzer.ToString());
							volume = CalcuateVolume(volume);
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
                    
					result.Add(new Accord(tones, new Const(duration), new Const(volume)));
                }

				else if ("opakuj" == keyword)
				{
					Scan();
					if ("kým" == keyword || "kym" == keyword)
					{
						Scan();
						Syntax test = Compare();
						result.Add(new WhileLoop(test, Parse()));
						analyzer.Check(Kind.WORD, "koniec");
						Scan();
					}
					else
					{
						Syntax count = Compare();
						analyzer.Check(Kind.WORD, new[] { "krat", "krát" });
						Scan();
						result.Add(new ForLoop(count, Parse()));
						Scan();
					}
				}

                else if ("ak" == keyword || "keď" == keyword || "ked" == keyword)
                {
					Syntax test = null, bodyT = null, bodyF = null;
					Scan();
					test = Compare();					
					bodyT = Parse();					
					keyword = analyzer.ToString();
					analyzer.Check(Kind.WORD, "koniec");
					var ifesle = new IfElse(test, bodyT, bodyF);
					Scan();
					keyword = analyzer.ToString();
					if ("inak" == keyword) 
					{
						Scan();
						ifesle._bodyFalse = Parse();
                        analyzer.Check(Kind.WORD, "koniec");
                        Scan();
                    }					
					result.Add(ifesle);
                }

				else if ("fun" == keyword || "funkcia" == keyword || "urob" == keyword || "podprogram" == keyword)
                {
					Scan();
					analyzer.Check(Kind.WORD);
					string name = analyzer.ToString();
					if (VirtualMachine.Variables.ContainsKey(name) || VirtualMachine.Subroutines.ContainsKey(name))
                    {
						return result;
                    }
					Scan();
					Subroutine sub = new Subroutine(name, null);
					VirtualMachine.Subroutines.Add(name, sub);
					sub.body = Parse();
					analyzer.Check(Kind.WORD, "koniec");
					Scan();
					result.Add(sub);
                }

				else if ("koniec" == keyword)
                {
					return result;
                }

				else if ("vypis" == keyword || "výpis" == keyword)
                {
					Scan();
					result.Add(new Print(Compare()));
                }			

				else if ("vlakno" == keyword || "vlákno" == keyword)
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
					Scan();
					Syntax randomConst = NumberGenerator();
					result.Add(randomConst);
                }

				else if ("pauza" == keyword)
                {
					Scan();
					Syntax expression = Compare();
					result.Add(new Pause(expression));
                }
				
				else
                {
					string name = analyzer.ToString();
					Scan();
					if ("=" != analyzer.ToString())
                    {
						if (!VirtualMachine.Subroutines.ContainsKey(name))
						{
							throw new System.Collections.Generic.KeyNotFoundException($"Chyba v riadku {analyzer.row} : Nepoznám {name}");
						}
						result.Add(new Call(name));
					}
					
					else
                    {
						if (VirtualMachine.Subroutines.ContainsKey(name))
						{
							throw new Exceptions.NameException($"Chyba v riadku {analyzer.row} : Podprogram {name} je už raz zadefinovaný");
						}
						
						Scan();
						if ("losuj" == analyzer.ToString())
						{
							Scan();
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
					Scan();
					result = NumberGenerator();
                }
				else if (!VirtualMachine.Variables.ContainsKey(name))
                {
					throw new System.Collections.Generic.KeyNotFoundException($"Chyba v riadku {analyzer.row} : Nepoznám premennú {name}");
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

		private static int CalcuateVolume(int percentage)
		{
			if (percentage > 100)
			{
				percentage = 100;
			}

			return Convert.ToInt32(Math.Floor(percentage * 127D / 100D));
		}

        private static int GetInstrumentCode(string instrument, int row)
        {
            int instrumentCode;

            switch (instrument)
            {
                case "husle":
                    instrumentCode = (int)GeneralMidiInstrument.Violin;
                    break;
                case "bicie":
                    instrumentCode = (int)GeneralMidiInstrument.SynthDrum;
                    break;
                case "gitara":
                    instrumentCode = (int)GeneralMidiInstrument.AcousticGuitarNylon;
                    break;
                case "organ":
                    instrumentCode = (int)GeneralMidiInstrument.ChurchOrgan;
                    break;
                case "spev":
                case "hlas":
                    instrumentCode = (int)GeneralMidiInstrument.VoiceOohs;
                    break;
                case "trubka":
                case "trúbka":
                    instrumentCode = (int)GeneralMidiInstrument.Trumpet;
                    break;
                case "harfa":
                    instrumentCode = (int)GeneralMidiInstrument.OrchestralHarp;
                    break;
                case "akordeon":
                case "akordeón":
                    instrumentCode = (int)GeneralMidiInstrument.Accordion;
                    break;
                case "flauta":
                    instrumentCode = (int)GeneralMidiInstrument.Flute;
                    break;
                case "klavir":
                case "klavír":
                case "piano":
                    instrumentCode = (int)GeneralMidiInstrument.AcousticGrandPiano;
                    break;
                default:
                    throw new Exceptions.SyntaxException($"Chyba v riadku {row} : Neznámy nástroj {instrument}");
            }
            return instrumentCode;
        }

        private static int GetToneCode(string tone, int row)
        {
            int code;
            switch (tone)
            {
                case "c1":
                case "c":
                    code = (int)Tone.C;
                    break;
                case "ck1":
                case "db1":
                case "ck":
                case "db":
                    code = (int)Tone.Cis;
                    break;
                case "d1":
                case "d":
                    code = (int)Tone.D;
                    break;
                case "dk1":
                case "eb1":
                case "dk":
                case "eb":
                    code = (int)Tone.Dis;
                    break;
                case "e1":
                case "fb1":
                case "e":
                case "fb":
                    code = (int)Tone.E;
                    break;
                case "ek1":
                case "f1":
                case "ek":
                case "f":
                    code = (int)Tone.F;
                    break;
                case "fk1":
                case "gb1":
                case "fk":
                case "gb":
                    code = (int)Tone.Fis;
                    break;
                case "g1":
                case "g":
                    code = (int)Tone.G;
                    break;
                case "gk1":
                case "ab1":
                case "gk":
                case "ab":
                    code = (int)Tone.Gis;
                    break;
                case "a1":
                case "a":
                    code = (int)Tone.A;
                    break;
                case "ak1":
                case "b1":
                case "ak":
                case "b":
                    code = (int)Tone.B;
                    break;
                case "h1":
                case "cb1":
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
                    throw new Exceptions.SyntaxException($"Chyba v riadku {row} : Neznámy tón {tone}");
            }
            return code;
        }

		private void SetToneParameters(ref int volume, ref int duration, ref int direction) 
		{
            string parameters = analyzer.ToString();
            while (parameters.IndexOf(":") > -1)
            {
                string number;
                if (parameters.StartsWith("h:"))
                {
                    Scan();
                    number = analyzer.ToString();
                    analyzer.Check(Kind.NUMBER);
                    volume = Convert.ToInt32(number);
                    volume = CalcuateVolume(volume);
                    Scan(); // preskoc cislo
                }
                else if (parameters.StartsWith("s:"))
                {
                    Scan();
                    number = analyzer.ToString();
                    analyzer.Check(Kind.NUMBER);
                    direction = Convert.ToInt32(number);
                    Scan(); // preskoc cislo
                }
                else if (parameters.StartsWith("d:"))
                {
                    Scan();
                    number = analyzer.ToString();
                    analyzer.Check(Kind.NUMBER);
                    duration = Convert.ToInt32(number);
                    Scan(); // preskoc cislo
                }
                else 
                {
                    Scan();
                }
                parameters = analyzer.ToString();
            }
		}
    }
}
