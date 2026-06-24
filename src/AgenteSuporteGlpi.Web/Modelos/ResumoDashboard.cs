namespace AgenteSuporteGlpi.Web.Modelos;

public sealed record ResumoDashboard
{
    public int TotalChamados { get; init; }
    public int TotalAnalisados { get; init; }
    public int TotalSistemasIdentificados { get; init; }
    public int AltaConfianca { get; init; }
    public List<ChamadoPorSistema> ChamadosPorSistema { get; init; } = [];
    public List<ConfiancaPorNivel> ConfiancaPorNivel { get; init; } = [];
    public List<ChamadoRecente> ChamadosRecentes { get; init; } = [];
}

public sealed record ChamadoPorSistema
{
    public string Sistema { get; init; } = "";
    public int Quantidade { get; init; }
}

public sealed record ConfiancaPorNivel
{
    public string Nivel { get; init; } = "";
    public int Quantidade { get; init; }
}

public sealed record ChamadoRecente
{
    public int Numero { get; init; }
    public string Titulo { get; init; } = "";
    public string Status { get; init; } = "";
    public string Sistema { get; init; } = "";
    public string Confianca { get; init; } = "";
    public string Data { get; init; } = "";
    public string Link { get; init; } = "";
}

public sealed record AnaliseIaViewModel
{
    public int NumeroChamado { get; init; }
    public string TituloChamado { get; init; } = "";
    public string Sistema { get; init; } = "";
    public string ResumoTecnico { get; init; } = "";
    public string PerguntasSolicitante { get; init; } = "";
    public string ProximosPassos { get; init; } = "";
    public string DataAnalise { get; init; } = "";
    public string Link { get; init; } = "";
}

public sealed record ChamadoDetalheViewModel
{
    public int Numero { get; init; }
    public string Titulo { get; init; } = "";
    public string Status { get; init; } = "";
    public string Prioridade { get; init; } = "";
    public string Responsavel { get; init; } = "";
    public string? Solicitante { get; init; }
    public string? Categoria { get; init; }
    public string DataAbertura { get; init; } = "";
    public string DataUltimaAtualizacao { get; init; } = "";
    public string Link { get; init; } = "";
    public string Sistema { get; init; } = "";
    public string Confianca { get; init; } = "";
    public int Pontuacao { get; init; }
    public string TermosEncontrados { get; init; } = "";
    public string? ResumoTecnicoIa { get; init; }
    public string? PerguntasIa { get; init; }
    public string? ProximosPassosIa { get; init; }
    public int TotalColetas { get; init; }
    public string? UltimaColeta { get; init; }
}
