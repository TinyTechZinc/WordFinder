using CommunityToolkit.Maui.Views;
using System.Text.RegularExpressions;
using UI.Views;
using WordFinder;
using WordFinder.Exceptions;

namespace UI.Pages;

public partial class FinderPage : ContentPage, IQueryAttributable
{
	public FinderPage()
	{
		InitializeComponent();
	}
	/// <summary>
	/// Allows additional rules to be added.
	/// </summary>
	/// <param name="query"></param>
	public void ApplyQueryAttributes(IDictionary<string, object> query)
	{
		if (query.TryGetValue("Rules", out var rules))
		{
			// to do
			if (rules != null) { }
		}
	}

	private void ButtonSearch_Clicked(object sender, EventArgs e)
	{
		DoSearch();
	}
	private async void DoSearch()
	{
		string characters = EntryCharacters.Text ?? "";
		//if (!CheckBoxOnlyThese.IsChecked && !CheckBoxIncludeAll.IsChecked && PickerCount.SelectedIndex == (int)WordRegex.CountRestriction.None && characters.Length > 0)
		//{
		//	if (!await DisplayAlert("Warning", "'Only These' and 'Include All' are unchecked and 'Restrict Count' is set to None. This will result in the Characters field being ignored.", "Continue", "Cancel"))
		//	{
		//		return;
		//	}
		//}
		if (CheckBoxOnlyThese.IsChecked && characters.Length == 0)
		{
			await DisplayAlert("No Characters", "'Only These' requires characters to be provided.", "OK");
			return;
		}
		RegexFinder Finder = new()
		{
			IncludeAll = CheckBoxIncludeAll.IsChecked,
			OnlyThese = CheckBoxOnlyThese.IsChecked,
			Characters = characters,
			ExcludeCharacters = EntryExcludeCharacters.Text ?? ""
		};

		if (RadioButtonAny.IsChecked)
		{
			Finder.RestrictWordLength = false;
		}
		else
		{
			Finder.RestrictWordLength = true;
			if (RadioButtonExact.IsChecked && Int32.TryParse(EntryExact.Text, out int exact))
			{
				Finder.IsLengthRange = false;
				Finder.WordLength = exact;
			}
			else if (RadioButtonRange.IsChecked)
			{
				Finder.IsLengthRange = true;
				var minStr = EntryMin.Text ?? "";
				var maxStr = EntryMax.Text ?? "";
				if (Int32.TryParse(minStr, out int min) && min > 0)
				{
					Finder.WordMinLength = min;
				}
				else if (minStr.Length == 0)
				{
					Finder.WordMinLength = null;
				}
				else
				{
					await DisplayAlert("Invalid Length", "Invalid length provided.", "OK");
					return;
				}
				if (Int32.TryParse(maxStr, out int max) && max > 0)
				{
					Finder.WordMaxLength = max;
				}
				else if (maxStr.Length == 0)
				{
					Finder.WordMaxLength = null;
				}
				else
				{
					await DisplayAlert("Invalid Length", "Invalid length provided.", "OK");
					return;
				}
				if (Finder.WordMinLength != null && Finder.WordMaxLength != null && Finder.WordMinLength > Finder.WordMaxLength)
				{
					await DisplayAlert("Invalid Length", "Minimum length is greater than maximum length.", "OK");
					return;
				}
			}
			else
			{
				await DisplayAlert("Invalid Length", "Invalid length provided.", "OK");
				return;
			}
		}
		// Add rules
		foreach (FinderRuleView rule in RuleList.Cast<FinderRuleView>())
		{
			Finder.AddRule(rule.Rule.Character, rule.Rule.Number, rule.Rule.RuleType);
		}

		// More to be added here

		List<string> foundWords = [];
		string regex = "";
		try
		{
			regex = Finder.GetRegex();
			await DisplayAlert("Regex (for debugging)", regex.Replace("\n", "\\n"), "OK");
			foundWords = (await Task.Run(async () =>
			{
				return RegexFinder.FindWords(await Globals.GetAllWords(), regex).AsEnumerable();
			})).ToList();
		}
		catch (FinderRuleConflictException ex)
		{
			await DisplayAlert("Rule Conflict", ex.Message, "OK");
			return;
		}
		catch (FinderDuplicateRuleException e)
		{
			await DisplayAlert("Duplicate Rule", e.Message, "OK");
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
		await Task.Run(() =>
		{
			foundWords.Sort();
		});
		Dictionary<string, object> param = new()
		{
			{ "Words", foundWords },
			{ "Regex", regex }
		};
		await Shell.Current.GoToAsync("WordFinderResultsPage", param);
	}

	private async void ButtonAddRule_Clicked(object sender, EventArgs e)
	{
		var rulePopup = new Popups.CharacterRulePopup();
		var result = await this.ShowPopupAsync(rulePopup);
		if (result != null && result is FinderRuleView.RuleDefinition rule)
		{
			RuleList.Add(new FinderRuleView(rule, Rule_EditClicked!, Rule_RemoveClicked!));
		}
	}
	private async void Rule_EditClicked(object sender, EventArgs e)
	{
		if (sender is FinderRuleView ruleView)
		{
			var rulePopup = new Popups.CharacterRulePopup(ruleView.Rule);
			var result = await this.ShowPopupAsync(rulePopup);
			if (result != null && result is FinderRuleView.RuleDefinition newRule)
			{
				ruleView.Rule = newRule;
			}
		}
	}
	private void Rule_RemoveClicked(object sender, EventArgs e)
	{
		if (sender is FinderRuleView rule)
		{
			RuleList.Remove(rule);
		}
	}
}