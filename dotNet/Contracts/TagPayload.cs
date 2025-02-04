using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Contracts;


public record TagPayload(string Name, string MacAddress, DateTime CreatedAt)
{
	
	public static TagPayload Create(string name, string macAddress)
	{
		var payload = new TagPayload(name, macAddress, DateTime.UtcNow);
		return payload;
	}
	
	public byte[] ToBuffer()
	{
		string formatString = $"{Name}_{MacAddress}_{CreatedAt}";
		return Encoding.ASCII.GetBytes(formatString);
	}
	
	
	public static TagPayload FromBuffer(byte[] buffer)
	{
		var str= Encoding.ASCII.GetString(buffer, 0, buffer.Length);
		var parts = str.Split('_');
		if (parts.Length != 3)
		{
			throw new ArgumentException("invalid buffer");
		}
		
		return new TagPayload(parts[0], parts[1], DateTime.Parse(parts[2]));
	}
}