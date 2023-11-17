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
		Duration,
		Direction,
		Insturment,
		// general
		Loop,
		Print,
		Push,
		Random,
		Minus,
		Add,
		Sub,
		Compare,
		Grt,
		Lwr,
		GrEq,
		LrEq,
		Eql,
		Diff,
		Mul,
		Div,
		Jmp,
		Get,
		Set,
		JmpIfFalse,
		Call,
		Return,
		Thrd,
		Pause
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
