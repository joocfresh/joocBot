﻿using Discord;
using Discord.Commands;
using Discord.WebSocket;
using joocBot.Albion;
using joocBot.Models;
using joocBot.Repositories;
using Markdig;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Net;
using System.Reflection.Metadata.Ecma335;
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

            //프로그램 구동시 채널별 구독상태초기화
            var channelRepository = new SubscribedChannelRepository();
            var subscribedChannelList = channelRepository.GetAll()?.ToList();
            if (subscribedChannelList != null && subscribedChannelList.Count != 0)
            {
                foreach (var item in subscribedChannelList)
                {
                    item.IsSubscribed = false;
                    channelRepository.SaveOne(item);
                }
            }

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
                case "p":case "patch":case "패치": case"패치노트":
                    returnMessage = GetHelpPatchNote();
                    _isMessageShow = true;
                    break;
                case "k":case "killlog": case "킬": case "킬로그":
                    var id = _lbionQueryManager.ConvertNameToIdOne(param);
                    var killEvent = _lbionQueryManager.SearchPlayersRecentEvent(param);
                    var inventoryCount = killEvent.Victim.Inventory?.Count(item => item != null);
                    embed = GetEventMessage(killEvent, id, context);
                    subEmbed = (inventoryCount > 0) ? GetSubEventMessage(killEvent, id) : new EmbedBuilder();
                    _isSubEmbedShow = (inventoryCount > 0) ? true : false;
                    //_isSubEmbedShow = (inventoryCount > 0)? true:false; _isMessageShow = true; _isEmbedShow = true;
                    break;
                case "subscribe":case "구독":
                    var channelRepository = new SubscribedChannelRepository();
                    var subscribedChannelList = channelRepository.GetAll()?.ToList();
                    //채널이 등록안되어 있으면 등록대기
                    var isRegistered = subscribedChannelList.Exists(channel => channel.Id == channelid);
                    if (!isRegistered)
                        subscribedChannelList.Add(new SubscribedChannel 
                        { 
                            Id = channelid,
                            IsAuthorized = false, //허가 떨어지면 구독가능
                            IsSubscribed = false, //구독에 대한 트리거
                            Name = $"[{context.Guild.Name}]{context.Channel.Name}",
                            Description = string.Empty,
                        });
                    var thisChannel = subscribedChannelList.FirstOrDefault(channel => channel.Id == channelid);
                    thisChannel = (thisChannel==null) ? new SubscribedChannel() : thisChannel;
                    if (thisChannel != null)
                    {
                        if (thisChannel.IsAuthorized)
                        {
                            if (!thisChannel.IsSubscribed)
                            {
                                thisChannel.IsSubscribed = true;
                                _ = channelRepository.SaveALL(subscribedChannelList);
                                _ = ExecuteSubscription(channelid, context);
                                returnMessage = $"[{context.Guild.Name}]#{context.Channel.Name}:{channelid}에서 이벤트 구독을 시작합니다.";
                            }
                            else
                            {
                                returnMessage = $"이미 구독중 입니다.";
                            }
                        }
                        else
                        {
                            returnMessage = $"[{context.Guild.Name}]#{context.Channel.Name}:{channelid} 등록 대기중이거나 허가되지 않은 채널입니다. 관리자에게 문의하세요.";
                            _ = channelRepository.SaveALL(subscribedChannelList);
                        }
                    }
                    else
                    {
                        returnMessage = $"[{context.Guild.Name}]#{context.Channel.Name}:{channelid} 채널이 등록되지 않았습니다.";
                        _ = channelRepository.SaveALL(subscribedChannelList);
                    }
                    _isMessageShow = true;
                    
                    break;
                case "unsubscribe": case "구독중지": case "구독취소":
                    TerminateSubscription(channelid, context);
                    returnMessage = $"[{context.Guild.Name}]#{context.Channel.Name}:{channelid}에서 이벤트 구독을 중지합니다.";
                    _isMessageShow = true;
                    break;
                case "clear": case "disband": case "청소": case "디스밴드":
                    var bandRepository = new MemberRepository();
                    var items = bandRepository.GetAll()?.ToList().Where(member => member.DiscordName == context.Channel.Id.ToString()).ToList();
                    foreach (var item in items)
                    {
                        bandRepository.DeleteOne(item);
                    }
                    returnMessage = $"이 채널의 모든 등록을 취소합니다.";
                    _isMessageShow = true;
                    break;
                case "d": case "delete":case "제명": case"제거":
                    if (string.IsNullOrEmpty(param))
                    {
                        returnMessage = $"플레이어명을 입력하세요.";
                    }
                    else
                    {
                        var memberRepository = new MemberRepository();
                        var members = memberRepository.GetAll()?.ToList().Where(member => member.DiscordName == context.Channel.Id.ToString()).ToList();
                        var member = members?.FirstOrDefault(one =>one.PlayerName == param);
                        memberRepository.DeleteOne(member);
                      
                        returnMessage = $" **{param}**이 제명 되었습니다.\n\n";

                        members = memberRepository.GetAll()?.ToList().Where(member => member.DiscordName == context.Channel.Id.ToString()).ToList();
                        var memberList = new StringBuilder();
                        memberList.AppendLine("```");
                        foreach (var item in members)
                            memberList.AppendLine($"- {item.PlayerId} | {item.PlayerName}");
                        memberList.AppendLine("```");

                        await context.Channel.SendMessageAsync($"구독자 리스트\n{memberList}");
                    }
                    
                    _isMessageShow = true;
                    break;
                case "r": case "register": case "등록":
                    if (string.IsNullOrEmpty(param))
                    {
                        var memberRepository = new MemberRepository();
                        var members = memberRepository.GetAll()?.ToList().Where(member => member.DiscordName == context.Channel.Id.ToString()).ToList();
                        var memberList = new StringBuilder();
                        memberList.AppendLine("```");
                        foreach (var member in members)
                            memberList.AppendLine($"- {member.PlayerId} | {member.PlayerName}");
                        memberList.AppendLine("```");

                        await context.Channel.SendMessageAsync($"구독자 리스트\n{memberList}");
                    }
                    else
                    {
                        var memberRepository = new MemberRepository();
                        id = _lbionQueryManager.ConvertNameToIdOne(param);
                        var guildName = _lbionQueryManager.ConvertNameToGuildOne(param);
                        memberRepository.SaveOne(new Member()
                        {
                            Id = Guid.NewGuid().ToString(),
                            PlayerId = id,
                            PlayerName = param,
                            GuildName = guildName,
                            DiscordName = context.Channel.Id.ToString()
                        });

                        var members = memberRepository.GetAll()?.Select(member =>
                        {
                            member.DiscordName = context.Channel.Id.ToString();
                            return member;
                        }).ToList();
                        var memberList = new StringBuilder();
                        memberList.AppendLine("```");
                        foreach (var member in members)
                            memberList.AppendLine($"- {member.PlayerId} | {member.PlayerName}");
                        memberList.AppendLine("```");

                        await context.Channel.SendMessageAsync($"구독자 리스트\n{memberList}");
                    }
                    break;
                case "server":case "서버":
                    returnMessage = string.IsNullOrWhiteSpace(param)?_lbionQueryManager.GetRegion(): _lbionQueryManager.SetRegion(param);
                    _isMessageShow = true; _isEmbedShow = false;
                    break;
                case "channel": case "채널":
                    returnMessage = $"현재 채널은 [{context.Guild.Name}]#{context.Channel.Name}({channelid}) 입니다.";
                    _isMessageShow = true;
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
            var subscriptionInfo = new SubscribedChannelRepository().GetAll()?.FirstOrDefault(item => item.Id == key);
            if (subscriptionInfo == null)
                return;
            await Task.Run(async () =>
            {
                while (subscriptionInfo.IsSubscribed == true)
                {
                    var memberRepository = new MemberRepository();
                    var members = memberRepository.GetAll()?.ToList().Where(member => member.DiscordName == context.Channel.Id.ToString()).ToList();
                    var memberList = new StringBuilder();
                    
                    foreach (var member in members)
                    {
                        var killEvents = _lbionQueryManager.SearchPlayersEventQueue(member.PlayerName, member.PlayerId);
                        foreach (var element in killEvents)
                        {
                            if (element.BattleId <= member.LastKillEvent || element.BattleId == 0)
                                continue;
                            else
                                member.LastKillEvent = element.BattleId;
                            var inventoryCount = element.Victim.Inventory?.Count(item => item != null);

                            var embed = GetEventMessage(element, member.PlayerId, context);
                            //await context.Channel.SendMessageAsync(embed: embed.Build());
                            if (inventoryCount > 0)
                            {
                                //Thread.Sleep(50);
                                EmbedBuilder? subEmbed = GetSubEventMessage(element, member.PlayerId);
                                await context.Channel.SendMessageAsync(embed: subEmbed.Build());
                            }
                            Thread.Sleep(1500);
                        }

                        //var killEvent = _lbionQueryManager.SearchPlayersRecentEvent(member.PlayerName,member.PlayerId);
                        //if (killEvent.BattleId == member.LastKillEvent || killEvent.BattleId == 0)
                        //    continue;
                        //else
                        //    member.LastKillEvent = killEvent.BattleId;
                        memberRepository.SaveOne(member);
                        Thread.Sleep(2000);
                    }

                    //await context.Channel.SendMessageAsync($"구독중... {subscriptionInfo.Name}\n{memberList}");
                    //memberRepository.SaveALL(members);
                    subscriptionInfo = new SubscribedChannelRepository().GetAll()?.FirstOrDefault(item => item.Id == key);
                    Thread.Sleep(5000);
                }
                //await context.Channel.SendMessageAsync($"구독중지. {subscriptionInfo.Name}");
            });
        }
        private void TerminateSubscription(ulong key, SocketCommandContext context)
        {
            var channelRepository = new SubscribedChannelRepository();
            var subscriptionInfo = channelRepository.GetAll()?.FirstOrDefault(item => item.Id == key);
            if (subscriptionInfo == null)
                return;
            subscriptionInfo.IsSubscribed = false;
            channelRepository.SaveOne(subscriptionInfo);
        }
        private void RegisterChannel()
        {

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
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    return Environment.NewLine + "[구글리의 깃허브에서 사용법 보기](https://github.com/joocfresh/joocBot/blob/joocfresh-patch-1/Help.md) 링크를 클릭하고 구글리의 깃허브로 놀러와서 별주세요.";
                        //client.GetStringAsync("https://raw.githubusercontent.com/joocfresh/joocBot/main/Help.md").Result;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                    return ex.Message;
                }
            }
            //return "도움말 불러오기 \n"+ File.ReadAllText(@"project/Help.md");
        }
        private string GetHelpPatchNote()
        {
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    return Environment.NewLine + client.GetStringAsync("https://raw.githubusercontent.com/joocfresh/joocBot/main/PatchNote.md").Result;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                    return ex.Message;
                }
            }
            //return "패치노트: \n" + File.ReadAllText(@"project/PatchNote.md");
        }
        private EmbedBuilder GetSubEventMessage(BattleEvent battleEvent, string id)
        {
            string title;
            Discord.Color color;
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
                color = Discord.Color.Green;
                title = $"[{battleEvent.Killer.GuildName}] {battleEvent.Killer.Name}님이 [{battleEvent.Victim.GuildName}] {battleEvent.Victim.Name}를 죽임.";
            }
            else
            {
                color = Discord.Color.Red;
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
        private EmbedBuilder GetEventMessage(BattleEvent battleEvent, string id , SocketCommandContext context) 
        {
            string title;
            Discord.Color color;
            string footerText = "Powered by GooglyMoogly";
            string server;
            string fieldTitle = $"￡킬명성: {battleEvent.TotalVictimKillFame.ToString()}";
            string fieldContext = GetFiledContext(battleEvent);
            string description;

            if (_lbionQueryManager.GetRegion().Contains("Eastern"))
                server = "live_sgp";
            else
                server = "live";

            string url = $"https://albiononline.com/killboard/kill/{battleEvent.EventId}?server={server}";

            var imagePaths = GetImagePaths(battleEvent);
            //var killboardImage = 
            var killboardImagePath = GetKillboardImage(imagePaths, battleEvent);
            
            var channelid = context.Channel.Id;
            var guildid = context.Guild.Id;
            // https://media.discordapp.net/attachments/1177166136375259196/1210461696825753610/196415874.png?ex=65eaa54d&is=65d8304d&hm=c8d4ccbe437c73856c9d9f9a4eaeab05c1accc4bbc0707e10e523e674eea1d6c&=&format=webp&quality=lossless&width=572&height=384
            var kGuildName = string.IsNullOrEmpty(battleEvent.Killer.GuildName) ? string.Empty : $"[{battleEvent.Killer.GuildName}]";
            var vGuildName = string.IsNullOrEmpty(battleEvent.Victim.GuildName) ? string.Empty : $"[{battleEvent.Victim.GuildName}]";

            if (battleEvent.Killer.Id == id)
            {
                color = Discord.Color.Green;
                title = $"{kGuildName}{battleEvent.Killer.Name}님이 {vGuildName}{battleEvent.Victim.Name}를 죽임.";
                description = "\n\n사망! 머더퍼커!\n";
            }
            else
            {
                description = "\n\n이런날도있는거죠 뭐...\n";
                color = Discord.Color.Red;
                title = $"{vGuildName}{battleEvent.Victim.Name}님이 {kGuildName}{battleEvent.Killer.Name}한테 당함.";
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

            context.Channel.SendFileAsync(killboardImagePath, "이벤트발생 - ", false, embed.Build());
            return embed;
        }

        private string GetKillboardImage(string[] imagePaths, BattleEvent battleEvent)
        {
            string eventId = battleEvent.EventId.ToString();
            var imagePath = $"./screenshot/{eventId}.png";
            try
            {
                //백그라운드 이미지 경로
                string gearImagePath = "https://render.albiononline.com/v1/item/T8_BAG@0.png?count=1&quality=0";
                string backgroundImagePath = "https://assets.albiononline.com/assets/images/killboard/gear.png?u91ddb7ca";
                string victimImagePath = "https://cdn2.iconfinder.com/data/icons/military-31/200/blood_kill_mark_splatter_kills_murder-512.png";
                string fameImagePath = "https://assets.albiononline.com/assets/images/killboard/fame-list__icons.png?u91ddb7ca";

                // 이미지 및 백그라운드 이미지 로딩
                var images = new System.Drawing.Image[imagePaths.Length];
                for (int i = 0; i < imagePaths.Length; i++)
                {
                    images[i] = LoadImage(imagePaths[i]);
                }
                var gearImage = LoadImage(gearImagePath);
                var backgroundImage = LoadImage(backgroundImagePath);
                var victimImage = LoadImage(victimImagePath);
                var fameImage = LoadImage(fameImagePath);

                // 투명도 조절
                float opacity = 0.5f; // 투명도를 조절하려면 0.0부터 1.0 사이의 값을 사용하세요
                ImageAttributes imageAttributes = new ImageAttributes();
                ColorMatrix transparencyMatrix = new ColorMatrix
                {
                    Matrix33 = opacity // 투명도 값을 설정합니다.
                };
                imageAttributes.SetColorMatrix(transparencyMatrix, ColorMatrixFlag.Default, ColorAdjustType.Bitmap);


                // 격자 크기 및 여백 설정
                int rows = 4;
                int cols = 7;
                int spacing = 5;

                int upperPadding = 150;

                // 전체 격자 크기 계산
                int gridWidth = cols * (gearImage.Width + spacing) - spacing;
                int gridHeight = rows * (gearImage.Height + spacing) - spacing;

                // 전체 스크린샷 크기 계산
                int screenWidth = gridWidth + 20; // 여분의 여백
                int screenHeight = gridHeight + 20; // 여분의 여백

                int colWidth = (gridWidth / 7);

                backgroundImage = new Bitmap(backgroundImage, new Size((int)(colWidth * 3.3), gridHeight));
                victimImage = new Bitmap(victimImage, new Size((int)(colWidth * 3.3), gridHeight));

                // 전체 스크린샷 생성
                using (Bitmap screenshot = new Bitmap(screenWidth, screenHeight + upperPadding))
                using (SolidBrush brush = new SolidBrush(System.Drawing.Color.Black))
                using (Graphics g = Graphics.FromImage(screenshot))
                {
                    g.Clear(System.Drawing.Color.FromArgb(222,222,218));

                    // 킬러 슬롯
                    g.DrawImage(backgroundImage, 0, upperPadding);
                    // 희생자 슬롯
                    g.DrawImage(backgroundImage, (int)(colWidth * 3.7), upperPadding);

                    // 희생자 혈흔
                    g.DrawImage(victimImage,
                    new Rectangle(colWidth * 4, upperPadding, colWidth * 3, screenHeight),
                        0, 0, colWidth * 3, screenHeight,
                        GraphicsUnit.Pixel,
                        imageAttributes
                        );

                    // 토탈 페임
                    g.DrawImage(fameImage, 
                        new Rectangle(colWidth * 3 + colWidth/2 - 74/2, (screenHeight/2) - 200, 74, 74),
                        0, 0, fameImage.Width/14, fameImage.Height, 
                        GraphicsUnit.Pixel
                        );
                    var stringFormat = new StringFormat() 
                    { 
                        Alignment=StringAlignment.Center,
                        LineAlignment=StringAlignment.Center,
                    };
                    var font = new Font("Arial", 50, FontStyle.Bold, GraphicsUnit.Pixel);
                    g.DrawString(battleEvent.TotalVictimKillFame.ToString(), font, brush, colWidth * 3 + colWidth / 2, (screenHeight / 2) - 150 + font.Height, stringFormat);

                    var timeFont = new Font("Arial", 20, FontStyle.Regular, GraphicsUnit.Pixel);

                    // 이벤트 시간
                    g.DrawString("UTC (KR +9)", timeFont, brush, colWidth * 3 + colWidth / 2, 50 + font.Height - timeFont.Height, stringFormat);
                    g.DrawString(battleEvent.TimeStamp.ToString(), timeFont, brush, colWidth * 3 + colWidth / 2, 50 + font.Height, stringFormat);

                    // 킬러 이름
                    var killerName = $"[{battleEvent.Killer.GuildName}]{battleEvent.Killer.Name}";
                    var killerItemPower = $"IP : {battleEvent.Killer.AverageItemPower}";
                    g.DrawString(killerName, font, brush, colWidth * 1 + colWidth / 2, 50, stringFormat);

                    // 희생자 이름
                    var victimName = $"[{battleEvent.Victim.GuildName}]{battleEvent.Victim.Name}";
                    var victimItemPower = $"IP : {battleEvent.Victim.AverageItemPower}";
                    g.DrawString(victimName, font, brush, colWidth * 5 + colWidth / 2, 50, stringFormat);

                    // 킬러 IP
                    var ipFont = new Font("Arial", 40, FontStyle.Italic, GraphicsUnit.Pixel);
                    g.DrawString(killerItemPower, ipFont, brush, colWidth * 1 + colWidth / 2, 50 + font.Height, stringFormat);

                    // 희생자 IP
                    g.DrawString(victimItemPower, ipFont, brush, colWidth * 5 + colWidth / 2, 50 + font.Height, stringFormat);

                    // 격자에 이미지 및 백그라운드 이미지 배치
                    for (int i = 0; i < rows; i++)
                    {
                        int row = i;

                        int x = spacing;
                        int y = spacing; // 상단에 배치
                        int offset = 0;
                        int rightPadding = spacing;
                        int bottomPadding = spacing;

                        if (i == rows - 1)
                            bottomPadding = 0;
                        // 나머지 칸에 이미지 배치
                        for (int j = 0; j < cols; j++)
                        {
                            int col = j;
                            if (j == cols - 1)
                                rightPadding = 0;
                            y = row * (gearImage.Height + spacing) + upperPadding;
                            x = (col) * (gearImage.Width + spacing);


                            g.DrawImage(images[j + (row * cols)], x, y);

                            int idx = j + (row * cols);
                            if (idx == 9|| idx == 13)
                            {
                                if (imagePaths[idx - 2].Contains("_2H_"))
                                {
                                    g.DrawImage(images[idx - 2],
                                        new Rectangle(x, y, colWidth, colWidth),
                                        0, 0, colWidth, colWidth,
                                        GraphicsUnit.Pixel,
                                        imageAttributes);
                                }
                            }
                        }

                    }
                    
                    // 스크린샷 저장
                    screenshot.Save(imagePath, System.Drawing.Imaging.ImageFormat.Png);

                    transparencyMatrix = null;
                    imageAttributes.Dispose();
                }
                return imagePath;
            }
            catch 
            {
                return string.Empty;
            }      
        }
        static System.Drawing.Image LoadImage(string url)
        {
            try
            {
                using (var webClient = new WebClient())
                {
                    var stream = webClient.OpenRead(url);
                    return System.Drawing.Image.FromStream(stream);
                }
            }
            catch
            {
                return new System.Drawing.Bitmap(11, 11);
            }

        }

        private string[] GetImagePaths(BattleEvent battleEvent)
        {
            List<string> imagePaths = new List<string>();
            var equipments = new List<Gear>();
            equipments.Add(battleEvent.Killer.Equipment.Bag ?? new Gear());
            equipments.Add(battleEvent.Killer.Equipment.Head ?? new Gear());
            equipments.Add(battleEvent.Killer.Equipment.Cape ?? new Gear());

            equipments.Add(new Gear());

            equipments.Add(battleEvent.Victim.Equipment.Bag ?? new Gear());
            equipments.Add(battleEvent.Victim.Equipment.Head ?? new Gear());
            equipments.Add(battleEvent.Victim.Equipment.Cape ?? new Gear());


            equipments.Add(battleEvent.Killer.Equipment.MainHand ?? new Gear());
            equipments.Add(battleEvent.Killer.Equipment.Armor ?? new Gear());
            equipments.Add(battleEvent.Killer.Equipment.OffHand ?? new Gear());

            equipments.Add(new Gear());

            equipments.Add(battleEvent.Victim.Equipment.MainHand ?? new Gear());
            equipments.Add(battleEvent.Victim.Equipment.Armor ?? new Gear());
            equipments.Add(battleEvent.Victim.Equipment.OffHand ?? new Gear());


            equipments.Add(battleEvent.Killer.Equipment.Potion ?? new Gear());
            equipments.Add(battleEvent.Killer.Equipment.Shoes ?? new Gear());
            equipments.Add(battleEvent.Killer.Equipment.Food ?? new Gear());

            equipments.Add(new Gear());

            equipments.Add(battleEvent.Victim.Equipment.Potion ?? new Gear());
            equipments.Add(battleEvent.Victim.Equipment.Shoes ?? new Gear());
            equipments.Add(battleEvent.Victim.Equipment.Food ?? new Gear());


            equipments.Add(new Gear());
            equipments.Add(battleEvent.Killer.Equipment.Mount ?? new Gear());
            equipments.Add(new Gear());

            equipments.Add(new Gear());

            equipments.Add(new Gear());
            equipments.Add(battleEvent.Victim.Equipment.Mount ?? new Gear());
            equipments.Add(new Gear());

            foreach (var equipment in equipments)
            {
                if (equipment.Type != null)
                {
                    imagePaths.Add(GetGearImageUrl(equipment.Type, equipment.Count, equipment.Quality));
                }
                else
                {
                    imagePaths.Add(string.Empty);
                }
            }

            return imagePaths.ToArray();
        }

        private string GetGearImageUrl(string? name,int count, int quality)
        {
            return $"https://render.albiononline.com/v1/item/{name}.png?count={count}&quality={quality}";
        }
        private string GetFiledContext(BattleEvent battleEvent)
        {
            var killersGear = ""; //GetEquipmentContext(battleEvent.Killer.Equipment);
            var victimsGear = ""; //GetEquipmentContext(battleEvent.Victim.Equipment);
            //var inventory = GetInventoryItems(battleEvent.Victim.Inventory);

            Func<string, string> Nvl = delegate (string name)
            {
                return string.IsNullOrEmpty(name)?"없음":name;
            };
            Func<string, string> NvlAlliance = delegate (string name)
            {
                return string.IsNullOrEmpty(name) ? string.Empty : $"[{name}]";
            };

            var killerAllianceName = NvlAlliance(battleEvent.Killer.AllianceName);
            var killerGuildName = Nvl(battleEvent.Killer.GuildName);
            var victimAllianceName = NvlAlliance(battleEvent.Victim.AllianceName);
            var victimGuildName = Nvl(battleEvent.Victim.GuildName);

            var participantQty = (battleEvent.numberOfParticipants - 1 == 0) ? "Solo kill" : $"외 {battleEvent.numberOfParticipants - 1}명";

            var killer = $"{Environment.NewLine}길드: {killerAllianceName}{killerGuildName}{Environment.NewLine}유저: **{battleEvent.Killer.Name}**     :crossed_swords: {participantQty} {Environment.NewLine}IP: {(int)battleEvent.Killer.AverageItemPower}{Environment.NewLine}";
            var victim = $"{Environment.NewLine}길드: {victimAllianceName}{victimGuildName}{Environment.NewLine}유저: **{battleEvent.Victim.Name}**     :skull_crossbones: {Environment.NewLine}IP: {(int)battleEvent.Victim.AverageItemPower}{Environment.NewLine}";
            string contributers = string.Empty;

            var datetime = $"발생일시: { battleEvent.TimeStamp.ToString()}(UTC) \n발생장소:{battleEvent.KillArea.ToString()}";
            var contributer = new StringBuilder("\n 어시스트:");


            var participants = battleEvent.Participants?.Where(member=>member.Name != battleEvent.Killer.Name).ToList();
            if (participants!=null&& participants.Any()) 
            {
                foreach (var participant in participants) 
                {
                    contributer.Append($" {participant.Name}(ip:{(int)participant.AverageItemPower}), ");
                }
                contributer.AppendLine();
                contributers = contributer.ToString();
            }
            var partyMember = new StringBuilder("\n 파티멤버:");


            var groupMembers = battleEvent.GroupMembers?.Where(member => member.Name != battleEvent.Killer.Name).ToList();
            if (groupMembers != null && groupMembers.Any())
            {
                foreach (var groupMember in groupMembers)
                {
                    partyMember.Append($" {groupMember.Name}, ");
                }
                partyMember.AppendLine();
                contributers = contributers + partyMember.ToString();
            }

            //var result = killer + killersGear.ToString() + victim + victimsGear.ToString() + contributers + datetime;
            var result = killer + victim + contributers + datetime;
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
                    if (gear.Index > 25)
                        gearURL = string.Empty;
                    else
                        gearURL = GetGearImageUrl(gearName, gearCount, gearQuality);
                    gears.AppendLine($"[{gear.Index}]번째 칸: [{gearName}]({gearURL}) 수량:{gearCount}");
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
                gears.AppendLine($"MainHand : [{gearName}]({ gearURL})");
            }
            else
                gears.AppendLine($"MainHand : [없음]");

            if (equipment.OffHand != null)
            {
                gearCount = equipment.OffHand.Count;
                gearQuality = equipment.OffHand.Quality;
                gearName = equipment.OffHand.Type;
                gearURL = GetGearImageUrl(gearName, gearCount, gearQuality); 
                gears.AppendLine($"OffHand  : [{gearName}]({ gearURL})");
            }
            else
                gears.AppendLine($"OffHand  : [없음]");

            if (equipment.Head != null)
            {
                gearCount = equipment.Head.Count;
                gearQuality = equipment.Head.Quality;
                gearName = equipment.Head.Type;
                gearURL = GetGearImageUrl(gearName, gearCount, gearQuality); 
                gears.AppendLine($"Head : [{gearName}]({ gearURL})");
            }
            else
                gears.AppendLine($"Head : [없음]");

            if (equipment.Armor != null) 
            {
                gearCount = equipment.Armor.Count;
                gearQuality = equipment.Armor.Quality;
                gearName = equipment.Armor.Type;
                gearURL = GetGearImageUrl(gearName, gearCount, gearQuality); 
                gears.AppendLine($"Armor    : [{gearName}]({ gearURL})");
            }
            else
                gears.AppendLine($"Armor    : [없음]");

            if (equipment.Shoes != null)
            {
                gearCount = equipment.Shoes.Count;
                gearQuality = equipment.Shoes.Quality;
                gearName = equipment.Shoes.Type;
                gearURL = GetGearImageUrl(gearName, gearCount, gearQuality); 
                gears.AppendLine($"Shoes    : [{gearName}]({ gearURL})");
            }
            else
                gears.AppendLine($"Shoes    : [없음]");

            if (equipment.Cape != null)
            {
                gearCount = equipment.Cape.Count;
                gearQuality = equipment.Cape.Quality;
                gearName = equipment.Cape.Type;
                gearURL = GetGearImageUrl(gearName, gearCount, gearQuality); 
                gears.AppendLine($"Cape : [{gearName}]({ gearURL})");
            }
            else
                gears.AppendLine($"Cape : [없음]");

            if (equipment.Potion != null)
            {
                gearCount = equipment.Potion.Count;
                gearQuality = equipment.Potion.Quality;
                gearName = equipment.Potion.Type;
                gearURL = GetGearImageUrl(gearName, gearCount, gearQuality); 
                gears.AppendLine($"Potion   : [{gearName}]({ gearURL})");
            }
            else
                gears.AppendLine($"Potion   : [없음]");
            if (equipment.Food != null)
            {
                gearCount = equipment.Food.Count;
                gearQuality = equipment.Food.Quality;
                gearName = equipment.Food.Type;
                gearURL = GetGearImageUrl(gearName, gearCount, gearQuality); 
                gears.AppendLine($"Food : [{gearName}]({ gearURL})");
            }
            else
                gears.AppendLine($"Food : [없음]");
            if (equipment.Mount != null)
            {
                gearCount = equipment.Mount.Count;
                gearQuality = equipment.Mount.Quality;
                gearName = equipment.Mount.Type;
                gearURL = GetGearImageUrl(gearName, gearCount, gearQuality); 
                gears.AppendLine($"Mount    : [{gearName}]({ gearURL})");
            }
            else
                gears.AppendLine($"Mount    : [없음]");
            if (equipment.Bag != null)
            {
                gearCount = equipment.Bag.Count;
                gearQuality = equipment.Bag.Quality;
                gearName = equipment.Bag.Type;
                gearURL = GetGearImageUrl(gearName, gearCount, gearQuality); 
                gears.AppendLine($"Bag  : [{gearName}]({ gearURL})");
            }
            else
                gears.AppendLine($"Bag  : [없음]");
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
