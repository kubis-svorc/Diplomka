using Sanford.Multimedia.Midi;
using System;
using System.Collections.Generic;
using Diplomka.Wrappers;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;

namespace Diplomka.Runtime
{
    public class VirtualMachine : IDisposable
	{
		private const int DEVICE_ID = 0, MemoryAllocSize = 0x10000; // memory alloc
		public delegate void PrinterFunc(string msg);
		public static PrinterFunc Print;

		private static OutputDevice OUTDEVICE;
		private static bool TERMINATED;

		public static Sequence SEQUENCE;
		public static Track TRACK;
		public static int CHANNEL = 0;
		
		public const int DEFAULT_CHANNEL = 1, DEFAULT_VOLUME = 127, DEFAULT_DURATION = 500;
		public const int MAX_THREAD_THRESHOLD = 4;
		public static int[] MEM;
		public static int ADR, PC, TOP;
		
		public static int CounterAddress = MemoryAllocSize - 1;
		public static IDictionary<string, int> Variables;
		public static IDictionary<string, Analyzators.Subroutine> Subroutines;

		public static ICollection<MyMusicCommand> Thread1;
		public static ICollection<MyMusicCommand> Thread2;
		public static ICollection<MyMusicCommand> Thread3;
		public static ICollection<MyMusicCommand> Thread4;

		static VirtualMachine()
		{
			MEM = new int[MemoryAllocSize];
			TERMINATED = false;
			ADR = 0;
			PC = 0;
			OUTDEVICE = new OutputDevice(DEVICE_ID);
			Variables = new Dictionary<string, int>();
			Subroutines = new Dictionary<string, Analyzators.Subroutine>();

			Thread1 = new LinkedList<MyMusicCommand>();
			Thread2 = new LinkedList<MyMusicCommand>();
			Thread3 = new LinkedList<MyMusicCommand>();
			Thread4 = new LinkedList<MyMusicCommand>();
			TOP = MemoryAllocSize;
			TRACK = new Track();
			SEQUENCE = new Sequence();
			SEQUENCE.Add(TRACK);
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
					SetTone(tone, duration, volume);
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
			MyMusicCommand command = new MyMusicCommand(message, 0);
			StoreCommand(command);			
		}

		public static void SetTone(int tone, int duration, int volume)
        {
			ChannelMessage message = new ChannelMessage(ChannelCommand.NoteOn, CHANNEL, tone, volume);
			MyMusicCommand command = new MyMusicCommand(message, duration);
			StoreCommand(command);
			message = new ChannelMessage(ChannelCommand.NoteOff, message.MidiChannel, tone, 0);
			command = new MyMusicCommand(message, duration);
			StoreCommand(command);
		}

		public static void SetPause(int duration)
        {
			MyMusicCommand myMusic = new MyMusicCommand(new ChannelMessage(ChannelCommand.NoteOn, CHANNEL, 0), duration);
			StoreCommand(myMusic);
			
			myMusic = new MyMusicCommand(new ChannelMessage(ChannelCommand.NoteOff, CHANNEL, 0), duration);
			StoreCommand(myMusic);
		}

		public static void SetVolume(int volume)
        {
			ChannelMessage message = new ChannelMessage(ChannelCommand.Controller, CHANNEL, (int)ControllerType.Volume, volume);
			MyMusicCommand command = new MyMusicCommand(message, 0);
			StoreCommand(command);
        }

		public static async Task Play(CancellationToken cancellationToken)
		{
			var thread1 = Task.Run(async () => 
			{
                foreach (MyMusicCommand cmd in Thread1)
                {
					if (cancellationToken.IsCancellationRequested)
                    {
						cmd.command = new ChannelMessage(ChannelCommand.NoteOff, cmd.command.MidiChannel, cmd.command.Data1, cmd.command.Data2);
						OUTDEVICE.Send(cmd.command);
						break;
					}
                    int delayTime = 0, delayInterval = 200, remainingDelay = cmd.duration;
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
                }
            });

            var thread2 = Task.Run(async () =>
            {
                foreach (MyMusicCommand cmd in Thread2)
                {
					if (cancellationToken.IsCancellationRequested)
					{
						cmd.command = new ChannelMessage(ChannelCommand.NoteOff, cmd.command.MidiChannel, cmd.command.Data1, cmd.command.Data2);
						OUTDEVICE.Send(cmd.command);
						break;
					}
                    int delayTime = 0, delayInterval = 200, remainingDelay = cmd.duration;
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
                }
            });

            var thread3 = Task.Run(async () =>
            {
				foreach (MyMusicCommand cmd in Thread3)
				{
					if (cancellationToken.IsCancellationRequested)
					{
						cmd.command = new ChannelMessage(ChannelCommand.NoteOff, cmd.command.MidiChannel, cmd.command.Data1, cmd.command.Data2);
						OUTDEVICE.Send(cmd.command);
						break;
					}
                    int delayTime = 0, delayInterval = 200, remainingDelay = cmd.duration;
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
                }
            });

            var thread4 = Task.Run(async () =>
            {
				foreach (MyMusicCommand cmd in Thread4)
				{
					if (cancellationToken.IsCancellationRequested)
					{
						cmd.command = new ChannelMessage(ChannelCommand.NoteOff, cmd.command.MidiChannel, cmd.command.Data1, cmd.command.Data2);
						OUTDEVICE.Send(cmd.command);
						break;
					}
                    int delayTime = 0, delayInterval = 200, remainingDelay = cmd.duration;
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
                }
            });

            await Task.WhenAll(new[] { thread1, thread2, thread3, thread4 });
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

		private static void StoreCommand(MyMusicCommand command)
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
				timepos += msg.duration;
				ticks = (int)(timepos * seq.Division / 500F);
				track.Insert(ticks, msg.command);
            }
			timepos = 0;
			foreach (var msg in Thread2)
			{
				timepos += msg.duration;
				ticks = (int)(timepos * seq.Division / 500F);
				track.Insert(ticks, msg.command);
			}
			timepos = 0;
			foreach (var msg in Thread3)
			{
				timepos += msg.duration;
				ticks = (int)(timepos * seq.Division / 500F);
				track.Insert(ticks, msg.command);
			}
			timepos = 0;
			foreach (var msg in Thread4)
			{
				timepos += msg.duration;
				ticks = (int)(timepos * seq.Division / 500F);
				track.Insert(ticks, msg.command);
			}
			try 
			{
				seq.Add(track);
				seq.Save(path);
			}
            catch
            {

            }
		} 
	}
}
