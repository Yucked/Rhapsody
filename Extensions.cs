using System;
using Concept.Authentication;
using Microsoft.AspNetCore.Authentication;

namespace Concept
{
    public static class Extensions
    {
        public static AuthenticationBuilder UseHeaderAuthentication(this AuthenticationBuilder builder,
            string authenticationScheme, Action<HeaderOptions> configureOptions)
            => builder.AddScheme<HeaderOptions, HeaderAuthenticationHandler>(authenticationScheme, configureOptions);

        public static AuthenticationBuilder UseHeaderAuthentication(this AuthenticationBuilder builder,
            Action<HeaderOptions> configureOptions)
            => builder.AddScheme<HeaderOptions, HeaderAuthenticationHandler>("HeaderAuth", configureOptions);
    }
}