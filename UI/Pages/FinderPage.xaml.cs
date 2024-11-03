using CommunityToolkit.Maui.Views;
using System.Text.RegularExpressions;
using UI.Views;
using WordFinder;

namespace UI.Pages;

public partial class FinderPage : ContentPage, IQueryAttributable
{
	readonly RegexFinder Finder;
	public FinderPage()
	{
		InitializeComponent();
		Finder = new();
	}
	/// <summary>
	/// Allows additional rules to be added.
	/// </summary>
	/// <param name="query"></param>
	public void ApplyQueryAttributes(IDictionary<string, object> query)
	{
		if (query.TryGetValue("NewRule", out var rule))
		{
			// to do
			if (rule != null) { }
		}
	}

	private void ButtonSearch_Clicked(object sender, EventArgs e)
	{
		DoSearch();
	}
	private async void DoSearch()
	{
		string characters = EntryCharacters.Text ?? "";
		if (characters != RegexFinder.GetSafeString(characters))
		{
			await DisplayAlert("Invalid", "Invalid characters in characters field.", "OK");
			return;
		}
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
		Finder.IncludeAll = CheckBoxIncludeAll.IsChecked;
		Finder.OnlyThese = CheckBoxOnlyThese.IsChecked;
		Finder.Characters = characters;

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
			else if (RadioButtonRange.IsChecked && Int32.TryParse(EntryMin.Text, out int min) && Int32.TryParse(EntryMax.Text, out int max))
			{
				Finder.IsLengthRange = true;
				Finder.WordMinLength = min;
				Finder.WordMaxLength = max;
			}
			else
			{
				await DisplayAlert("Invalid Length", "Invalid length provided.", "OK");
				return;
			}
		}
		// Read rules
		Finder.CharacterRules.Clear();
		foreach (FinderRuleView rule in RuleList)
		{
			Finder.AddRule(rule.Rule.Character, rule.Rule.Number, rule.Rule.RuleType);
		}

		// More to be added here

		string regex = Finder.GetRegex();

		await DisplayAlert(Title, regex, "OK");

		using var fileStream = await FileSystem.OpenAppPackageFileAsync("words.txt");
		using var streamReader = new StreamReader(fileStream);
		string content = await streamReader.ReadToEndAsync();

		List<string> foundWords = [];
		try
		{
			foundWords = await Task.Run(() =>
			{
				string textLines = Regex.Replace(content, "(\r\n|\r|\n)", "\n");
				return RegexFinder.FindWords(textLines, regex);
			});
		}
		catch
		{
			await DisplayAlert("Invalid Regex", "Internal regular expression match failed.\nDouble check search criteria.", "OK");
			return;
		}
		if (foundWords.Count == 0)
		{
			await DisplayAlert("No Words Found", "No words found matching the criteria.", "OK");
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
		};
		await Shell.Current.GoToAsync("WordFinderResultsPage", param);
	}

	private async void ButtonAddRule_Clicked(object sender, EventArgs e)
	{
		var rulePopup = new Popups.CharacterRulePopup();
		var result = await this.ShowPopupAsync(rulePopup);
		if (result != null && result is RegexFinder.CharRule rule)
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
			if (result != null && result is RegexFinder.CharRule newRule)
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