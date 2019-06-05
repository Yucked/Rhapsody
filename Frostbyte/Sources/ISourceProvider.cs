namespace Frostbyte.Sources
{
    public interface ISourceProvider
    {
        bool IsEnabled { get; }

        string Prefix { get; }
    }
}