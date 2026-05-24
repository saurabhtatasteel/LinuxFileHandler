namespace LinuxFileHandler.Entities
{
	public class FileProcessResult
	{
		public bool IsSuccess { get; set; }
		public string Message { get; set; } = string.Empty;
		public FileProcessError Error { get; set; } = FileProcessError.None;
	}
}
