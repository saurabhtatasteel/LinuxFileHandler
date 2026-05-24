using LinuxFileHandler.BLL.Interfaces;
using LinuxFileHandler.Entities;
using LinuxFileHandler.Utilities;
using System.Diagnostics;
using SRL = Serilog;

namespace LinuxFileHandler.BLL
{
	public class UploadFileProcessor : IUploadFileProcessor
	{

		private readonly SRL.ILogger _logger;
		private readonly IConfiguration _configuration;

		public UploadFileProcessor(SRL.ILogger logger, IConfiguration configuration)
		{
			_logger = logger;
			_configuration = configuration;
		}

		public async Task<Tuple<MemoryStream, FileProcessResult>> ProcessFileAsync(IFormFile file, string uploadsPath, string processedPath, Guid uploadId, IWebHostEnvironment environment)
		{
			// Validate file
			var validFileTypes = (_configuration["ApplicationSettings:ValidFileTypes"] ?? string.Empty)
		.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

			var maxFileSize = _configuration["ApplicationSettings:MaxFileSizeMB"];

			if (!Validator.IsValidPdf(file, validFileTypes, long.Parse(maxFileSize), out ValidationResult validationResult))
			{
				var validationResultFailReason =
				validationResult == ValidationResult.InvalidFileSize ? $"Invalid file size. Size more than {maxFileSize} MB not allowed" :
				validationResult == ValidationResult.InvalidFileType ? $"Invalid file type" :
				validationResult == ValidationResult.NoFileUploaded ? $"No file uploaded" : String.Empty;

				_logger.Error($"UploadID (uploadId) : File {file?.FileName} upload failed. {validationResultFailReason}. Please upload a valid {string.Join(" or ", validFileTypes)} file that meets the specified criteria");

				return await Task.FromResult(new Tuple<MemoryStream, FileProcessResult>(null, new FileProcessResult
				{
					Error = FileProcessError.InvalidFile,
					IsSuccess = false,
					Message = $"UploadID {uploadId} : File {file?.FileName} upload failed, {validationResultFailReason}, Please upload a valid {string.Join(" or ", validFileTypes)} file that meets the specified criteria"
				}));

				//return BadRequest($"UploadID {uploadId] : File (file7.Filelame) upload failed. {validationResultFailReason]. Please upload a valid (String
			}
			else
				_logger.Information("UploadID {uploadId} : File validation succeeded", uploadId);


			Directory.CreateDirectory(uploadsPath);
			Directory.CreateDirectory(processedPath);

			_logger.Information("UpLoadID {uploadId} : Uploadpath : {uploadsPath}, ProcessedPath : {processedPath}",
			uploadId, uploadsPath, processedPath);

			// Save uploaded file
			var uniqueFileName = Utilities.Utilities.GenerateUniqueFileName(file);
			var uploadedFilePath = Path.Combine(uploadsPath, uniqueFileName);
			using (var stream = new FileStream(uploadedFilePath, FileMode.Create))
			{
				await file.CopyToAsync(stream);
			}

			_logger.Information("UploadID {uploadId} : File copied to : {uploadedFilePath}", uploadId, uploadedFilePath);

			string scriptPath;
			var batchFileOS = _configuration["ApplicationSettings:BatchFileOS"];
			ProcessStartInfo startInfo;
			if (batchFileOS == "Linux")
			{
				scriptPath = Path.Combine(environment.ContentRootPath, "Scripts", "linux-process-file.sh");
				startInfo = new ProcessStartInfo
				{
					FileName = "/bin/bash",
					RedirectStandardOutput = true,
					RedirectStandardError = true,
					UseShellExecute = false,
					CreateNoWindow = true,
					WorkingDirectory = Path.GetDirectoryName(scriptPath)
				};

				startInfo.ArgumentList.Add(scriptPath);
				startInfo.ArgumentList.Add(uploadedFilePath);
				startInfo.ArgumentList.Add(processedPath);

			}

			else
			{


				scriptPath = Path.Combine(environment.ContentRootPath, "Scripts", "win-process-file.bat");
				startInfo = new ProcessStartInfo
				{
					FileName = "cmd.exe",
					RedirectStandardOutput = true,
					RedirectStandardError = true,
					UseShellExecute = false,
					CreateNoWindow = true,
					WorkingDirectory = Path.GetDirectoryName(scriptPath)
				};
				startInfo.ArgumentList.Add("/c");
				startInfo.ArgumentList.Add(scriptPath);
				startInfo.ArgumentList.Add(uploadedFilePath);
				startInfo.ArgumentList.Add(processedPath);

			}

			var process = new Process { StartInfo = startInfo };
			process.Start();



			_logger.Information("UploadID {uploadId} : Batch file process started", uploadId);
			string output = await process.StandardOutput.ReadToEndAsync();
			string error = await process.StandardError.ReadToEndAsync();

			_logger.Information("UploadID {uploadId} : File copy uploads directory - output : {output}, error : {error}", uploadId, output, error);
			await process.WaitForExitAsync();
			if (process.ExitCode != 0)
			{

				_logger.Error("UploadID {uploadId} : Script execution failed for file {FileName}. ExitCode: {ExitCode}, Output: {Output}, Error: {Error}",
				uploadId, file.FileName, process.ExitCode, output, error);

				//return StatusCode(500, $"Script failed: [error}");
				return new Tuple<MemoryStream, FileProcessResult>(null, new FileProcessResult
				{
					Error = FileProcessError.ScriptExecutionFailed,
					Message = $"UploadID {uploadId} : Script execution failed for file {file.FileName}. Please check the logs for more details",
					IsSuccess = false,
				});
			}
			else
				_logger.Information("UploadID {uploadId} : Batch file process completed", uploadId);

			// Expected processed file
			var processedFilePath = Path.Combine(processedPath, uniqueFileName);
			if (!System.IO.File.Exists(processedFilePath))
			{
				_logger.Error("UploadID {uploadId} :Processed file not found for {FileName} after script execution. Output: {Output}, Error: {Error}",
				uploadId, file.FileName, output, error);

				return new Tuple<MemoryStream, FileProcessResult>(null, new FileProcessResult
				{
					Error = FileProcessError.ProcessedFileNotFound,
					IsSuccess = false,
					Message = $"UploadID {uploadId} : Processed file not found for {file.FileName} after script execution. Please check the logs for more details"
					//return StatusCode(500, "Processed file not found.");
				});
			}
			else
				_logger.Information("UploadID {uploadId} : Picked file from processed directory {processedFilePath}", uploadId, processedFilePath);

			// Read processed file
			var memory = new MemoryStream();
			using (var stream = new FileStream(processedFilePath, FileMode.Open))
			{
				await stream.CopyToAsync(memory);
			}


			memory.Position = 0;

			return new Tuple<MemoryStream, FileProcessResult>(memory, new FileProcessResult
			{
				Error = FileProcessError.None,
				IsSuccess = true,
				Message = $"UploadID {uploadId} : File processed successfully for {file.FileName}"
			});
		}
	}
}
