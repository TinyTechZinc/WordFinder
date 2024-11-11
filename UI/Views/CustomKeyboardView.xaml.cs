using Microsoft.Maui.Handlers;
using System.Diagnostics;

namespace UI.Views;

public partial class CustomKeyboardView : ContentView
{
	public event EventHandler<char?>? KeyPressed;
	public event EventHandler? BackPressed;
	public event EventHandler? SpecialPressed;
	public event EventHandler? EnterPressed;
	public CustomKeyboardView()
	{
		InitializeComponent();
		KeyBack.Pressed += OnBackPressed;
		KeySpecial.Pressed += OnSpecialPressed;
		foreach (var child in GridKeys)
		{
			if (child is CustomKeyboardButtonView button && child != KeyBack && child != KeySpecial)
				button.Pressed += OnKeyPressed;
		}
		// This my best attempt to get keyboard input... It only works while the page is in focus (focusing the custom keyboard will pull focus away) :(
#if WINDOWS
		var nativeView = (AppShell.Current.Handler as ViewHandler)?.PlatformView;
		if (nativeView != null)
		{
			nativeView.KeyDown += (s, e) =>
			{
				Debug.WriteLine($"KeyDown: {e.Key}");
				switch (e.Key)
				{
					case Windows.System.VirtualKey.Back:
						OnBackPressed(this, "");
						break;
					// This does not quite work
					// The enter key gets captured by something else
					// - Usually, the menu or a button
					case Windows.System.VirtualKey.Enter:
					case Windows.System.VirtualKey.Accept:
						OnEnterPressed(this, "");
						break;
					case Windows.System.VirtualKey.Number0:
					case Windows.System.VirtualKey.NumberPad0:
						OnKeyPressed(this, "0");
						break;
					case Windows.System.VirtualKey.Number1:
					case Windows.System.VirtualKey.NumberPad1:
						OnKeyPressed(this, "1");
						break;
					case Windows.System.VirtualKey.Number2:
					case Windows.System.VirtualKey.NumberPad2:
						OnKeyPressed(this, "2");
						break;
					case Windows.System.VirtualKey.Number3:
					case Windows.System.VirtualKey.NumberPad3:
						OnKeyPressed(this, "3");
						break;
					case Windows.System.VirtualKey.Number4:
					case Windows.System.VirtualKey.NumberPad4:
						OnKeyPressed(this, "4");
						break;
					case Windows.System.VirtualKey.Number5:
					case Windows.System.VirtualKey.NumberPad5:
						OnKeyPressed(this, "5");
						break;
					case Windows.System.VirtualKey.Number6:
					case Windows.System.VirtualKey.NumberPad6:
						OnKeyPressed(this, "6");
						break;
					case Windows.System.VirtualKey.Number7:
					case Windows.System.VirtualKey.NumberPad7:
						OnKeyPressed(this, "7");
						break;
					case Windows.System.VirtualKey.Number8:
					case Windows.System.VirtualKey.NumberPad8:
						OnKeyPressed(this, "8");
						break;
					case Windows.System.VirtualKey.Number9:
					case Windows.System.VirtualKey.NumberPad9:
						OnKeyPressed(this, "9");
						break;
					case Windows.System.VirtualKey.A:
						OnKeyPressed(this, "A");
						break;
					case Windows.System.VirtualKey.B:
						OnKeyPressed(this, "B");
						break;
					case Windows.System.VirtualKey.C:
						OnKeyPressed(this, "C");
						break;
					case Windows.System.VirtualKey.D:
						OnKeyPressed(this, "D");
						break;
					case Windows.System.VirtualKey.E:
						OnKeyPressed(this, "E");
						break;
					case Windows.System.VirtualKey.F:
						OnKeyPressed(this, "F");
						break;
					case Windows.System.VirtualKey.G:
						OnKeyPressed(this, "G");
						break;
					case Windows.System.VirtualKey.H:
						OnKeyPressed(this, "H");
						break;
					case Windows.System.VirtualKey.I:
						OnKeyPressed(this, "I");
						break;
					case Windows.System.VirtualKey.J:
						OnKeyPressed(this, "J");
						break;
					case Windows.System.VirtualKey.K:
						OnKeyPressed(this, "K");
						break;
					case Windows.System.VirtualKey.L:
						OnKeyPressed(this, "L");
						break;
					case Windows.System.VirtualKey.M:
						OnKeyPressed(this, "M");
						break;
					case Windows.System.VirtualKey.N:
						OnKeyPressed(this, "N");
						break;
					case Windows.System.VirtualKey.O:
						OnKeyPressed(this, "O");
						break;
					case Windows.System.VirtualKey.P:
						OnKeyPressed(this, "P");
						break;
					case Windows.System.VirtualKey.Q:
						OnKeyPressed(this, "Q");
						break;
					case Windows.System.VirtualKey.R:
						OnKeyPressed(this, "R");
						break;
					case Windows.System.VirtualKey.S:
						OnKeyPressed(this, "S");
						break;
					case Windows.System.VirtualKey.T:
						OnKeyPressed(this, "T");
						break;
					case Windows.System.VirtualKey.U:
						OnKeyPressed(this, "U");
						break;
					case Windows.System.VirtualKey.V:
						OnKeyPressed(this, "V");
						break;
					case Windows.System.VirtualKey.W:
						OnKeyPressed(this, "W");
						break;
					case Windows.System.VirtualKey.X:
						OnKeyPressed(this, "X");
						break;
					case Windows.System.VirtualKey.Y:
						OnKeyPressed(this, "Y");
						break;
					case Windows.System.VirtualKey.Z:
						OnKeyPressed(this, "Z");
						break;
					case Windows.System.VirtualKey.Multiply:
						break;
					case Windows.System.VirtualKey.Add:
						break;
					case Windows.System.VirtualKey.Separator:
						break;
					case Windows.System.VirtualKey.Subtract:
						break;
					case Windows.System.VirtualKey.Divide:
						break;
					case Windows.System.VirtualKey.Decimal:
					case (Windows.System.VirtualKey)190:
						OnKeyPressed(this, ".");
						break;
					default:
						break;
				}
			};
		}
#endif
	}

	public void OnKeyPressed(object? sender, string c)
	{
		KeyPressed?.Invoke(sender, c.ElementAtOrDefault(0));
	}
	public void OnBackPressed(object? sender, string e)
	{
		BackPressed?.Invoke(sender, EventArgs.Empty);
	}
	public void OnSpecialPressed(object? sender, string e)
	{
		SpecialPressed?.Invoke(sender, EventArgs.Empty);
	}
	public void OnEnterPressed(object? sender, string e)
	{
		EnterPressed?.Invoke(sender, EventArgs.Empty);
	}
}