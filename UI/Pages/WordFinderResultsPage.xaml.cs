namespace UI.Pages;

public partial class WordFinderResultsPage : ContentPage, IQueryAttributable
{
	private string WordsString = default!;
	public WordFinderResultsPage()
	{
		InitializeComponent();
	}

	public async void ApplyQueryAttributes(IDictionary<string, object> query)
	{
		await Task.Run(() =>
		{
			if (query.TryGetValue("Words", out var temp)) {
				var words = temp as List<string>;
				if (words != null)
				{
					WordsString = string.Join("\n", words);
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
		((Editor)sender).Text = WordsString;
	}
}