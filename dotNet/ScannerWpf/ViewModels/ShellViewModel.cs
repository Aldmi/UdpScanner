
using System.Net.Sockets;
using Caliburn.Micro;
using Contracts.Services;

namespace ScannerWpf.ViewModels;

public class ShellViewModel : PropertyChangedBase
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
	
	
	public BindableCollection<string> LogList { get; private set; }
	
	public BindableCollection<string> ScannerResultList { get; private set; }
	
	#endregion



	public ShellViewModel()
	{
		SubNetworkAddress = "192.168.1";
		RequestPort = 11000;
		ListenPort = 11001;
		ScanPeriod = TimeSpan.FromSeconds(1);
		ButtonStartStopText = "Start";
		LogList= ["ddsadas", "dgfsdgfsdfg"];
	}
	
	
	
	public bool CanStartStopSearch => !string.IsNullOrWhiteSpace(SubNetworkAddress) &&
	                                  RequestPort > 0 &&
	                                  ListenPort > 0 &&
	                                  ScanPeriod > TimeSpan.Zero;
	                                  

	public async Task StartStopSearch()
	{
		_udpScanner= _udpScanner ?? new UdpScanner
		{
			SubNetworkAddress = SubNetworkAddress,
			RequestPort = RequestPort,
			ListenPort = ListenPort,
			ScanPeriod = ScanPeriod
		};

		if (_isStarted)
		{
			_isStarted = false;
			//await _udpScanner.Stop();
			ButtonStartStopText = "Start";
		}
		else
		{
			_isStarted = true;
			//var status = await _udpScanner.Start();
			ButtonStartStopText = "Stop";
		}
		
		//MessageBox.Show(string.Format("Hello {0}!", SubNetworkAddress)); //Don't do this in real life :)
	}


	private void CreateUdpScanner()
	{
		if (_udpScanner == null)
		{
			_udpScanner= new UdpScanner
			{
				SubNetworkAddress = SubNetworkAddress,
				RequestPort = RequestPort,
				ListenPort = ListenPort,
				ScanPeriod = ScanPeriod
			};
			
			//TODO: подписка на события от сканера (лог и полученные данные от тегов) 
		}
	}
}