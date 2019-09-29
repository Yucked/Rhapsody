using Microsoft.AspNetCore.Authentication;

namespace Concept.Authentication
{
    public sealed class HeaderOptions : AuthenticationSchemeOptions
    {
        public string Authorization { get; set; }
    }
}