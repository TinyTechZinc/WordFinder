using Microsoft.Maui.Handlers;
using System.Diagnostics;

namespace UI.Views;

public partial class CustomKeyboardView : ContentView
{
	public event EventHandler<char?>? KeyPressed;
	public CustomKeyboardView()
	{
		InitializeComponent();
		foreach (var child in GridKeys)
		{
			if (child is CustomKeyboardButtonView button)
				button.Pressed += OnKeyPressed;
		}
		// This my best attempt to get key board input... It only works while the page menu is in focus :(
#if WINDOWS
		var nativeView = (AppShell.Current.Handler as ViewHandler)?.PlatformView;
		if (nativeView != null)
		{
			nativeView.KeyDown += (s, e) => Debug.WriteLine($"KeyDown: {e.Key}");
		}
#endif
	}

	public void OnKeyPressed(object? sender, string c)
	{
		KeyPressed?.Invoke(sender, c.ElementAtOrDefault(0));
	}
}