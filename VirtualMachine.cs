﻿using Sanford.Multimedia.Midi;
using System;
using System.Collections.Generic;
using Diplomka.Wrappers;
using System.Threading.Tasks;

namespace Diplomka.Runtime
{
    public class VirtualMachine : IDisposable
	{
		private static OutputDevice outDevice;
		public static Sequence sequence;
		public static Track track;
		public static int Channel = 0;
		public static string ChannelName = "hlavne";
		private const int DEVICE_ID = 0, MemoryAllocSize = 100; // memory alloc
		public const int DEFAULT_CHANNEL = 1, DEFAULT_VOLUME = 127, DEFAULT_DURATION = 500;
		private static int midiPos;
		public static int[] mem;
		public static int adr, pc, top;
		private static bool terminated;
		public static int CounterAddress = MemoryAllocSize - 1;
		public static IDictionary<string, int> Variables;
		public static IDictionary<string, Analyzators.Subroutine> Subroutines;
		//public static IDictionary<string, List<MyMusicCommand>> Threads;
		public static List<MyMusicCommand> Thread1;
		public static List<MyMusicCommand> Thread2;

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
			Thread1 = new List<MyMusicCommand>();
			Thread2 = new List<MyMusicCommand>();
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
			midiPos = 0;
			Variables.Clear();
			Subroutines.Clear();
			//Threads.Clear();
			Thread1.Clear();
			Thread2.Clear();
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
					//pc++;
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

				case (int)Instruction.Insturment:
					/*pc++;
					int tone = mem[top];
					top++;
					int volume = mem[top];
					top++;
					int duration = mem[top];
					top++;
					SetTone(tone, duration, volume);
					pc++;
					break;*/
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
					Channel++;// = mem[pc];
					break;

				default:
					terminated = true;
					break;
			}
		}

        public static async Task Start()
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
			//Threads[ChannelName].Add(command);
			//outDevice.Send(message);			
			//midiPos++;
		}

		public static void SetTone(int tone, int duration, int volume)
		{
			ChannelMessage message = new ChannelMessage(ChannelCommand.NoteOn, Channel, tone, volume);
			MyMusicCommand command = new MyMusicCommand(message, duration);

			//Threads[ChannelName].Add(command);
			StoreCommand(command);

			message = new ChannelMessage(ChannelCommand.NoteOff, Channel, tone, 0);
			command = new MyMusicCommand(message, duration);

			//Threads[ChannelName].Add(command);
			StoreCommand(command);
        }

		public static void SetVolume(int volume)
        {
			var message = new ChannelMessage(command:ChannelCommand.Controller, 
											midiChannel: Channel, 
											data1: (int)ControllerType.Volume, 
											data2: volume);
			MyMusicCommand command = new MyMusicCommand(message, 0);
			StoreCommand(command);
			//Threads[ChannelName].Add(command);
			//midiPos++;
			//outDevice.Send(message);
        }

		public static async Task Play() 
		{
			Task thread1 = Task.Run(() => 
			{
                foreach (MyMusicCommand cmd in Thread1)
                {
					outDevice.Send(cmd.command);
					System.Threading.Thread.Sleep(cmd.duration);
                }
			});

			Task thread2 = Task.Run(() =>
			{
				foreach (MyMusicCommand cmd in Thread2)
				{
					outDevice.Send(cmd.command);
					System.Threading.Thread.Sleep(cmd.duration);
				}
			});
			await Task.WhenAll(new [] { thread1, thread2 });
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
        }

		private static void StoreCommand(MyMusicCommand command)
        {
			switch (Channel)
			{
				case 2:
					Thread2.Add(command);
					break;
				default:
					Thread1.Add(command);
					break;
			}
		}
		
	}

}
