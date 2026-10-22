namespace Corpro.Aplicacion.Comun;

/// <summary>Resultado de un caso de uso: valor o errores de validación por campo.</summary>
public sealed class Resultado<T>
{
    public T? Valor { get; }
    public IReadOnlyDictionary<string, string[]> Errores { get; }
    public bool EsExitoso => Errores.Count == 0;

    private Resultado(T? valor, IReadOnlyDictionary<string, string[]> errores)
    {
        Valor = valor;
        Errores = errores;
    }

    public static Resultado<T> Exito(T valor) => new(valor, new Dictionary<string, string[]>());
    public static Resultado<T> Fallo(IReadOnlyDictionary<string, string[]> errores) => new(default, errores);
    public static Resultado<T> Fallo(string campo, string mensaje) => new(default, new Dictionary<string, string[]> { [campo] = [mensaje] });
}
