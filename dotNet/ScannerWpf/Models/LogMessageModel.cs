using Caliburn.Micro;
using CSharpFunctionalExtensions;

namespace ScannerWpf.Models;

public class LogMessageModel : PropertyChangedBase
{
	public string Message { get; set; }
	public bool IsError { get; set; }
	
	public LogMessageModel(Result<string> messageResult)
	{
		IsError = messageResult.IsFailure;
		Message =  IsError ? messageResult.Error : messageResult.Value;
	}
}