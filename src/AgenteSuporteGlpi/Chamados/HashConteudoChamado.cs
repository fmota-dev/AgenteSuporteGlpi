using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace AgenteSuporteGlpi.Chamados;

public static partial class HashConteudoChamado
{
    public static string Calcular(string conteudo)
    {
        ArgumentNullException.ThrowIfNull(conteudo);

        var normalizado = EspacosDuplicados().Replace(conteudo.Trim(), " ");
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(normalizado));

        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    [GeneratedRegex(@"\s+")]
    private static partial Regex EspacosDuplicados();
}
