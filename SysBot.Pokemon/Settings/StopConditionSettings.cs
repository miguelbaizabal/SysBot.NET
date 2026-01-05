using PKHeX.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace SysBot.Pokemon;

public class StopConditionSettings
{
    private const string StopConditions = nameof(StopConditions);
    public override string ToString() => "Configuración de Condiciones de Detención";

    [Category(StopConditions), Description("Detiene solo en Pokémon de esta especie. Sin restricciones si se establece en \"None\".")]
    public Species StopOnSpecies { get; set; }

    [Category(StopConditions), Description("Detiene solo en Pokémon con este FormID. Sin restricciones si se deja en blanco.")]
    public int? StopOnForm { get; set; }

    [Category(StopConditions), Description("Detiene solo en Pokémon de la naturaleza especificada.")]
    public Nature TargetNature { get; set; } = Nature.Random;

    [Category(StopConditions), Description("Mínimos IVs aceptados en el formato HP/Atk/Def/SpA/SpD/Spe. Usa \"x\" para IVs sin revisar y \"/\" como separador.")]
    public string TargetMinIVs { get; set; } = "";

    [Category(StopConditions), Description("Máximos IVs aceptados en el formato HP/Atk/Def/SpA/SpD/Spe. Usa \"x\" para IVs sin revisar y \"/\" como separador.")]
    public string TargetMaxIVs { get; set; } = "";

    [Category(StopConditions), Description("Selecciona el tipo de shiny para detenerse.")]
    public TargetShinyType ShinyTarget { get; set; } = TargetShinyType.DisableOption;

    [Category(StopConditions), Description("Permite filtrar por tamaño mínimo o máximo para detenerse.")]
    public TargetHeightType HeightTarget { get; set; } = TargetHeightType.DisableOption;

    [Category(StopConditions), Description("Detiene solo en Pokémon que tienen una marca.")]
    public bool MarkOnly { get; set; }

    [Category(StopConditions), Description("Lista de marcas a ignorar separadas por comas. Usa el nombre completo, por ejemplo \"Uncommon Mark, Dawn Mark, Prideful Mark\".")]
    public string UnwantedMarks { get; set; } = "";

    [Category(StopConditions), Description("Mantiene presionado el botón Capturar para grabar un clip de 30 segundos cuando EncounterBot o Fossilbot encuentran un Pokémon coincidente.")]
    public bool CaptureVideoClip { get; set; }

    [Category(StopConditions), Description("Tiempo adicional en milisegundos que se esperará después de que se detecte un encuentro antes de presionar Capturar para EncounterBot o Fossilbot.")]
    public int ExtraTimeWaitCaptureVideo { get; set; } = 10000;

    [Category(StopConditions), Description("Si se establece en TRUE, coincidirá tanto con la configuración de ShinyTarget como con la de TargetIVs. De lo contrario, buscará una coincidencia de ShinyTarget o de TargetIVs.")]
    public bool MatchShinyAndIV { get; set; } = true;

    [Category(StopConditions), Description("Si no está vacío, la cadena proporcionada se antepondrá al mensaje de registro de resultado encontrado para enviar alertas a quien usted especifique. Para Discord, use <@userIDnumber> para mencionar.")]
    public string MatchFoundEchoMention { get; set; } = string.Empty;

    public static bool EncounterFound<T>(T pk, int[] targetminIVs, int[] targetmaxIVs, StopConditionSettings settings, IReadOnlyList<string>? marklist) where T : PKM
    {
        // Match Nature and Species if they were specified.
        if (settings.StopOnSpecies != Species.None && settings.StopOnSpecies != (Species)pk.Species)
            return false;

        if (settings.StopOnForm.HasValue && settings.StopOnForm != pk.Form)
            return false;

        if (settings.TargetNature != Nature.Random && settings.TargetNature != pk.Nature)
            return false;

        // Return if it doesn't have a mark, or it has an unwanted mark.
        var unmarked = pk is IRibbonIndex m && !HasMark(m);
        var unwanted = marklist is not null && pk is IRibbonIndex m2 && settings.IsUnwantedMark(GetMarkName(m2), marklist);
        if (settings.MarkOnly && (unmarked || unwanted))
            return false;

        if (settings.ShinyTarget != TargetShinyType.DisableOption)
        {
            bool shinymatch = settings.ShinyTarget switch
            {
                TargetShinyType.AnyShiny => pk.IsShiny,
                TargetShinyType.NonShiny => !pk.IsShiny,
                TargetShinyType.StarOnly => pk.IsShiny && pk.ShinyXor != 0,
                TargetShinyType.SquareOnly => pk.ShinyXor == 0,
                TargetShinyType.DisableOption => true,
                _ => throw new ArgumentException(nameof(TargetShinyType)),
            };

            // If we only needed to match one of the criteria and it shiny match'd, return true.
            // If we needed to match both criteria, and it didn't shiny match, return false.
            if (!settings.MatchShinyAndIV && shinymatch)
                return true;
            if (settings.MatchShinyAndIV && !shinymatch)
                return false;
        }

        if (settings.HeightTarget != TargetHeightType.DisableOption && pk is PK8 p)
        {
            var value = p.HeightScalar;
            bool heightmatch = settings.HeightTarget switch
            {
                TargetHeightType.MinOnly => value is 0,
                TargetHeightType.MaxOnly => value is 255,
                TargetHeightType.MinOrMax => value is 0 or 255,
                _ => throw new ArgumentException(nameof(TargetHeightType)),
            };

            if (!heightmatch)
                return false;
        }

        // Reorder the speed to be last.
        Span<int> pkIVList = stackalloc int[6];
        pk.GetIVs(pkIVList);
        (pkIVList[5], pkIVList[3], pkIVList[4]) = (pkIVList[3], pkIVList[4], pkIVList[5]);

        for (int i = 0; i < 6; i++)
        {
            if (targetminIVs[i] > pkIVList[i] || targetmaxIVs[i] < pkIVList[i])
                return false;
        }
        return true;
    }

