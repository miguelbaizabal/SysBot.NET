using PKHeX.Core;
using SysBot.Base;
using System;
using System.Collections.Generic;
using System.IO;

namespace SysBot.Pokemon;

public class PokemonPool<T>(BaseConfig Settings) : List<T>
    where T : PKM, new()
{
    private readonly int ExpectedSize = new T().Data.Length;
    private bool Randomized => Settings.Shuffled;

    public readonly Dictionary<string, LedyRequest<T>> Files = [];
    private int Counter;

    public T GetRandomPoke()
    {
        var choice = this[Counter];
        Counter = (Counter + 1) % Count;
        if (Counter == 0 && Randomized)
            Shuffle(this, 0, Count, Util.Rand);
        return choice;
    }

    public static void Shuffle(IList<T> items, int start, int end, Random rnd)
    {
        for (int i = start; i < end; i++)
        {
            int index = i + rnd.Next(end - i);
            (items[index], items[i]) = (items[i], items[index]);
        }
    }

    public T GetRandomSurprise()
    {
        while (true)
        {
            var rand = GetRandomPoke();
            if (DisallowRandomRecipientTrade(rand))
                continue;
            return rand;
        }
    }

    public bool Reload(string path, SearchOption opt = SearchOption.AllDirectories)
    {
        if (!Directory.Exists(path))
            return false;
        Clear();
        Files.Clear();
        return LoadFolder(path, opt);
    }

    public bool LoadFolder(string path, SearchOption opt = SearchOption.AllDirectories)
    {
        if (!Directory.Exists(path))
            return false;

        var loadedAny = false;
        var files = Directory.EnumerateFiles(path, "*", opt);
        var matchFiles = LoadUtil.GetFilesOfSize(files, ExpectedSize);

        int surpriseBlocked = 0;
        foreach (var file in matchFiles)
        {
            var data = File.ReadAllBytes(file);
            var prefer = EntityFileExtension.GetContextFromExtension(file);
            var pkm = EntityFormat.GetFromBytes(data, prefer);
            if (pkm is null)
                continue;
            if (pkm is not T dest)
                continue;

            if (dest.Species == 0)
            {
                LogUtil.LogInfo("Omitido: El archivo proporcionado no es válido: " + dest.FileName, nameof(PokemonPool<T>));
                continue;
            }

            var la = new LegalityAnalysis(dest);
            if (!la.Valid)
            {
                var reason = la.Report();
                LogUtil.LogInfo($"Omitido: El archivo proporcionado no es legal: {dest.FileName} -- {reason}", nameof(PokemonPool<T>));
                continue;
            }
            if (!dest.CanBeTraded(la.EncounterOriginal))
            {
                LogUtil.LogInfo($"Omitido: El archivo proporcionado no puede ser intercambiado: {dest.FileName}", nameof(PokemonPool<T>));
                continue;
            }

            if (typeof(T) == typeof(PK8) && DisallowRandomRecipientTrade(dest))
            {
                LogUtil.LogInfo($"El archivo proporcionado fue cargado pero no puede ser intercambiado por Intercambio Sorpresa: {dest.FileName}", nameof(PokemonPool<T>));
                surpriseBlocked++;
            }

            if (Settings.Legality.ResetHOMETracker && dest is IHomeTrack h)
                h.Tracker = 0;

            var fn = Path.GetFileNameWithoutExtension(file);
            fn = StringsUtil.Sanitize(fn);

            // Since file names can be sanitized to the same string, only add one of them.
            if (!Files.ContainsKey(fn))
            {
                Add(dest);
                Files.Add(fn, new LedyRequest<T>(dest, fn));
            }
            else
            {
                LogUtil.LogInfo("El archivo proporcionado no fue agregado debido a nombre duplicado: " + dest.FileName, nameof(PokemonPool<T>));
            }
            loadedAny = true;
        }

        if (typeof(T) == typeof(PK8) && surpriseBlocked == Count)
            LogUtil.LogInfo("El intercambio sorpresa fallará; no se cargó ningún archivo compatible.", nameof(PokemonPool<T>));

        return loadedAny;
    }

    public static bool DisallowRandomRecipientTrade(T pk)
    {
        // Surprise Trade currently bans Mythicals and Legendaries, not Sub-Legendaries.
        if (SpeciesCategory.IsLegendary(pk.Species))
            return true;
        if (SpeciesCategory.IsMythical(pk.Species))
            return true;

        return false;
    }
}
