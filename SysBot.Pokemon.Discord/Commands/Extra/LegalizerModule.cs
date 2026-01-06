using Discord;
using Discord.Commands;
using PKHeX.Core;
using System.Linq;
using System.Threading.Tasks;

namespace SysBot.Pokemon.Discord
{
    public class LegalizerModule<T> : ModuleBase<SocketCommandContext> where T : PKM, new()
    {
        [Command("convert"), Alias("showdown")]
        [Summary("Intenta convertir el conjunto de Showdown al formato RegenTemplate.")]
        [Priority(1)]
        public async Task ConvertShowdown([Summary("Generación/Formato")] byte gen, [Remainder][Summary("Conjunto de Showdown")] string content)
        {
            var deleteMessageTask = LegalizerModule<T>.DeleteCommandMessageAsync(Context.Message, 2000);
            var convertTask = Context.Channel.ReplyWithLegalizedSetAsync(content, gen);
            await Task.WhenAll(deleteMessageTask, convertTask).ConfigureAwait(false);
        }

        [Command("convert"), Alias("showdown")]
        [Summary("Intenta convertir el conjunto de Showdown al formato RegenTemplate.")]
        [Priority(0)]
        public async Task ConvertShowdown([Remainder][Summary("Conjunto de Showdown")] string content)
        {
            var deleteMessageTask = LegalizerModule<T>.DeleteCommandMessageAsync(Context.Message, 2000);
            var convertTask = Context.Channel.ReplyWithLegalizedSetAsync<T>(content);
            await Task.WhenAll(deleteMessageTask, convertTask).ConfigureAwait(false);
        }

        [Command("legalize"), Alias("alm")]
        [Summary("Intenta legalizar los datos adjuntos de Pokémon y mostrarlos como RegenTemplate.")]
        public async Task LegalizeAsync()
        {
            var deleteMessageTask = LegalizerModule<T>.DeleteCommandMessageAsync(Context.Message, 2000);
            var legalizationTasks = Context.Message.Attachments.Select(att =>
                Context.Channel.ReplyWithLegalizedSetAsync(att)
            ).ToArray();

            await Task.WhenAll(deleteMessageTask, Task.WhenAll(legalizationTasks)).ConfigureAwait(false);
        }

        private static async Task DeleteCommandMessageAsync(IUserMessage message, int delayMilliseconds)
        {
            await Task.Delay(delayMilliseconds).ConfigureAwait(false);
            await message.DeleteAsync().ConfigureAwait(false);
        }
    }
}
