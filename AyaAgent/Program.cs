using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using System.Reflection;

namespace AyaAgent
{
    public class Config
    {
        //빈 문자열(string.Empty)로 초기화
        public string Token { get; set; } = string.Empty;
    }

    class Program
    {
        private readonly DiscordSocketClient _client; //봇 클라이언트
        private readonly CommandService _commands; //명령어 수신 클라이언트

        public Program()
        {
            _client = new DiscordSocketClient(new DiscordSocketConfig()
            {
                LogLevel = LogSeverity.Verbose, //봇의 로그 레벨 설정
                GatewayIntents = GatewayIntents.Guilds | GatewayIntents.GuildMessages | GatewayIntents.MessageContent
            });
            _commands = new CommandService(new CommandServiceConfig() //명령어 수신 클라이언트 초기화
            {
                LogLevel = LogSeverity.Verbose //봇의 로그 레벨 설정
            });
        }
        
        //프로그램의 진입점
        static void Main(string[] args)
        {
            new Program().BotMain().GetAwaiter().GetResult(); //봇의 진입점 실행
        }

        //봇의 진입점, 봇의 거의 모든 작업이 비동기로 작동되기 때문에 비동기 함수로 생성해야 함.
        public async Task BotMain()
        {
            var configJson = File.ReadAllText("userInfo/config.json");
            var config = JsonSerializer.Deserialize<Config>(configJson);

            if (config == null || string.IsNullOrEmpty(config.Token))
            {
                Console.WriteLine("Error: config.json is invalid or token is missing. Please check the file at userInfo/config.json\n에러: config.json이 유효하지 않거나 토큰이 인식되지 않습니다. userInfo/config.json 파일을 확인해주세요.");
                return;
            }

            //로그 수신 시 로그 출력 함수에서 출력되도록 설정
            _client.Log += OnClientLogReceived;
            _commands.Log += OnClientLogReceived;

            //프로젝트에 있는 모든 명령어 모듈 등록
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), null);
            await _client.LoginAsync(TokenType.Bot, config.Token); //봇 토큰 사용해 서버에 로그인
            await _client.StartAsync(); //봇이 이벤트를 수신하기 시작
            _client.MessageReceived += OnClientMessage; //봇이 메시지를 수신할 때 처리하도록 설정
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
            message.HasMentionPrefix(_client.CurrentUser, ref pos)) ||
            message.Author.IsBot)
                return;

            var context = new SocketCommandContext(_client, message); //수신된 메시지에 대한 컨텍스트 생성

            //CommandService에 명령어 실행 요청
            var result = await _commands.ExecuteAsync(
                context: context,
                argPos: pos,
                services: null);

            //명령어 실행이 실패했을 경우(예: 없는 명령어), 오류 출력
            if (!result.IsSuccess)
            {
                Console.WriteLine($"명령어 실행 실패: {result.ErrorReason}");
            }
        }

        // 봇의 로그를 출력하는 함수
        private Task OnClientLogReceived(LogMessage msg)
        {
            Console.WriteLine(msg.ToString()); //로그 출력
            return Task.CompletedTask;
        }
    }
}
