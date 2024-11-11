namespace UI.Views;

public partial class SolverWordView : ContentView
{
	public static readonly BindableProperty CanRemoveProperty = BindableProperty.Create(
		nameof(CanRemove),
		typeof(bool),
		typeof(SolverWordView),
		true,
		BindingMode.TwoWay);
	public bool CanRemove
	{
		set => SetValue(CanRemoveProperty, value);
		get => (bool)GetValue(CanRemoveProperty);
	}
	public static readonly BindableProperty BorderColorProperty = BindableProperty.Create(
		nameof(BorderColor),
		typeof(Color),
		typeof(SolverWordView),
		Colors.Gray,
		BindingMode.TwoWay);
	public Color BorderColor
	{
		set => SetValue(BorderColorProperty, value);
		get => (Color)GetValue(BorderColorProperty);
	}
	public static readonly BindableProperty CanEditProperty = BindableProperty.Create(
		nameof(CanEdit),
		typeof(bool),
		typeof(SolverWordView),
		true,
		BindingMode.TwoWay);
	public bool CanEdit
	{
		set
		{
			SetValue(CanEditProperty, value);
			foreach (SolverLetterView letter in LetterHolder.Children.Cast<SolverLetterView>())
			{
				letter.CanToggle = value;
			}
			if (value)
			{
				BorderColor = Colors.Gray;
			}
			else
			{
				BorderColor = Colors.Transparent;
			}
		}
		get => (bool)GetValue(CanEditProperty);
	}
	public static readonly BindableProperty LetterWidthProperty = BindableProperty.Create(
		nameof(LetterWidth),
		typeof(double),
		typeof(SolverWordView),
		30.0,
		BindingMode.TwoWay);
	public double LetterWidth
	{
		set => SetValue(LetterWidthProperty, value);
		get => (double)GetValue(LetterWidthProperty);
	}
	public event EventHandler? RemoveClicked;
	public event EventHandler? WordClicked;
	public bool IsComplete => LetterHolder.Children.Cast<SolverLetterView>().All(l => l.Letter != ' ');
	private SolverLetterView.SolverLetterStates _defaultLetterState = SolverLetterView.SolverLetterStates.None;
	public SolverLetterView.SolverLetterStates DefaultLetterState
	{
		set
		{
			_defaultLetterState = value;
			foreach (SolverLetterView letter in LetterHolder.Children.Cast<SolverLetterView>())
			{
				letter.LetterState = value;
			}
		}
		get => _defaultLetterState;
	}
	public SolverWordView(string word, SolverLetterView.SolverLetterStates defaultState, bool canEdit = true, bool canRemove = true)
		: this(defaultState, canEdit, canRemove)
	{
		foreach (char letter in word)
		{
			AddLetter(letter, DefaultLetterState, canEdit);
		}
	}
	public SolverWordView(int length, SolverLetterView.SolverLetterStates defaultState, bool canEdit = true, bool canRemove = true)
		: this(defaultState, canEdit, canRemove)
	{
		for (int i = 0; i < length; i++)
		{
			AddLetter(' ', DefaultLetterState, canEdit);
		}
	}
	public SolverWordView(SolverLetterView.SolverLetterStates defaultState, bool canEdit = true, bool canRemove = false)
		: this()
	{
		DefaultLetterState = defaultState;
		CanEdit = canEdit;
		CanRemove = canRemove;
	}
	public SolverWordView()
	{
		InitializeComponent();
		StackContainer.SizeChanged += StackContainer_SizeChanged;
	}
	private void StackContainer_SizeChanged(object? sender, EventArgs e)
	{
		LetterWidth = StackContainer.Width / (LetterHolder.Count + 2);
	}
	public void AddLetter(char letter, SolverLetterView.SolverLetterStates state, bool canToggle)
	{
		var toAdd = new SolverLetterView(letter, state, canToggle);
		toAdd.LetterClicked += OnClicked;
		LetterHolder.Children.Add(toAdd);
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
	private void OnClicked(object? sender, EventArgs? e)
	{
		WordClicked?.Invoke(this, e ?? new());
	}
	private void ButtonRemove_Clicked(object sender, EventArgs e)
	{
		if (CanRemove)
		{
			RemoveClicked?.Invoke(this, e);
		}
	}
	private void TapGestureRecognizer_Tapped(object sender, TappedEventArgs e)
	{
		OnClicked(sender, e);
	}
}