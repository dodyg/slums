namespace Slums.Infrastructure.Content;

public sealed class ContentLoadException : Exception
{
    public ContentLoadException()
    {
    }

    public ContentLoadException(string message) : base(message)
    {
    }

    public ContentLoadException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
