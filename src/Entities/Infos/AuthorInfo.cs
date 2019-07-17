namespace Frostbyte.Entities.Infos
{
    public readonly struct AuthorInfo
    {
        public string Name { get; }
        public string Url { get; }
        public string AvatarUrl { get; }

        public AuthorInfo(string name, string url, string avatarUrl)
        {
            Name = name;
            Url = url;
            AvatarUrl = avatarUrl;
        }
    }
}