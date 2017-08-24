using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using static System.String;

namespace DotNetShipping.Helpers
{
    public static class USPSHelpers
    {
        /// <summary>
        /// Removes encoded characters from mail service name for human consumption
        /// </summary>
        /// <param name="mailServiceName"></param>
        /// <returns></returns>
        public static String SanitizeMailServiceName(this String mailServiceName)
        {
            if (!IsNullOrEmpty(mailServiceName))
                return Regex.Replace(mailServiceName, "&lt.*&gt;", "");

            return mailServiceName;
        }
    }
}
