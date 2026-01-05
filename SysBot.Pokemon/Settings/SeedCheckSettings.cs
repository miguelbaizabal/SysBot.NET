using System.ComponentModel;

namespace SysBot.Pokemon;

public class SeedCheckSettings
{
    private const string FeatureToggle = nameof(FeatureToggle);
    public override string ToString() => "Configuración de Verificación de Semillas";

    [Category(FeatureToggle), Description("Cuando está habilitado, las verificaciones de semillas devolverán todos los resultados posibles en lugar del primer resultado válido.")]
    public bool ShowAllZ3Results { get; set; }

    [Category(FeatureToggle), Description("Permite devolver solo el marco shiny más cercano, el primer marco shiny con estrella y cuadrado, o los primeros tres marcos shiny.")]
    public SeedCheckResults ResultDisplayMode { get; set; }
}

public enum SeedCheckResults
{
    ClosestOnly,            // Only gets the first shiny
    FirstStarAndSquare,     // Gets the first star shiny and first square shiny
    FirstThree,             // Gets the first three frames
}
