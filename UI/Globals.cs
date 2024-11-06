namespace UI
{
	internal static class Globals
	{
		private static string? CurrentWordList = null;
		private static string? AllWords = null;
		public async static Task<string> GetAllWords()
		{
			if (AllWords == null || CurrentWordList != SettingsService.WordFile)
			{
				CurrentWordList = SettingsService.WordFile;
				var stream = await FileSystem.OpenAppPackageFileAsync(CurrentWordList);
				using StreamReader reader = new(stream);
				AllWords = (await reader.ReadToEndAsync()).ReplaceLineEndings("\n");
				return AllWords;
			}
			else
			{
				return AllWords;
			}
		}
	}
}
