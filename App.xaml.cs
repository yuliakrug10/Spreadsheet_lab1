using Microsoft.Extensions.DependencyInjection;

namespace Spreadsheet_lab1;

public partial class App : Application
{
	public App()
	{
		InitializeComponent();
	}

	protected override Window CreateWindow(IActivationState activationState)
	{
		return new Window(new AppShell());
	}
}