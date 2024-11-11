namespace UI.Pages;

public partial class WordFinderResultsPage : ContentPage, IQueryAttributable
{
	private string WordsString = "";
	private string RegexString = "";
	public WordFinderResultsPage()
	{
		InitializeComponent();
	}

	public async void ApplyQueryAttributes(IDictionary<string, object> query)
	{
		await Task.Run(() =>
		{
			if (query.TryGetValue("Words", out var temp)) {
				if (temp is List<string> words)
				{
					WordsString = string.Join("\n", words);
					Dispatcher.Dispatch(() =>
					{
						Title = $"Words Found: {words.Count}";
					});
				}
			}
			if (query.TryGetValue("Regex", out var temp2))
			{
				if (temp2 is string regex)
				{
					RegexString = regex.Replace("\n", "\\n");
					Dispatcher.Dispatch(() =>
					{
						ToolCopyRegex.IsEnabled = RegexString != "";
					});
				}
			}
		});
		Editor editor = new()
		{
			IsSpellCheckEnabled = false,
			IsTextPredictionEnabled = false,
			IsReadOnly = false,
			Text = WordsString,
		};
		editor.TextChanged += Editor_TextChanged;
		ListWords.Children.Add(editor);
		OnPropertyChanged();
	}
	private void Editor_TextChanged(object? sender, TextChangedEventArgs e)
	{
		((Editor)sender!).Text = WordsString;
	}
	private void ToolCopyRegex_Clicked(object sender, EventArgs e)
	{
		Clipboard.Default.SetTextAsync(RegexString);
	}
}