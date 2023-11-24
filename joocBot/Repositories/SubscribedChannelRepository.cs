using joocBot.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace joocBot.Repositories
{
    public class SubscribedChannelRepository
    {
        private static Mutex mutex = new Mutex();
        private const string DIRECTORY = "./Project/";
        private const string FILE = "SubscribedChannelRepository.json";

        private void SaveElements(IEnumerable<SubscribedChannel> elements)
        {
            try
            {
                var path = Path.Combine(DIRECTORY, FILE);
                mutex.WaitOne(); // 다른 스레드가 접근하지 못하도록 대기
                string jsonData = JsonConvert.SerializeObject(elements, Formatting.Indented);
                File.WriteAllText(path, jsonData);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                throw;
            }
            finally
            {
                mutex.ReleaseMutex();
            }
        }
        private List<SubscribedChannel> LoadElements()
        {
            IEnumerable<SubscribedChannel>? elements = GetAll();

            List<SubscribedChannel> list;
            if (elements == null)
                list = new List<SubscribedChannel>();
            else
                list = elements.ToList();
            return list;
        }
        public bool DeleteOne(SubscribedChannel element)
        {
            List<SubscribedChannel> elements = LoadElements();
            SubscribedChannel? selectedElement = elements.Find(s => s.Id == element.Id);
            if (selectedElement == null)
                return false;
            elements.Remove(selectedElement);
            SaveElements(elements);
            return true;
        }

        public bool Exist(SubscribedChannel element) => LoadElements().Exists(s => s.Id == element.Id);

        public IEnumerable<SubscribedChannel>? GetAll()
        {
            try
            {
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
                var jsonObject = File.ReadAllText(path);
                return string.IsNullOrWhiteSpace(jsonObject)
                    ? new List<SubscribedChannel>() : JsonConvert.DeserializeObject<IEnumerable<SubscribedChannel>>(jsonObject);
            }
            catch
            {
                return new List<SubscribedChannel>();
            }
        }

        public bool SaveOne(SubscribedChannel element)
        {
            try
            {
                List<SubscribedChannel> elements = LoadElements(); //new List<BsonDocument>(); //
                SubscribedChannel? selectedElement = elements.Find(s => s.Id == element.Id);
                if (selectedElement != null)
                {
                    selectedElement.Update(element);
                }
                else
                {
                    elements.Add(element);
                }
                SaveElements(elements);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }
        public bool SaveALL(List<SubscribedChannel> elements)
        {
            try
            {
                SaveElements(elements);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }
    }
}
