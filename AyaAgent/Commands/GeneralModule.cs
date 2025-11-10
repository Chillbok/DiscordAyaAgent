using Discord.Interactions;
using System.Threading.Tasks;

namespace AyaAgent.Commands
{
    public class GeneralModule : InteractionModuleBase<SocketInteractionContext>
    {
        [SlashCommand("안녕", "아야가 인사를 건넵니다.")]
        public async Task PingAsync()
        {
            await RespondAsync("안녕? 나는 만년설의 현자, 아야라고 해.");
        }
    }
}
