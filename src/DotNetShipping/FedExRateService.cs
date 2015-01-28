using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DotNetShipping.RateServiceWebReference 
{
    public partial class RateService
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="production"></param>
        public RateService(bool production) 
        {
            if (production == true)
            {
                this.Url = "https://ws.fedex.com:443/web-services/rate";
            }
            else
            {
                this.Url = "https://wsbeta.fedex.com:443/web-services";
            }

            if ((this.IsLocalFileSystemWebService(this.Url) == true)) {
                this.UseDefaultCredentials = true;
                this.useDefaultCredentialsSetExplicitly = false;
            }
            else {
                this.useDefaultCredentialsSetExplicitly = true;
            }
        }
    }
}