    public static void InitializeTargetIVs(PokeTradeHubConfig config, out int[] min, out int[] max)
    {
        min = ReadTargetIVs(config.StopConditions, true);
        max = ReadTargetIVs(config.StopConditions, false);
    }

    private static int[] ReadTargetIVs(StopConditionSettings settings, bool min)
    {
        int[] targetIVs = new int[6];
        char[] split = ['/'];

        string[] splitIVs = min
            ? settings.TargetMinIVs.Split(split, StringSplitOptions.RemoveEmptyEntries)
            : settings.TargetMaxIVs.Split(split, StringSplitOptions.RemoveEmptyEntries);

        // Only accept up to 6 values.  Fill it in with default values if they don't provide 6.
        // Anything that isn't an integer will be a wild card.
        for (int i = 0; i < 6; i++)
        {
            if (i < splitIVs.Length)
            {
                var str = splitIVs[i];
                if (int.TryParse(str, out var val))
                {
                    targetIVs[i] = val;
                    continue;
                }
            }
            targetIVs[i] = min ? 0 : 31;
        }
        return targetIVs;
    }

    private static bool HasMark(IRibbonIndex pk)
    {
        for (var mark = RibbonIndex.MarkLunchtime; mark <= RibbonIndex.MarkSlump; mark++)
        {
            if (pk.GetRibbon((int)mark))
                return true;
        }
        return false;
    }

    public static ReadOnlySpan<BattleTemplateToken> TokenOrder =>
    [
        BattleTemplateToken.FirstLine,
        BattleTemplateToken.Shiny,
        BattleTemplateToken.Nature,
        BattleTemplateToken.IVs,
    ];

    public static string GetPrintName(PKM pk)
    {
        const LanguageID lang = LanguageID.English;
        var settings = new BattleTemplateExportSettings(TokenOrder, lang);
        var set = ShowdownParsing.GetShowdownText(pk, settings);

        // Since we can match on Min/Max Height for transfer to future games, display it.
        if (pk is IScaledSize p)
            set += $"\nHeight: {p.HeightScalar}";

        // Add the mark if it has one.
        if (pk is IRibbonIndex r)
        {
            var rstring = GetMarkName(r);
            if (!string.IsNullOrEmpty(rstring))
                set += $"\nPokémon has the **{GetMarkName(r)}**!";
        }
        return set;
    }

    public static void ReadUnwantedMarks(StopConditionSettings settings, out IReadOnlyList<string> marks) =>
        marks = settings.UnwantedMarks.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).ToList();

    public virtual bool IsUnwantedMark(string mark, IReadOnlyList<string> marklist) => marklist.Contains(mark);

    public static string GetMarkName(IRibbonIndex pk)
    {
        for (var mark = RibbonIndex.MarkLunchtime; mark <= RibbonIndex.MarkSlump; mark++)
        {
            if (pk.GetRibbon((int)mark))
                return GameInfo.Strings.Ribbons.GetName($"Ribbon{mark}");
        }
        return "";
    }
}

public enum TargetShinyType
{
    DisableOption,  // Doesn't care
    NonShiny,       // Match nonshiny only
    AnyShiny,       // Match any shiny regardless of type
    StarOnly,       // Match star shiny only
    SquareOnly,     // Match square shiny only
}

public enum TargetHeightType
{
    DisableOption,  // Doesn't care
    MinOnly,        // 0 Height only
    MaxOnly,        // 255 Height only
    MinOrMax,       // 0 or 255 Height
}
