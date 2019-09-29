using Microsoft.AspNetCore.Authentication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Concept.Authentication
{
    public class HeaderOptions : AuthenticationSchemeOptions
    {
        /// <summary>
        /// Is the value in the Header to valid the authorize:
        /// Authorization: MyInvenciblePassword
        /// </summary>
        public string Authorization { get; set; } = "MyInvenciblePassword";
    }
}