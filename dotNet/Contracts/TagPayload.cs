using System.Net;
using System.Text;

namespace Contracts;


public record TagPayload(string Name, Dictionary<string, string> MacAddress, DateTime CreatedAtUtc)
{
	
	public static TagPayload Create(string name, Dictionary<string, string> macAddress)
	{
		var payload = new TagPayload(name, macAddress, DateTime.UtcNow);
		return payload;
	}
	
	public byte[] ToBuffer()
	{
		long unixTime = ((DateTimeOffset)CreatedAtUtc).ToUnixTimeSeconds();
		
		string macAddressString = string.Join(";", MacAddress.Select(x => $"{x.Key}={x.Value}"));
		
		string formatString = $"{Name}_{macAddressString}_{unixTime}";
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
		var macAddressString = parts[1];
		DateTimeOffset dateTimeOffset = DateTimeOffset.FromUnixTimeSeconds(int.Parse(parts[2]));
		
		//парсить macAddressString
		Dictionary<string, string> macAddress = macAddressStringToDictionary(macAddressString);
		
		var createdAtUtc = dateTimeOffset.UtcDateTime;
		return new TagPayload(name, macAddress, createdAtUtc);
	}


	public static Dictionary<string, string> macAddressStringToDictionary(ReadOnlySpan<char> span)
	{
		Dictionary<string, string> result = new Dictionary<string, string>();
		foreach(var chunk in span.Split(';'))
		{
			var dictionaryItem=span[chunk];
			var  splitIndex= dictionaryItem.IndexOf('=');
			var key= dictionaryItem.Slice(0, splitIndex);
			var value= dictionaryItem.Slice(splitIndex+1);
			result[key.ToString()] = value.ToString();
		}

		return result;
	}


	public override string ToString()
	{
		string macAddressString = string.Join(";", MacAddress.Select(x => $"{x.Key}={x.Value}\n"));
		return $"Tag name= {Name} \t Payload= '{macAddressString}' \t Created at UTC= {CreatedAtUtc}";
	}
}