using System.Net;
using System.Text;

namespace Contracts;


public record TagPayload(string Name, string MacAddress, DateTime CreatedAtUtc)
{
	
	public static TagPayload Create(string name,  string macAddress)
	{
		var payload = new TagPayload(name, macAddress, DateTime.UtcNow);
		return payload;
	}
	
	public byte[] ToBuffer()
	{
		long unixTime = ((DateTimeOffset)CreatedAtUtc).ToUnixTimeSeconds();
		string formatString = $"{Name}_{MacAddress}_{unixTime}";
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
		
		var name = parts[0];
		var macAddress = parts[1];
		DateTimeOffset dateTimeOffset = DateTimeOffset.FromUnixTimeSeconds(int.Parse(parts[2]));
		var createdAtUtc = dateTimeOffset.UtcDateTime;
		return new TagPayload(name, macAddress, createdAtUtc);
	}
}