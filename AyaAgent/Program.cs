using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Threading.Tasks;

namespace AyaAgent
{
    class Program
    {
        DiscordSocketClient client; //봇 클라이언트
        CommandService commands; //명령어 수신 클라이언트
        //프로그램의 진입점
        static void Main(string[] args)
        {
            new Program().BotMain().GetAwaiter().GetResult(); //봇의 진입점 실행
        }

        //봇의 진입점, 봇의 거의 모든 작업이 비동기로 작동되기 때문에 비동기 함수로 생성해야 함.
        public async Task BotMain()
        {
            client = new DiscordSocketClient(new DiscordSocketConfig()
            {
                LogLevel = LogSeverity.Verbose //봇의 로그 레벨 설정
            });
            commands = new CommandService(new CommandServiceConfig() //명령어 수신 클라이언트 초기화
            {
                LogLevel = LogSeverity
                .Verbose //봇의 로그 레벨 설정
            });

            //로그 수신 시 로그 출력 함수에서 출력되도록 설정
            client.Log += OnClientLogReceived;
            commands.Log += OnClientLogReceived;

            await client.LoginAsync(TokenType.Bot, "Bot Token"); //봇 토큰 사용해 서버에 로그인
            await client.StartAsync(); //봇이 이벤트를 수신하기 시작

            client.MessageReceived += OnClientMessage; //봇이 메시지를 수신할 때 처리하도록 설정
            await Task.Delay(-1); //봇이 종료되지 않도록 블로킹
        }

        private async Task OnClientMessage(SocketMessage arg)
        {
            //수신한 메시지가 사용자가 보낸 게 아닐 때 취소
            var message = arg as SocketUserMessage;
            if (message == null) return;

            int pos = 0;

            //메시지 앞에 !이 달려있지 않고, 자신이 호출된 게 아니거나 다른 봇이 호출했다면 취소
            if (!(message.HasCharPrefix('!', ref pos) ||
            message.HasMentionPrefix(client.CurrentUser, ref pos)) ||
            message.Author.IsBot)
                return;

            var context = new SocketCommandContext(client, message); //수신된 메시지에 대한 컨텍스트 생성
            await context.Channel.SendMessageAsync("명령어 수신됨 - " + message.Content); //수신된 명령어를 다시 보낸다.
        }

        // 봇의 로그를 출력하는 함수
        private Task OnClientLogReceived(LogMessage msg)
        {
            Console.WriteLine(msg.ToString()); //로그 출력
            return Task.CompletedTask;
        }
    }
}
