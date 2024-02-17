using Sanford.Multimedia.Midi;
using System;
using System.Collections.Generic;
using Diplomka.Wrappers;
using System.Threading;
using System.Threading.Tasks;

namespace Diplomka.Runtime
{
    public class VirtualMachine : IDisposable
	{
		private const int DEVICE_ID = 0, MemoryAllocSize = 100; // memory alloc

		private static OutputDevice outDevice;
		private static bool terminated;

		public static Sequence sequence;
		public static Track track;
		public static int Channel = 0;
		public static string ChannelName = "hlavne";
		
		public const int DEFAULT_CHANNEL = 1, DEFAULT_VOLUME = 127, DEFAULT_DURATION = 500;
		public const int MAX_THREAD_THRESHOLD = 4;
		public static int[] mem;
		public static int adr, pc, top;
		
		public static int CounterAddress = MemoryAllocSize - 1;
		public static IDictionary<string, int> Variables;
		public static IDictionary<string, Analyzators.Subroutine> Subroutines;

		public static ICollection<MyMusicCommand> Thread1;
		public static ICollection<MyMusicCommand> Thread2;
		public static ICollection<MyMusicCommand> Thread3;
		public static ICollection<MyMusicCommand> Thread4;

		static VirtualMachine()
		{
			mem = new int[MemoryAllocSize];
			terminated = false;
			adr = 0;
			pc = 0;
			outDevice = new OutputDevice(DEVICE_ID);
			Variables = new Dictionary<string, int>();
			Subroutines = new Dictionary<string, Analyzators.Subroutine>();
			//Threads = new Dictionary<string, List<MyMusicCommand>>();
			Thread1 = new LinkedList<MyMusicCommand>();
			Thread2 = new LinkedList<MyMusicCommand>();
			Thread3 = new LinkedList<MyMusicCommand>();
			Thread4 = new LinkedList<MyMusicCommand>();
			top = MemoryAllocSize;
			track = new Track();
			sequence = new Sequence();
			sequence.Add(track);
		}

		public static void Reset()
		{
			pc = 0;
			adr = 0;
			terminated = false;
			Channel = 0;
			Variables.Clear();
			Subroutines.Clear();

			Thread1.Clear();
			Thread2.Clear();
			Thread3.Clear();
			Thread4.Clear();
        }

		public static void Poke(int code)
		{
			mem[adr] = code;
			adr++;
		}

		private static void Execute()
		{
			int index;
			switch (mem[pc])
			{
				case (int)Instruction.Sound:
					pc++;
					int tone = mem[top];
					top++;
					int volume = mem[top];
					top++;
					int duration = mem[top];
					top++;
					SetTone(tone, duration, volume);
					break;

				case (int)Instruction.Insturment:
					pc++;
					int instrument = mem[top];
					top++;
					SetInstrument(instrument);
					break;

				case (int)Instruction.Loop:
					pc++;
					mem[top]--;
					if (mem[top] > 0)
					{
						pc = mem[pc];
					}
					else
					{
						top++;
						pc++;
					}
					break;

				case (int)Instruction.Random:
					pc++;
					int min = mem[pc];
					pc++;
					int max = mem[pc];
					pc++;
					top--;
					mem[top] = new Random().Next(min, max);
					break;

				case (int)Instruction.Push:
					pc++;
					top--;
					mem[top] = mem[pc];
					pc++;
					break;

				case (int)Instruction.Get:
					pc++;
					index = mem[pc];
					pc++;
					top--;
					mem[top] = mem[index];
					break;

				case (int)Instruction.Set:
					pc++;
					index = mem[pc];
					pc++;
					mem[index] = mem[top];
					top++;
					break;
				
				case (int)Instruction.Print:
					pc++;
					string tmp = mem[top].ToString();
					top++;
					MainWindow.PrintInfo(tmp);
					break;

				case (int)Instruction.Jmp:
					pc = mem[pc + 1];
					break;

				case (int)Instruction.JmpIfFalse:
					pc++;
					if (mem[top] == 0)
                    {
						pc = mem[pc];
                    }
					else
                    {
						pc++;
                    }
					top++;
					break;

				case (int)Instruction.Call:
					pc++;
					top--;
					mem[top] = pc + 1;
					pc = mem[pc];
					break;

				case (int)Instruction.Return:
					pc = mem[top];
					top++;
					break;

				case (int)Instruction.Minus:
					pc++;
					mem[top] = -mem[top];
					break;

				case (int)Instruction.Add:
					pc++;
					mem[top + 1] = mem[top + 1] + mem[top];
					top++;
					break;

				case (int)Instruction.Sub:
					pc++;
					mem[top + 1] = mem[top + 1] - mem[top];
					top++;
					break;

				case (int)Instruction.Mul:
					pc++;
					mem[top + 1] = mem[top + 1] * mem[top];
					top++;
					break;

				case (int)Instruction.Div:
					pc++;
					mem[top + 1] = mem[top + 1] / mem[top];
					top++;
					break;

				case (int)Instruction.Grt:
					pc++;
					mem[top + 1] = (mem[top + 1] > mem[top]) ? 1 : 0;
					top++;
					break;
				
				case (int)Instruction.Lwr:
					pc++;
					mem[top + 1] = (mem[top + 1] < mem[top]) ? 1 : 0;
					top++;
					break;

				case (int)Instruction.GrEq:
					pc++;
					mem[top + 1] = (mem[top + 1] >= mem[top]) ? 1 : 0;
					top++;
					break;

				case (int)Instruction.Diff:
					pc++;
					mem[top + 1] = (mem[top + 1] != mem[top]) ? 1 : 0;
					top++;
					break;

				case (int)Instruction.LrEq:
					pc++;
					mem[top + 1] = (mem[top + 1] <= mem[top]) ? 1 : 0;
					top++;
					break;

				case (int)Instruction.Eql:
					pc++;
					mem[top + 1] = (mem[top + 1] == mem[top]) ? 1 : 0;
					top++;
					break;

				case (int)Instruction.Thrd:
					pc++;
					Channel++;
					break;

				case (int)Instruction.Pause:
					pc++;
					duration = mem[top];
					top++;
					SetPause(duration);
					break;

				default:
					terminated = true;
					break;
			}
		}

