using Discord;
using Discord.Commands;
using Discord.WebSocket;
using joocBot.Albion;
using joocBot.Models;
using joocBot.Repositories;
using System;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace DiscordBot
{
    class Program
    {
        private DiscordSocketClient? _client; //봇 클라이언트
        private CommandService? _commands;    //명령어 수신 클라이언트
        private ChatBotRepository? _chatBot;
        private AlbionQueryManager? _lbionQueryManager;
        private List<SubscribedChannel> _SubscribedChannelList = new List<SubscribedChannel>();
        private bool _isMessageShow;
        private bool _isEmbedShow;
        private bool _isSubEmbedShow;
        /// <summary>
        /// 프로그램의 진입점
        /// </summary>
        /// <param name="args"></param>
        static void Main()
        {
            new Program().BotMain().GetAwaiter().GetResult();   //봇의 진입점 실행
        }

        /// <summary>
        /// 봇의 진입점, 봇의 거의 모든 작업이 비동기로 작동되기 때문에 비동기 함수로 생성해야 함
        /// </summary>
        /// <returns></returns>
        public async Task BotMain()
        {
            _client = new DiscordSocketClient(new DiscordSocketConfig()
            {    //디스코드 봇 초기화
                LogLevel = LogSeverity.Verbose                              //봇의 로그 레벨 설정 
            });
            _commands = new CommandService(new CommandServiceConfig()        //명령어 수신 클라이언트 초기화
            {
                LogLevel = LogSeverity.Verbose                              //봇의 로그 레벨 설정
            });
            _chatBot = new ChatBotRepository();                             //봇의 토큰 가져오기
            var token = _chatBot.GetToken().Token;

            _lbionQueryManager = new AlbionQueryManager();                  //알비온 쿼리매니저 생성

            //로그 수신 시 로그 출력 함수에서 출력되도록 설정
            _client.Log += OnClientLogReceived;
            _commands.Log += OnClientLogReceived;

            await _client.LoginAsync(TokenType.Bot, token); //봇의 토큰을 사용해 서버에 로그인
            await _client.StartAsync();                         //봇이 이벤트를 수신하기 시작

            _client.MessageReceived += OnClientMessage;         //봇이 메시지를 수신할 때 처리하도록 설정
            
            await Task.Delay(-1);   //봇이 종료되지 않도록 블로킹
        }
        // Disable the warning.
        #pragma warning disable CS8602
        private async Task OnClientMessage(SocketMessage arg)
        {
            //수신한 메시지가 사용자가 보낸 게 아닐 때 취소
            //var message = arg as SocketUserMessage;
            //if (message == null) return;

            //수신한 메시지가 사용자가 보낸 게 아닐 때 취소
            if (arg is not SocketUserMessage message) return;

            int pos = 0;

            string returnMessage = string.Empty;
            var embed = new EmbedBuilder
            {
                // Embed property can be set within object initializer
                Title = "Empty embed",
                Description = "I am a description set by initializer."
            };
            var subEmbed = new EmbedBuilder();

            var userMessage = message.Content.Replace(_client.CurrentUser.Mention, string.Empty);

            //메시지 앞에 !이 달려있지 않고, 자신이 호출된게 아니거나 다른 봇이 호출했다면 취소
            var isPrefix = userMessage.StartsWith(" /");
            var isBotCallAsSelf = message.HasMentionPrefix(_client.CurrentUser, ref pos);
            var isAuthor = message.Author.IsBot;

            if (!isPrefix || !isBotCallAsSelf || isAuthor)
                return;

            // 메시지 커맨드와 파라미터 자르기
            string command = string.Empty;
            string param = string.Empty;
            if (userMessage[2..].Contains(' '))
            {
                command = userMessage[2..].Split(' ')[0];
                param = userMessage[2..].Split(' ')[1];
            }
            else
                command = userMessage[2..];

            _isEmbedShow = false;
            _isMessageShow = false;
            _isSubEmbedShow = false;
            // case insensitive
            var context = new SocketCommandContext(_client, message);                   //수신된 메시지에 대한 컨텍스트 생성   
            var channelid = context.Channel.Id;

            switch (command.ToLower())
            {
                case "e":case "echo":case "에코":case "테스트":
                    var offset = 2 + command.Length;
                    returnMessage = GetEchoMessage(userMessage, offset);
                    _isMessageShow = true;
                    break;
                case "h":case "help":case "도움말":case "하이구글리":
                    returnMessage = GetHelpMessage();
                    _isMessageShow = true;
                    break;
                case "k":case "killlog": case "킬": case "킬로그":
                    var id = _lbionQueryManager.ConvertNameToIdOne(param);
                    var killEvent = _lbionQueryManager.SearchPlayersRecentEvent(param);
                    var inventoryCount = killEvent.Victim.Inventory?.Count(item => item != null);
                    embed = GetEventMessage(killEvent, id);
                    subEmbed = (inventoryCount > 0) ? GetSubEventMessage(killEvent, id) : new EmbedBuilder();
                    _isSubEmbedShow = (inventoryCount > 0)? true:false; _isMessageShow = true; _isEmbedShow = true;
                    break;
                case "subscribe":case "구독":
                    var isRegistered = _SubscribedChannelList.Exists(channel => channel.Id == channelid);
                    if (!isRegistered)
                        _SubscribedChannelList.Add(new SubscribedChannel 
                        { 
                            Id = channelid,
                            IsAuthorized = false,
                            IsSubscribed = false,
                            Name = context.Channel.Name,
                            Description = string.Empty,
                        });

                    _ = ExecuteSubscription(channelid, context);
                    returnMessage = $"[{context.Guild.Name}]#{context.Channel.Name}:{channelid}에서 이벤트 구독을 시작합니다.";
                    _isMessageShow = true;
                    break;
                case "unsubscribe": case "구독중지":
                    _ = TerminateSubscription(channelid, context);
                    returnMessage = $"[{context.Guild.Name}]#{context.Channel.Name}:{channelid}에서 이벤트 구독을 중지합니다.";
                    _isMessageShow = true;
                    break;
                case "server":case "서버":
                    returnMessage = string.IsNullOrWhiteSpace(param)?_lbionQueryManager.GetRegion(): _lbionQueryManager.SetRegion(param);
                    _isMessageShow = true; _isEmbedShow = false;
                    break;
                case "search":case "검색":
                    returnMessage = GetPlayerInfos(param);
                    _isMessageShow = true; _isEmbedShow = false;
                    break;
                default:
                    returnMessage = "존재하지 않는 명령어임! \n도움말은 /h, /help, /도움말, /하이구글리 \n";
                    _isMessageShow = true;
                    break;
            }

            if (_isMessageShow)
                await context.Channel.SendMessageAsync($"명령어 수신됨 - {returnMessage}"); //수신된 명령어를 다시 보낸다.
            //Your embed needs to be built before it is able to be sent
            if (_isEmbedShow)
                await context.Channel.SendMessageAsync(embed: embed.Build());
            if (_isSubEmbedShow)
                await context.Channel.SendMessageAsync(embed: subEmbed.Build());
        }
        private async Task ExecuteSubscription(ulong key, SocketCommandContext context)
        {
            var subscriptionInfo = _SubscribedChannelList.FirstOrDefault(item => item.Id == key);
            if (subscriptionInfo == null)
                return;
            await Task.Run(async () =>
            {
                while (subscriptionInfo.IsSubscribed == true)
                {
                    await context.Channel.SendMessageAsync($"구독중... {subscriptionInfo.Name}");
                    Thread.Sleep(5000);
                }
                //await context.Channel.SendMessageAsync($"구독중지. {subscriptionInfo.Name}");
            });
        }
        private async Task TerminateSubscription(ulong key, SocketCommandContext context)
        {
            var subscriptionInfo = _SubscribedChannelList.FirstOrDefault(item => item.Id == key);
            if (subscriptionInfo == null)
                return;
            await context.Channel.SendMessageAsync($"구독을 정지합니다. {subscriptionInfo.Name}");
            subscriptionInfo.IsSubscribed = false;
        }

        private string GetPlayerInfos(string lookupName)
        {
            var playerList = new StringBuilder();
            foreach (var item in _lbionQueryManager.SearchPlayers(lookupName))
                playerList.AppendLine($"{item.Id} : [{item.GuildName}]{item.Name} K/D({item.KillFame}/{item.DeathFame}) Ratio({item.FameRatio})");
            var result = $"\n ## **Player List**: \n ```\n{playerList}``` ";
            if (playerList.Length == 0)
                result = "```검색결과 찾을 수 없음.```";
            return result;
        }
        private string GetEchoMessage(string param, int offset)
        {
            return param[(offset)..];
        }
        private string GetHelpMessage()
        {
            return "도움말 불러오기"; ////var inventory = GetInventoryItems(battleEvent.Victim.Inventory);
        }
        private EmbedBuilder GetSubEventMessage(BattleEvent battleEvent, string id)
        {
            string title;
            Color color;
            string footerText = "Powered by GooglyMoogly";
            string server;
            string fieldTitle = $"팬티 털어서 나온것들: ";
            string fieldValue = $"총 {battleEvent.Victim.Inventory?.Count(item => item != null)}개 품목";
            string description = GetInventoryItems(battleEvent.Victim.Inventory).ToString();

            if (_lbionQueryManager.GetRegion() == "Eastern")
                server = "live_sgp";
            else
                server = "live";

            string url = $"https://albiononline.com/killboard/kill/{battleEvent.EventId}?server={server}";

            if (battleEvent.Killer.Id == id)
            {
                color = Color.Green;
                title = $"[{battleEvent.Killer.GuildName}] {battleEvent.Killer.Name}님이 [{battleEvent.Victim.GuildName}] {battleEvent.Victim.Name}를 죽임.";
            }
            else
            {
                color = Color.Red;
                title = $"[{battleEvent.Victim.GuildName}] {battleEvent.Victim.Name}님이 [{battleEvent.Killer.GuildName}] {battleEvent.Killer.Name}한테 당함.";
            }

            var embed = new EmbedBuilder
            {
                // Embed property can be set within object initializer
                Title = "**노획한 아이템 리스트**",
                Description = "I am a description set by initializer."
            };

            embed.AddField(fieldTitle, fieldValue)
                .WithFooter(footer => footer.Text = footerText)
                .WithColor(color)
                .WithImageUrl("https://render.albiononline.com/v1/item/T8_BAG@0.png?count=1&quality=0")
                .WithDescription(description)
                .WithCurrentTimestamp();

            return embed;
        }
        private EmbedBuilder GetEventMessage(BattleEvent battleEvent, string id) 
        {
            string title;
            Color color;
            string footerText = "Powered by GooglyMoogly";
            string server;
            string fieldTitle = $"킬명성: {battleEvent.TotalVictimKillFame.ToString()}";
            string fieldContext = GetFiledContext(battleEvent);
            string description;

            if (_lbionQueryManager.GetRegion() == "Eastern")
                server = "live_sgp";
            else
                server = "live";

            string url = $"https://albiononline.com/killboard/kill/{battleEvent.EventId}?server={server}";

            if (battleEvent.Killer.Id == id)
            {
                color = Color.Green;
                title = $"[{battleEvent.Killer.GuildName}] {battleEvent.Killer.Name}님이 [{battleEvent.Victim.GuildName}] {battleEvent.Victim.Name}를 죽임.";
                description = "\n사망! 머더퍼커!\n";
            }
            else
            {
                description = "\n이런날도있는거죠...\n";
                color = Color.Red;
                title = $"[{battleEvent.Victim.GuildName}] {battleEvent.Victim.Name}님이 [{battleEvent.Killer.GuildName}] {battleEvent.Killer.Name}한테 당함.";
            }

            var embed = new EmbedBuilder
            {
                // Embed property can be set within object initializer
                Title = "I am a Title set by initializer.",
                Description = "I am a description set by initializer."
            };

            embed.AddField(fieldTitle, description)
                .WithFooter(footer => footer.Text = footerText)
                .WithColor(color)
                .WithTitle(title)
                .WithImageUrl("https://avatars.githubusercontent.com/u/15712519?v=4")
                .WithDescription(fieldContext)
                .WithUrl(url)
                .WithCurrentTimestamp();

            return embed;
        }

        private string GetGearImageUrl(string? name,int count, int quality)
        {
            return $"https://render.albiononline.com/v1/item/{name}.png?count={count}&quality={quality}";
        }
        private string GetFiledContext(BattleEvent battleEvent)
        {
            var killersGear = GetEquipmentContext(battleEvent.Killer.Equipment);
            var victimsGear = GetEquipmentContext(battleEvent.Victim.Equipment);
            //var inventory = GetInventoryItems(battleEvent.Victim.Inventory);

            Func<string, string> Nvl = delegate (string name)
            {
                return string.IsNullOrEmpty(name)?"없음":name;
            };

            var killerAllianceName = Nvl(battleEvent.Killer.AllianceName);
            var killerGuildName = Nvl(battleEvent.Killer.GuildName);
            var victimAllianceName = Nvl(battleEvent.Victim.AllianceName);
            var victimGuildName = Nvl(battleEvent.Victim.GuildName);

            var killer = $"{Environment.NewLine}길드: [{killerAllianceName}]{killerGuildName}{Environment.NewLine}유저: **{battleEvent.Killer.Name}**  :crossed_swords: {Environment.NewLine}IP: {battleEvent.Killer.AverageItemPower}{Environment.NewLine}";
            var victim = $"{Environment.NewLine}길드: [{victimAllianceName}]{victimGuildName}{Environment.NewLine}유저: **{battleEvent.Victim.Name}**  :cry: {Environment.NewLine}IP: {battleEvent.Victim.AverageItemPower}{Environment.NewLine}";
            //var inventoryCount = $"{Environment.NewLine}센터까서 나온 아이템 목록: {Environment.NewLine}";

            var result = killer + killersGear.ToString() + victim + victimsGear.ToString();// + inventoryCount + inventory.ToString();
            return result;
        }
        private StringBuilder GetInventoryItems(Gear[]? inventory)
        {
            var gears = new StringBuilder();
            string? gearName;
            string? gearURL;
            int gearCount;
            int gearQuality;

            foreach (var gear in inventory?.Select((g, i) => new { Value = g, Index = i })) 
            {
                if (gear.Value != null)
                {
                    gearCount = gear.Value.Count;
                    gearQuality = gear.Value.Quality;
                    gearName = gear.Value.Type;
                    gearURL = GetGearImageUrl(gearName, gearCount, gearQuality);
                    gears.AppendLine($"[{gear.Index}]번째 칸: [{gearName}]({gearURL}) 품질:{gearQuality} 수량:{gearCount}");
                }
            }
            gears.AppendLine($"-----이하 비었음-----");

            return gears;
        }
        private StringBuilder GetEquipmentContext(Equipment? equipment)
        {
            string? gearName;
            string? gearURL;
            int gearCount;
            int gearQuality;
            var gears = new StringBuilder();

            if (equipment.MainHand != null)
            {
                gearCount = equipment.MainHand.Count;
                gearQuality = equipment.MainHand.Quality;
                gearName = equipment.MainHand.Type;
                gearURL = GetGearImageUrl(gearName, gearCount, gearQuality); 
                gears.AppendLine($"MainHand: [{gearName}]({ gearURL})");
            }
            else
                gears.AppendLine($"MainHand: [없음]");

            if (equipment.OffHand != null)
            {
                gearCount = equipment.OffHand.Count;
                gearQuality = equipment.OffHand.Quality;
                gearName = equipment.OffHand.Type;
                gearURL = GetGearImageUrl(gearName, gearCount, gearQuality); 
                gears.AppendLine($"OffHand: [{gearName}]({ gearURL})");
            }
            else
                gears.AppendLine($"OffHand: [없음]");

            if (equipment.Head != null)
            {
                gearCount = equipment.Head.Count;
                gearQuality = equipment.Head.Quality;
                gearName = equipment.Head.Type;
                gearURL = GetGearImageUrl(gearName, gearCount, gearQuality); 
                gears.AppendLine($"Head: [{gearName}]({ gearURL})");
            }
            else
                gears.AppendLine($"Head: [없음]");

            if (equipment.Armor != null) 
            {
                gearCount = equipment.Armor.Count;
                gearQuality = equipment.Armor.Quality;
                gearName = equipment.Armor.Type;
                gearURL = GetGearImageUrl(gearName, gearCount, gearQuality); 
                gears.AppendLine($"Armor: [{gearName}]({ gearURL})");
            }
            else
                gears.AppendLine($"Armor: [없음]");

            if (equipment.Shoes != null)
            {
                gearCount = equipment.Shoes.Count;
                gearQuality = equipment.Shoes.Quality;
                gearName = equipment.Shoes.Type;
                gearURL = GetGearImageUrl(gearName, gearCount, gearQuality); 
                gears.AppendLine($"Shoes: [{gearName}]({ gearURL})");
            }
            else
                gears.AppendLine($"Shoes: [없음]");

            if (equipment.Cape != null)
            {
                gearCount = equipment.Cape.Count;
                gearQuality = equipment.Cape.Quality;
                gearName = equipment.Cape.Type;
                gearURL = GetGearImageUrl(gearName, gearCount, gearQuality); 
                gears.AppendLine($"Cape: [{gearName}]({ gearURL})");
            }
            else
                gears.AppendLine($"Cape: [없음]");

            if (equipment.Potion != null)
            {
                gearCount = equipment.Potion.Count;
                gearQuality = equipment.Potion.Quality;
                gearName = equipment.Potion.Type;
                gearURL = GetGearImageUrl(gearName, gearCount, gearQuality); 
                gears.AppendLine($"Potion: [{gearName}]({ gearURL})");
            }
            else
                gears.AppendLine($"Potion: [없음]");
            if (equipment.Food != null)
            {
                gearCount = equipment.Food.Count;
                gearQuality = equipment.Food.Quality;
                gearName = equipment.Food.Type;
                gearURL = GetGearImageUrl(gearName, gearCount, gearQuality); 
                gears.AppendLine($"Food: [{gearName}]({ gearURL})");
            }
            else
                gears.AppendLine($"Food: [없음]");
            if (equipment.Mount != null)
            {
                gearCount = equipment.Mount.Count;
                gearQuality = equipment.Mount.Quality;
                gearName = equipment.Mount.Type;
                gearURL = GetGearImageUrl(gearName, gearCount, gearQuality); 
                gears.AppendLine($"Mount: [{gearName}]({ gearURL})");
            }
            else
                gears.AppendLine($"Mount: [없음]");
            if (equipment.Bag != null)
            {
                gearCount = equipment.Bag.Count;
                gearQuality = equipment.Bag.Quality;
                gearName = equipment.Bag.Type;
                gearURL = GetGearImageUrl(gearName, gearCount, gearQuality); 
                gears.AppendLine($"Bag: [{gearName}]({ gearURL})");
            }
            else
                gears.AppendLine($"Bag: [없음]");
            return gears;
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
