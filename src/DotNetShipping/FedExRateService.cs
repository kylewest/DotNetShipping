namespace DotNetShipping.RateServiceWebReference
{
    public partial class RateService
    {
        /// <summary>
        /// </summary>
        /// <param name="production"></param>
        public RateService(bool production)
        {
            if (production)
            {
                Url = "https://ws.fedex.com:443/web-services/rate";
            }
            else
            {
                Url = "https://wsbeta.fedex.com:443/web-services";
            }

            if (IsLocalFileSystemWebService(Url))
            {
                UseDefaultCredentials = true;
                useDefaultCredentialsSetExplicitly = false;
            }
            else
            {
                useDefaultCredentialsSetExplicitly = true;
            }
        }
    }
}
