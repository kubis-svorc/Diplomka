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
		Sound		= 1,
		Volume		= 2,
		Duration	= 3,
		Direction	= 4,
		Insturment	= 5,
		// general
		Loop		= 6,
		Print		= 7,
		Push		= 8,
		Minus		= 9,
		Add			= 10,
		Sub			= 11,
		Compare		= 12,
		Mul			= 13,
		Div			= 14,
		Jmp			= 15,
		Get			= 16,
		Set			= 17,
		JmpIfFalse	= 18,
		Call		= 19,
		Return		= 20,
		
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
