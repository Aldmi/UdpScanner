using System.Net.NetworkInformation;

namespace Tag;

public static class SystemInfoCollector
{
	public static Dictionary<string, string> GetUpMacAddress()
	{
		Dictionary<string, string> systemInfo = new Dictionary<string, string>();
		// Получаем все сетевые интерфейсы
		var networkInterfaces = NetworkInterface.GetAllNetworkInterfaces()
			.Where(x => x.OperationalStatus == OperationalStatus.Up);
		
		foreach (var networkInterface in networkInterfaces)
		{
			// Получаем MAC-адрес
			PhysicalAddress physicalAddress = networkInterface.GetPhysicalAddress();
			string macAddress = string.Join(":", physicalAddress.GetAddressBytes().Select(b => b.ToString("X2")));
			if (!string.IsNullOrWhiteSpace(macAddress))
			{
				systemInfo[networkInterface.Name] = macAddress;
			}
		}
		return systemInfo;
	}
}