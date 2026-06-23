namespace AgenteSuporteGlpi.Chamados;

public sealed record ResultadoMudancaChamado(bool EhNovo, bool FoiAlterado)
{
    public static ResultadoMudancaChamado Avaliar(string? hashAnterior, string hashAtual)
    {
        if (string.IsNullOrWhiteSpace(hashAtual))
        {
            throw new ArgumentException("Hash atual e obrigatorio.", nameof(hashAtual));
        }

        if (string.IsNullOrWhiteSpace(hashAnterior))
        {
            return new ResultadoMudancaChamado(EhNovo: true, FoiAlterado: true);
        }

        var foiAlterado = !string.Equals(hashAnterior, hashAtual, StringComparison.Ordinal);
        return new ResultadoMudancaChamado(EhNovo: false, FoiAlterado: foiAlterado);
    }
}
