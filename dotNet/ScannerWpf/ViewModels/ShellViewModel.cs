using Caliburn.Micro;
using Contracts.Services;
using ScannerWpf.Models;

namespace ScannerWpf.ViewModels;

public class ShellViewModel : Screen
{
	private UdpScanner? _udpScanner;
	private bool _isStarted;
	
	
	#region props
	
	string _subNetworkAddress;
	public string SubNetworkAddress
	{
		get => _subNetworkAddress;
		set
		{
			_subNetworkAddress = value;
			NotifyOfPropertyChange(() => SubNetworkAddress);
			NotifyOfPropertyChange(() => CanStartStopSearch);
		}
	}
	
	int _requestPort;
	public int RequestPort
	{
		get => _requestPort;
		set
		{
			_requestPort = value;
			NotifyOfPropertyChange(() => RequestPort);
			NotifyOfPropertyChange(() => CanStartStopSearch);
		}
	}
	
	int _listenPort;
	public int ListenPort
	{
		get => _listenPort;
		set
		{
			_listenPort = value;
			NotifyOfPropertyChange(() => ListenPort);
			NotifyOfPropertyChange(() => CanStartStopSearch);
		}
	}
	
	TimeSpan _scanPeriod;
	public TimeSpan ScanPeriod
	{
		get => _scanPeriod;
		set
		{
			_scanPeriod = value;
			NotifyOfPropertyChange(() => ScanPeriod);
			NotifyOfPropertyChange(() => CanStartStopSearch);
		}
	}
	
	string _buttonStartStopText;
	public string ButtonStartStopText
	{
		get => _buttonStartStopText;
		set
		{
			_buttonStartStopText = value;
			NotifyOfPropertyChange(() => ButtonStartStopText);
		}
	}


	public BindableCollection<LogMessageModel> LogList { get; private set; } = [];

	public BindableCollection<TagResponseModel> ScannerResultList { get; private set; } = [];
	
	#endregion



	public ShellViewModel()
	{
		SubNetworkAddress = "192.168.1";
		RequestPort = 11000;
		ListenPort = 11001;
		ScanPeriod = TimeSpan.FromSeconds(1);
		ButtonStartStopText = "Start";
		ScannerResultList.Add(new TagResponseModel(){Name = "Tag 1", MacAddress = "11-22-33-44-55-66", IpAddress = "192.168.01.1", CreatedAtUtc = DateTime.UtcNow});
	}
	
	
	
	protected override void OnViewLoaded(object view)
	{
		base.OnViewLoaded(view);
	}
	
	
	
	public bool CanStartStopSearch => !string.IsNullOrWhiteSpace(SubNetworkAddress) &&
	                                  RequestPort > 0 &&
	                                  ListenPort > 0 &&
	                                  ScanPeriod > TimeSpan.Zero;
	                                  

	public async Task StartStopSearch()
	{
		CreateUdpScanner();

		if (_isStarted)
		{
			_isStarted = false;
			ButtonStartStopText = "Start";
			await _udpScanner!.Stop();
		}
		else
		{
			_isStarted = true;
			ButtonStartStopText = "Stop";
			await _udpScanner!.Start();
		}
	}


	private void CreateUdpScanner()
	{
		if (_udpScanner is null)
		{
			_udpScanner= new UdpScanner
			{
				SubNetworkAddress = SubNetworkAddress,
				RequestPort = RequestPort,
				ListenPort = ListenPort,
				ScanPeriod = ScanPeriod
			};
			
			_udpScanner.Logs.Subscribe(item=>LogList.Add(new LogMessageModel(item)));
			
			//TODO: подписка на события от сканера (лог и полученные данные от тегов) 
		}
	}
}