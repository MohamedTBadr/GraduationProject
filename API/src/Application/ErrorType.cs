namespace Application
{
    public record Error(ErrorType Type, int Code, string Description)
    {
        public static Error Validation(int code, string description)
            => new(ErrorType.Validation, code, description);

        public static Error NotFound(int code, string description)
            => new(ErrorType.NotFound, code, description);

        public static Error Conflict(int code, string description)
            => new(ErrorType.Conflict, code, description);

        public static Error Unauthorized(int code, string description)
            => new(ErrorType.Unauthorized, code, description);

        public static Error Forbidden(int code, string description)
            => new(ErrorType.Forbidden, code, description);

        public static Error BusinessRule(int code, string description)
            => new(ErrorType.BusinessRule, code, description);

        public static Error Unexpected(int code, string description)
            => new(ErrorType.Unexpected, code, description);

        public static Error InvalidOperation(int code, string description)
            => new(ErrorType.InvalidOperation, code, description);


    }
    public enum ErrorType
    {
        // Validation & Input
        Validation,          // Input fails validation rules (e.g., required field missing, invalid format)
        InvalidOperation,    // Operation is not valid in the current state

        // Resource Errors
        NotFound,            // Requested resource does not exist
        AlreadyExists,       // Resource already exists (duplicate creation attempt)
        Conflict,            // State conflict (e.g., concurrent update, version mismatch)

        // Authorization & Authentication
        Unauthorized,        // User is not authenticated
        Forbidden,           // User is authenticated but lacks permission

        // Business Rule Violations
        BusinessRule,        // General business rule violation
        Expired,             // Resource or token has expired (e.g., subscription, session)
        LimitExceeded,       // Quota or rate limit exceeded
        InsufficientFunds,   // Balance/credit too low for operation

        // External Dependencies
        ExternalService,     // Third-party Service failure
        Timeout,             // Operation timed out

        // System / Infrastructure
        Unexpected,          // Unhandled / unknown error (fallback)
        Unavailable,         // Service is temporarily unavailable
        NotImplemented       // Feature/operation not yet implemented
    }
}
