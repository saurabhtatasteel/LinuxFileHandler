namespace LinuxFileHandler.Utilities
{
	public static class Utilities
	{
		/// <summary>
		/// Generate Unique file name using datetimestamp
		/// </summary>
		/// <param name="file"></param>
		/// <returns></returns>
		/// <exception cref="NotImplementedException"></exception>
		public static string GenerateUniqueFileName(IFormFile file)
		{
			string uploadedFileName;
			try
			{
				var originalFileName = Path.GetFileName(file?.FileName ?? string.Empty);
				var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(originalFileName);
				var fileExtension = Path.GetExtension(originalFileName);
				var dateTimestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmssfff");
				uploadedFileName = $"{fileNameWithoutExtension}_{dateTimestamp}{fileExtension}";
			}
			catch (Exception ex)
			{
				throw new NotImplementedException($"Unique filename generation failed {ex.StackTrace}");
	}
			return uploadedFileName;
		}
	}
}
