namespace AgenteSuporteGlpi.Web.Models;

public class MPerfil
{
    public int Codigo { get; set; }
    public string Nome { get; set; } = string.Empty;
    public int CodigoStatus { get; set; }
    public string NomeStatus { get; set; } = "Ativo";
    public int QuantidadeUsuariosVinculados { get; set; }
    public DateTime? DataCriacao { get; set; }
}
