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
            _requestor = new AlbionApiRequestor
            {
                Region = (RegionCode)LoadRegion()
            };
        }
        private readonly AlbionApiRequestor _requestor;
        private const string DIRECTORY = "./Project/";
        private const string FILE = "AlbionQueryManager.env";

        private static int LoadRegion()
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
                var settings = new Dictionary<string, string>();
                using (var reader = new StreamReader(path))
                {
                    string? line;
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
        private static int SaveRegion(int code) 
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
                using (var writer = new StreamWriter(path))
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
            return $"알비온 서버: {_requestor.Region.ToString()} (디폴트:동부)";
        }
        public string SetRegion(string embeddedCode)
        {
            _requestor.Region = embeddedCode switch
            {
                "W" or "w" => (RegionCode)SaveRegion((int)RegionCode.Western),
                "E" or "e" => (RegionCode)SaveRegion((int)RegionCode.Eastern),
                _ => (RegionCode)SaveRegion((int)RegionCode.Default),
            };
            return $"알비온 서버: {_requestor.Region.ToString()} (방금설정됨)";
        }

        #pragma warning disable CS8604 // 가능한 null 참조 인수입니다.
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
        public string ConvertNameToGuildOne(string username)
        {
            var jsonString = _requestor.SearchUsername(username);
            var jsonObject = JsonConvert.DeserializeObject<Search>(jsonString);

            var Alliance = (jsonObject == null)
                ? string.Empty
                : jsonObject.players.Select(p => p.AllianceName).ToList()[0];
            var guild = (jsonObject == null)
                ? string.Empty
                : jsonObject.players.Select(p => p.GuildName).ToList()[0];
            return string.IsNullOrWhiteSpace(Alliance)? guild: $"[{Alliance}]{guild}";
        }

        public List<BattleEvent> SearchPlayersEvents(string username, string? id = default)
        {
            if (string.IsNullOrEmpty(id))
                id = ConvertNameToIdOne(username);

            var killJson = _requestor.Kills(id);
            var deathJson = _requestor.Deaths(id);
            var kills = JsonConvert.DeserializeObject<List<BattleEvent>>(killJson) ?? new List<BattleEvent>();
            var deaths = JsonConvert.DeserializeObject<List<BattleEvent>>(deathJson) ?? new List<BattleEvent>();

            var jsonObject = new List<BattleEvent>();
            jsonObject.AddRange(deaths);
            jsonObject.AddRange(kills);

            //var results = (jsonObject == null)
            //    ? new List<BattleEvent>()
            //    : jsonObject;

            var results = jsonObject.OrderByDescending(be=>be.BattleId).ToList();
            return results;
        }
        public BattleEvent SearchPlayersRecentEvent(string username, string? id = default)
        {
            try
            {
                var result = (SearchPlayersEvents(username, id).Count > 0) ? SearchPlayersEvents(username, id).First() : new BattleEvent();
                return result;
            }
            catch (Exception ex) 
            {
                Console.WriteLine(ex);
                return new BattleEvent();
            }
            finally 
            { 
            }
        }
        public List<BattleEvent> SearchPlayersKills(string username)
        {
            var id = ConvertNameToIdOne(username);
            string param;
            if (string.IsNullOrEmpty(id))
                param = username;
            else
                param = id;

            var killJson = _requestor.Kills(param);
            var kills = JsonConvert.DeserializeObject<List<BattleEvent>>(killJson) ?? new List<BattleEvent>();

            var jsonObject = new List<BattleEvent>();
            jsonObject.AddRange(kills);

            var results = jsonObject.OrderByDescending(be => be.BattleId).ToList();
            return results;
        }
        public BattleEvent SearchPlayersRecentKill(string username)
        {
            return SearchPlayersKills(username).First();
        }
        public List<BattleEvent> SearchPlayersDeaths(string username)
        {
            var id = ConvertNameToIdOne(username);
            string param;
            if (string.IsNullOrEmpty(id))
                param = username;
            else
                param = id;

            var deathJson = _requestor.Deaths(param);
            var deaths = JsonConvert.DeserializeObject<List<BattleEvent>>(deathJson) ?? new List<BattleEvent>();

            var jsonObject = new List<BattleEvent>();
            jsonObject.AddRange(deaths);

            var results = jsonObject.OrderByDescending(be => be.BattleId).ToList();
            return results;
        }
        public BattleEvent SearchPlayersRecentDeath(string username)
        {
            return SearchPlayersDeaths(username).First();
        }
    }
}