        public static void Start()
        {
			while (!terminated)
			{
				Execute();
			}
		}

		public static void SetInstrument(int instrumentCode)
        {
			ChannelMessage message = new ChannelMessage(ChannelCommand.ProgramChange, Channel, instrumentCode, 0);
			MyMusicCommand command = new MyMusicCommand(message, 0);
			StoreCommand(command);			
		}

		public static void SetTone(int tone, int duration, int volume)
        {
			ChannelMessage message = new ChannelMessage(ChannelCommand.NoteOn, Channel, tone, volume);
			MyMusicCommand command = new MyMusicCommand(message, duration);
			StoreCommand(command);
			message = new ChannelMessage(ChannelCommand.NoteOff, message.MidiChannel, tone, 0);
			command = new MyMusicCommand(message, duration);
			StoreCommand(command);
		}

		public static void SetPause(int duration)
        {
			MyMusicCommand myMusic = new MyMusicCommand(new ChannelMessage(ChannelCommand.NoteOn, Channel, 0), duration);
			StoreCommand(myMusic);
			
			myMusic = new MyMusicCommand(new ChannelMessage(ChannelCommand.NoteOff, Channel, 0), duration);
			StoreCommand(myMusic);
		}

		public static void SetVolume(int volume)
        {
			ChannelMessage message = new ChannelMessage(ChannelCommand.Controller, Channel, (int)ControllerType.Volume, volume);
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
						outDevice.Send(cmd.command);
						break;
					}
					int delayTime = 0, delayInterval = 200, remainingDelay = cmd.duration;
					outDevice.Send(cmd.command);
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
                }
			});

            var thread2 = Task.Run(async () =>
            {
                foreach (MyMusicCommand cmd in Thread2)
                {
					if (cancellationToken.IsCancellationRequested)
					{
						cmd.command = new ChannelMessage(ChannelCommand.NoteOff, cmd.command.MidiChannel, cmd.command.Data1, cmd.command.Data2);
						outDevice.Send(cmd.command);
						break;
					}
					int delayTime = 0, delayInterval = 200, remainingDelay = cmd.duration;
					outDevice.Send(cmd.command);
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
				}
            });

            var thread3 = Task.Run(async () =>
            {
				foreach (MyMusicCommand cmd in Thread3)
				{
					if (cancellationToken.IsCancellationRequested)
					{
						cmd.command = new ChannelMessage(ChannelCommand.NoteOff, cmd.command.MidiChannel, cmd.command.Data1, cmd.command.Data2);
						outDevice.Send(cmd.command);
						break;
					}
					int delayTime = 0, delayInterval = 200, remainingDelay = cmd.duration;
					outDevice.Send(cmd.command);
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
				}
			});

            var thread4 = Task.Run(async () =>
            {
				foreach (MyMusicCommand cmd in Thread4)
				{
					if (cancellationToken.IsCancellationRequested)
					{
						cmd.command = new ChannelMessage(ChannelCommand.NoteOff, cmd.command.MidiChannel, cmd.command.Data1, cmd.command.Data2);
						outDevice.Send(cmd.command);
						break;
					}
					int delayTime = 0, delayInterval = 200, remainingDelay = cmd.duration;
					outDevice.Send(cmd.command);
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
				}
			});

            await Task.WhenAll(new[] { thread1, thread2, thread3, thread4 });
		}
		
		public static void SetJumpToProgramBody()
        {
			int offset = Variables.Count;
            Poke((int)Instruction.Jmp);
            Poke(2 + offset);
            adr += offset;
		}
		
		public VirtualMachine()
        {
			if (outDevice is null || outDevice.IsDisposed)
            {
				outDevice = new OutputDevice(DEVICE_ID);
            }
        }
		
		public void Dispose()
        {
			for (int i = 0; i < MemoryAllocSize; i++) 
			{
				mem[i] = -1;
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
			switch (Channel)
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
					outDevice.Send(new ChannelMessage(ChannelCommand.NoteOff, i, 0));
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
