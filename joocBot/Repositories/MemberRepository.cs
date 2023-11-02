using joocBot.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace joocBot.Repositories
{
    public class MemberRepository : IRepository<Member>
    {
        private static Mutex mutex = new Mutex();
        private const string DIRECTORY = "./Project/";
        private const string FILE = "MemberRepository.json";

        private void SaveElements(IEnumerable<Member> elements)
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
        private List<Member> LoadElements()
        {
            IEnumerable<Member>? elements = GetAll();

            List<Member> list;
            if (elements == null)
                list = new List<Member>();
            else
                list = elements.ToList();
            return list;
        }
        public bool DeleteOne(Member element)
        {
            List<Member> elements = LoadElements();
            Member? selectedElement = elements.Find(s => s.Id == element.Id);
            if (selectedElement == null)
                return false;
            elements.Remove(selectedElement);
            SaveElements(elements);
            return true;
        }

        public bool Exist(Member element) => LoadElements().Exists(s => s.Id == element.Id);

        public IEnumerable<Member>? GetAll()
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
                    ? new List<Member>() : JsonConvert.DeserializeObject<IEnumerable<Member>>(jsonObject);
            }
            catch
            {
                return new List<Member>();
            }
        }

        public bool SaveOne(Member element)
        {
            try
            {
                List<Member> elementrs = LoadElements(); //new List<BsonDocument>(); //
                Member? selectedElement = elementrs.Find(s => s.Id == element.Id);
                if (selectedElement != null)
                {
                    selectedElement.Update(element);
                }
                else
                {
                    elementrs.Add(element);
                }
                SaveElements(elementrs);
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
