namespace PAL
{
    // PAL/Exceptions/DomainException.cs
    using BLL;

    namespace PAL.Exceptions
    {
        public class DomainException(Error error) : Exception(error.Description)
        {
            public Error Error { get; } = error;
        }
    }
}
