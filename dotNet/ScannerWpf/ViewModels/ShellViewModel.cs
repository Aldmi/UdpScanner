using System.Windows;
using Caliburn.Micro;

namespace ScannerWpf.ViewModels;

public class ShellViewModel : PropertyChangedBase
{
	string name;

	public string Name
	{
		get { return name; }
		set
		{
			name = value;
			NotifyOfPropertyChange(() => Name);
			NotifyOfPropertyChange(() => CanSayHello);
		}
	}

	public bool CanSayHello => !string.IsNullOrWhiteSpace(Name);

	public void SayHello()
	{
		MessageBox.Show(string.Format("Hello {0}!", Name)); //Don't do this in real life :)
	}
}