using System.Net;
using System.Net.Sockets;
using System.Reactive.Subjects;
using CSharpFunctionalExtensions;

namespace Contracts.Services;

public enum ScannerStatus
{
	Started,
	Stopped,
	Error,
}

public class UdpScanner
{
	private CancellationTokenSource? _cts;
	private readonly Subject<Result<string>> _logsRx = new(); 
	private readonly Subject<(TagPayload payload, IPAddress ip)> _tagResponseRx = new(); 
	
	public required string SubNetworkAddress { get; init; }
	public required int ListenPort { get; init; }
	public required int RequestPort { get; init; }
	public TimeSpan ScanPeriod { get; init; }
	public ScannerStatus Status { get; private set; } = ScannerStatus.Stopped;
	
	public IObservable<Result<string>> LogsRx => _logsRx;
	public IObservable<(TagPayload payload, IPAddress ip)> TagResponseRx => _tagResponseRx;



	
	public async Task<Result<ScannerStatus>> Start()
	{
		if(Status == ScannerStatus.Started)
			return Status;

		try
		{
			_cts = new CancellationTokenSource();
			var senderTask = CreateSenderTask(_cts.Token);
			var listenerTask = CreateListenerTask(_cts.Token);
			Status = ScannerStatus.Started;
			LogWriteInfo("Started");
			await await Task.WhenAny(senderTask, listenerTask);
			LogWriteInfo("Stopped");
			Status = ScannerStatus.Stopped;
			return Result.Success(Status);
		}
		catch (SocketException e)
		{
			Status = ScannerStatus.Error;
			return Result.Failure<ScannerStatus>($"SocketException= '{e.Message}'");
		}
		catch (OperationCanceledException)
		{
			Status = ScannerStatus.Stopped;
			return Result.Success(Status);
		}
		catch (Exception e)
		{
			Status = ScannerStatus.Error;
			return Result.Failure<ScannerStatus>($"Exception= '{e.Message}'");
		}
		finally
		{
			 Stop();
			_cts?.Dispose();
		}
	}


	public void Stop()
	{
		if (_cts is { IsCancellationRequested: false })
		{
			//await _cts?.CancelAsync()!;
			 _cts?.Cancel();
		}
	}


	private async Task CreateSenderTask(CancellationToken ct)
	{
		Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp) { EnableBroadcast = true };
		IPEndPoint broadcastEp = new IPEndPoint(IPAddress.Parse(SubNetworkAddress + ".255"), RequestPort); //255.255.255.255 - общий адрес широковещательной рассылки для локальной сети.

		try
		{
			while (!ct.IsCancellationRequested)
			{
				var payload = ScannerPayload.Create(ListenPort);
				var sendBytes = await socket.SendToAsync(payload.ToBuffer(), broadcastEp, ct);
				await Task.Delay(ScanPeriod, CancellationToken.None);
				LogWriteInfo($"Sending query to tag {sendBytes}  Ep={broadcastEp}  Payload= '{payload}'");
				//Console.WriteLine($"Sending query to tag {sendBytes}  Ep={broadcastEp}  Payload= '{payload}'");
				//throw new Exception("DEBUG EXCEPTION");
			}
		}
		finally
		{
			socket.Close();
		}
	}


	//private int counetr = 0;
	
	private async Task CreateListenerTask(CancellationToken ct)
	{
		UdpClient listener = new UdpClient(ListenPort) { EnableBroadcast = true };
		//IPEndPoint groupEp = new IPEndPoint(IPAddress.Any, ListenPort);
		//IPEndPoint groupEp = new IPEndPoint(IPAddress.Parse(localIp), listenPort);  //слушаю на своем Ip конкретный порт для ответа

		try
		{
			while (!ct.IsCancellationRequested)
			{
				LogWriteInfo("Waiting tag response");
				var result = await listener.ReceiveAsync(ct);
				var buffer = result.Buffer;
				var tagIp = result.RemoteEndPoint.Address;

				//Обработка ответа
				var tagPayload = TagPayload.FromBuffer(buffer);
				_tagResponseRx.OnNext((tagPayload, tagIp));
				LogWriteInfo($"Received response from TAG '{tagPayload}'");
				// if (counetr++ > 3)
				// {
				// 	counetr = -1000;
				// 	throw new Exception("DEBUG EXCEPTION");
				// }
			}
		}
		finally
		{
			listener.Close();
		}
	}


	private void LogWriteError(string errorMessage)
	{
		_logsRx.OnNext(Result.Failure<string>(errorMessage));
	}
	
	private void LogWriteInfo(string message)
	{
		_logsRx.OnNext(Result.Success(message));
	}
}