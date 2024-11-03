using System.Text.RegularExpressions;

namespace WordFinder
{
	public class RegexFinder
	{
		public class CharNum
		{
			public char Character { get; set; }
			public int Number { get; set; }
		}

		public class CharRule : CharNum
		{
			public CharacterRule RuleType { get; set; } = default!;

			public override string ToString()
			{
				return $"{Character}|{Number}|{RuleType}";
			}
		}

		public enum CharacterRule
		{
			AtPosition,
			NotAtPosition,
			ExactCount,
			MinCount,
			MaxCount,
			Exclude
		}

		public enum CountRestriction
		{
			None,
			ExactCount,
			MinCount,
			MaxCount
		}

		public string Characters { get; set; } = default!;
		public List<CharRule> CharacterRules { get; set; } = [];
		public bool RestrictWordLength { get; set; }
		public int WordLength { get; set; }
		public int WordMaxLength { get; set; }
		public int WordMinLength { get; set; }
		public bool IsLengthRange { get; set; }
		public CountRestriction RestrictCount { get; set; }
		public bool IncludeAll { get; set; }
		public bool OnlyThese { get; set; }

		public override string ToString()
		{
			return $"Characters: {Characters}\n" +
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
			if (Character.ToString() != GetSafeString(Character.ToString()))
			{
				return;
			}
			CharRule newRule = new()
			{
				Character = Character.ToString().ToLower()[0],
				Number = Number,
				RuleType = Rule
			};
			if (SkipOrReplaceUnneeded && IsUnneeded(newRule))
			{
				// Do nothing
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
				&& (
					r.Number == Number
					|| r.RuleType == CharacterRule.Exclude
				)
				&& r.RuleType == Rule
			);
		}

		private bool IsUnneeded(CharRule Rule)
		{
			// Check for exact matches
			if (CharacterRules.Any(
				r => r.RuleType == Rule.RuleType
				&& r.Character == Rule.Character
				&& (
					r.Number == Rule.Number
					|| r.RuleType == CharacterRule.Exclude
					)
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
			// Line Beginnings
			regex += "^";
			if (OnlyThese)
			{
				// Find words with only these characters
				regex += $"([{chars}]";
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
					regex += $"{{{WordMinLength},{WordMaxLength}}}";
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

			// Create rules to handle other restrictions
			List<CharRule> rules = [];
			if (RestrictCount == CountRestriction.ExactCount)
			{
				rules.AddRange(charactersWithCount.Select(r => ConvertToRule(r, CharacterRule.ExactCount)));
			}
			else if (RestrictCount == CountRestriction.MinCount)
			{
				rules.AddRange(charactersWithCount.Select(r => ConvertToRule(r, CharacterRule.MinCount)));
			}
			else if (RestrictCount == CountRestriction.MaxCount)
			{
				rules.AddRange(charactersWithCount.Select(r => ConvertToRule(r, CharacterRule.MaxCount)));
			}
			// RestrictExactCount and RestrictMinCount already cause the characters to be required.
			if (IncludeAll && RestrictCount != CountRestriction.ExactCount && RestrictCount != CountRestriction.MinCount)
			{
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
			return $"{GetRegexFromRules(CharacterRules.Concat(rules))}{regex}";
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

		private static CharRule ConvertToRule(CharNum CharacterWithNumber, CharacterRule Rule)
		{
			return new CharRule
			{
				Character = CharacterWithNumber.Character,
				Number = CharacterWithNumber.Number,
				RuleType = Rule
			};
		}

		private static string GetRegexFromRules(IEnumerable<CharRule> rules)
		{
			string regexPosition = "";
			string regexCount = "";
			string excludedChars = "";

			foreach (CharRule rule in rules)
			{
				if (rule.RuleType == CharacterRule.AtPosition)
				{
					regexPosition += $"(?=.{{{rule.Number - 1}}}{rule.Character}.*)";
				}
				else if (rule.RuleType == CharacterRule.NotAtPosition)
				{
					regexPosition += $"(?!.{{{rule.Number - 1}}}{rule.Character}.*)";
				}
				else if (rule.RuleType == CharacterRule.ExactCount)
				{
					regexCount += $"(?=(.*{rule.Character}.*){{{rule.Number}}})";
				}
				else if (rule.RuleType == CharacterRule.MinCount)
				{
					regexCount += $"(?=(.*{rule.Character}.*){{{rule.Number},}})";
				}
				else if (rule.RuleType == CharacterRule.MaxCount)
				{
					regexCount += $"(?!(.*{rule.Character}.*){{{rule.Number + 1},}})";
				}
				else if (rule.RuleType == CharacterRule.Exclude)
				{
					excludedChars += rule.Character;
				}
			}
			return $"{regexPosition}{regexCount}{(excludedChars.Length > 0 ? $"(?!.*[{excludedChars}].*)" : "")}";
		}

		static private List<CharNum> GetCharactersWithCount(string Characters)
		{
			List<CharNum> list = [];
			foreach (char c in GetSafeString(Characters).ToCharArray())
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

		static public string GetSafeString(string str)
		{
			if (str == null) { return ""; }
			// Replace all characters that are not alphanumeric and not empty
			return Regex.Replace(str.ToLower(), "[^a-zA-Z0-9]", "");
		}
	}
}
