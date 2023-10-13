using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using joocBot.Models;
using Newtonsoft.Json;
using static System.Collections.Specialized.BitVector32;

namespace joocBot.Repositories
{
    public class ChatBotRepository
    {
        private static Mutex mutex = new Mutex();
        private const string DIRECTORY = "./Project/";
        private const string FILE = "Token.json";
        public void SaveToken(ChatBot chatBot)
        {

            try
            {
                mutex.WaitOne(); // 다른 스레드가 접근하지 못하도록 대기
                var path = Path.Combine(DIRECTORY, FILE);
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
                string jsonData = JsonConvert.SerializeObject(chatBot);
                File.WriteAllText(path, jsonData);
            }
            catch (Exception)
            {

                throw;
            }
            finally
            {
                mutex.ReleaseMutex();
            }
        }

        public ChatBot GetToken()
        {
            try
            {
                var path = Path.Combine(DIRECTORY, FILE);
                string? jsonData = default;
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
                jsonData = File.ReadAllText(path);
                return string.IsNullOrWhiteSpace(jsonData)
                    ? new ChatBot() 
                    : JsonConvert.DeserializeObject<ChatBot>(jsonData) 
                    ?? new ChatBot();
            }
            catch
            {
                return new ChatBot();
            }
        }


        public bool Vaild(string argToken)
        {
            return GetToken().Token == argToken;
        }
    }
}
