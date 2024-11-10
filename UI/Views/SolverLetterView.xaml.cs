namespace UI.Views;

public partial class SolverLetterView : ContentView
{
	public enum SolverLetterStates
	{
		None,
		Good,
		Bad,
		Partial
	}
	public SolverLetterStates _letterState;
	public SolverLetterStates LetterState
	{
		get => _letterState;
		set
		{
			_letterState = value;
			LableLetter.BackgroundColor = value switch
			{
				SolverLetterStates.None => Colors.Transparent,
				SolverLetterStates.Good => Colors.SpringGreen,
				SolverLetterStates.Bad => Colors.Silver,
				SolverLetterStates.Partial => Colors.PeachPuff,
				_ => Colors.Transparent,
			};
		}
	}
	public static readonly BindableProperty LetterProperty = BindableProperty.Create(
		nameof(Letter),
		typeof(char),
		typeof(SolverLetterView),
		' ',
		BindingMode.TwoWay);
	public char Letter
	{
		set => SetValue(LetterProperty, value);
		get => (char)GetValue(LetterProperty);
	}
	public bool CanToggle { get; set; } = true;
	public SolverLetterView()
	{
		InitializeComponent();
	}
	public SolverLetterView(char letter, bool canToggle = true) : this()
	{
		Letter = letter;
		CanToggle = canToggle;
	}
	public void ToggleState()
	{
		LetterState = LetterState switch
		{
			SolverLetterStates.None => SolverLetterStates.Good,
			SolverLetterStates.Good => SolverLetterStates.Bad,
			SolverLetterStates.Bad => SolverLetterStates.Partial,
			SolverLetterStates.Partial => SolverLetterStates.Good,
			_ => SolverLetterStates.None,
		};
	}
	private void TapGestureRecognizer_Tapped(object sender, TappedEventArgs e)
	{
		if (CanToggle)
		{
			ToggleState();
		}
	}
}