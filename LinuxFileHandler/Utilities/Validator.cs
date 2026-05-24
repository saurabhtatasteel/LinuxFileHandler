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


	}
}
