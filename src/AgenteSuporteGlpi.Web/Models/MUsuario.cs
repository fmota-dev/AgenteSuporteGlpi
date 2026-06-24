namespace AgenteSuporteGlpi.Web.Models;

public class MUsuario
{
    public int Codigo { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string SenhaCriptografada { get; set; } = string.Empty;
    public int CodigoStatus { get; set; }
    public int CodigoPerfil { get; set; }
    public string NomeStatus { get; set; } = "Ativo";
    public string NomePerfil { get; set; } = string.Empty;
    public bool BloqueioHabilitado { get; set; } = true;
    public int FalhasAcesso { get; set; }
    public DateTimeOffset? FimBloqueio { get; set; }
    public DateTime? DataUltimoAcesso { get; set; }
}
