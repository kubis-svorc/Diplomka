using System;
using Sanford.Multimedia.Midi;
using Diplomka.Analyzators;

namespace Diplomka.Runtime
{
	internal class Interpreter : IDisposable
	{
		private const int CHANNEL = 1;

		LexicalAnalyzer lexicalAnalyzer;		

		private OutputDevice outDevice;

		public delegate void FunReference();

		private FunReference Scan;

		public Interpreter(string programCode) 
			: this()
		{
			lexicalAnalyzer.input = programCode;
		}

		public Interpreter()
		{
			outDevice = new OutputDevice(0);
			lexicalAnalyzer = new LexicalAnalyzer();
			Scan = lexicalAnalyzer.Scan;
		}

		public void Init()
		{
			lexicalAnalyzer.Init();
		}

		public void Init(string codeText)
		{
			lexicalAnalyzer.Init(codeText);
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
				case "c2k":
				case "d2b":
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

		private void SetDirection(sbyte direction)
		{
			int left = 127, right = 127;
			switch (direction)
			{
				case 10:
					left = 0;
					break;
				case -10:
					right = 0;
					break;
				case 9:
					left = 10;
					break;
				case -9:
					right = 10;
					break;
				case 8:
					left = 20;
					break;
				case -8:
					right = 20;
					break;
				case 7:
					left = 30;
					break;
				case -7:
					right = 30;
					break;
				case 6:
					left = 40;
					break;
				case -6:
					right = 40;
					break;
				case 5:
					left = 50;
					break;
				case -5:
					right = 50;
					break;
				case 4:
					left = 60;
					break;
				case -4:
					right = 60;
					break;
				case 3:
					left = 70;
					break;
				case -3:
					right = 70;
					break;
				case 2:
					left = 80;
					break;
				case -2:
					right = 80;
					break;
				case 1:
					left = 90;
					break;
				case -1:
					right = 90;
					break;
				case 0:					
				default:
					break;
			}

			var leftChannelMessage = new ChannelMessage(ChannelCommand.Controller, CHANNEL, (int)ControllerType.Volume, left);
			var rightChannelMessage = new ChannelMessage(ChannelCommand.Controller, CHANNEL, (int)ControllerType.Volume, right);

			outDevice.Send(leftChannelMessage);
			outDevice.Send(rightChannelMessage);
		}

		public void Play(string tone, int volume, int playSpeed)
		{
			int toneCode = GetToneCode(tone);
			if (toneCode == 0)
			{
				throw new Exception($"Tone {tone} was not recodniges");
			}

			try
			{
				//new ChannelMessage(ChannelCommand.ProgramChange, 1, 1, 0); //change instrument
				//outDevice.Send(channelMessage);
				var channelMessage = new ChannelMessage(ChannelCommand.NoteOn, CHANNEL, toneCode, volume);  //play tone
				outDevice.Send(channelMessage);

				System.Threading.Thread.Sleep(playSpeed);

				channelMessage = new ChannelMessage(ChannelCommand.NoteOff, CHANNEL, toneCode, volume); //switch tone off
				outDevice.Send(channelMessage);
			}

			catch (ArgumentOutOfRangeException)
			{
				throw;
			}

		}

		public void SetInstrument(string instrument)
        {
			ChannelMessage message;
			int instrumentCode;

			switch (instrument)
            {
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

				case "klavir":
				case "piano":
				default:
					instrumentCode = (int)GeneralMidiInstrument.AcousticGrandPiano;
					break;
			}
            message = new ChannelMessage(ChannelCommand.ProgramChange, CHANNEL, instrumentCode, 0);
			outDevice.Send(message);
		}
		
		public void Interpret()
		{
			while (lexicalAnalyzer.kind != Kind.NOTHING)
			{
				if ("nastroj" == lexicalAnalyzer.ToString())
                {
					Scan();
					lexicalAnalyzer.Check(Kind.WORD);
					SetInstrument(lexicalAnalyzer.ToString());
					Scan();
				}
				else if ("hraj" == lexicalAnalyzer.ToString())
				{
					Scan();
					lexicalAnalyzer.Check(Kind.WORD);
					string tone = lexicalAnalyzer.ToString();
					int volume = 127, duration = 500;           // smer s:, hlasitost h:, dlzka d:
					sbyte direction;

					Scan();
					if (lexicalAnalyzer.kind == Kind.NUMBER)
					{
						tone += lexicalAnalyzer.ToString();
						Scan();
					}
					string parameters = lexicalAnalyzer.ToString();					
					while (parameters.IndexOf(":") > -1)    //spracuj parametre d, s, h
					{
						if ("h:" == parameters)
						{
							Scan();
							lexicalAnalyzer.Check(Kind.NUMBER);
							volume = Convert.ToInt32(lexicalAnalyzer.ToString());
							Scan();
						}

						else if ("s:" == parameters)
						{
							Scan();
							lexicalAnalyzer.Check(Kind.NUMBER);
							direction = Convert.ToSByte(lexicalAnalyzer.ToString());
							if (0 != direction)
                            {
								SetDirection(direction);
                            }							
							Scan();
						}

						else if ("d:" == parameters)
						{
							Scan();
							lexicalAnalyzer.Check(Kind.NUMBER);
							duration = Convert.ToInt32(lexicalAnalyzer.ToString());
							Scan();
						}
						parameters = lexicalAnalyzer.ToString();
					}
					Play(tone: tone, volume: volume, playSpeed: duration);
				}
				else if ("pauza" == lexicalAnalyzer.ToString())
				{
					Scan();
					lexicalAnalyzer.Check(Kind.NUMBER);
					short duration = Convert.ToInt16(lexicalAnalyzer.ToString());
					System.Threading.Thread.Sleep(duration);
					Scan();
				}
				else if ("opakuj" == lexicalAnalyzer.token.ToString())
				{
					Scan();	
					if (lexicalAnalyzer.kind == Kind.NUMBER)
                    {

                    }
					int counter = Convert.ToInt32(lexicalAnalyzer.ToString());
					Scan();
					Scan();
					int start = lexicalAnalyzer.position;
					while (counter > 0)
                    {
						lexicalAnalyzer.index = start;
						lexicalAnalyzer.Next();
						Scan();
						Interpret();
						counter--;
                    }

				}
				else if ("ak" == lexicalAnalyzer.token.ToString())
				{
					Scan();
					string vetvenie = lexicalAnalyzer.ToString();
					Console.WriteLine($"vetvenie {vetvenie}");
					Scan();
				}
				else if ("urob" == lexicalAnalyzer.token.ToString())
				{
					Scan();
					string podprogram = lexicalAnalyzer.ToString();
					Console.WriteLine($"vytvorenie podprogramu {podprogram}");
					Scan();
				}
				else
				{
					lexicalAnalyzer.kind = Kind.NOTHING;
				}

			}
		}

		public void Dispose()
		{
			outDevice.Close();
		}
	
	}
}
