using Contracts.Services;

/*
 Запускает 2 параллельные задачи
 1. Прослушивает Udp socket на указанном порту
       --- Ждет ответа от устройств

 2. Шлет в цикле запрос scanQuery
        --- Запрос определенного формата (в котором указан Ip и port для ответа) БРОАДКАСТ в ОТВЕТЕ НЕ нужен
 */


Console.WriteLine("Scanner Starting....");

//From settings----------------------------------------------------------
string subNetworkAddress = "192.168.1"; //адрес рассылки для подсети
const int listenPort = 11001;
const int requestPort = 11000;
TimeSpan scanPeriod = TimeSpan.FromSeconds(1);
//-----------------------------------------------------------------------

var udpScanner= new UdpScanner
{
	SubNetworkAddress = subNetworkAddress,
	ListenPort = listenPort,
	RequestPort = requestPort,
	ScanPeriod = scanPeriod
};


// Task.Run(async () =>
//  {
//  	await Task.Delay(3000);
//  	await udpScanner.Stop();
//  });

udpScanner.LogsRx
	.Subscribe(result =>
	{
		var message= result.IsFailure ? $"ERROR= {result.Error}" : result.Value;
		Console.WriteLine($"LOG: {message}");
	} );

udpScanner.TagResponseRx
	.Subscribe(result =>
	{
		var ip = result.ip;
		Console.WriteLine("---------------------------------------");
		Console.WriteLine($"TAG: [{ip}]  payload=[{result.payload}]");
		Console.WriteLine("---------------------------------------");
	} );


var res= await udpScanner.Start();

Console.WriteLine($"{res}");
Console.ReadKey();
