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

		private const char END = '\0';

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
				look = END;
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
			
			else if (look == '<' || look == '>' || look == '!' || look == '=')
            {
				token.Append(look);
				Next();
				if (look == '=')
                {
					token.Append(look);
					Next();
                }
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

		/// <summary>
		/// Return builder.ToString()
		/// </summary>
		/// <returns>String reprezentation of token</returns>
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
				throw new SyntaxException(message);
			}

			if (!string.IsNullOrWhiteSpace(expectedToken) && expectedToken != ToString())
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
	
		public void Check(Kind expected, string[] expectedTokens) 
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
				throw new SyntaxException(message);
			}

			if (!expectedTokens.Contains<string>(ToString()))
            {
				int row = input.Substring(0, position).Count(s => s == '\r');
				string actualToken = ToString();
				if (string.IsNullOrEmpty(actualToken))
				{
					actualToken = "nič";
				}
				string message = $"Chyba v riadku {row}: Očakával som {string.Join(",", expectedTokens)}, dostal som {actualToken}";
				throw new SyntaxException(message);
            }
		}
	}
}
