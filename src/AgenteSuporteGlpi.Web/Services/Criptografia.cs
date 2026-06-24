using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;

namespace AgenteSuporteGlpi.Web.Services;

public class Criptografia(IConfiguration configuracao)
{
    private const string PrefixoHashIdentity = "AQAAAA";
    private readonly PasswordHasher<object> _hashSenha = new();
    private readonly byte[] _chave = ConverterBase64(configuracao["Criptografia:Chave"]);
    private readonly byte[] _vetorInicializacao = ConverterBase64(configuracao["Criptografia:VetorInicializacao"]);

    public string Criptografar(string texto)
    {
        if (string.IsNullOrWhiteSpace(texto))
        {
            return string.Empty;
        }

        using var aes = Aes.Create();
        aes.Key = _chave;
        aes.IV = _vetorInicializacao;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;

        var bytesTexto = Encoding.UTF8.GetBytes(texto.Trim());
        using var transformador = aes.CreateEncryptor();
        var bytesCriptografados = transformador.TransformFinalBlock(bytesTexto, 0, bytesTexto.Length);

        return Convert.ToBase64String(bytesCriptografados);
    }

    public string Descriptografar(string textoCriptografado)
    {
        if (string.IsNullOrWhiteSpace(textoCriptografado))
        {
            return string.Empty;
        }

        using var aes = Aes.Create();
        aes.Key = _chave;
        aes.IV = _vetorInicializacao;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;

        var bytesCriptografados = Convert.FromBase64String(textoCriptografado);
        using var transformador = aes.CreateDecryptor();
        var bytesDescriptografados = transformador.TransformFinalBlock(bytesCriptografados, 0, bytesCriptografados.Length);

        return Encoding.UTF8.GetString(bytesDescriptografados);
    }

    public string GerarHashSenha(string senha)
        => _hashSenha.HashPassword(new object(), senha);

    public bool VerificarSenha(string senhaInformada, string senhaPersistida)
    {
        if (string.IsNullOrWhiteSpace(senhaInformada) || string.IsNullOrWhiteSpace(senhaPersistida))
        {
            return false;
        }

        if (!SenhaEstaHashada(senhaPersistida))
        {
            return string.Equals(
                senhaInformada,
                senhaPersistida,
                StringComparison.Ordinal);
        }

        var resultado = _hashSenha.VerifyHashedPassword(new object(), senhaPersistida, senhaInformada);
        return resultado is PasswordVerificationResult.Success or PasswordVerificationResult.SuccessRehashNeeded;
    }

    public bool SenhaEstaHashada(string senhaPersistida)
        => !string.IsNullOrWhiteSpace(senhaPersistida) &&
           senhaPersistida.StartsWith(PrefixoHashIdentity, StringComparison.Ordinal);

    private static byte[] ConverterBase64(string? valor)
    {
        if (string.IsNullOrWhiteSpace(valor))
        {
            throw new InvalidOperationException("As configurações de criptografia não foram encontradas no appsettings.");
        }

        try
        {
            return Convert.FromBase64String(valor);
        }
        catch (FormatException excecao)
        {
            throw new InvalidOperationException("Os valores de chave ou vetor de inicialização da criptografia não estão em Base64 válido.", excecao);
        }
    }
}
