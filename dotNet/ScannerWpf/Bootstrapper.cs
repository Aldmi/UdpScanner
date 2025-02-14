using System.Windows;
using Caliburn.Micro;
using ScannerWpf.ViewModels;

namespace ScannerWpf;

public class Bootstrapper : BootstrapperBase
{
	public Bootstrapper()
	{
		Initialize();
	}

	protected override  async void OnStartup(object sender, StartupEventArgs e)
	{
		await DisplayRootViewForAsync<ShellViewModel>();
	}
}