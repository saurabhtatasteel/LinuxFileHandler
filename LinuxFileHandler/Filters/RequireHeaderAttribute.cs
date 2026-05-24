using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace LinuxFileHandler.Filters
{
	public class RequireHeaderAttribute : Attribute, IAuthorizationFilter
	{
		protected string HeaderName { get; }

		public RequireHeaderAttribute(string headerName)
		{
			HeaderName = headerName;
		}

		public virtual void OnAuthorization(AuthorizationFilterContext filterContext)
		{
			if (!TryGetHeaderValue(filterContext.HttpContext, out _))
			{
				filterContext.Result = new UnauthorizedObjectResult($"Missing required header; {HeaderName}");
			}
		}

		protected bool TryGetHeaderValue(HttpContext httpContext, out string? headerValue)
		{
			headerValue = null;
			if (!httpContext.Request.Headers.TryGetValue(HeaderName, out var value) ||
				string.IsNullOrWhiteSpace(value))
			{
				return false;
			}

			headerValue = value.ToString();
			return true;
		}

	}
}
