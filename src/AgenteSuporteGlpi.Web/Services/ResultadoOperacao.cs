namespace AgenteSuporteGlpi.Web.Services;

public class ResultadoOperacao
{
    public bool Sucesso { get; init; }
    public string Mensagem { get; init; } = string.Empty;

    public static ResultadoOperacao CriarSucesso(string mensagem)
        => new() { Sucesso = true, Mensagem = mensagem };

    public static ResultadoOperacao CriarFalha(string mensagem)
        => new() { Sucesso = false, Mensagem = mensagem };
}
