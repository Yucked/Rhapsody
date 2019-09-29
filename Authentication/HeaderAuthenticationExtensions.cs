using Microsoft.AspNetCore.Authentication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Concept.Authentication
{
    public static class HeaderAuthenticationExtensions
    {
        public static AuthenticationBuilder AddHeaderAuth(
            this AuthenticationBuilder builder, string authenticationScheme, Action<HeaderOptions> configureOptions)
            => builder.AddScheme<HeaderOptions, HeaderAuthenticationHandler>(authenticationScheme, configureOptions);

        public static AuthenticationBuilder AddHeaderAuth(this AuthenticationBuilder builder, Action<HeaderOptions> configureOptions)
            => builder.AddScheme<HeaderOptions, HeaderAuthenticationHandler>(HeaderDefaults.AuthenticationScheme, configureOptions);
    }
}