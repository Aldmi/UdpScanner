using System.Net;
using System.Text;
namespace Contracts;


public record ScannerPayload(int ListenPortNumber, DateTime CreatedAt)
{
	public static ScannerPayload Create(int portNumber)
	{
		var payload = new ScannerPayload(portNumber, DateTime.UtcNow);
		return payload;
	}
	
	public byte[] ToBuffer()
	{
		string formatString = $"{ListenPortNumber}_{CreatedAt}";
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
		return new ScannerPayload (int.Parse(parts[0]), DateTime.Parse(parts[1]));
	}
}