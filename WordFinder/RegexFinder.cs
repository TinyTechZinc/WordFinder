using System.Text.RegularExpressions;

namespace WordFinder
{
	public class RegexFinder
	{
		public class CharNum
		{
			public char Character { get; set; }
			public int Number { get; set; }
			public CharRule ConvertToRule(CharacterRule Rule)
			{
				return new CharRule
				{
					Character = Character,
					Number = Number,
					RuleType = Rule
				};
			}
		}

		public class CharRule : CharNum
		{
			public CharacterRule RuleType { get; set; } = default!;

			public override string ToString()
			{
				return $"{Character}|{Number}|{RuleType}";
			}
			public string ConvertToRegex()
			{
				if (RuleType == CharacterRule.AtPosition)
				{
					return $"(?=.{{{Number - 1}}}[{EscapeCharacters(Character)}].*)";
				}
				else if (RuleType == CharacterRule.NotAtPosition)
				{
					return $"(?!.{{{Number - 1}}}[{EscapeCharacters(Character)}].*)";
				}
				else if (RuleType == CharacterRule.ExactCount)
				{
					return $"(?=(.*[{EscapeCharacters(Character)}].*){{{Number}}})";
				}
				else if (RuleType == CharacterRule.MinCount)
				{
					return $"(?=(.*[{EscapeCharacters(Character)}].*){{{Number},}})";
				}
				else if (RuleType == CharacterRule.MaxCount)
				{
					return $"(?!(.*[{EscapeCharacters(Character)}].*){{{Number + 1},}})";
				}
				else
				{
					return "";
				}
			}
		}

		public enum CharacterRule
		{
			AtPosition,
			NotAtPosition,
			ExactCount,
			MinCount,
			MaxCount
		}

		[Flags]
		public enum CountRestriction
		{
			None =			0b0,
			ExactCount =	0b1,
			MinCount =		0b10,
			MaxCount =		0b100
		}

		public string Characters { get; set; } = "";
		public string ExcludeCharacters { get; set; } = "";
		public List<CharRule> CharacterRules { get; set; } = [];
		public bool RestrictWordLength { get; set; }
		public int WordLength { get; set; }
		public int? WordMaxLength { get; set; }
		public int? WordMinLength { get; set; }
		public bool IsLengthRange { get; set; }
		public CountRestriction RestrictCount { get; set; }
		public bool IncludeAll { get; set; }
		public bool OnlyThese { get; set; }
		public static readonly char[] EscapeTheseCharacters = [
			'.', '*', '+',
			'\\', '^', '$', '|', '?', '-',
			'[', ']', '(', ')', '{', '}'
		];

		public override string ToString()
		{
			return $"Characters: {Characters}\n" +
				$"ExcludeCharacters: {ExcludeCharacters}\n" +
				$"CharacterRules: {string.Join(",", CharacterRules)}\n" +
				$"WordLength: {WordLength}\n" +
				$"WordMaxLength: {WordMaxLength}\n" +
				$"WordMinLength: {WordMinLength}\n" +
				$"IsLengthRange: {IsLengthRange}\n" +
				$"RestrictCount: {RestrictCount}\n" +
				$"IncludeAll: {IncludeAll}\n" +
				$"OnlyThese: {OnlyThese}";
		}

		public void AddRule(char Character, int Number, CharacterRule Rule, bool SkipOrReplaceUnneeded = true)
		{
			CharRule newRule = new()
			{
				Character = Character.ToString().ToLower()[0],
				Number = Number,
				RuleType = Rule
			};
			if (SkipOrReplaceUnneeded && IsUnneeded(newRule))
			{
				// Do nothing, this is a redundant rule
			}
			else
			{
				if (SkipOrReplaceUnneeded)
				// Remove rules being replaced
				{
					if (newRule.RuleType == CharacterRule.MinCount)
					// Replace the lower bound
					{
						CharacterRules.RemoveAll(
							r => r.RuleType == CharacterRule.MinCount
							&& r.Character == newRule.Character
							&& r.Number < newRule.Number
						);
					}
					if (newRule.RuleType == CharacterRule.MaxCount)
					// Replace the upper bound
					{
						CharacterRules.RemoveAll(
							r => r.RuleType == CharacterRule.MaxCount
							&& r.Character == newRule.Character
							&& r.Number > newRule.Number
						);
					}
					if (newRule.RuleType == CharacterRule.ExactCount)
					// Replace both upper and lower bounds with exact
					{
						CharacterRules.RemoveAll(
							r => (
								r.RuleType == CharacterRule.MinCount
								|| r.RuleType == CharacterRule.MaxCount
							) && r.Character == newRule.Character
						);
					}
				}
				CharacterRules.Add(newRule);
			}
		}

		public void RemoveRule(char Character, int Number, CharacterRule Rule)
		{
			CharacterRules.RemoveAll(r =>
				r.Character == Character.ToString().ToLower()[0]
				&& r.Number == Number
				&& r.RuleType == Rule
			);
		}

