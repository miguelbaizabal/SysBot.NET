using Discord;
using Discord.Commands;
using PKHeX.Core;
using PKHeX.Core.AutoMod;
using SysBot.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SysBot.Pokemon.Discord
{
    public class MysteryEggModule<T> : ModuleBase<SocketCommandContext> where T : PKM, new()
    {
        private static TradeQueueInfo<T> Info => SysCord<T>.Runner.Hub.Queues.Info;
        private static readonly Dictionary<EntityContext, List<ushort>> BreedableSpeciesCache = [];
        private const int DefaultMaxGenerationAttempts = 30;

        [Command("mysteryegg")]
        [Alias("me")]
        [Summary("Intercambia un huevo generado de un Pokémon aleatorio.")]
        public async Task TradeMysteryEggAsync()
        {
            // LGPE does not support eggs/breeding
            var context = GetContext();
            if (context == EntityContext.None || typeof(T).Name == "PB7")
            {
                await ReplyAsync("Los huevos misteriosos no están disponibles para Let's Go Pikachu/Eevee ya que el juego no soporta la crianza.").ConfigureAwait(false);
                return;
            }

            var userID = Context.User.Id;
            if (Info.IsUserInQueue(userID))
            {
                await ReplyAsync("Ya tienes un intercambio existente en la cola. Por favor, espera hasta que sea procesado.").ConfigureAwait(false);
                return;
            }

            var code = Info.GetRandomTradeCode(userID);
            _ = Task.Run(async () =>
            {
                try
                {
                    await ProcessMysteryEggTradeAsync(code).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    LogUtil.LogSafe(ex, nameof(MysteryEggModule<T>));
                }
            });
        }

        [Command("batchMysteryEgg")]
        [Alias("bme")]
        [Summary("Intercambia múltiples huevos misteriosos a la vez (hasta 4).")]
        public async Task BatchMysteryEggAsync([Summary("Número de huevos (1-4)")] int count = 2)
        {
            // LGPE does not support eggs/breeding
            var context = GetContext();
            if (context == EntityContext.None || typeof(T).Name == "PB7")
            {
                await ReplyAsync("Los huevos misteriosos no están disponibles para Let's Go Pikachu/Eevee ya que el juego no soporta la crianza.").ConfigureAwait(false);
                return;
            }

            var userID = Context.User.Id;
            if (!await Helpers<T>.EnsureUserNotInQueueAsync(userID))
            {
                await Helpers<T>.ReplyAndDeleteAsync(Context,
                    "Ya tienes un intercambio existente en la cola que no puede ser eliminado. Por favor, espera hasta que sea procesado.", 2);
                return;
            }

            // Validate count
            const int maxEggs = 4;
            if (count < 1 || count > maxEggs)
            {
                await Helpers<T>.ReplyAndDeleteAsync(Context,
                    $"Numero de huevos inválido. Por favor, especifica entre 1 y {maxEggs} huevos.", 5);
                return;
            }

            var processingMessage = await Context.Channel.SendMessageAsync($"{Context.User.Mention} Generando {count} Huevos Misteriosos...");
            _ = Task.Run(async () =>
            {
                try
                {
                    var batchEggList = new List<T>();
                    var failedCount = 0;

                    // Generate all mystery eggs
                    for (int i = 0; i < count; i++)
                    {
                        var egg = GenerateLegalMysteryEgg();
                        if (egg != null)
                        {
                            batchEggList.Add(egg);
                        }
                        else
                        {
                            failedCount++;
                        }
                    }

                    await processingMessage.DeleteAsync();

                    // Check if we generated any eggs
                    if (batchEggList.Count == 0)
                    {
                        await Context.Channel.SendMessageAsync($"{Context.User.Mention} Error al generar huevos misteriosos. Por favor, inténtalo de nuevo.");
                        return;
                    }

                    // Warn if some eggs failed
                    if (failedCount > 0)
                    {
                        await Context.Channel.SendMessageAsync($"{Context.User.Mention} Advertencia: Error al generar {failedCount} huevo(s). Procediendo con {batchEggList.Count} huevo(s).");
                    }

                    // Add batch to queue
                    var batchTradeCode = Info.GetRandomTradeCode(userID);
                    await ProcessBatchMysteryEggs(Context, batchEggList, batchTradeCode, count);
                }
                catch (Exception ex)
                {
                    try
                    {
                        await processingMessage.DeleteAsync();
                    }
                    catch { }

                    await Context.Channel.SendMessageAsync($"{Context.User.Mention} Ocurrió un error mientras se procesaba tu solicitud de huevos misteriosos por lotes. Por favor, inténtalo de nuevo.");
                    Base.LogUtil.LogError($"Error al procesar el lote de huevos misteriosos: {ex.Message}", nameof(BatchMysteryEggAsync));
                }
            });

            if (Context.Message is IUserMessage userMessage)
                _ = Helpers<T>.DeleteMessagesAfterDelayAsync(userMessage, null, 2);
        }

        private static async Task ProcessBatchMysteryEggs(SocketCommandContext context, List<T> batchEggList, int batchTradeCode, int totalEggs)
        {
            var sig = context.User.GetFavor();
            var firstEgg = batchEggList[0];
            var trainer = new PokeTradeTrainerInfo(context.User.Username, context.User.Id);
            var notifier = new DiscordTradeNotifier<T>(firstEgg, trainer, batchTradeCode, context.User, 1, totalEggs, true, lgcode: []);

            int uniqueTradeID = GenerateUniqueTradeID();

            var detail = new PokeTradeDetail<T>(firstEgg, trainer, notifier, PokeTradeType.Batch, batchTradeCode,
                sig == RequestSignificance.Favored, null, 1, batchEggList.Count, true, uniqueTradeID)
            {
                BatchTrades = batchEggList
            };

            var trade = new TradeEntry<T>(detail, context.User.Id, PokeRoutineType.Batch, context.User.Username, uniqueTradeID);
            var hub = SysCord<T>.Runner.Hub;
            var Info = hub.Queues.Info;
            var added = Info.AddToTradeQueue(trade, context.User.Id, false, sig == RequestSignificance.Owner);

            // Send trade code once
            await EmbedHelper.SendTradeCodeEmbedAsync(context.User, batchTradeCode).ConfigureAwait(false);

            // Start queue position updates for Discord notification
            if (added != QueueResultAdd.AlreadyInQueue && notifier is DiscordTradeNotifier<T> discordNotifier)
            {
                await discordNotifier.SendInitialQueueUpdate().ConfigureAwait(false);
            }

            if (added == QueueResultAdd.AlreadyInQueue)
            {
                await context.Channel.SendMessageAsync("¡Ya estás en la cola!").ConfigureAwait(false);
                return;
            }

            var position = Info.CheckPosition(context.User.Id, uniqueTradeID, PokeRoutineType.Batch);
            var botct = Info.Hub.Bots.Count;
            var baseEta = position.Position > botct ? Info.Hub.Config.Queues.EstimateDelay(position.Position, botct) : 0;

            // Send initial batch summary message
            await context.Channel.SendMessageAsync($"{context.User.Mention} - Lote de {batchEggList.Count} Huevos Misteriosos añadidos a la cola! Posición: {position.Position}. Tiempo estimado: {baseEta:F1} min(s).").ConfigureAwait(false);

            // Create and send embeds for each egg
            if (SysCord<T>.Runner.Config.Trade.TradeEmbedSettings.UseEmbeds)
            {
                for (int i = 0; i < batchEggList.Count; i++)
                {
                    var pk = batchEggList[i];
                    var embed = CreateMysteryEggEmbed(context, pk, i + 1, batchEggList.Count, position.Position);
                    await context.Channel.SendMessageAsync(embed: embed).ConfigureAwait(false);

                    // Small delay between embeds to avoid rate limiting
                    if (i < batchEggList.Count - 1)
                    {
                        await Task.Delay(500);
                    }
                }
            }
        }

        private static Embed CreateMysteryEggEmbed(SocketCommandContext context, T pk, int eggNumber, int totalEggs, int queuePosition)
        {
            var embedBuilder = new EmbedBuilder()
                .WithColor(global::Discord.Color.Gold)
                .WithTitle($"🥚 Huevo Misterioso número {eggNumber} de {totalEggs}")
                .WithDescription("¡Un huevo misterioso que contiene un Pokémon aleatorio!")
                .WithImageUrl("https://raw.githubusercontent.com/hexbyt3/sprites/main/mysteryegg3.png")
                .WithFooter($"Intercambio por Lotes número {eggNumber} de {totalEggs}" + (eggNumber == 1 ? $" | Posición: {queuePosition}" : ""))
                .WithAuthor(new EmbedAuthorBuilder()
                    .WithName($"Huevo Misterioso para {context.User.Username}")
                    .WithIconUrl(context.User.GetAvatarUrl() ?? context.User.GetDefaultAvatarUrl())
                    .WithUrl("https://genpkm.com"));

            return embedBuilder.Build();
        }

        private static int GenerateUniqueTradeID()
        {
            long timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            int randomValue = Random.Shared.Next(1000);
            return (int)((timestamp % int.MaxValue) * 1000 + randomValue);
        }

        /// <summary>
        /// Generates a legal mystery egg with shiny status, perfect IVs, and hidden ability if available.
        /// </summary>
        /// <param name="maxAttempts">Maximum number of species to try before giving up</param>
        /// <returns>A legal egg Pokemon, or null if generation failed</returns>
        public static T? GenerateLegalMysteryEgg(int maxAttempts = DefaultMaxGenerationAttempts)
        {
            // Generate eggs with desired attributes (shiny, 6IV, HA) by requesting them in the ShowdownSet
            // This ensures proper correlation for BDSP and clean generation for all games

            var context = GetContext();
            if (context == EntityContext.None)
                return null;

            var breedableSpecies = GetBreedableSpecies(context);
            if (breedableSpecies.Count == 0)
                return null;

            var random = new Random();
            var shuffled = breedableSpecies.OrderBy(_ => random.Next()).Take(maxAttempts).ToList();

            var sav = AutoLegalityWrapper.GetTrainerInfo<T>();

            // Temporarily set game priority to ensure eggs generate for the correct game
            var originalPriority = APILegality.PriorityOrder?.ToList() ?? [];
            APILegality.PriorityOrder = GetPriorityOrder();

            try
            {
                foreach (var species in shuffled)
                {
                    var set = CreateEggShowdownSet(species, context);
                    var template = AutoLegalityWrapper.GetTemplate(set);

                    // Use ALM's GenerateEgg method to properly generate eggs
                    var pk = sav.GenerateEgg(template, out var result);

                    if (pk == null || result != LegalizationResult.Regenerated)
                        continue;

                    pk = EntityConverter.ConvertToType(pk, typeof(T), out _) ?? pk;
                    if (pk is not T validPk)
                        continue;

                    var la = new LegalityAnalysis(validPk);
                    if (la.Valid)
                        return validPk;
                }
            }
            finally
            {
                APILegality.PriorityOrder = originalPriority;
            }

            return null;
        }

        private static ShowdownSet CreateEggShowdownSet(ushort species, EntityContext context)
        {
            var speciesName = GameInfo.Strings.Species[species];
            var setString = $"{speciesName}\nShiny: Yes\nIVs: 31/31/31/31/31/31";

            // Try to add hidden ability if available
            var hiddenAbilityName = GetHiddenAbilityName(species, context);
            if (!string.IsNullOrEmpty(hiddenAbilityName))
                setString += $"\nAbility: {hiddenAbilityName}";

            return new ShowdownSet(setString);
        }

        private static string? GetHiddenAbilityName(ushort species, EntityContext context)
        {
            var personalTable = GetPersonalTable(context);
            if (personalTable == null)
                return null;

            try
            {
                var pi = personalTable.GetFormEntry(species, 0);
                if (pi is IPersonalAbility12H piH)
                {
                    var hiddenAbilityID = piH.AbilityH;
                    // Check if the hidden ability is different from regular abilities and valid
                    if (hiddenAbilityID > 0 && hiddenAbilityID < GameInfo.Strings.Ability.Count)
                        return GameInfo.Strings.Ability[hiddenAbilityID];
                }
            }
            catch (Exception ex)
            {
                LogUtil.LogSafe(ex, $"Error al obtener la habilidad oculta para la especie {species}");
            }

            return null;
        }

        private static List<ushort> GetBreedableSpecies(EntityContext context)
        {
            lock (BreedableSpeciesCache)
            {
                if (BreedableSpeciesCache.TryGetValue(context, out var cached))
                    return cached;
            }

            var personalTable = GetPersonalTable(context);
            if (personalTable == null)
                return [];

            var breedable = new List<ushort>();

            for (ushort species = 1; species <= personalTable.MaxSpeciesID; species++)
            {
                if (!Breeding.CanHatchAsEgg(species))
                    continue;

                if (!personalTable.IsSpeciesInGame(species))
                    continue;

                breedable.Add(species);
            }

            lock (BreedableSpeciesCache)
            {
                BreedableSpeciesCache[context] = breedable;
            }

            return breedable;
        }

        private static EntityContext GetContext() => typeof(T).Name switch
        {
            "PB8" => EntityContext.Gen8b,
            "PK8" => EntityContext.Gen8,
            "PK9" => EntityContext.Gen9,
            _ => EntityContext.None
        };

        private static List<GameVersion> GetPriorityOrder() => GetContext() switch
        {
            EntityContext.Gen8b => [GameVersion.BD, GameVersion.SP],
            EntityContext.Gen8 => [GameVersion.SW, GameVersion.SH],
            EntityContext.Gen9 => [GameVersion.SL, GameVersion.VL],
            _ => [] // Return empty list for unsupported contexts
        };

        private static IPersonalTable? GetPersonalTable(EntityContext context) => context switch
        {
            EntityContext.Gen8b => PersonalTable.BDSP,
            EntityContext.Gen8 => PersonalTable.SWSH,
            EntityContext.Gen9 => PersonalTable.SV,
            _ => null
        };

        private async Task ProcessMysteryEggTradeAsync(int code)
        {
            var mysteryEgg = GenerateLegalMysteryEgg();
            if (mysteryEgg == null)
            {
                await ReplyAsync("Error al generar un huevo misterioso legal. Por favor, inténtalo de nuevo más tarde.").ConfigureAwait(false);
                return;
            }

            var sig = Context.User.GetFavor();
            await QueueHelper<T>.AddToQueueAsync(
                Context, code, Context.User.Username, sig, mysteryEgg,
                PokeRoutineType.LinkTrade, PokeTradeType.Specific, Context.User,
                isMysteryEgg: true, lgcode: GenerateRandomPictocodes(3)
            ).ConfigureAwait(false);

            if (Context.Message is IUserMessage userMessage)
                _ = DeleteMessageAfterDelay(userMessage, 2000);
        }

        private static async Task DeleteMessageAfterDelay(IUserMessage message, int delayMilliseconds)
        {
            await Task.Delay(delayMilliseconds).ConfigureAwait(false);
            try
            {
                await message.DeleteAsync().ConfigureAwait(false);
            }
            catch
            {
                // Message may have already been deleted
            }
        }

        private static List<Pictocodes> GenerateRandomPictocodes(int count)
        {
            var random = new Random();
            var values = Enum.GetValues<Pictocodes>();
            var result = new List<Pictocodes>(count);
            for (int i = 0; i < count; i++)
                result.Add(values[random.Next(values.Length)]);
            return result;
        }
    }
}
