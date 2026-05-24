using Hangfire.Dashboard;

namespace Web.Api.Middlewares
{
    public sealed class HangfireAdminAuthorizationFilter : IDashboardAuthorizationFilter
    {
        public bool Authorize(DashboardContext context)
        {
            var httpContext = context.GetHttpContext();
            return httpContext.User.Identity?.IsAuthenticated == true &&
                   httpContext.User.IsInRole("Admin");
        }
    }
}
