using Discord;
using Discord.Interactions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace AyaAgent.Commands
{
    public class GeneralModule : InteractionModuleBase<SocketInteractionContext>
    {
        [SlashCommand("안녕", "아야가 인사를 건넵니다.")]
        public async Task PingAsync()
        {
            await RespondAsync($"{Context.User.Mention}, 안녕? 나는 만년설의 현자, 아야라고 해.");
        }

        [SlashCommand("점심추천", "아야가 점심 메뉴를 추천해줍니다.")]
        public async Task LunchMenuRecommendAsync()
        {
            try
            {
                // 메뉴 리스트 가져오기
                var menuList = await GetJsonToListAsync("menu.json");

                // 메뉴 리스트가 비어 있는지 확인
                if (menuList == null || !menuList.Any())
                {
                    await RespondAsync("메뉴판이 텅 비어있네. 미안, 오늘은 추천해주기 힘들 것 같아.", ephemeral: true);
                    return;
                }

                //무작위로 메뉴 하나를 선택
                var selectedMenu = GetRandomFromList(menuList);

                //최종 응답 보내기
                string userMention = Context.User.Mention;
                await RespondAsync($"점심을 먹고 싶구나, {userMention}? 그럼 오늘은 ** {selectedMenu}** 어때?");
            }
            catch (FileNotFoundException ex)
            {
                //에러 로그 남기기
                Console.WriteLine($"ERROR: {ex.Message}");
                // catch 블록으로 파일을 못 찾았을 때의 오류를 처리
                await RespondAsync("에르핀이 메뉴판을 훔쳐갔대... 오늘은 추천해주기 힘들 것 같아.", ephemeral: true);
                return; //오류 발생했으므로 메서드 종료
            }
            catch (JsonException ex)
            {
                //에러 로그 남기기
                Console.WriteLine($"ERROR: {ex.Message}");
                //그 외 다른 파일 읽기 오류도 처리
                await RespondAsync("누가 메뉴판에 장난을 쳤나봐. 읽을 수가 없어... 관리자한테 알려줘야겠는걸.", ephemeral: true);
                return; //오류 발생했으므로 메서드 종료
            }
            catch (Exception ex)
            {
                //개발자가 오류를 확인할 수 있도록 콘솔에 로그 남기기
                Console.WriteLine($"ERROR: {ex}");
                await RespondAsync("`아야가 충격을 받은 듯 멍을 때리고 서 있습니다. 나중에 다시 물어봅시다.`", ephemeral: true);
                return;
            }
        }

        //menu.json 파일을 읽고 파싱해 메뉴 리스트를 반환
        private async Task<List<string>> GetJsonToListAsync(string jsonFileName)
        {
            string basePath = AppDomain.CurrentDomain.BaseDirectory;
            string menuFilePath = Path.Combine(basePath, "GlobalData", jsonFileName);

            string jsonContent = await File.ReadAllTextAsync(menuFilePath);
            var menuList = JsonConvert.DeserializeObject<List<string>>(jsonContent);

            return menuList;
        }

        // 주어진 리스트에서 무작위로 아이템 하나 선택해서 반환
        private string GetRandomFromList(List<string> menuList)
        {
            Random random = new Random();
            int randomIndex = random.Next(menuList.Count);
            return menuList[randomIndex];
        }
    }
}
