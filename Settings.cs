namespace Concept
{
    public sealed class Settings
    {
        public string Hostname { get; set; }
        public int Port { get; set; }
        public string Authorization { get; set; }


        public static Settings CreateDefault()
            => new Settings
            {
                Hostname = "127.0.0.1",
                Port = 6969,
                Authorization = "Conceptual"
            };
    }
}