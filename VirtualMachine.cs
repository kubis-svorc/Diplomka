using Sanford.Multimedia.Midi;
using System;
using System.Collections.Generic;

namespace Diplomka.Runtime
{
    public class VirtualMachine : IDisposable
	{
		private static OutputDevice outDevice;

		public static Sequence sequence;

		public static Track track;

		private const int CHANNEL = 1, DEVICE_ID = 0, MemoryAllocSize = 100; // memory alloc

		public const int DEFAULT_VOLUME = 127, DEFAULT_DURATION = 500;

		private static int midiPos;

		public static int[] mem;

		public static int adr, pc, top;

		private static bool terminated;

		public static int CounterAddress = MemoryAllocSize - 1;

		public static IDictionary<string, int> Variables;

		//public static IDictionary<string, Analyzators.Subroutine> Subroutines;
			
		static VirtualMachine()
		{
			mem = new int[MemoryAllocSize];
			terminated = false;
			adr = 0;
			pc = 0;
			outDevice = new OutputDevice(DEVICE_ID);
			Variables = new Dictionary<string, int>();
			//Subroutines = new Dictionary<string, Analyzators.Subroutine>();
			top = MemoryAllocSize;
			track = new Track();
			sequence = new Sequence();
			sequence.Add(track);
		}

		private static void AddCommandToTrack(IMidiMessage message, int duration)
        {
			track.Insert(message: message, position: midiPos);
			midiPos += duration;
        }

		public static void Reset()
		{
			pc = 0;
			adr = 0;
			terminated = false;
			Variables.Clear();
			midiPos = 0;			
		}

		public static void Poke(int code)
		{
			mem[adr] = code;
			adr++;
		}

		private static void Execute(ref int volume, ref int duration)
		{
			int index;
			switch (mem[pc])
			{
				case (int)Instruction.Sound:
					pc++;
					Play(mem[pc], duration: ref duration, volume: ref volume);
					pc++;
					break;

				case (int)Instruction.Volume:
					pc++;
					volume = mem[pc];
					pc++;
					break;

				case (int)Instruction.Duration:
					pc++;
					duration = mem[pc];
					pc++;
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

				case (int)Instruction.Insturment:
					pc++;
					SetInstrument(mem[pc]);
					pc++;
					break;
				
				case (int)Instruction.Push:
					pc++;
					top--;
					mem[top] = mem[pc];
					pc++;
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

				default:
					terminated = true;
					break;
			}
		}

        public static void Start()
        {
			int volume = DEFAULT_VOLUME, duration = DEFAULT_DURATION;
			while (!terminated)
            {
                Execute(volume: ref volume, duration: ref duration);
            }
        }

        public static void Play(int tone, ref int duration, ref int volume)
        {
			ChannelMessage message = new ChannelMessage(ChannelCommand.NoteOn, CHANNEL, tone, volume);
			outDevice.Send(message);
			System.Threading.Thread.Sleep(duration);
			midiPos += duration;

			message = new ChannelMessage(ChannelCommand.NoteOff, CHANNEL, tone, 0);
			midiPos++;
			outDevice.Send(message);

			volume = DEFAULT_VOLUME;
			duration = DEFAULT_DURATION;
		}

		public static void SetInstrument(int instrumentCode)
        {
			ChannelMessage message = new ChannelMessage(ChannelCommand.ProgramChange, CHANNEL, instrumentCode, 0);
			outDevice.Send(message);			
			midiPos ++;
		}

        public static void SetVolume(int volume)
        {
			var message = new ChannelMessage(command:ChannelCommand.Controller, 
											midiChannel: CHANNEL, 
											data1: (int)ControllerType.Volume, 
											data2: volume);
			midiPos++;
			outDevice.Send(message);
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
            outDevice.Dispose();
        }

	}

}
