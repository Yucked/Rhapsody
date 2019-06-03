namespace Frostbyte.Entities.Audio
{
    public class Author
    {
        public string Id { get; set; }
        
        public string? Url { get; set; }

        public Author(string id)
        {
            Id = id;
        }

        public Author(string id, string url)
        {
            Id = id;
            Url = url;
        }
    }
}