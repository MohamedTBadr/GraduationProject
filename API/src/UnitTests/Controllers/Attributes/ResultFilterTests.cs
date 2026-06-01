using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Moq;
using Web.Api.Controllers.Attributes;
using Xunit;
using Application;

namespace Application.UnitTests.Controllers.Attributes
{
    public class ResultFilterTests
    {
        [Fact]
        public void OnActionExecuting_DoesNothing()
        {
            // Arrange
            var filter = new ResultFilter();
            var actionContext = new ActionContext(new DefaultHttpContext(), new RouteData(), new ActionDescriptor());
            var context = new ActionExecutingContext(actionContext, new List<IFilterMetadata>(), new Dictionary<string, object?>(), new object());

            // Act
            filter.OnActionExecuting(context);

            // Assert
            Assert.Null(context.Result); // No result set
        }
        [Fact]
        public void OnActionExecuted_ResultIsNotObjectResult_DoesNothing()
        {
            // Arrange
            var filter = new ResultFilter();
            var actionContext = new ActionContext(new DefaultHttpContext(), new RouteData(), new ActionDescriptor());
            var context = new ActionExecutedContext(actionContext, new List<IFilterMetadata>(), new object())
            {
                Result = new ViewResult()
            };

            // Act
            filter.OnActionExecuted(context);

            // Assert
            Assert.IsType<ViewResult>(context.Result);
        }

        [Fact]
        public void OnActionExecuted_ObjectResultValueIsNull_DoesNothing()
        {
            // Arrange
            var filter = new ResultFilter();
            var actionContext = new ActionContext(new DefaultHttpContext(), new RouteData(), new ActionDescriptor());
            var context = new ActionExecutedContext(actionContext, new List<IFilterMetadata>(), new object())
            {
                Result = new ObjectResult(null)
            };

            // Act
            filter.OnActionExecuted(context);

            // Assert
            var result = Assert.IsType<ObjectResult>(context.Result);
            Assert.Null(result.Value);
        }

        [Fact]
        public void OnActionExecuted_ObjectResultValueIsNotGenericResult_DoesNothing()
        {
            // Arrange
            var filter = new ResultFilter();
            var actionContext = new ActionContext(new DefaultHttpContext(), new RouteData(), new ActionDescriptor());
            var context = new ActionExecutedContext(actionContext, new List<IFilterMetadata>(), new object())
            {
                Result = new ObjectResult("Not a Result<T>")
            };

            // Act
            filter.OnActionExecuted(context);

            // Assert
            var result = Assert.IsType<ObjectResult>(context.Result);
            Assert.Equal("Not a Result<T>", result.Value);
        }

        [Fact]
        public void OnActionExecuted_IsSuccessWithoutSuccessStatusCodeAttribute_DoesNothing()
        {
            // Arrange
            var filter = new ResultFilter();
            var actionContext = new ActionContext(new DefaultHttpContext(), new RouteData(), new ActionDescriptor
            {
                EndpointMetadata = new List<object>()
            });
            var context = new ActionExecutedContext(actionContext, new List<IFilterMetadata>(), new object())
            {
                Result = new ObjectResult(Result<string>.Success("OK"))
            };

            // Act
            filter.OnActionExecuted(context);

            // Assert
            var result = Assert.IsType<ObjectResult>(context.Result);
            Assert.IsType<Result<string>>(result.Value);
        }

        [Fact]
        public void OnActionExecuted_IsSuccessWithSuccessStatusCodeAttribute_UpdatesResultAndStatusCode()
        {
            // Arrange
            var filter = new ResultFilter();
            var actionContext = new ActionContext(new DefaultHttpContext(), new RouteData(), new ActionDescriptor
            {
                EndpointMetadata = new List<object> { new SuccessStatusCodeAttribute(201) }
            });
            var context = new ActionExecutedContext(actionContext, new List<IFilterMetadata>(), new object())
            {
                Result = new ObjectResult(Result<string>.Success("OK"))
            };

            // Act
            filter.OnActionExecuted(context);

            // Assert
            var result = Assert.IsType<ObjectResult>(context.Result);
            Assert.Equal("OK", result.Value);
            Assert.Equal(201, result.StatusCode);
        }

        [Fact]
        public void OnActionExecuted_IsFailure_UpdatesResultWithMapError()
        {
            // Arrange
            var filter = new ResultFilter();
            var actionContext = new ActionContext(new DefaultHttpContext(), new RouteData(), new ActionDescriptor());
            var error = new Error(ErrorType.Validation, 422, "Validation failed");
            var resultObj = Result<string>.Failure(error);
            var context = new ActionExecutedContext(actionContext, new List<IFilterMetadata>(), new object())
            {
                Result = new ObjectResult(resultObj)
            };

            // Act
            filter.OnActionExecuted(context);

            // Assert
            var result = Assert.IsType<ObjectResult>(context.Result);
            Assert.Equal(422, result.StatusCode);
            Assert.NotNull(result.Value);
        }
    }
}
