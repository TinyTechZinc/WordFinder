namespace UI.Views;

public partial class LetterView : ContentView
{
	public static readonly BindableProperty LetterProperty = BindableProperty.Create(
		nameof(Letter),
		typeof(char),
		typeof(LetterView),
		' ',
		BindingMode.TwoWay);
	public char Letter
	{
		get => (char)GetValue(LetterProperty);
		set => SetValue(LetterProperty, value);
	}
	public static readonly BindableProperty LetterBorderStrokeProperty = BindableProperty.Create(
		nameof(LetterBorderStroke),
		typeof(Color),
		typeof(LetterView),
		Colors.Red,
		BindingMode.TwoWay);
	public Color LetterBorderStroke
	{
		get => (Color)GetValue(LetterBorderStrokeProperty);
		set => SetValue(LetterBorderStrokeProperty, value);
	}
	public static readonly BindableProperty LetterBackgroundProperty = BindableProperty.Create(
		nameof(LetterBackground),
		typeof(Color),
		typeof(LetterView),
		Colors.Red,
		BindingMode.TwoWay);
	public Color LetterBackground
	{
		get => (Color)GetValue(LetterBackgroundProperty);
		set => SetValue(LetterBackgroundProperty, value);
	}
	private WordGame.Game.CharacterState _state;
	public WordGame.Game.CharacterState State
	{
		get => _state;
		set
		{
			_state = value;
			switch (value)
			{
				case WordGame.Game.CharacterState.None:
					LetterBorderStroke = Colors.Transparent;
					break;
				case WordGame.Game.CharacterState.Bad:
					LetterBackground = Colors.LightGray;
					LetterBorderStroke = Colors.Gray;
					break;
				case WordGame.Game.CharacterState.Partial:
					LetterBackground = Color.FromArgb("#FFFFA0");
					LetterBorderStroke = Colors.Yellow;
					break;
				case WordGame.Game.CharacterState.Good:
					LetterBackground = Colors.LightGreen;
					LetterBorderStroke = Colors.Green;
					break;
			}
		}
	}
	public LetterView()
	{
		InitializeComponent();
	}

	private void TapGestureRecognizer_Tapped(object sender, TappedEventArgs e)
	{

	}
}