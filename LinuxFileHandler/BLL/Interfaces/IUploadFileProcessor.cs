using LinuxFileHandler.Configurations;
using LinuxFileHandler.Entities;

namespace LinuxFileHandler.BLL.Interfaces
{
	/// <summary>
	/// Interface for UploadFileProcessor
	/// </summary>
	public interface IUploadFileProcessor
	{
		Task<Tuple<MemoryStream,FileProcessResult>> ProcessFileAsync(IFormFile file, ApplicationSettings applicationSettings, 
			Guid uploadId, IWebHostEnvironment environment);
	}
}
