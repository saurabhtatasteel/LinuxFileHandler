using LinuxFileHandler.Configurations;
using System.Net.NetworkInformation;

namespace LinuxFileHandler.Utilities
{
	public static class Validator
	{

		public static bool IsValidPdf(IFormFile file, string[] acceptableFileTypes, long maxFileSize, out ValidationResult validationResult)
		{
			validationResult = ValidationResult.Valid;
			// Validate file presence
			if (file == null || file.Length == 0)
			{
				validationResult = ValidationResult.NoFileUploaded;
				return false;
			}

			// Validate file extension
			var fileExtension = Path.GetExtension(file.FileName);
			var receivedFileType = acceptableFileTypes.Where(m => m.ToString().Equals(fileExtension, StringComparison.OrdinalIgnoreCase));
			if (!receivedFileType?.Any() ?? false)
			{
				validationResult = ValidationResult.InvalidFileType;
				return false;
			}

			// Validate file size (e.g., max 10 MB)
			if (file.Length > maxFileSize * 1024 * 1024)
			{
				validationResult = ValidationResult.InvalidFileSize;
				return false;
			}
			// all validations passed
			return true;

		}

		/// <summary>
		/// Validate ApplicationSettings value in appSettings.json
		/// </summary>
		/// <param name="applicationSettings"></param>
		/// <returns></returns>
		public static bool IsValid(this ApplicationSettings applicationSettings)
		{
			if (applicationSettings == null ||
				(applicationSettings != null && !HasValueInField(applicationSettings.ValidFileTypes)) ||
				(applicationSettings != null && !HasValueInField(applicationSettings.FullUploadsPath)) ||
				(applicationSettings != null && !HasValueInField(applicationSettings.UploadsPath)) ||
				(applicationSettings != null && !HasValueInField(applicationSettings.FullProcessedPath)) ||
				(applicationSettings != null && !HasValueInField(applicationSettings.AppBasePath)) ||
				(applicationSettings != null && !HasValueInField(applicationSettings.BatchFileOS)) ||
				(applicationSettings != null && !HasValueInField(applicationSettings.BatchFileOutputReturnType)) ||
				(applicationSettings != null && !HasValueInField(applicationSettings.XAPIKey)) ||
				(applicationSettings != null && !HasValueInField(applicationSettings.MaxFileSizeMB)) ||
				(applicationSettings != null && !HasValueInField(applicationSettings.ProcessedPath))
				)
			{
				return false;
			}

			return true;
		}

		private static bool HasValueInField(string param)
		{
			if (string.IsNullOrEmpty(param) || string.IsNullOrWhiteSpace(param))
				return false;
			else
				return true;
		}


	}
}
