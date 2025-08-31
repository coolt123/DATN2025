using DATN.DbContexts;
using System.Security.Claims;

namespace DATN.Middleware
{
    public class MergeSessionProductViewsMiddleware
    {
        private readonly RequestDelegate _next;

        public MergeSessionProductViewsMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, Data dbContext)
        {
            if (context.User.Identity?.IsAuthenticated == true)
            {
                var sessionId = context.Session.GetString("SessionId");
                if (string.IsNullOrEmpty(sessionId))
                {
                    sessionId = Guid.NewGuid().ToString();
                    context.Session.SetString("SessionId", sessionId);
                }
                var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (!string.IsNullOrEmpty(userId))
                {
                    var viewsToUpdate = dbContext.ProductViews
                        .Where(v => v.SessionId == sessionId && v.UserId == null);

                    foreach (var view in viewsToUpdate)
                    {
                        view.UserId = userId;
                    }

                    await dbContext.SaveChangesAsync();
                }
            }

            await _next(context);
        }
    }

}
