using Microsoft.AspNetCore.Mvc.Filters;

namespace lab11.Filters
{
    public class LogActionFilter : IActionFilter
    {
        public void OnActionExecuting(ActionExecutingContext context)
        {
            Console.WriteLine(
                $"Action executing: {context.ActionDescriptor.DisplayName}");
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            Console.WriteLine(
                $"Action executed: {context.ActionDescriptor.DisplayName}");
        }
    }
}