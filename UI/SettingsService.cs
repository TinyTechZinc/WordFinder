namespace UI
{
	public sealed class SettingsService
	{
		const string _WordFile = "word_file";
		const string WordFileDefault = "words.txt";
		public static string WordFile
		{
			set => Preferences.Set(_WordFile, value);
			get => Preferences.Get(_WordFile, WordFileDefault);
		}
	}
}
