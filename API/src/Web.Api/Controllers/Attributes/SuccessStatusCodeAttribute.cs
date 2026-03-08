// PAL/Filters/SuccessStatusCodeAttribute.cs
namespace Web.Api.Controllers.Attributes
{
    [AttributeUsage(AttributeTargets.Method)]
    public class SuccessStatusCodeAttribute(int statusCode) : Attribute
    {
        public int StatusCode { get; } = statusCode;
    }
}