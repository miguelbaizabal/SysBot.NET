using Discord;
using Discord.WebSocket;
using System.Linq;

namespace SysBot.Pokemon.Discord;

public static class MedalHelpers
{
    public static int GetCurrentMilestone(int totalTrades)
    {
        int[] milestones = [700, 650, 600, 550, 500, 450, 400, 350, 300, 250, 200, 150, 100, 50, 1];
        return milestones.FirstOrDefault(m => totalTrades >= m, 0);
    }

    public static Embed CreateMedalsEmbed(SocketUser user, int milestone, int totalTrades)
    {
        string status = milestone switch
        {
            1 => "Entrenador Novato",
            50 => "Entrenador Principiante",
            100 => "Profesor Pokémon",
            150 => "Especialista Pokémon",
            200 => "Campeón Pokémon",
            250 => "Héroe Pokémon",
            300 => "Élite Pokémon",
            350 => "Comerciante Pokémon",
            400 => "Sabio Pokémon",
            450 => "Leyenda Pokémon",
            500 => "Maestro Regional",
            550 => "Maestro de Intercambio",
            600 => "Famoso Mundial",
            650 => "Maestro Pokémon",
            700 => "Dios Pokémon",
            _ => "Nuevo Entrenador"
        };

        string description = $"Intercambios Totales: **{totalTrades}**\n**Estado Actual:** {status}";

        if (milestone > 0)
        {
            string imageUrl = $"https://raw.githubusercontent.com/Secludedly/ZE-FusionBot-Sprite-Images/main/{milestone:D3}.png";
            return new EmbedBuilder()
                .WithTitle($"Estado de intercambio de {user.Username}")
                .WithColor(new Color(255, 215, 0))
                .WithDescription(description)
                .WithThumbnailUrl(imageUrl)
                .Build();
        }
        else
        {
            return new EmbedBuilder()
                .WithTitle($"Estado de intercambio de {user.Username}")
                .WithColor(new Color(255, 215, 0))
                .WithDescription(description)
                .Build();
        }
    }
}
