namespace UI.Views;

public partial class SolverWordView : ContentView
{
	public bool _canEdit;
	public bool CanEdit
	{
		set
		{
			_canEdit = value;
			foreach (SolverLetterView letter in LetterHolder.Children.Cast<SolverLetterView>())
			{
				letter.CanToggle = value;
			}
		}
		get => _canEdit;
	}
	public bool IsComplete => LetterHolder.Children.Cast<SolverLetterView>().All(l => l.Letter != ' ');
	public SolverWordView(string word, bool canEdit = true) : this(canEdit)
	{
		foreach (char letter in word)
		{
			LetterHolder.Children.Add(new SolverLetterView(letter, canEdit));
		}
	}
	public SolverWordView(int length, bool canEdit = true) : this(canEdit)
	{
		for (int i = 0; i < length; i++)
		{
			LetterHolder.Children.Add(new SolverLetterView(' ', canEdit));
		}
	}
	public SolverWordView(bool canEdit = true) : this()
	{
		CanEdit = canEdit;
	}
	public SolverWordView()
	{
		InitializeComponent();
	}
	public void BackSpace()
	{
		if (LetterHolder.Children.Count > 0)
		{
			var toChange = LetterHolder.Children.Cast<SolverLetterView>().LastOrDefault(l => l.Letter != ' ');
			if (toChange != null)
			{
				toChange.Letter = ' ';
			}
		}
	}
	public void Clear()
	{
		foreach (SolverLetterView letter in LetterHolder.Children.Cast<SolverLetterView>())
		{
			letter.Letter = ' ';
		}
	}
	public void TypeLetter(char letter)
	{
		var toChange = LetterHolder.Children.Cast<SolverLetterView>().FirstOrDefault(l => l.Letter == ' ');
		if (toChange != null)
		{
			toChange.Letter = letter;
		}
	}
	public string GetWord()
	{
		return new string(LetterHolder.Children.Cast<SolverLetterView>().Select(l => l.Letter).ToArray());
	}
	public IEnumerable<SolverLetterView.SolverLetterStates> GetStates()
	{
		return LetterHolder.Children.Cast<SolverLetterView>().Select(l => l.LetterState);
	}
}