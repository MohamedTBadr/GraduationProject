// PAL/Filters/SuccessStatusCodeAttribute.cs
namespace PAL.Controllers.Attributes
{
    [AttributeUsage(AttributeTargets.Method)]
    public class SuccessStatusCodeAttribute(int statusCode) : Attribute
    {
        public int StatusCode { get; } = statusCode;
    }
}