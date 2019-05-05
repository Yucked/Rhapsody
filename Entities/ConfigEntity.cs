namespace Frostbyte.Entities
{
    public sealed class ConfigEntity
    {
        public int Port { get; set; }
        public string Host { get; set; }
        public string Password { get; set; }
        public SourcesEntity Sources { get; set; }

        internal string Url
        {
            get { return $"http://{Host}:{Port}/"; }
        }
    }
}