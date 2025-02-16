using System.Buffers;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Contracts;

Console.WriteLine("Tag Starting....");
var cts = new CancellationTokenSource(); //TODO: сработка токена по нажатию кнопки 'q' в консоли

//From settings----------------------------------------------------------
const int listenPort = 11000;
const string tagName = "Device1";

//Init-----------------------------------------------------------------
var macAddress = "10-20-30-40-5F-FF";
var tagIpAddress= NetworkHelpers.GetLocalIpAddress("192.168.1");

//Listener---------------------------------------------------------------
Task tagTask = Task.Factory.StartNew(async () =>
	{
		UdpClient listener = new UdpClient(listenPort) { EnableBroadcast = true };
		//Принимаю broadcast.
		//IPAddress.Loopback - позволяет серверу принимать пакеты, только в локальной сети
		//IPAddress.Any - позволяет серверу принимать пакеты, отправленные на любой из IP-адресов машины
		IPEndPoint groupEp = new IPEndPoint(IPAddress.Loopback, listenPort); 
		try
		{
			while (!cts.IsCancellationRequested)
			{
				//Слушаем listenPort для получения заапроса от сканера
				Console.WriteLine("Waiting scanner query");
				var result =  listener.ReceiveAsync().GetAwaiter().GetResult();
				var buffer = result.Buffer;
				var scannerIpAddress = result.RemoteEndPoint.Address;
				
				Console.WriteLine($"Received broadcast from scanner {groupEp} :");
                //Обработка broadcast сообщения от scanner
				var scannerPayload= ScannerPayload.FromBuffer(buffer);
				Console.WriteLine($"ScannerPayload {scannerPayload}");

				//Создание и Отправка ответа сканеру.
				var epScanner = new IPEndPoint(scannerIpAddress, scannerPayload.ListenPortNumber); //берет из запроса ip сканера и порт (куда отправить ответ)
				var tagPayload = TagPayload.Create(tagName, macAddress);
				var sendBytes= listener.Send(tagPayload.ToBuffer(), epScanner);
				Console.WriteLine($"Sent message to scanner {sendBytes} epScanner='{epScanner}' tagPayload= '{tagPayload}'");
			}
		}
		catch (SocketException e)
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
	},
	cts.Token,
	TaskCreationOptions.LongRunning,
	TaskScheduler.Default);


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