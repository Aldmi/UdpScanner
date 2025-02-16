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
	
	private string _macAddress;
	public string MacAddress
	{
		get => _macAddress;
		set
		{
			_macAddress = value;
			NotifyOfPropertyChange(() => MacAddress);
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