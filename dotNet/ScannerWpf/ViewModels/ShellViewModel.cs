using Caliburn.Micro;
using Contracts.Services;
using ScannerWpf.Models;

namespace ScannerWpf.ViewModels;

public class ShellViewModel : Screen
{
	private UdpScanner? _udpScanner;
	List<IDisposable> _udpScannerSubscriptions = [];
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


	public BindableCollection<LogMessageModel> LogList { get; private set; } = [];//TODO: разукрасить вывод лога

	public BindableCollection<TagResponseModel> ScannerResultList { get; private set; } = [];
	
	#endregion



	public ShellViewModel()
	{
		SubNetworkAddress = "192.168.1";
		RequestPort = 11000;
		ListenPort = 11001;
		ScanPeriod = TimeSpan.FromSeconds(1);
		ButtonStartStopText = "Start";
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
			_udpScanner!.Stop();
		}
		else
		{
			_isStarted = true;
			ButtonStartStopText = "Stop";
			ScannerResultList.Clear();
			await _udpScanner!.Start();
		}
	}


	public void ClearLogButton()
	{
		LogList.Clear();
	}


	private void CreateUdpScanner()
	{
		if (_udpScanner is null)
		{
			_udpScanner = new UdpScanner
			{
				SubNetworkAddress = SubNetworkAddress,
				RequestPort = RequestPort,
				ListenPort = ListenPort,
				ScanPeriod = ScanPeriod
			};
			
			var logsRxSubs = _udpScanner.LogsRx.Subscribe(item => LogList.Add(new LogMessageModel(item)));
			var tagResponseRxSubs= _udpScanner.TagResponseRx.Subscribe(tagResponse =>
			{
				var tagKey = tagResponse.ip.ToString();
				var existingItem = ScannerResultList.FirstOrDefault(tagResponseModel => tagResponseModel.IpAddress == tagKey);
				//UPDATE
				if (existingItem is not null)
				{
					existingItem.Name = tagResponse.payload.Name;
					existingItem.MacAddressDict = tagResponse.payload.MacAddress;
					existingItem.CreatedAtUtc = tagResponse.payload.CreatedAtUtc;
				}
				//ADD
				else
				{
					ScannerResultList.Add(new TagResponseModel
					{
						Name = tagResponse.payload.Name,
						IpAddress = tagKey,
						MacAddressDict = tagResponse.payload.MacAddress,
						CreatedAtUtc = tagResponse.payload.CreatedAtUtc
					});
				}
			});
			_udpScannerSubscriptions.AddRange([logsRxSubs, tagResponseRxSubs]);
		}
	}
	
	
	protected override Task OnDeactivateAsync(bool close, CancellationToken cancellationToken)
	{
		_udpScanner?.Stop();
		_udpScannerSubscriptions.ForEach(x => x.Dispose());
		return base.OnDeactivateAsync(close, cancellationToken);
	}
}