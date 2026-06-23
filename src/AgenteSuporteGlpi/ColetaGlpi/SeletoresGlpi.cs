namespace AgenteSuporteGlpi.ColetaGlpi;

public sealed class SeletoresGlpi
{
    public string CampoUsuario { get; init; } = "input[name='login_name']";
    public string CampoSenha { get; init; } = "input[name='login_password']";
    public string BotaoEntrar { get; init; } = "button[type='submit']";
    public string LinhaChamado { get; init; } = "table tbody tr";
    public string ConteudoChamado { get; init; } = "#ticket-content";
    public string LinkProximaPagina { get; init; } = "a:has-text('Próximo')";
}
