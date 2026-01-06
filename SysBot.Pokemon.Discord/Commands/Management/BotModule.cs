using Discord;
using Discord.Commands;
using Newtonsoft.Json.Linq;
using PKHeX.Core;
using SysBot.Pokemon.Helpers;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SysBot.Pokemon.Discord
{
    public class BotModule<T> : ModuleBase<SocketCommandContext> where T : PKM, new()
    {
        [Command("botStatus")]
        [Summary("Obtiene el estado de los bots.")]
        [RequireSudo]
        public async Task GetStatusAsync()
        {
            var me = SysCord<T>.Runner;
            var bots = me.Bots.Select(z => z.Bot).OfType<PokeRoutineExecutorBase>().ToArray();
            if (bots.Length == 0)
            {
                await ReplyAsync("No hay bots configurados.").ConfigureAwait(false);
                return;
            }

            var summaries = bots.Select(GetDetailedSummary);
            var lines = string.Join(Environment.NewLine, summaries);
            await ReplyAsync(Format.Code(lines)).ConfigureAwait(false);
        }

        private static string GetBotIPFromJsonConfig()
        {
            try
            {
                // Read the file and parse the JSON
                var jsonData = File.ReadAllText(PokeBot.ConfigPath);
                var config = JObject.Parse(jsonData);

                // Access the IP address from the first bot in the Bots array
#pragma warning disable CS8602 // Dereference of a possibly null reference.
                var ip = config["Bots"][0]["Connection"]["IP"].ToString();
#pragma warning restore CS8602 // Dereference of a possibly null reference.
                return ip;
            }
            catch (Exception ex)
            {
                // Handle any errors that occur during reading or parsing the file
                Console.WriteLine($"Error al leer el archivo de configuración: {ex.Message}");
                return "192.168.1.1"; // Default IP if error occurs
            }
        }

        private static string GetDetailedSummary(PokeRoutineExecutorBase z)
        {
            return $"- {z.Connection.Name} | {z.Connection.Label} - {z.Config.CurrentRoutineType} ~ {z.LastTime:hh:mm:ss} | {z.LastLogged}";
        }

        [Command("botStart")]
        [Summary("Inicia el bot actualmente en ejecución.")]
        [RequireSudo]
        public async Task StartBotAsync([Summary("Dirección IP del bot")] string? ip = null)
        {
            if (ip == null)
                ip = BotModule<T>.GetBotIPFromJsonConfig();

            var bot = SysCord<T>.Runner.GetBot(ip);
            if (bot == null)
            {
                await ReplyAsync($"Ningún bot tiene esa dirección IP ({ip}).").ConfigureAwait(false);
                return;
            }

            bot.Start();
            await ReplyAsync("El bot ha sido iniciado.").ConfigureAwait(false);
        }

        [Command("botStop")]
        [Summary("Detiene el bot actualmente en ejecución.")]
        [RequireSudo]
        public async Task StopBotAsync([Summary("Dirección IP del bot")] string? ip = null)
        {
            if (ip == null)
                ip = BotModule<T>.GetBotIPFromJsonConfig();

            var bot = SysCord<T>.Runner.GetBot(ip);
            if (bot == null)
            {
                await ReplyAsync($"Ningún bot tiene esa dirección IP ({ip}).").ConfigureAwait(false);
                return;
            }

            bot.Stop();
            await ReplyAsync("El bot ha sido detenido.").ConfigureAwait(false);
        }

        [Command("botIdle")]
        [Alias("botPause")]
        [Summary("Envía un comando al bot actualmente en ejecución para que entre en estado de espera.")]
        [RequireSudo]
        public async Task IdleBotAsync([Summary("Dirección IP del bot")] string? ip = null)
        {
            if (ip == null)
                ip = BotModule<T>.GetBotIPFromJsonConfig();

            var bot = SysCord<T>.Runner.GetBot(ip);
            if (bot == null)
            {
                await ReplyAsync($"Ningún bot tiene esa dirección IP ({ip}).").ConfigureAwait(false);
                return;
            }

            bot.Pause();
            await ReplyAsync("El bot ha sido puesto en espera.").ConfigureAwait(false);
        }

        [Command("botChange")]
        [Summary("Cambia la rutina del bot actualmente en ejecución (intercambios).")]
        [RequireSudo]
        public async Task ChangeTaskAsync([Summary("Nombre del tipo de rutina")] PokeRoutineType task, [Summary("Dirección IP del bot")] string? ip = null)
        {
            if (ip == null)
                ip = BotModule<T>.GetBotIPFromJsonConfig();

            var bot = SysCord<T>.Runner.GetBot(ip);
            if (bot == null)
            {
                await ReplyAsync($"Ningún bot tiene esa dirección IP ({ip}).").ConfigureAwait(false);
                return;
            }

            bot.Bot.Config.Initialize(task);
            await ReplyAsync($"El bot ha cambiado su rutina a {task}.").ConfigureAwait(false);
        }

        [Command("botRestart")]
        [Summary("Reinicia el bot actualmente en ejecución.")]
        [RequireSudo]
        public async Task RestartBotAsync([Summary("Dirección IP del bot")] string? ip = null)
        {
            if (ip == null)
                ip = BotModule<T>.GetBotIPFromJsonConfig();

            var bot = SysCord<T>.Runner.GetBot(ip);
            if (bot == null)
            {
                await ReplyAsync($"Ningún bot tiene esa dirección IP ({ip}).").ConfigureAwait(false);
                return;
            }

            var c = bot.Bot.Connection;
            c.Reset();
            bot.Start();
            await ReplyAsync("El bot ha sido reiniciado.").ConfigureAwait(false);
        }
    }
}
