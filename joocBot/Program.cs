using Discord;
using Discord.Commands;
using Discord.WebSocket;
using joocBot.Albion;
using joocBot.Repositories;
using System;
using System.Threading.Tasks;

namespace DiscordBot
{
    class Program
    {
        DiscordSocketClient? client; //봇 클라이언트
        CommandService? commands;    //명령어 수신 클라이언트
        ChatBotRepository? _chatBot;
        /// <summary>
        /// 프로그램의 진입점
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            new Program().BotMain().GetAwaiter().GetResult();   //봇의 진입점 실행
        }

        /// <summary>
        /// 봇의 진입점, 봇의 거의 모든 작업이 비동기로 작동되기 때문에 비동기 함수로 생성해야 함
        /// </summary>
        /// <returns></returns>
        public async Task BotMain()
        {
            client = new DiscordSocketClient(new DiscordSocketConfig()
            {    //디스코드 봇 초기화
                LogLevel = LogSeverity.Verbose                              //봇의 로그 레벨 설정 
            });
            commands = new CommandService(new CommandServiceConfig()        //명령어 수신 클라이언트 초기화
            {
                LogLevel = LogSeverity.Verbose                              //봇의 로그 레벨 설정
            });
            _chatBot = new ChatBotRepository();                             //봇의 토큰 가져오기
            var token = _chatBot.GetToken().Token;

            //로그 수신 시 로그 출력 함수에서 출력되도록 설정
            client.Log += OnClientLogReceived;
            commands.Log += OnClientLogReceived;

            await client.LoginAsync(TokenType.Bot, token); //봇의 토큰을 사용해 서버에 로그인
            await client.StartAsync();                         //봇이 이벤트를 수신하기 시작

            client.MessageReceived += OnClientMessage;         //봇이 메시지를 수신할 때 처리하도록 설정

            await Task.Delay(-1);   //봇이 종료되지 않도록 블로킹
        }

        private async Task OnClientMessage(SocketMessage arg)
        {
            //수신한 메시지가 사용자가 보낸 게 아닐 때 취소
            var message = arg as SocketUserMessage;
            if (message == null) return;

            int pos = 0;

            string returnMessage = string.Empty;
            var userMessage = message.Content.Replace(client.CurrentUser.Mention, string.Empty);

            //메시지 앞에 !이 달려있지 않고, 자신이 호출된게 아니거나 다른 봇이 호출했다면 취소
            var isPrefix = userMessage.StartsWith(" /");
            var isBotCallAsSelf = message.HasMentionPrefix(client.CurrentUser, ref pos);
            var isAuthor = message.Author.IsBot;

            if (!isPrefix || !isBotCallAsSelf || isAuthor)
                return;

            var prefix = userMessage.Substring(2).Split(' ')[0];
            var postfix = userMessage.Substring(2).Split(' ')[1];

            // case insensitive
            switch (prefix.ToLower())
            {
                case "echo":case "e":
                case "따라해":
                case "앵무새":
                    returnMessage = userMessage.Substring(2 + prefix.Length);
                    break;
                case "killlog":
                case "k":

                    using(AlbionApiRequestor requestor = new AlbionApiRequestor())
                    {
                        returnMessage = requestor.SearchUsername(postfix);
                    }

                    //returnMessage = "대략 킬로그를 불러온다.";
                    break;
                case "help":
                case "h":
                case "도움말":
                case "하이구글리":
                    returnMessage = "도움말 불러오기";
                    break;
                default:
                    returnMessage = "존재하지 않는 명령어 도움말은 /help, /h ,/도움말, /하이구글리";
                    break;
            }


            var context = new SocketCommandContext(client, message);                    //수신된 메시지에 대한 컨텍스트 생성   

            await context.Channel.SendMessageAsync($"명령어 수신됨 - {returnMessage}"); //수신된 명령어를 다시 보낸다.
        }

        /// <summary>
        /// 봇의 로그를 출력하는 함수
        /// </summary>
        /// <param name="msg">봇의 클라이언트에서 수신된 로그</param>
        /// <returns></returns>
        private Task OnClientLogReceived(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());  //로그 출력
            return Task.CompletedTask;
        }
    }
}