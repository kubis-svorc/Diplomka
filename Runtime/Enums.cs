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
		// general
		Loop = 5,
		Print = 6,
		Push = 7,
		Random = 8,
		Minus = 9,
		Add = 10,
		Sub = 11,
		Compare = 12,
		Grt = 13,
		Lwr = 14,
		GrEq = 15,
		LrEq = 16,
		Eql = 17,
		Diff = 18,
		Mul = 19,
		Div = 20,
		Jmp = 21,
		Get = 22,
		Set = 23,
		JmpIfFalse = 24,
		Call = 25,
		Return = 26,
		Thrd = 27,
		Pause = 28
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
