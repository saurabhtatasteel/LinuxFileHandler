using LinuxFileHandler.BLL;
using LinuxFileHandler.Entities;
using Microsoft.AspNetCore.Mvc;
using SRL = Serilog;

namespace LinuxFileHandler.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class UploadFileController : ControllerBase
	{
		private readonly IWebHostEnvironment _environment;
		private readonly SRL.ILogger _logger;
		private readonly IConfiguration _configuration;

		public UploadFileController(IWebHostEnvironment webHostEnvironment, SRL.ILogger logger, IConfiguration configuration)
		{
			_environment = webHostEnvironment;
			_logger = logger;
			_configuration = configuration;
		}

		[HttpPost("upload")]
		public async Task<IActionResult> UploadFile(IFormFile file)
		{
			var guid = Guid.NewGuid();

			_logger.Information("Upload {guid} : Received upload request for file {FileName}", guid, file?.FileName);

			try
			{
				var output = await new UploadFileProcessor(_logger, _configuration).
					ProcessFileAsync(file, _configuration["ApplicationSettings:UploadsPath"] ??
					Path.Combine(_environment.ContentRootPath, "Uploads"), _configuration["ApplicationSettings:ProcessedPath"] ??
					Path.Combine(_environment.ContentRootPath, "Processed"), guid, _environment).ConfigureAwait(false);

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
						var batchFileOutputReturnType = _configuration["ApplicationSettings:BatchFileOutputReturnType"];
						return File(
						output.Item1,
						//"application/octet-strean",
						batchFileOutputReturnType!,
						file.FileName);
					}
					else
					{
						_logger.Error("UploadID {guid} : File {FileName} processing failed. Error: {Message}", guid, file?.FileName, output.Item2?.Message);
						return StatusCode(508, $"UploadID {guid} : Internal server error: {output.Item2.Message}");
					}
				}
			}
			catch (Exception ex)
			{
				_logger.Error("UploadID {guid} : Error processing file {FileName}", guid, file?.FileName ?? ex.Message);
				return StatusCode(500, $"UploadID {guid} : Internal server error: {ex.Message}");
			}

			_logger.Information("UploadID {guid} : Uploading file {FileName} failed", guid, file?.FileName);
			return StatusCode(500, $"UploadID {guid} : Unknown error occurred. Check logs");
		}
	}
}
