using System;
using System.ComponentModel;

namespace SysBot.Pokemon;

public class FavoredPrioritySettings : IFavoredCPQSetting
{
    private const string Operation = nameof(Operation);
    private const string Configure = nameof(Configure);
    public override string ToString() => "Configuración de Favoritismo";

    // We want to allow hosts to give preferential treatment, while still providing service to users without favor.
    // These are the minimum values that we permit. These values yield a fair placement for the favored.
    private const int _mfi = 2;
    private const float _bmin = 1;
    private const float _bmax = 3;
    private const float _mexp = 0.5f;
    private const float _mmul = 0.1f;

    private int _minimumFreeAhead = _mfi;
    private float _bypassFactor = 1.5f;
    private float _exponent = 0.777f;
    private float _multiply = 0.5f;

    [Category(Operation), Description("Determina cómo se calcula la posición de inserción de los usuarios favoritos. \"None\" evitará que se aplique ningún favoritismo.")]
    public FavoredMode Mode { get; set; }

    [Category(Configure), Description("Insertado después de (usuarios no favoritos)^(exponente) usuarios no favoritos.")]
    public float Exponent
    {
        get => _exponent;
        set => _exponent = Math.Max(_mexp, value);
    }

    [Category(Configure), Description("Multiply: Insertado después de (usuarios no favoritos)*(multiplicar) usuarios no favoritos. Establecer esto en 0.2 agrega después del 20% de usuarios.")]
    public float Multiply
    {
        get => _multiply;
        set => _multiply = Math.Max(_mmul, value);
    }

    [Category(Configure), Description("Número de usuarios no favoritos a no saltarse. Esto solo se aplica si hay un número significativo de usuarios no favoritos en la cola.")]
    public int MinimumFreeAhead
    {
        get => _minimumFreeAhead;
        set => _minimumFreeAhead = Math.Max(_mfi, value);
    }

    [Category(Configure), Description("Número mínimo de usuarios no favoritos en cola para causar que {MinimumFreeAhead} sea aplicado. Cuando el número mencionado es mayor que este valor, un usuario favorito no se coloca por delante de {MinimumFreeAhead} usuarios no favoritos.")]
    public int MinimumFreeBypass => (int)Math.Ceiling(MinimumFreeAhead * MinimumFreeBypassFactor);

    [Category(Configure), Description("Escalar que se multiplica con {MinimumFreeAhead} para determinar el valor de {MinimumFreeBypass}.")]
    public float MinimumFreeBypassFactor
    {
        get => _bypassFactor;
        set => _bypassFactor = Math.Min(_bmax, Math.Max(_bmin, value));
    }
}
