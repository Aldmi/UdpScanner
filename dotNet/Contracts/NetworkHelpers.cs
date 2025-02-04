using System.Net;
using System.Net.Sockets;

namespace Contracts;

public abstract class NetworkHelpers
{
	/// <summary>
	/// Вернуть IpAddress текущего компьютера
	/// </summary>
	/// <param name="subNetworkAddress">Под сеть локального адреса</param>
	/// <exception cref="Exception"></exception>
	public static string GetLocalIpAddress(string subNetworkAddress)
	{
		var host = Dns.GetHostEntry(Dns.GetHostName());
		foreach (var ip in host.AddressList)
		{
			if (ip.AddressFamily == AddressFamily.InterNetwork && ip.ToString().StartsWith(subNetworkAddress))
			{
				return ip.ToString();
			}
		}
		throw new Exception("No network adapters with an IPv4 address in the system!");
	}
	
	/// <summary>
	/// флаг подключения сети
	/// </summary>
	static bool GetIsNetworkAvailable()
	{
		return System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable();
	}
}