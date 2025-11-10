using Discord;
using Discord.WebSocket;
using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using System.Reflection;
using Discord.Interactions;

namespace AyaAgent
{
    public class Config
    {
        public string Token { get; set; } = string.Empty;
        public ulong TestGuildId { get; set; }
    }

    class Program
    {
        private readonly DiscordSocketClient _client;
        private readonly InteractionService _commands;
        private Config? _config; // 클래스 필드로 설정을 저장합니다.

        public Program()
        {
            _client = new DiscordSocketClient(new DiscordSocketConfig()
            {
                LogLevel = LogSeverity.Verbose,
                GatewayIntents = GatewayIntents.Guilds | GatewayIntents.GuildMessages
            });
            
            _commands = new InteractionService(_client.Rest, new InteractionServiceConfig()
            {
                LogLevel = LogSeverity.Verbose
            });
        }
        
        static void Main(string[] args)
        {
            new Program().BotMain().GetAwaiter().GetResult();
        }

        public async Task BotMain()
        {
            var configJson = File.ReadAllText("userInfo/config.json");
            _config = JsonSerializer.Deserialize<Config>(configJson); // 설정을 필드에 저장

            if (_config == null || string.IsNullOrEmpty(_config.Token))
            {
                Console.WriteLine("Error: config.json is invalid or token is missing.");
                return;
            }
            if (_config.TestGuildId == 0)
            {
                Console.WriteLine("Error: TestGuildId is missing in config.json. Please add your test server's ID.");
                return;
            }

            _client.Log += OnClientLogReceived;
            _commands.Log += OnClientLogReceived;
            
            _client.Ready += OnClientReady;
            _client.InteractionCreated += OnInteractionCreated;

            await _client.LoginAsync(TokenType.Bot, _config.Token); // 필드에서 토큰 사용
            await _client.StartAsync();
            
            await Task.Delay(-1);
        }

        private async Task OnClientReady()
        {
            // 이제 여기서 파일을 다시 읽을 필요가 없습니다.
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), null);
            
            // 필드에서 TestGuildId를 사용합니다.
            // BotMain에서 null 체크를 통과했으므로, ! (null-forgiving operator)를 사용해 컴파일러에게 null이 아님을 알려줍니다.
            await _commands.RegisterCommandsToGuildAsync(_config!.TestGuildId);
            
            Console.WriteLine("봇이 준비되었습니다. 명령어를 등록했습니다.");
        }

        private async Task OnInteractionCreated(SocketInteraction interaction)
        {
            try
            {
                var context = new SocketInteractionContext(_client, interaction);
                var result = await _commands.ExecuteCommandAsync(context, null);

                if (!result.IsSuccess)
                {
                    Console.WriteLine($"명령어 실행 실패: {result.ErrorReason}");
                    await interaction.RespondAsync($"오류가 발생했습니다: {result.ErrorReason}", ephemeral: true);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"예외 발생: {ex}");
                if (interaction.HasResponded)
                    await interaction.FollowupAsync("처리 중 예기치 않은 오류가 발생했습니다.", ephemeral: true);
                else
                    await interaction.RespondAsync("처리 중 예기치 않은 오류가 발생했습니다.", ephemeral: true);
            }
        }

        private Task OnClientLogReceived(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }
    }
}
