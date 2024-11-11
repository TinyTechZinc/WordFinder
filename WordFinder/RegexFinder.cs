using System.Diagnostics;
using System.Text.RegularExpressions;
using WordFinder.Exceptions;

namespace WordFinder
{
	public class RegexFinder
	{
		public enum CharacterRule
		{
			AtPosition,
			NotAtPosition,
			ExactCount,
			MinCount,
			MaxCount
		}
		[Flags]
		public enum CountRestrictions
		{
			None =			0b0,
			ExactCount =	0b1,
			MinCount =		0b10,
			MaxCount =		0b100
		}
		public string Characters { get; set; } = "";
		/// <summary>
		/// Require all characters in Characters to be used.
		/// Sets the minimum count for each character to the number of times it appears in Characters.
		/// </summary>
		public bool IncludeAll { get; set; }
		/// <summary>
		/// Limit the characters used to only those in Characters.
		/// </summary>
		public bool OnlyThese { get; set; }
		/// <summary>
		/// Add constraints on the length of the word.
		/// </summary>
		public bool RestrictWordLength { get; set; }
		/// <summary>
		/// Determines whether the word length is a range or exact.
		/// Ignored if <see cref="RestrictWordLength"/> is <see langword="false"/>.
		/// </summary>
		public bool IsLengthRange { get; set; }
		/// <summary>
		/// Sets the required word length.
		/// Ignored if <see cref="RestrictWordLength"/> is <see langword="false"/> or <see cref="IsLengthRange"/> is <see langword="true"/>.
		/// </summary>
		public int WordLength { get; set; }
		/// <summary>
		/// Optionally add a maximum length for the word.
		/// Ignored if <see cref="RestrictWordLength"/> or <see cref="IsLengthRange"/> is <see langword="false"/>.
		/// </summary>
		public int? WordMaxLength { get; set; }
		/// <summary>
		/// Optionally add a minimum length for the word.
		/// Ignored if <see cref="RestrictWordLength"/> or <see cref="IsLengthRange"/> is <see langword="false"/>.
		/// </summary>
		public int? WordMinLength { get; set; }
		/// <summary>
		/// Exclude these characters from the word.
		/// </summary>
		public string ExcludeCharacters { get; set; } = "";
		/// <summary>
		/// Add constraints on the number of times each character can appear in the word.
		/// </summary>
		public CountRestrictions RestrictCount { get; set; }
		/// <summary>
		/// The set of characters considered dangerous to the regular expression and must be escaped.
		/// </summary>
		public static readonly char[] EscapeTheseCharacters = [
			'.', '*', '+',
			'\\', '^', '$', '|', '?', '-',
			'[', ']', '(', ')', '{', '}'
		];
		private Dictionary<int, char> CharacterAtPosition { get; set; } = [];
		private Dictionary<int, List<char>> CharactersNotAtPosition { get; set; } = [];
		public class CountConstraint
		{
			public int? ExactCount { get; set; }
			public int? MinCount { get; set; }
			public int? MaxCount { get; set; }
			public string ToRegex(char character)
			{
				string regex = "";
				if (ExactCount != null)
				{
					regex += $"(?=(.*[{EscapeCharacters(character)}].*){{{ExactCount}}})";
					// Negative lookahead to ensure the character does not appear more than the exact count
					regex += $"(?!(.*[{EscapeCharacters(character)}].*){{{ExactCount + 1}}})";
				}
				// We cannot combine MinCount and MaxCount because MaxCount must be a negative lookahead
				else
				{
					if (MinCount != null)
					{
						regex += $"(?=(.*[{EscapeCharacters(character)}].*){{{MinCount},}})";
					}
					if (MaxCount != null)
					{
						regex += $"(?!(.*[{EscapeCharacters(character)}].*){{{MaxCount + 1},}})";
					}
				}
				return regex;
			}
		}
		private Dictionary<char, CountConstraint> CharacterCount { get; set; } = [];
		public override string ToString()
		{
			return $"Characters: {Characters}\n" +
				$"ExcludeCharacters: {ExcludeCharacters}\n" +
				$"WordLength: {WordLength}\n" +
				$"WordMaxLength: {WordMaxLength}\n" +
				$"WordMinLength: {WordMinLength}\n" +
				$"IsLengthRange: {IsLengthRange}\n" +
				$"RestrictCount: {RestrictCount}\n" +
				$"IncludeAll: {IncludeAll}\n" +
				$"OnlyThese: {OnlyThese}";
		}
		public void SetCharacterAtPosition(char character, int position, bool errorOnDuplicateOrConflict = true)
		{
			if (CharactersNotAtPosition.TryGetValue(position, out var chars) && chars.Contains(character))
			{
				throw new FinderRuleConflictException($"Character '{character}' cannot be at position {position} because it is already excluded from that position.");
			}
			if (!CharacterAtPosition.TryAdd(position, character))
			{
				if (errorOnDuplicateOrConflict)
				{
					if (CharacterAtPosition[position] != character)
					{
						throw new FinderRuleConflictException($"Cannot add '{character}' to position {position}, position already has '{CharacterAtPosition[position]}'.");
					}
					else
					{
						throw new FinderDuplicateRuleException($"Already has '{character}' at position {position}.");
					}
				}
				else
				{
					// Replace the character
					CharacterAtPosition[position] = character;
				}
			}
		}
		public void SetCharacterNotAtPosition(char character, int position, bool errorOnDuplicateOrConflict = true)
		{
			if (errorOnDuplicateOrConflict && CharacterAtPosition.TryGetValue(position, out var c) && c == character)
			{
				throw new FinderRuleConflictException($"Character '{character}' cannot be excluded from position {position} because it is already at that position.");
			}
			if (!CharactersNotAtPosition.TryGetValue(position, out var chars))
			{
				chars = [character];
				CharactersNotAtPosition.Add(position, chars);
			}
			else if (errorOnDuplicateOrConflict && chars.Contains(character))
			{
				throw new FinderDuplicateRuleException($"Character '{character}' is already excluded from position {position}.");
			}
			else
			{
				chars.Add(character);
			}
		}
		public void SetCharacterCount(char character, CountRestrictions countRestriction, int count, bool errorOnDuplicateOrConflict = true)
		{
			// Obtain or create count constraint
			if (!CharacterCount.TryGetValue(character, out var constraint))
			{
				constraint = new CountConstraint();
				CharacterCount.Add(character, constraint);
			}
			switch (countRestriction)
			{
				case CountRestrictions.None:
					throw new ArgumentException("Count restriction must be specified.", nameof(countRestriction));
				case CountRestrictions.ExactCount:
					if (constraint.ExactCount != null)
					{
						if (errorOnDuplicateOrConflict)
						{
							throw new FinderRuleConflictException($"Exact count for '{character}' is already set to {constraint.ExactCount}.");
						}
						else
						{
							constraint.ExactCount = count;
						}
					}
					else
					{
						constraint.ExactCount = count;
					}
					break;
				case CountRestrictions.MinCount:
					if (constraint.MinCount != null && constraint.MinCount > count)
					{
						if (errorOnDuplicateOrConflict)
						{
							throw new FinderRuleConflictException($"A tighter minimum count for '{character}' is already set to {constraint.MinCount}.");
						}
						else
						{
							constraint.MinCount = count;
						}
					}
					else
					{
						constraint.MinCount = count;
					}
					break;
				case CountRestrictions.MaxCount:
					if (constraint.MaxCount != null && constraint.MaxCount < count)
					{
						if (errorOnDuplicateOrConflict)
						{
							throw new FinderRuleConflictException($"A tighter maximum count for '{character}' is already set to {constraint.MaxCount}.");
						}
						else
						{
							constraint.MaxCount = count;
						}
					}
					else
					{
						constraint.MaxCount = count;
					}
					break;
				default:
					throw new NotImplementedException($"Count restriction {countRestriction} is not implemented.");
			}
		}
		public void AddRule(char character, int number, CharacterRule rule, bool errorOnDuplicateOrConflict = true)
		{
			switch (rule)
			{
				case CharacterRule.AtPosition:
					SetCharacterAtPosition(character, number, errorOnDuplicateOrConflict);
					break;
				case CharacterRule.NotAtPosition:
					SetCharacterNotAtPosition(character, number, errorOnDuplicateOrConflict);
					break;
				case CharacterRule.ExactCount:
					SetCharacterCount(character, CountRestrictions.ExactCount, number, errorOnDuplicateOrConflict);
					break;
				case CharacterRule.MinCount:
					SetCharacterCount(character, CountRestrictions.MinCount, number, errorOnDuplicateOrConflict);
					break;
				case CharacterRule.MaxCount:
					SetCharacterCount(character, CountRestrictions.MaxCount, number, errorOnDuplicateOrConflict);
					break;
				default:
					throw new NotImplementedException($"Rule type {rule} is not implemented.");
			}
		}
		public string GetRegex()
		{
			// Validate
			if (Characters.Intersect(ExcludeCharacters).Any())
			{
				throw new FinderRuleConflictException("The same character is in both Characters and Exclude Characters.");
			}
			string filler;
			if (OnlyThese)
			{
				filler = $"[{EscapeCharacters(string.Join(null, Characters.Distinct()))}]";
			}
			else if (ExcludeCharacters.Length > 0)
			{
				// Negative character groups would normally allow newlines, so \n needs to be explicitly excluded
				filler = $"[^{EscapeCharacters(string.Join(null, ExcludeCharacters.Distinct()))}\n]";
			}
			else
			{
				filler = ".";
			}
			string regex = "";
			if (CharacterAtPosition.Count > 0 || CharactersNotAtPosition.Count > 0)
			{
				var maxPosition = Math.Max(
					CharacterAtPosition.Count == 0 ? 0 : CharacterAtPosition.Keys.Max(),
					CharactersNotAtPosition.Count == 0 ? 0 : CharactersNotAtPosition.Keys.Max());
				if (RestrictWordLength)
				{
					if (IsLengthRange && maxPosition > WordMaxLength)
					{
						throw new FinderRuleConflictException("There is at least one 'Character at position' or 'Character not at position' rule beyond the maximum word length.");
					}
					if (!IsLengthRange && maxPosition > WordLength)
					{
						throw new FinderRuleConflictException("There is at least one 'Character at position' or 'Character not at position' rule beyond the word length.");
					}
				}
				int buffer = 0;
				for (int i = 1; i <= maxPosition; i++)
				{
					if (CharacterAtPosition.TryGetValue(i, out var c))
					{
						if (buffer > 0)
						{
							if (buffer == 1)
							{
								regex += $"{filler}";
							}
							else
							{
								regex += $"{filler}{{{buffer}}}";
							}
							buffer = 0;
						}
						regex += $"[{EscapeCharacters(c)}]";
					}
					else if (CharactersNotAtPosition.TryGetValue(i, out var xChars))
					{
						if (buffer > 0)
						{
							if (buffer == 1)
							{
								regex += $"{filler}";
							}
							else
							{
								regex += $"{filler}{{{buffer}}}";
							}
							buffer = 0;
						}
						if (OnlyThese)
						{
							// The filler specifies the allowed characters
							regex += $"(?![{EscapeCharacters(string.Join(null, xChars))}]){filler}";
						}
						else
						{
							// It can be any character except the excluded ones
							regex += $"[^{EscapeCharacters(string.Join(null, xChars.Concat(ExcludeCharacters).Distinct()))}\n]";
						}
					}
					else
					{
						buffer++;
					}
				}
				// buffer should be 0 so we can ignore it now
				// - It is only incremented when there more character positions to handle
				// - It is reset to 0 when a character position is handled
				// - The loop ends when the last character position is handled (i == maxPosition)
				// - Therefore buffer should be 0
				Debug.Assert(buffer == 0);
				// Add filler for remaining characters to match length requirements
				if (RestrictWordLength)
				{
					if (IsLengthRange)
					{
						// Range length
						if (WordMaxLength != null && WordMaxLength > maxPosition)
						{
							if (WordMinLength != null && WordMinLength > maxPosition)
							{
								regex += $"{filler}{{{WordMinLength - maxPosition},{WordMaxLength - maxPosition}}}";
							}
							else
							{
								// Ignore min length because it is already met or not set
								regex += $"{filler}{{0,{WordMaxLength - maxPosition}}}";
							}
						}
						else if (WordMaxLength == null)
						{
							// No max length
							if (WordMinLength != null && WordMinLength > maxPosition)
							{
								regex += $"{filler}{{{WordMinLength - maxPosition},}}";
							}
							else
							{
								regex += $"{filler}*";
							}
						}
						else
						{
							// Already reached the max length, nothing more to add
						}
					}
					else if (WordLength > maxPosition)
					{
						// Exact length
						regex += $"{filler}{{{WordLength - maxPosition}}}";
					}
				}
				else
				{
					// Any length
					regex += $"{filler}*";
				}
			}
			else
			{
				// There are no character position rules
				// regex == "" here
				if (RestrictWordLength)
				{
					if (IsLengthRange)
					{
						if (WordMinLength == null && WordMaxLength == null)
						{
							// Any Length
							regex += $"{filler}+";
						}
						else
						{
							regex += $"{filler}{{{WordMinLength ?? 0},{(WordMaxLength != null ? WordMaxLength : "")}}}";
						}
					}
					else
					{
						// Exact length
						regex += $"{filler}{{{WordLength}}}";
					}
				}
				else
				{
					// Any Length
					regex += $"{filler}+";
				}
			}
			// Adjust character count constraints using IncludeAll
			if (IncludeAll)
			{
				// Add minimum count restriction if it is not already set
				RestrictCount |= CountRestrictions.MinCount;
			}

			// Handle character count requirements
			if (RestrictCount != CountRestrictions.None)
			{
				// Count the number of times each character appears
				Dictionary<char, int> charCounts = [];
				foreach (char c in Characters)
				{
					if (charCounts.TryGetValue(c, out var count))
					{
						charCounts[c] = count + 1;
					}
					else
					{
						charCounts.Add(c, 1);
					}
				}
				// Add constraints for each character
				foreach (var cc in charCounts)
				{
					if ((RestrictCount & CountRestrictions.ExactCount) == CountRestrictions.ExactCount)
					{
						if (CharacterCount.TryGetValue(cc.Key, out var constraint))
						{
							// Overwrites existing constraint
							constraint.ExactCount = cc.Value;
						}
						else
						{
							CharacterCount.Add(cc.Key, new CountConstraint { ExactCount = cc.Value });
						}
					}
					else // If the exact count is used, the min and max can be ignored
					{
						if ((RestrictCount & CountRestrictions.MinCount) == CountRestrictions.MinCount)
						{
							if (CharacterCount.TryGetValue(cc.Key, out var constraint))
							{
								// Keep the tighter constraint
								if (constraint.MinCount == null || cc.Value > constraint.MinCount)
								{
									constraint.MinCount = cc.Value;
								}
							}
							else
							{
								CharacterCount.Add(cc.Key, new CountConstraint { MinCount = cc.Value });
							}
						}
						if ((RestrictCount & CountRestrictions.MaxCount) == CountRestrictions.MaxCount)
						{
							if (CharacterCount.TryGetValue(cc.Key, out var constraint))
							{
								// Keep the tighter constraint
								if (constraint.MaxCount == null || cc.Value < constraint.MaxCount)
								{
									constraint.MaxCount = cc.Value;
								}
							}
							else
							{
								CharacterCount.Add(cc.Key, new CountConstraint { MaxCount = cc.Value });
							}
						}
					}
				}
			}
			// Build character count regex
			string countRegex = "";
			foreach (var cc in CharacterCount)
			{
				countRegex += cc.Value.ToRegex(cc.Key);
			}
			return $"^(?<word>{countRegex}{regex})$";
		}

		/// <summary>
		/// ToSearch should contain words separated by newlines
		/// </summary>
		/// <param name="ToSearch"></param>
		/// <returns></returns>
		public static List<string> FindWords(string ToSearch, string regex)
		{
			List<string> words = [];
			foreach (Match match in Regex.Matches(ToSearch, regex, RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.ExplicitCapture))
			{
				words.Add(match.Value);
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
