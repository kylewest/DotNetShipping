using System.IO;

namespace DotNetShipping
{
    public static class Extensions
    {
        public static string ToUpsShipCode(this string str)
        {
            switch (str)
            {
                case "UPS Next Day Air":
                    return "01";

                case "UPS 2nd Day Air":
                    return "02";

                case "UPS Ground":
                    return "03";

                case "UPS Worldwide Express":
                    return "07";

                case "UPS Worldwide Expedited":
                    return "08";

                case "UPS Standard to Canada":
                    return "11";

                case "UPS Standard":
                    return "11";

                case "UPS 3 Day Select":
                    return "12";

                case "UPS Next Day Air Saver":
                    return "13";

                case "UPS Next Day Air Early AM":
                    return "14";

                case "UPS Worldwide Express Plus":
                    return "54";

                case "UPS 2nd Day Air AM":
                    return "59";

                case "UPS Saver":
                    return "65";

                case "UPS Worldwide Saver":
                    return "65";

				case "UPS Express Saver":
					return "65";

				case "UPS Sure Post":
                    return "93";

                default:
                    throw new InvalidDataException("Invalid ship code!");
            }
        }
    }
}
