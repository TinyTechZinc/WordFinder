using System.Text.RegularExpressions;
using UI.Views;
using WordFinder;
using WordFinder.Exceptions;

namespace UI.Pages;

public partial class SolverPage : ContentPage
{
	private const char SpecialCharacter = '.';
	private SolverWordView? _selectedWord = null;
	private SolverWordView? SelectedWord
	{
		get => _selectedWord;
		set
		{
			if (_selectedWord != null)
			{
				_selectedWord.CanEdit = false;
			}
			_selectedWord = value;
			if (_selectedWord != null)
			{
				_selectedWord.CanEdit = true;
			}
		}
	}
	private int _wordLength;
	public int WordLength
	{
		get => _wordLength;
		set
		{
			_wordLength = value;
			Reset();
		}
	}
	public SolverPage() : this(5) { }
	public SolverPage(int length)
	{
		InitializeComponent();
		WordLength = length; // This will call Reset()
		MyKeyboard.KeyPressed += MyKeyboard_KeyPressed;
		MyKeyboard.BackPressed += MyKeyboard_BackPressed;
		MyKeyboard.SpecialPressed += MyKeyboard_SpecialPressed;
		MyKeyboard.EnterPressed += MyKeyboard_EnterPressed;
	}
	private void MyKeyboard_EnterPressed(object? sender, EventArgs e)
	{
		AddWord();
	}
	private void MyKeyboard_SpecialPressed(object? sender, EventArgs e)
	{
		SelectedWord?.TypeLetter(SpecialCharacter);
	}
	private void MyKeyboard_BackPressed(object? sender, EventArgs e)
	{
		SelectedWord?.BackSpace();
	}
	private void MyKeyboard_KeyPressed(object? sender, char? e)
	{
		if (e.HasValue)
			SelectedWord?.TypeLetter(e.Value);
	}
	public void Reset()
	{
		WordHolder.Children.Clear();
		AddWord();
	}
	public void AddWord()
	{
		SelectedWord = new SolverWordView(WordLength, SolverLetterView.SolverLetterStates.Bad, true, true);
		SelectedWord.WordClicked += Word_Clicked;
		SelectedWord.RemoveClicked += Word_RemoveClicked;
		WordHolder.Children.Add(SelectedWord);
	}
	public async Task Search()
	{
		// Check if all words are complete
		if (WordHolder.Children.Count == 0)
		{
			await DisplayAlert("No Words", "Please add at least one word before searching", "OK");
			return;
		}
		else if (WordHolder.Children.Cast<SolverWordView>().Any(w => !w.IsComplete))
		{
			await DisplayAlert("Incomplete Word", "Please complete all words before searching", "OK");
			return;
		}
		// Gather constraints
		RegexFinder finder = new()
		{
			RestrictWordLength = true,
			IsLengthRange = false,
			WordLength = WordLength
		};
		char[] letterPositions = new char[WordLength];
		HashSet<char>[] lettersNotAtPosition = new HashSet<char>[WordLength];
		List<char> allCharacters = [];
		for (int i = 0; i < WordLength; i++)
		{
			lettersNotAtPosition[i] = [];
		}
		foreach (var word in WordHolder.Children.Cast<SolverWordView>())
		{
			var wordString = word.GetWord();
			int i = 0;
			Dictionary<char, int> letterCount = [];
			HashSet<char> lettersToLimit = [];
			foreach (var state in word.GetStates())
			{
				// The special/any character does not really have much functionality
				// - It allows the word to be completed without adding more constraints
				if (wordString[i] == SpecialCharacter) continue;
				switch (state)
				{
					case SolverLetterView.SolverLetterStates.Good:
						// Record position
						if (letterPositions[i] != default(char) && letterPositions[i] != wordString[i])
						{
							await DisplayAlert("Conflicting State", $"More than one character is at the same position.", "OK");
							return;
						}
						else
						{
							letterPositions[i] = wordString[i];
						}
						// Track letter count
						if (letterCount.TryGetValue(wordString[i], out int value))
						{
							letterCount[wordString[i]] = ++value;
						}
						else
						{
							letterCount[wordString[i]] = 1;
						}
						break;
					case SolverLetterView.SolverLetterStates.Bad:
						// Limit the max count (this may cause the letter to be excluded)
						lettersToLimit.Add(wordString[i]);
						lettersNotAtPosition[i].Add(wordString[i]);
						break;
					case SolverLetterView.SolverLetterStates.Partial:
						// Exclude the letter from this position
						lettersNotAtPosition[i].Add(wordString[i]);
						// Track letter count
						if (letterCount.TryGetValue(wordString[i], out int value2))
						{
							letterCount[wordString[i]] = ++value2;
						}
						else
						{
							letterCount[wordString[i]] = 1;
						}
						break;
					default:
						await DisplayAlert("Invalid State", $"Unexpected state for letter {wordString[i]}", "OK");
						return;
				}
				i++;
			}
			// Add count constraints
			foreach (var kvp in letterCount)
			{
				try
				{
					if (lettersToLimit.Contains(kvp.Key))
					{
						finder.AddRule(kvp.Key, kvp.Value, RegexFinder.CharacterRule.ExactCount);

					}
					else
					{
						finder.AddRule(kvp.Key, kvp.Value, RegexFinder.CharacterRule.MinCount);
					}
				}
				catch (FinderRuleConflictException ex)
				{
					await DisplayAlert("Rule Conflict", ex.Message, "OK");
					return;
				}
				catch (FinderDuplicateRuleException)
				{
					// This is fine.
				}
			}
			// Add exclude constraints
			foreach (var c in lettersToLimit)
			{
				if (!letterCount.ContainsKey(c))
				{
					try
					{
						finder.ExcludeCharacters += c;
					}
					catch (FinderRuleConflictException ex)
					{
						await DisplayAlert("Rule Conflict", ex.Message, "OK");
						return;
					}
					catch (FinderDuplicateRuleException)
					{
						// This is fine.
					}
				}
				// Otherwise, it has already been limited.
			}
			// Add good and partial characters to all characters
			allCharacters.AddRange(letterCount.Keys);
		}
		// Add position constraints
		for (int i = 0; i < WordLength; i++)
		{
			if (letterPositions[i] != default(char))
			{
				try
				{
					finder.AddRule(letterPositions[i], i + 1, RegexFinder.CharacterRule.AtPosition);
				}
				catch (FinderRuleConflictException ex)
				{
					await DisplayAlert("Rule Conflict", ex.Message, "OK");
					return;
				}
				catch (FinderDuplicateRuleException)
				{
					// This is fine.
				}
			}
			if (lettersNotAtPosition[i].Count > 0)
			{
				foreach (var c in lettersNotAtPosition[i])
				{
					try
					{
						finder.AddRule(c, i + 1, RegexFinder.CharacterRule.NotAtPosition);
					}
					catch (FinderRuleConflictException ex)
					{
						await DisplayAlert("Rule Conflict", ex.Message, "OK");
						return;
					}
					catch (FinderDuplicateRuleException)
					{
						// This is fine.
					}
				}
			}
		}
		// Add characters (so it can be used for validation); this *should* not affect the search
		finder.Characters = new string([.. allCharacters.Distinct()]);
		// Search
		string regex = "";
		List<string> foundWords = [];
		try
		{
			regex = finder.GetRegex().ToLower();
			await DisplayAlert("Regex (for debugging)", regex.Replace("\n", "\\n"), "OK");
			foundWords = [.. await Task.Run(async () =>
			{
				return RegexFinder.FindWords(await Globals.GetAllWords(), regex).AsEnumerable();
			})];
		}
		catch (FinderRuleConflictException ex)
		{
			await DisplayAlert("Rule Conflict", ex.Message, "OK");
			return;
		}
		catch (FinderDuplicateRuleException e)
		{
			await DisplayAlert("Rule Conflict", e.Message, "OK");
			return;
		}
		catch
		{
			await DisplayAlert("Error", $"Something went wrong. Regex: \"{regex}\"", "OK");
			return;
		}
		//catch
		//{
		//	throw;
		//}

		// Display
		if (foundWords.Count == 0)
		{
			await DisplayAlert("No Words Found", "No words match the criteria.", "OK");
			return;
		}
		if (foundWords.Count > 10000)
		{
			await DisplayAlert("Too Many Words", $"Too many words found ({foundWords.Count}).\nPlease refine your search.", "OK");
			return;
		}
		await Task.Run(foundWords.Sort);
		Dictionary<string, object> param = new()
		{
			{ "Words", foundWords },
			{ "Regex", regex }
		};
		await Shell.Current.GoToAsync("WordFinderResultsPage", param);
	}
	private void Word_RemoveClicked(object? sender, EventArgs e)
	{
		if (sender is SolverWordView letter)
		{
			WordHolder.Children.Remove(letter);
		}
	}
	private void Word_Clicked(object? sender, EventArgs e)
	{
		if (sender is SolverWordView word && SelectedWord != word)
		{
			SelectedWord = word;
		}
	}
	private void ToolReset_Clicked(object sender, EventArgs e)
	{
		Reset();
	}
	private void ToolAddWord_Clicked(object sender, EventArgs e)
	{
		AddWord();
	}
	private async void ToolSearch_Clicked(object sender, EventArgs e)
	{
		ToolSearch.IsEnabled = false;
		await Search();
		ToolSearch.IsEnabled = true;
	}
}