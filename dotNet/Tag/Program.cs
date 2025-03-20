using System.Buffers;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using Contracts;
using DotNetEnv;
using Microsoft.Extensions.Configuration;
using Tag;

Console.WriteLine("Tag Starting....");

// Загрузка настроек
Env.Load();
string? listenPortSetting = Environment.GetEnvironmentVariable("UDP_TAG__listenPort");
if (!int.TryParse(listenPortSetting, out var _listenPort))
{
	Console.WriteLine($"Listening on port Invalid  '{listenPortSetting}'");
	Console.ReadKey();
	return -1;
}

string? tagNameSetting = Environment.GetEnvironmentVariable("UDP_TAG__tagName");
if (string.IsNullOrEmpty(tagNameSetting))
{
	Console.WriteLine($"TagName Invalid  '{tagNameSetting}'");
	Console.ReadKey();
	return -1;
}

string tagName = tagNameSetting;
int listenPort = _listenPort;


//Формируем Payload Тега
var macAddress = SystemInfoCollector.GetUpMacAddress();


var cts = new CancellationTokenSource(); //TODO: сработка токена по нажатию кнопки 'q' в консоли
//Listener---------------------------------------------------------------
var tagTask = Task.Run(async () =>
	{
		while (!cts.IsCancellationRequested)
		{
			Console.WriteLine("listener Starting....");
			UdpClient listener = new UdpClient(listenPort) { EnableBroadcast = true };
			try
			{
				while (!cts.IsCancellationRequested)
				{
					//Слушаем listenPort для получения заапроса от сканера
					Console.WriteLine("Waiting scanner query");
					var result = await listener.ReceiveAsync(cts.Token);
					var buffer = result.Buffer;
					var scannerIpAddress = result.RemoteEndPoint.Address;
					
					Console.WriteLine($"Received broadcast from scanner {scannerIpAddress}");
					//Обработка broadcast сообщения от scanner
					var scannerPayload = ScannerPayload.FromBuffer(buffer);
					Console.WriteLine($"ScannerPayload {scannerPayload}");
					
					//Создание и Отправка ответа сканеру.
					var epScanner = new IPEndPoint(scannerIpAddress, scannerPayload.ListenPortNumber); //берет из запроса ip сканера и порт (куда отправить ответ)
					var tagPayload = TagPayload.Create(tagName, macAddress);
					var sendBytes = listener.Send(tagPayload.ToBuffer(), epScanner);
					Console.WriteLine($"Sent message to scanner {sendBytes} epScanner='{epScanner}' tagPayload= '{tagPayload}'");
				}
			}
			catch (SocketException e)
			{
				Console.WriteLine(e);
			}
			catch (OperationCanceledException e)
			{
				Console.WriteLine(e);
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
			}
			finally
			{
				listener.Close();
			}
			
			Console.WriteLine("listener ReStarting....");
			await Task.Delay(1000);
		}
	},
	cts.Token);

try
{
	await tagTask;
}
catch (Exception e)
{
	Console.WriteLine(e);
}

Console.WriteLine("Tag Stopped");
Console.ReadKey();
return 0;