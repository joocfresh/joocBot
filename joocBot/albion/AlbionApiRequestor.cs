using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace joocBot.Albion
{
    public enum RegionCode
    {
        Default = 0,
        Eastern = 1,
        Western = 2,
    }
    public class AlbionApiRequestor
    {
        private const string WESTERN_URL = "https://gameinfo.albiononline.com/api/gameinfo/";
        private const string EASTERN_URL = "https://gameinfo-sgp.albiononline.com/api/gameinfo/";
        public AlbionApiRequestor() 
        {
            Region = RegionCode.Default;
        }

        public string BaseURL { get; private set; } = string.Empty;

        private RegionCode _region;
        public RegionCode Region
        {
            get
            {
                return _region;
            }
            set
            {
                _region = value;
                switch (value) 
                { 
                    case RegionCode.Default:
                    case RegionCode.Eastern:
                        BaseURL = EASTERN_URL;
                        break;
                    case RegionCode.Western:
                        BaseURL = WESTERN_URL;
                        break;
                }
            }
        }
        // Disable the warning.
        #pragma warning disable SYSLIB0014
        private string CallWebRequest(string fuctionURL)
        {
            string responseFromServer = string.Empty;

            try
            {
                WebRequest request = WebRequest.Create(BaseURL + fuctionURL);
                request.Method = "GET";
                request.ContentType = "application/json";
                request.Headers["user-agent"] = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/116.0.0.0 Safari/537.36";
                using WebResponse response = request.GetResponse();
                using Stream dataStream = response.GetResponseStream();
                using var reader = new StreamReader(dataStream);
                responseFromServer = reader.ReadToEnd();

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

            return responseFromServer;
        }
        public string SearchUsername(string username)
        {
            string result;
            try
            {
                result = CallWebRequest($"search?q={username}");

                return result;
            }
            catch (Exception e)
            {

                return $"실패했습니다. 왜 그랬을까요?: [m:{e.Message}] [stack:{e.StackTrace}] [src:{e.Source}]";
            }
        }
        public string Kills(string id)
        {
            string result;
            try
            {
                result = CallWebRequest($"players/{id}/kills");

                return result;
            }
            catch (Exception e)
            {

                return $"실패했습니다. 왜 그랬을까요?: [m:{e.Message}] [stack:{e.StackTrace}] [src:{e.Source}]";
            }
        }
        public string Deaths(string id)
        {
            string result;
            try
            {
                result = CallWebRequest($"players/{id}/deaths");

                return result;
            }
            catch (Exception e)
            {

                return $"실패했습니다. 왜 그랬을까요?: [m:{e.Message}] [stack:{e.StackTrace}] [src:{e.Source}]";
            }
        }
    }
}
