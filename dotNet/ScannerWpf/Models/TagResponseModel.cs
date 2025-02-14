namespace ScannerWpf.Models;

public class TagResponseModel
{
	public string IpAddress { get; set; }
	public string Name { get; set; }
	public string MacAddress { get; set; }
	public DateTime CreatedAtUtc { get; set; }
}