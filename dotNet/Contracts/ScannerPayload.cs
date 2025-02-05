using System.Net;
using System.Text;
namespace Contracts;


public record ScannerPayload(int ListenPortNumber, DateTime CreatedAtUtc)
{
	public static ScannerPayload Create(int portNumber)
	{
		var payload = new ScannerPayload(portNumber, DateTime.UtcNow);
		return payload;
	}
	
	public byte[] ToBuffer()
	{
		long unixTime = ((DateTimeOffset)CreatedAtUtc).ToUnixTimeSeconds();
		string formatString = $"{ListenPortNumber}_{unixTime}";
		return Encoding.ASCII.GetBytes(formatString);
	}
	
	public static ScannerPayload FromBuffer(byte[] buffer)
	{
		var str= Encoding.ASCII.GetString(buffer, 0, buffer.Length); //TODO: можно ли преобразовать в Span<string>
		var parts = str.Split('_');
		if (parts.Length != 2)
		{
			throw new ArgumentException("invalid buffer");
		}
		
		DateTimeOffset dateTimeOffset = DateTimeOffset.FromUnixTimeSeconds(int.Parse(parts[1]));
		var createdAtUtc = dateTimeOffset.UtcDateTime;
		var listenPortNumber = int.Parse(parts[0]);
		return new ScannerPayload (listenPortNumber, createdAtUtc);
	}
}