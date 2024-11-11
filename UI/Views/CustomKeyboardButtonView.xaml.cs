using CommunityToolkit.Mvvm.Input;
using System.Diagnostics;

namespace UI.Views;

public partial class CustomKeyboardButtonView : ContentView
{
	public static readonly BindableProperty KeyCharacterProperty = BindableProperty.Create(
		nameof(KeyCharacter),
		typeof(string),
		typeof(CustomKeyboardButtonView),
		"",
		BindingMode.TwoWay);
	public string KeyCharacter
	{
		get => (string)GetValue(KeyCharacterProperty);
		set => SetValue(KeyCharacterProperty, value);
	}
	public event EventHandler<string>? Pressed;
	[RelayCommand]
	public void Tapped(string c)
	{
		Pressed?.Invoke(this, c);
		Debug.WriteLine($"Tapped: {c}");
	}
	public CustomKeyboardButtonView()
	{
		InitializeComponent();
	}
}