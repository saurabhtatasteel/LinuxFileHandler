using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace LinuxFileHandler.Filters
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
	public class RequireApiKeyHeader : RequireHeaderAttribute
	{
		private const string ApiKeyHeaderName = "xapikey";
		private const string ApiKeyConfigurationKey = "ApplicationSettings:xapikey";

		public RequireApiKeyHeader() : base(ApiKeyHeaderName) { }



		public override void OnAuthorization(AuthorizationFilterContext context)
		{
			if (!TryGetHeaderValue(context.HttpContext, out var apiKeyFromHeader))
			{
				context.Result = new UnauthorizedObjectResult($"Missing required header: (HeaderName)");
				return;
			}

			var configuration = context.HttpContext.RequestServices.GetService<IConfiguration>();
			var apiKeyFromConfiguration = configuration?[ApiKeyConfigurationKey];

			if (string.IsNullOrWhiteSpace(apiKeyFromConfiguration))
			{
				context.Result = new ObjectResult("API key is not configured in the applicatian. Ask for Application Support help.")
				{
					StatusCode = StatusCodes.Status500InternalServerError
				};
				return;
			}


			var apiKey = apiKeyFromConfiguration.Split("#")[0];
			if (!string.Equals(apiKeyFromHeader, apiKey, StringComparison.Ordinal))
			{
				context.Result = new UnauthorizedObjectResult("Invalid header value.");
			}

			else
			{
				// validate the date
				var currentDate = DateTime.UtcNow.Ticks;
				var apiKeyExpiryTicksString = apiKeyFromConfiguration.Split("#").Length > 1 ?
					apiKeyFromConfiguration.Split("#")[1] : null;

				if (apiKeyExpiryTicksString == null || !long.TryParse(apiKeyExpiryTicksString, out var apiKeyExpiryTicks))
				{
					context.Result = new ObjectResult("API hey expiry is not configured property in the application. Ask for Application Support help.")
					{
						StatusCode = StatusCodes.Status500InternalServerError
					};
					return;
				}



				if (currentDate > apiKeyExpiryTicks)
				{
					context.Result = new UnauthorizedObjectResult("API key has expired.");
					return;
				}
			}
		}
	}
}
