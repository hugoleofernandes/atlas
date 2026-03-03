namespace Atlas.SharedKernel.Application;

public static class ErrorCodes
{
    public static class Staff
    {
        public const string AlreadyExists = "STAFF_001";
        public const string NotFound = "STAFF_002";
    }

    public static class Common
    {
        public const string ValidationFailed = "COMMON_001";
        public const string UnexpectedError = "COMMON_999";
    }
}