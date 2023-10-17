using joocBot.Albion;

namespace TestBot
{
    [TestClass]
    public class UnitTest1
    {
        #region ChatBotRepository test
        [TestMethod]
        public void TestMethod1()
        {
            var chatBot = new ChatBot() { Token = "1234" };
            var chatBotRepository = new ChatBotRepository();
            chatBotRepository.SaveToken(chatBot);
        }
        [TestMethod]
        public void TestMethod2()
        {
            var chatBotRepository = new ChatBotRepository();
            var chatBot = chatBotRepository.GetToken();

            Console.WriteLine(chatBot.Token);
        }
        [TestMethod]
        public void TestMethod3()
        {
            var chatBotRepository = new ChatBotRepository();
            var valid = chatBotRepository.Vaild("1234");
            Assert.IsTrue(valid);
        }
        #endregion

        #region AlbionApiRequestor test
        [TestMethod]
        public void TestAlbionMethod1()
        {
            var albionApi = new AlbionApiRequestor()
            {
                Region = RegionCode.Eastern
            };

            //var apiResponse = albionApi.SearchUsername("ChingChangChong");
            var apiResponse = albionApi.SearchUsername("mono1y");
        }
        #endregion
    }
}