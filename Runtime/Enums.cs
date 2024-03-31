namespace Diplomka.Runtime
{
	public enum Kind
	{
		NOTHING = 0,
		NUMBER = 1,
		WORD = 2,
		SYMBOL = 3
	}

	public enum Instruction
	{
		// musical
		Sound = 1,
		Duration = 2,
		Direction = 3,
		Insturment = 4,
		Accord = 5,
		// general
		Loop = 6,
		Print = 7,
		Push = 8,
		Random = 9,
		Minus = 10,
		Add = 11,
		Sub = 12,
		Compare = 13,
		Grt = 14,
		Lwr = 15,
		GrEq = 16,
		LrEq = 17,
		Eql = 18,
		Diff = 19,
		Mul = 20,
		Div = 21,
		Jmp = 22,
		Get = 23,
		Set = 24,
		JmpIfFalse = 25,
		Call = 26,
		Return = 27,
		Thrd = 28,
		Pause = 29
	}

	public enum Tone
	{
		C = 60,
		Cis = 61,
		Des = 61,
		D = 62,
		Dis = 63,
		Es = 63,
		E = 64,
		F = 65,
		Fis = 66,
		Ges = 66,
		G = 67,
		Gis = 68,
		As = 68,
		A = 69,
		Ais = 70,
		B = 70,
		H = 71,
		C2 = 72,
		Cis2 = 73,
		Des2 = 73,
		D2 = 74,
		Dis2 = 75,
		Es2 = 57,
		E2 = 76,
		F2 = 77,
		Fis2 = 78,
		Ges2 = 78,
		G2 = 79,
		Gis2 = 80,
		As2 = 80,
		A2 = 81,
		Ais2 = 82,
		B2 = 82,
		H2 = 83,
		C3 = 84
	}

}
