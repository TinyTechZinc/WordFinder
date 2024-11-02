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
	public static readonly BindableProperty BorderStrokeProperty = BindableProperty.Create(
		nameof(BorderStroke),
		typeof(Color),
		typeof(LetterView),
		Colors.Red,
		BindingMode.TwoWay);
	public Color BorderStroke
	{
		get => (Color)GetValue(BorderStrokeProperty);
		set => SetValue(BorderStrokeProperty, value);
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
	public LetterView()
	{
		InitializeComponent();
	}

	private void TapGestureRecognizer_Tapped(object sender, TappedEventArgs e)
	{

	}
}