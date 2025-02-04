using System.Net;
using System.Net.Sockets;
using System.Text;
using Contracts;

/*
 Запускает 2 параллельные задачи
 1. Прослушивает Udp socket на указанном порту
       --- Ждет ответа от устройств

 2. Шлет в цикле запрос scanQuery
        --- Запрос определенного формата (в котором указан Ip и port для ответа) БРОАДКАСТ в ОТВЕТЕ НЕ нужен
 */


Console.WriteLine("Scanner Starting....");
var cts = new CancellationTokenSource(); //TODO: сработка токена по нажатию кнопки 'q' в консоли

//From settings----------------------------------------------------------
string subNetworkAddress = "192.168.1"; //адрес рассылки для подсети
const int listenPort = 11001;
int portRequest = 11000;

TimeSpan scanPeriod = TimeSpan.FromSeconds(2);


//Sender-----------------------------------------------------------------
Task scanQueryTask = Task.Factory.StartNew(async () =>
	{
		Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp)
			{ EnableBroadcast = true };

		IPEndPoint broadcastEp = new IPEndPoint(IPAddress.Parse(subNetworkAddress + ".255"), portRequest); //255.255.255.255 - общий адрес широковещательной рассылки для локальной сети.
		while (!cts.IsCancellationRequested)
		{
			var payload = ScannerPayload.Create(listenPort);
			var sendBytes = await socket.SendToAsync(payload.ToBuffer(), broadcastEp, cts.Token); //TODO: cts.Token - как правильно завершать
			await Task.Delay(scanPeriod, cts.Token);
			Console.WriteLine($"Sending query to tag {sendBytes}  Ep={broadcastEp}  Payload= '{payload}'");
		}

		socket.Dispose();
	},
	cts.Token,
	TaskCreationOptions.LongRunning,
	TaskScheduler.Default);


//Listener---------------------------------------------------------------
Dictionary<string, TagPayload> tagsDict = new Dictionary<string, TagPayload>();
Task scanResponseTask = Task.Factory.StartNew(async () =>
	{
		UdpClient listener = new UdpClient(listenPort) { EnableBroadcast = true };
		IPEndPoint groupEp = new IPEndPoint(IPAddress.Any, listenPort);
		//IPEndPoint groupEp = new IPEndPoint(IPAddress.Parse(localIp), listenPort);  //слушаю на своем Ip конкретный порт для ответа

		try
		{
			while (!cts.IsCancellationRequested)
			{
				Console.WriteLine("Waiting tag response");
				byte[] bytes = listener.Receive(ref groupEp);    //TODO: заменить на ReceiveAsync
				var tagIp = groupEp.Address.ToString();
				
				//Обработка ответа
				var tagPayload = TagPayload.FromBuffer(bytes);
				tagsDict.TryAdd(tagIp, tagPayload);
				Console.WriteLine($"---------------------------------------");

				// foreach (var tag in tagsDict)
				// {
				// 	Console.WriteLine($"TAGS= '{tag.Key}: {tag.Value}'");
				// }
				Console.WriteLine($"Received response from TAG {groupEp}=  '{tagPayload}'"); //Добавлять в список новый элемент по mac-addr
				Console.WriteLine($"---------------------------------------\n\n");
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
	await Task.WhenAll(scanQueryTask, scanResponseTask);
}
catch (Exception e)
{
	Console.WriteLine(e);
}

Console.WriteLine("Scanner Stopped");
Console.ReadKey();
