namespace LinuxFileHandler.Configurations
{
	public class ApplicationSettings
	{
		public string XAPIKey { get; set; }
		public string BatchFileOS { get; set; }
		public string ValidFileTypes { get; set; }
		public string MaxFileSizeMB { get; set; }
		public string BatchFileOutputReturnType { get; set; }
		public string AppBasePath { get; set; }
		public string UploadsPath { get; set; }		
		public string ProcessedPath { get; set; }
		public string FullUploadsPath { get; set; }
		public string FullProcessedPath { get; set; }
	}
}
