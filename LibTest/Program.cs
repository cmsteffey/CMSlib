using System;
using CMSlib.Extensions;
using System.Threading;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Net.Http;
namespace LibTest
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.Write("GuildId: ");
            //string id = Console.ReadLine();
            HttpClient client = new();
            client.DefaultRequestHeaders.Authorization = new("Bot", "ODA1ODUxNjQ3NDkzNjY4OTQ1.YBg51A.dceYGC8xoHnmtfCWeLvKr0QpW7Q");
            DiscordSlashCommand command = new DiscordSlashCommand
            {
                name = "bookmark",
                description = "Silently bookmarks the most recent message in this channel",
                options = new DiscordSlashCommandOption[] {
                    new()
                    {
                        type = 3,
                        name = "bookmark_name",
                        description = "The name to give this bookmark, so it can be searched up in this bot's DMs",
                        required = false
                    }
                }
            };
            
            string content = JsonSerializer.Serialize(command);
            Console.WriteLine(content);
            StringContent scontent = new(content, System.Text.Encoding.UTF8, "application/json");
            HttpResponseMessage response = null;
            bool cont = true;
            
            response = await client.PostAsync($"https://discord.com/api/v8/applications/805851647493668945/commands", 
                scontent);
                    
            Console.WriteLine(response?.StatusCode);
            Console.WriteLine(await response?.Content.ReadAsStringAsync());
            
            Console.ReadLine(); 
            
        }
    }
    
    public class DiscordSlashCommand
    {
        public string name { get; set; }
        public string description { get; set; }
        public DiscordSlashCommandOption[] options { get; set; }
    }
    public class DiscordSlashCommandOption
    {
        public int type { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public bool required { get; set; }


    }
    public class DiscordSlashCommandOptionChoice
    {
        public string name { get; set; }
    }
    public class DiscordSlashCommandOptionChoiceInt : DiscordSlashCommandOptionChoice
    {
        public int value { get; set; }
    }
    public class DiscordSlashCommandOptionChoiceString : DiscordSlashCommandOptionChoice
    {
        public string value { get; set; }

    }

    
}
