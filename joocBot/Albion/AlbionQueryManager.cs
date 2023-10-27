using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Newtonsoft.Json;
using joocBot.Models;

namespace joocBot.Albion
{
    public class AlbionQueryManager
    {
        public AlbionQueryManager() 
        {
            _requestor = new AlbionApiRequestor();
            _requestor.Region = (RegionCode)LoadRegion();
        }
        private AlbionApiRequestor _requestor;
        private const string DIRECTORY = "./Project/";
        private const string FILE = "AlbionQueryManager.env";
        private int LoadRegion()
        {
            string path = Path.Combine(DIRECTORY, FILE);
            string region = string.Empty;
            try
            {
                if (!Directory.Exists(DIRECTORY))
                {
                    // 디렉토리가 존재하지 않으면 새로운 디렉토리 생성
                    Directory.CreateDirectory(DIRECTORY);
                }
                if (!File.Exists(path))
                {
                    // 파일이 존재하지 않으면 새로운 파일 생성
                    using (File.Create(path))
                    {
                    }
                }

                // 파일에서 API 토큰 및 지역 정보 읽기
                Dictionary<string, string> settings = new Dictionary<string, string>();
                using (StreamReader reader = new StreamReader(path))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        string[] parts = line.Split('=');
                        if (parts.Length == 2)
                        {
                            settings[parts[0]] = parts[1];
                        }
                    }
                }

                if (settings.ContainsKey("Region"))
                {
                    string item = settings["Region"];
                    Console.WriteLine("지역 정보: " + item);
                    region = item;
                }
                if (string.IsNullOrWhiteSpace(region))
                {
                    return 0;
                }
                return Convert.ToInt32(region);
            }
            catch (Exception ex)
            {
                Console.WriteLine("파일 읽기 중 오류 발생: " + ex.Message);
                return 0;
            }
        }

        private int SaveRegion(int code) 
        {
            string path = Path.Combine(DIRECTORY, FILE);
            try
            {
                if (!Directory.Exists(DIRECTORY))
                {
                    // 디렉토리가 존재하지 않으면 새로운 디렉토리 생성
                    Directory.CreateDirectory(DIRECTORY);
                }
                if (!File.Exists(path))
                {
                    // 파일이 존재하지 않으면 새로운 파일 생성
                    using (File.Create(path))
                    {
                    }
                }
                using (StreamWriter writer = new StreamWriter(path))
                {
                    writer.WriteLine($"Region={code}");
                }
                Console.WriteLine("지역 정보가 파일에 저장되었습니다.");
                return code;
            }
            catch (Exception ex)
            {
                Console.WriteLine("파일 저장 중 오류 발생: " + ex.Message);
                return -1;
            }    
        }

        public string GetRegion() 
        { 
            return _requestor.Region.ToString();
        }
        public string SetRegion(string embeddedCode)
        {
            switch (embeddedCode)
            {
                case "W":
                case "w":
                    _requestor.Region = (RegionCode)SaveRegion((int)RegionCode.Western);
                    break;
                case "E":
                case "e":
                    _requestor.Region = (RegionCode)SaveRegion((int)RegionCode.Eastern);
                    break;
                default:
                    _requestor.Region = (RegionCode)SaveRegion((int)RegionCode.Default);
                    break;
            }
           return _requestor.Region.ToString();
        }
        public List<Player> SearchPlayers(string username)
        {
            var jsonString = _requestor.SearchUsername(username);
            var jsonObject = JsonConvert.DeserializeObject<Search>(jsonString);

            var results = (jsonObject == null)
                ? new List<Player>()
                : jsonObject.players.ToList();
            return results;
        }
        public List<string> ConvertNameToId(string username)
        {
            var jsonString = _requestor.SearchUsername(username);
            var jsonObject = JsonConvert.DeserializeObject<Search>(jsonString);

            var results = (jsonObject == null)
                ? new List<string>()
                : jsonObject.players.Select(p => p.Id).ToList();
            return results;
        }
        public string ConvertNameToIdOne(string username)
        {
            var jsonString = _requestor.SearchUsername(username);
            var jsonObject = JsonConvert.DeserializeObject<Search>(jsonString);

            var results = (jsonObject == null)
                ? string.Empty
                : jsonObject.players.Select(p => p.Id).ToList()[0];
            return results;
        }
        public List<BattleEvent> SearchPlayersKills(string username)
        {
            var id = ConvertNameToIdOne(username);
            var param = string.Empty;
            if (string.IsNullOrEmpty(id))
                param = username;
            else
                param = id;

            var jsonString = _requestor.Kills(param);
            var jsonObject = JsonConvert.DeserializeObject<List<BattleEvent>>(jsonString);

            var results = (jsonObject == null)
                ? new List<BattleEvent>()
                : jsonObject;
            return results;
        }
    }
}