		private bool IsUnneeded(CharRule Rule)
		{
			// Check for exact matches
			if (CharacterRules.Any(
				r => r.RuleType == Rule.RuleType
				&& r.Character == Rule.Character
				&& r.Number == Rule.Number
				)
			)
			{
				return true;
			}
			// Check if there is already a tighter lower bound
			else if (Rule.RuleType == CharacterRule.MinCount
				&& CharacterRules.Any(
					r => r.RuleType == CharacterRule.MinCount
					&& r.Number > Rule.Number
				)
			)
			{
				return true;
			}
			// Check if there is already a tighter upper bound
			else if (Rule.RuleType == CharacterRule.MaxCount
				&& CharacterRules.Any(
					r => r.RuleType == CharacterRule.MaxCount
					&& r.Number < Rule.Number
				)
			)
			{
				return true;
			}
			else
			{
				// There are no rules that make this rule unneeded
				return false;
			}
		}
		
		public string GetRegex()
		{
			// Get characters and counts
			List<CharNum> charactersWithCount = GetCharactersWithCount(Characters);
			string chars = "";
			foreach (CharNum c in charactersWithCount)
			{
				chars += c.Character.ToString();
			}
			string regex = "";
			// Line Beginning
			regex += "^";
			if (OnlyThese)
			{
				// Find words with only these characters
				regex += $"([{EscapeCharacters(chars)}]";
			}
			else
			{
				regex += "(.";
			}
			if (RestrictWordLength)
			{
				// With length
				if (IsLengthRange)
				{
					// Range length
					if (WordMinLength != null)
					{
						regex += $"{{{WordMinLength},{WordMaxLength}}}";
					}
					else if (WordMaxLength != null)
					{
						regex += $"{{0,{WordMaxLength}}}";
					}
					else
					{
						// Any Length
						regex += "+";
					}
				}
				else
				{
					// Exact length
					regex += $"{{{WordLength}}}";
				}
			}
			else
			{
				regex += "+";
			}
			// Separate using line end
			regex += ")$";

			// Create rules to handle character restrictions
			List<CharRule> rules = [];
			if ((RestrictCount & CountRestriction.ExactCount) == CountRestriction.ExactCount)
			{
				rules.AddRange(charactersWithCount.Select(r => r.ConvertToRule(CharacterRule.ExactCount)));
			}
			else if ((RestrictCount & CountRestriction.MinCount) == CountRestriction.MinCount)
			{
				rules.AddRange(charactersWithCount.Select(r => r.ConvertToRule(CharacterRule.MinCount)));
			}
			else if ((RestrictCount & CountRestriction.MaxCount) == CountRestriction.MaxCount)
			{
				rules.AddRange(charactersWithCount.Select(r => r.ConvertToRule(CharacterRule.MaxCount)));
			}
			// RestrictExactCount and RestrictMinCount already cause the characters to be required.
			if (IncludeAll && 
				(RestrictCount & CountRestriction.ExactCount | RestrictCount & CountRestriction.MinCount) == CountRestriction.None)
			{
				// Not using ConvertToRule because we only want at least one of each character
				rules.AddRange(charactersWithCount.Select(
					r => new CharRule
					{
						Character = r.Character,
						Number = 1,
						// Require at least one
						RuleType = CharacterRule.MinCount
					}
				));
			}
			// Build final regex string
			string ruleRegex = "";
			foreach (CharRule rule in CharacterRules.Concat(rules))
			{
				ruleRegex += rule.ConvertToRegex();
			}
			return $"{ruleRegex}{(ExcludeCharacters.Length > 0 ? $"(?!.*[{EscapeCharacters(ExcludeCharacters)}].*)" : "")}{regex}";
		}

		/// <summary>
		/// ToSearch should contain words separated by newlines
		/// </summary>
		/// <param name="ToSearch"></param>
		/// <returns></returns>
		public static List<string> FindWords(string ToSearch, string regex)
		{
			MatchCollection matches = Regex.Matches(
				ToSearch,
				regex,
				RegexOptions.IgnoreCase | RegexOptions.Multiline
			);
			List<string> words = [];
			foreach (Match match in matches.Cast<Match>())
			{
				words.Add(match.Groups[0].Value);
			}
			return words;
		}

		public List<string> FindWords(string ToSearch)
		{
			return FindWords(ToSearch, GetRegex());
		}

		public List<string> FindWordsFromFile(string FilePath)
		{
			return FindWords(File.ReadAllText(FilePath).ReplaceLineEndings("\n"));
		}

		static private List<CharNum> GetCharactersWithCount(string Characters)
		{
			List<CharNum> list = [];
			foreach (char c in Characters)
			{
				int i = list.FindIndex(j => j.Character == c);
				if (i < 0)
				{
					list.Add(
						new CharNum
						{
							Character = c,
							Number = 1
						}
					);
				}
				else
				{
					list[i].Number++;
				}
			}
			return list;
		}
		static public string EscapeCharacters(string str)
		{
			string toReturn = "";
			foreach (char c in str)
			{
				if (EscapeTheseCharacters.Contains(c))
				{
					toReturn += $"\\{c}";
				}
				else
				{
					toReturn += c;
				}
			}
			return toReturn;
		}
		static public string EscapeCharacters(char c)
		{
			if (EscapeTheseCharacters.Contains(c))
			{
				return $"\\{c}";
			}
			else
			{
				return c.ToString();
			}
		}
	}
}
