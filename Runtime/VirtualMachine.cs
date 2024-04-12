namespace Diplomka.Runtime
{
    using Sanford.Multimedia.Midi;
    using System;
    using System.Collections.Generic;
    using Diplomka.Wrappers;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Linq;

    public class VirtualMachine : IDisposable
	{
		private const int DEVICE_ID = 0, MemoryAllocSize = 0x10000; // memory alloc
		public delegate void PrinterFunc(string msg);
		public static PrinterFunc Print;

		private static OutputDevice OUTDEVICE;
		private static bool TERMINATED;

		public static int CHANNEL = 0;
		
		public const int DEFAULT_CHANNEL = 1, DEFAULT_VOLUME = 127, DEFAULT_DURATION = 500;
		public const int MAX_THREAD_THRESHOLD = 4;
		public static int[] MEM;
		public static int ADR, PC, TOP;
		
		public static int CounterAddress = MemoryAllocSize - 1;
		public static IDictionary<string, int> Variables;
		public static IDictionary<string, Analyzators.Subroutine> Subroutines;

		public static ICollection<IMyMusicCommand> Thread1;
		public static ICollection<IMyMusicCommand> Thread2;
		public static ICollection<IMyMusicCommand> Thread3;
		public static ICollection<IMyMusicCommand> Thread4;

		static VirtualMachine()
		{
			MEM = new int[MemoryAllocSize];
			TERMINATED = false;
			ADR = 0;
			PC = 0;
			OUTDEVICE = new OutputDevice(DEVICE_ID);
			Variables = new Dictionary<string, int>();
			Subroutines = new Dictionary<string, Analyzators.Subroutine>();

			Thread1 = new LinkedList<IMyMusicCommand>();
			Thread2 = new LinkedList<IMyMusicCommand>();
			Thread3 = new LinkedList<IMyMusicCommand>();
			Thread4 = new LinkedList<IMyMusicCommand>();
			TOP = MemoryAllocSize;			
		}

		public static void Reset()
		{
			PC = 0;
			ADR = 0;
            Array.Clear(MEM);

            TERMINATED = false;
			CHANNEL = 0;
			Variables.Clear();
			Subroutines.Clear();

			Thread1.Clear();
			Thread2.Clear();
			Thread3.Clear();
			Thread4.Clear();
        }

		public static void Poke(int code)
		{
			MEM[ADR] = code;
			ADR++;
		}

		private static void Execute()
		{
			int index;
			switch (MEM[PC])
			{
				case (int)Instruction.Sound:
					PC++;
					int tone = MEM[TOP];
					TOP++;
					int volume = MEM[TOP];
					TOP++;
					int duration = MEM[TOP];
					TOP++;
					SetTonePlay(tone, duration, volume);
					SetToneStop(tone, duration);
					break;

				case (int)Instruction.Accord:
					PC++;
					int count = MEM[TOP];
					TOP++;

                    int[] tones = new int[count];
					for (int i = 0; i < count; i++)
					{
						tones[i] = MEM[TOP];
						TOP++;
					}
					volume = MEM[TOP];
					TOP++;
					duration = MEM[TOP];
					TOP++;
					SetAccordPlay(tones, duration, volume);
					SetAccordStop(tones, duration);
					break;

				case (int)Instruction.Insturment:
					PC++;
					int instrument = MEM[TOP];
					TOP++;
					SetInstrument(instrument);
					break;

				case (int)Instruction.Loop:
					PC++;
					MEM[TOP]--;
					if (MEM[TOP] > 0)
					{
						PC = MEM[PC];
					}
					else
					{
						TOP++;
						PC++;
					}
					break;

				case (int)Instruction.Random:
					PC++;
					int min = MEM[PC];
					PC++;
					int max = MEM[PC];
					PC++;
					TOP--;
					MEM[TOP] = new Random().Next(min, max);
					break;

				case (int)Instruction.RandomTone:
					PC++;
					volume = MEM[PC];
					PC++;
					duration = MEM[PC];
					PC++;
					tone = new Random().Next(60, 85);
                    SetTonePlay(tone, duration, volume);
					SetToneStop(tone, duration);
					//TOP--;
					//MEM[TOP] = new Random().Next(60, 85);
					break;

				case (int)Instruction.Push:
					PC++;
					TOP--;
					MEM[TOP] = MEM[PC];
					PC++;
					break;

				case (int)Instruction.Get:
					PC++;
					index = MEM[PC];
					PC++;
					TOP--;
					MEM[TOP] = MEM[index];
					break;

				case (int)Instruction.Set:
					PC++;
					index = MEM[PC];
					PC++;
					MEM[index] = MEM[TOP];
					TOP++;
					break;
				
				case (int)Instruction.Print:
					PC++;
					index = MEM[PC];
					PC++;
					var pair = Variables.First((p) => p.Value == index);
					string tmp = $"Hodnota {pair.Key} : {MEM[pair.Value]}";
					Print(tmp);
					break;

				case (int)Instruction.Jmp:
					PC = MEM[PC + 1];
					break;

				case (int)Instruction.JmpIfFalse:
					PC++;
					if (MEM[TOP] == 0)
                    {
						PC = MEM[PC];
                    }
					else
                    {
						PC++;
                    }
					TOP++;
					break;

				case (int)Instruction.Call:
					PC++;
					TOP--;
					MEM[TOP] = PC + 1;
					PC = MEM[PC];
					break;

				case (int)Instruction.Return:
					PC = MEM[TOP];
					TOP++;
					break;

				case (int)Instruction.Minus:
					PC++;
					MEM[TOP] = -MEM[TOP];
					break;

				case (int)Instruction.Add:
					PC++;
					MEM[TOP + 1] = MEM[TOP + 1] + MEM[TOP];
					TOP++;
					break;

				case (int)Instruction.Sub:
					PC++;
					MEM[TOP + 1] = MEM[TOP + 1] - MEM[TOP];
					TOP++;
					break;

				case (int)Instruction.Mul:
					PC++;
					MEM[TOP + 1] = MEM[TOP + 1] * MEM[TOP];
					TOP++;
					break;

				case (int)Instruction.Div:
					PC++;
					MEM[TOP + 1] = MEM[TOP + 1] / MEM[TOP];
					TOP++;
					break;

				case (int)Instruction.Grt:
					PC++;
					MEM[TOP + 1] = (MEM[TOP + 1] > MEM[TOP]) ? 1 : 0;
					TOP++;
					break;
				
				case (int)Instruction.Lwr:
					PC++;
					MEM[TOP + 1] = (MEM[TOP + 1] < MEM[TOP]) ? 1 : 0;
					TOP++;
					break;

				case (int)Instruction.GrEq:
					PC++;
					MEM[TOP + 1] = (MEM[TOP + 1] >= MEM[TOP]) ? 1 : 0;
					TOP++;
					break;

				case (int)Instruction.Diff:
					PC++;
					MEM[TOP + 1] = (MEM[TOP + 1] != MEM[TOP]) ? 1 : 0;
					TOP++;
					break;

				case (int)Instruction.LrEq:
					PC++;
					MEM[TOP + 1] = (MEM[TOP + 1] <= MEM[TOP]) ? 1 : 0;
					TOP++;
					break;

				case (int)Instruction.Eql:
					PC++;
					MEM[TOP + 1] = (MEM[TOP + 1] == MEM[TOP]) ? 1 : 0;
					TOP++;
					break;

				case (int)Instruction.Thrd:
					PC++;
					CHANNEL++;
					break;

				case (int)Instruction.Pause:
					PC++;
					duration = MEM[TOP];
					TOP++;
					SetPause(duration);
					break;

				default:
					TERMINATED = true;
					break;
			}
		}

        public static void Start()
        {
			while (!TERMINATED)
			{
				Execute();
			}
		}

		public static void SetInstrument(int instrumentCode)
        {
			ChannelMessage message = new ChannelMessage(ChannelCommand.ProgramChange, CHANNEL, instrumentCode, 0);
            IMyMusicCommand command = new MyToneCommand(message, 0);
			StoreCommand(command);			
		}

		public static void SetTonePlay(int tone, int duration, int volume)
        {
			ChannelMessage message = new ChannelMessage(ChannelCommand.NoteOn, CHANNEL, tone, volume);
            IMyMusicCommand command = new MyToneCommand(message, duration);
			StoreCommand(command);
		}

		public static void SetToneStop(int tone, int duration) 
		{
			ChannelMessage message = new ChannelMessage(ChannelCommand.NoteOff, CHANNEL, tone, 0);
            IMyMusicCommand command = new MyToneCommand(message, duration);
			StoreCommand(command);
		}

        public static void SetAccordPlay(int[] tones, int duration, int volume)
		{
			ChannelMessage[] messages = new ChannelMessage[tones.Length];
			for(int i = 0; i < messages.Length; i++)
            {
				messages[i] = new(ChannelCommand.NoteOn, CHANNEL, tones[i], volume);
            }
            IMyMusicCommand command = new MyAccordCommand(messages, duration);
            StoreCommand(command);
        }

        public static void SetAccordStop(int[] tones, int duration)
        {
            ChannelMessage[] messages = new ChannelMessage[tones.Length];
            for (int i = 0; i < messages.Length; i++)
            {
                messages[i] = new(ChannelCommand.NoteOff, CHANNEL, tones[i], 0);
            }
            IMyMusicCommand command = new MyAccordCommand(messages, duration);
            StoreCommand(command);
        }

        public static void SetPause(int duration)
        {
			MyToneCommand myMusic = new MyToneCommand(new ChannelMessage(ChannelCommand.NoteOn, CHANNEL, 0), duration);
			StoreCommand(myMusic);
			
			myMusic = new MyToneCommand(new ChannelMessage(ChannelCommand.NoteOff, CHANNEL, 0), duration);
			StoreCommand(myMusic);
		}

		public static void SetVolume(int volume)
        {
			ChannelMessage message = new ChannelMessage(ChannelCommand.Controller, CHANNEL, (int)ControllerType.Volume, volume);
			MyToneCommand command = new MyToneCommand(message, 0);
			StoreCommand(command);
        }

		public static async Task Play(CancellationToken cancellationToken)
		{
			var thread1 = Task.Run(() => PlayFunction(Thread1, cancellationToken), cancellationToken);
			var thread2 = Task.Run(() => PlayFunction(Thread2, cancellationToken), cancellationToken);
			var thread3 = Task.Run(() => PlayFunction(Thread3, cancellationToken), cancellationToken);
			var thread4 = Task.Run(() => PlayFunction(Thread4, cancellationToken), cancellationToken);
			await Task.WhenAll(thread1, thread2, thread3, thread4);
		}
		
		public static void SetJumpToProgramBody()
        {
			int offset = Variables.Count;
            Poke((int)Instruction.Jmp);
            Poke(2 + offset);
            ADR += offset;
		}
				
		public void Dispose()
        {
			for (int i = 0; i < MemoryAllocSize; i++) 
			{
				MEM[i] = -1;
			}
			Variables.Clear();
			Subroutines.Clear();

			Thread1.Clear();
			Thread2.Clear();
			Thread3.Clear();
			Thread4.Clear();
        }

		private static void StoreCommand(IMyMusicCommand command)
        {
			switch (CHANNEL)
			{
				case 4:
					Thread4.Add(command);
					break;
				case 3:
					Thread3.Add(command);
					break;
				case 2:
					Thread2.Add(command);
					break;
				case 1:	//Default to channel 1?
				default:
					Thread1.Add(command);
					break;
			}
		}
		
		public static Task StopAndCancel()
        {
			return Task.Run(() =>
			{
				for (int i = 0; i < 4; i++)
				{
					OUTDEVICE.Send(new ChannelMessage(ChannelCommand.NoteOff, i, 0));
				}
			});
        }

		public static void SaveMIDI(string path)
		{
			Sequence seq = new Sequence();
			Track track = new Track();
			int ticks, timepos = 0;

			foreach (var msg in Thread1)
			{
                timepos += msg.Duration;
                ticks = (int)(timepos * seq.Division / 500F);
                switch (msg)
				{
					case MyToneCommand cmd:
                        track.Insert(ticks, cmd.command);
                        break;

					case MyAccordCommand acc:
						for (int i = 0; i < acc.commands.Length; i++)
						{
							track.Insert(ticks, acc.commands[i]);
						}
                        break;
				}
			}

            timepos = 0;
            foreach (var msg in Thread2)
            {
                timepos += msg.Duration;
                ticks = (int)(timepos * seq.Division / 500F);
                switch (msg)
                {
                    case MyToneCommand cmd:
                        track.Insert(ticks, cmd.command);
                        break;

                    case MyAccordCommand acc:
                        for (int i = 0; i < 3; i++)
                        {
                            track.Insert(ticks, acc.commands[i]);
                        }
                        break;
                }
            }

            timepos = 0;
            foreach (var msg in Thread3)
            {
                timepos += msg.Duration;
                ticks = (int)(timepos * seq.Division / 500F);
                switch (msg)
                {
                    case MyToneCommand cmd:
                        track.Insert(ticks, cmd.command);
                        break;

                    case MyAccordCommand acc:
                        for (int i = 0; i < 3; i++)
                        {
                            track.Insert(ticks, acc.commands[i]);
                        }
                        break;
                }
            }

            timepos = 0;
            foreach (var msg in Thread4)
            {
                timepos += msg.Duration;
                ticks = (int)(timepos * seq.Division / 500F);
                switch (msg)
                {
                    case MyToneCommand cmd:
                        track.Insert(ticks, cmd.command);
                        break;

                    case MyAccordCommand acc:
                        for (int i = 0; i < 3; i++)
                        {
                            track.Insert(ticks, acc.commands[i]);
                        }
                        break;
                }
            }

			try
			{
				seq.Add(track);
				seq.Save(path);
			}
			catch (Exception ex)
			{
				Print("Nastala chyba pri ukladaní MIDI súboru: " + ex.Message);
			}
		}

		private static async Task PlayFunction(IEnumerable<IMyMusicCommand> commands, CancellationToken cancellationToken) 
		{
            foreach (IMyMusicCommand command in commands)
            {
                switch (command)
                {
                    case MyToneCommand cmd:
                        if (cancellationToken.IsCancellationRequested)
                        {
                            cmd.command = new ChannelMessage(ChannelCommand.NoteOff, cmd.command.MidiChannel, cmd.command.Data1, cmd.command.Data2);
                            OUTDEVICE.Send(cmd.command);
                            break;
                        }
                        int delayTime = 0, delayInterval = 200, remainingDelay = cmd.Duration;
                        OUTDEVICE.Send(cmd.command);
                        #region Delay calc
                        while (remainingDelay > 0)
                        {
                            delayTime = Math.Min(remainingDelay, delayInterval);
                            await Task.Delay(delayTime);
                            remainingDelay -= delayTime;
                            if (cancellationToken.IsCancellationRequested)
                            {
                                remainingDelay = -1;
                            }
                        }
                        #endregion
                        break;

                    case MyAccordCommand acc:
                        if (cancellationToken.IsCancellationRequested)
                        {
							int channel, data1, data2;
							for (int i = 0; i < acc.commands.Length; i++)
							{
								channel = acc.commands[i].MidiChannel;
								data1 = acc.commands[i].Data1;
								data2 = acc.commands[i].Data2;
								var cmd = new ChannelMessage(ChannelCommand.NoteOff, channel, data1, data2);
                                OUTDEVICE.Send(cmd);
                            }
							acc.commands = null;
                            break;
                        }
                        delayTime = 0; delayInterval = 200; remainingDelay = acc.Duration;
                        foreach (var cmd in acc.commands)
                        {
                            OUTDEVICE.Send(cmd);
                        }
                        #region Delay calc
                        while (remainingDelay > 0)
                        {
                            delayTime = Math.Min(remainingDelay, delayInterval);
                            await Task.Delay(delayTime);
                            remainingDelay -= delayTime;
                            if (cancellationToken.IsCancellationRequested)
                            {
                                remainingDelay = -1;
                            }
                        }
                        #endregion
                        break;
                }
            }
        }
	}
}
