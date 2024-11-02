namespace UI
{
	public partial class AppShell : Shell
	{
		public AppShell()
		{
			InitializeComponent();
			Routing.RegisterRoute("WordFinderResultsPage", typeof(Pages.WordFinderResultsPage));
		}
	}
}
