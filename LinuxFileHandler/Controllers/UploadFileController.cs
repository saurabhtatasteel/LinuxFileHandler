using LinuxFileHandler.BLL;
using LinuxFileHandler.Configurations;
using LinuxFileHandler.Entities;
using LinuxFileHandler.Filters;
using LinuxFileHandler.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SRL = Serilog;

namespace LinuxFileHandler.Controllers
{
	[RequireApiKeyHeader]
	[Route("api/[controller]")]
	[ApiController]
	public class UploadFileController : ControllerBase
	{
		private readonly IWebHostEnvironment _environment;
		private readonly SRL.ILogger _logger;
		//private readonly IConfiguration _configuration;
		private readonly ApplicationSettings _applicationSettings;

		public UploadFileController(IWebHostEnvironment webHostEnvironment, SRL.ILogger logger,
			//IConfiguration configuration, 
			IOptions<ApplicationSettings> applicationSettings)
		{
			_environment = webHostEnvironment;
			_logger = logger;
			//_configuration = configuration;
			_applicationSettings = applicationSettings.Value;
			_applicationSettings.FullUploadsPath = Path.Combine(_applicationSettings.AppBasePath,
				_applicationSettings.UploadsPath) ?? Path.Combine(_environment.ContentRootPath, "Uploads");

			_applicationSettings.FullProcessedPath = Path.Combine(_applicationSettings.AppBasePath,
				_applicationSettings.ProcessedPath) ?? Path.Combine(_environment.ContentRootPath, "Processed");
		}

		[HttpPost("upload")]
		public async Task<IActionResult> UploadFile(IFormFile file)
		{
			var guid = Guid.NewGuid();

			if (file == null)
			{
				_logger.Information("UploadID {guid} : File is null. Uploading file failed", guid);
				return StatusCode(400, $"UploadID {guid} : Upload valid file. File is missing");
			}

			
			_logger.Information($"Upload {guid} : Received upload request for file {file.FileName}", guid, file?.FileName);

			try
			{				
				if (!_applicationSettings.IsValid())
				{
					return StatusCode(500, $"UploadID {guid} : Missing configuration");
				}

				var output = await new UploadFileProcessor(_logger).
					ProcessFileAsync(file, _applicationSettings, guid, _environment).ConfigureAwait(false);

				if (!output.Item2.IsSuccess)
				{
					if (output.Item2.Error == FileProcessError.InvalidFile)
						return BadRequest(output.Item2.Message);
					else
						return StatusCode(580, output.Item2.Message);

				}

				if (output.Item2.IsSuccess)
				{
					if (output.Item1 != null)
					{
						_logger.Information("UploadID {guid} : File {FileName} processed successfully", guid, file?.FileName);
						//var batchFileOutputReturnType = _configuration["ApplicationSettings:BatchFileOutputReturnType"];
						var batchFileOutputReturnType = _applicationSettings.BatchFileOutputReturnType;
						return File(
						output.Item1,
						//"application/octet-strean",
						batchFileOutputReturnType!,
						file.FileName);
					}
					else
					{
						_logger.Error("UploadID {guid} : File {FileName} processing failed. Error: {Message}", guid, file?.FileName, output.Item2?.Message);
						return StatusCode(500, $"UploadID {guid} : Internal server error: {output.Item2.Message}");
					}
				}
			}
			catch (Exception ex)
			{
				_logger.Error("UploadID {guid} : Error processing file {FileName} : Error: {message}, {ex.StackTrace}",
					guid, file?.FileName ?? "null", ex.Message, ex.StackTrace);
				return StatusCode(500, $"UploadID {guid} : Internal server error: {ex.Message}");
			}

			_logger.Information("UploadID {guid} : Uploading file {FileName} failed", guid, file?.FileName);
			return StatusCode(500, $"UploadID {guid} : Unknown error occurred. Check logs");
		}
	}
}
