namespace Diplomka.Analyzators
{
	using System.Text;
	using Diplomka.Exceptions;
	using Diplomka.Runtime;
	using System.Linq;

	internal class LexicalAnalyzer
	{
		public string input;

		public char look;

		public int index, position;

		public StringBuilder token;

		public Kind kind;

		public LexicalAnalyzer()
		{
			token = new StringBuilder();
		}

		public void Next()
		{
			if (index >= input.Length)
			{
				look = '\0';
			}
			else
			{
				look = input[index];
				index++;
			}
		}

		public void Scan()
		{
			while (char.IsWhiteSpace(look))
            {
				Next();
            }

			//while (' ' == look || '\n' == look || '\r' == look || '\t' == look)
			//{
			//	Next();
			//}

			token.Clear();
			position = index - 1;

			if (char.IsNumber(look) || '-' == look)
			{
				do
				{
					token.Append(look);
					Next();

				} while (char.IsNumber(look));
				kind = Kind.NUMBER;
			}
			else if (char.IsLetter(look) || look == ':')// || look == '#')
			{
				do
				{
					token.Append(look);
					Next();

				} while (char.IsLetter(look) || look == ':');// || look == '#');
				kind = Kind.WORD;
			}
			else if (look != '\0')
			{
				token.Append(look);
				Next();
				kind = Kind.SYMBOL;
			}
			else
			{
				kind = Kind.NOTHING;
			}
		}

		public void Init()
		{
			index = 0;
			Next();
			Scan();									
		}

		public void Init(string codeText)
		{
			input = codeText;
			Init();
		}

		public override string ToString()
		{
			return token.ToString();
		}

		public void Check(Kind expected, string expectedToken = null)
		{
			string kindStr;
			switch (kind)
			{
				case Kind.NOTHING:
					kindStr = "koniec programu";
					break;
				case Kind.NUMBER:
					kindStr = "číslo";
					break;
				case Kind.WORD:
					kindStr = "slovo";
					break;
				case Kind.SYMBOL:
				default:
					kindStr = "symbol";
					break;
			}


			if (kind != expected)
			{
				string message;
				switch (expected)
				{
					case Kind.NUMBER:
						message = $"Chyba na pozicií {position} : Očakával som číslo, dostal som {kindStr}";
						break;
					case Kind.WORD:
						message = $"Chyba na pozicií {position} : Očakával som slovo, dostal som {kindStr}";
						break;
					case Kind.NOTHING:
						message = $"Chyba na pozicií {position} : Očákával som koniec programu, dostal som {kindStr}";
						break;
					case Kind.SYMBOL:
					default:
						message = $"Chyba na pozicií {position} : Očakával som symbol, dostal som {kindStr}";
						break;
				}
			}

			if (expectedToken != null && expectedToken != ToString())
			{
				int row = input.Substring(0, position).Count(s => s == '\r');
				string actualToken = ToString();
				if (string.IsNullOrEmpty(actualToken))
                {
					actualToken = "nič";
                }
				throw new SyntaxException($"Chyba v riadku {row}: Očakával som {expectedToken}, dostal som {actualToken}");
			}
		}		
	}
}
