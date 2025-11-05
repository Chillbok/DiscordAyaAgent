using Discord.Commands;
using System.Threading.Tasks;

namespace AyaAgent.Commands
{
    public class GeneralModule : ModuleBase<SocketCommandContext>
    {
        [Command("안녕")]
        public async Task PingAsync()
        {
            await ReplyAsync("안녕? 나는 만년설의 현자, 아야라고 해.");
        }
    }
}
