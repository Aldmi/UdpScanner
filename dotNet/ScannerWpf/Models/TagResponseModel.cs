using Caliburn.Micro;

namespace ScannerWpf.Models;

public class TagResponseModel : PropertyChangedBase
{
	private string _ipAddress;
	public string IpAddress
	{
		get => _ipAddress;
		set
		{
			_ipAddress = value;
			NotifyOfPropertyChange(() => IpAddress);
		}
	}
	
	private string _name;
	public string Name
	{
		get => _name;
		set
		{
			_name = value;
			NotifyOfPropertyChange(() => Name);
		}
	}

	private Dictionary<string, string> _macAddressDict;
	public Dictionary<string, string> MacAddressDict
	{
		get => _macAddressDict;
		set
		{
			_macAddressDict = value;
			NotifyOfPropertyChange(() => MacAddressDict);
		}
	}
	
	
	private DateTime _createdAtUtc;
	public DateTime CreatedAtUtc
	{
		get => _createdAtUtc;
		set
		{
			_createdAtUtc = value;
			NotifyOfPropertyChange(() => CreatedAtUtc);
		}
	}
}