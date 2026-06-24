using AgenteSuporteGlpi.Banco;
using AgenteSuporteGlpi.Chamados;
using AgenteSuporteGlpi.Configuracao;
using AgenteSuporteGlpi.Contratos;
using AgenteSuporteGlpi.Execucoes;
using AgenteSuporteGlpi.IA;
using AgenteSuporteGlpi.Sistemas;
using AgenteSuporteGlpi.SqlServer;
using AgenteSuporteGlpi.ColetaGlpi;
using ConsultorSQLServer.Security;
using ConsultorSQLServer.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace AgenteSuporteGlpi;

internal sealed partial class Program : IHostedService
{
    private readonly IHostApplicationLifetime _lifetime;
    private readonly IColetorGlpi _coletor;
    private readonly IRepositorioChamados _repositorio;
    private readonly InicializadorBanco _dbInit;
    private readonly ConfiguracaoGlpi _configuracaoGlpi;
    private readonly IReadOnlyList<SistemaConfigurado> _sistemas;
    private readonly AnalisadorChamado _analisador;
    private readonly IConsultaSqlServerSomenteLeitura _consultaSql;

    private static bool _modoAnalise;

    public Program(
        IHostApplicationLifetime lifetime,
        IColetorGlpi coletor,
        IRepositorioChamados repositorio,
        InicializadorBanco dbInit,
        ConfiguracaoGlpi configuracaoGlpi,
        IReadOnlyList<SistemaConfigurado> sistemas,
        AnalisadorChamado analisador,
        IConsultaSqlServerSomenteLeitura consultaSql)
    {
        _lifetime = lifetime;
        _coletor = coletor;
        _repositorio = repositorio;
        _dbInit = dbInit;
        _configuracaoGlpi = configuracaoGlpi;
        _sistemas = sistemas;
        _analisador = analisador;
        _consultaSql = consultaSql;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            if (_modoAnalise)
                await ExecutarAnaliseAsync(cancellationToken);
            else
                await ExecutarColetaAsync(cancellationToken);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            Console.Error.WriteLine($"ERRO FATAL: {ex.Message}");
            Environment.ExitCode = 1;
        }
        finally
        {
            _lifetime.StopApplication();
        }
    }

    private async Task ExecutarColetaAsync(CancellationToken cancellationToken)
    {
        Console.WriteLine("=== Iniciando pipeline de coleta GLPI ===");

        await _dbInit.InicializarAsync(cancellationToken);
        Console.WriteLine("Banco inicializado.");

        var execucaoId = await _repositorio.IniciarExecucaoAsync("Coleta", cancellationToken);
        await _repositorio.RegistrarEventoAsync(execucaoId, "Info", "Inicio", "Pipeline de coleta iniciado", null, cancellationToken);

        var chamados = await _coletor.ColetarListaAsync(cancellationToken);
        Console.WriteLine($"Chamados encontrados via GLPI: {chamados.Count}");
        await _repositorio.RegistrarEventoAsync(execucaoId, "Info", "ColetaLista", $"Encontrados: {chamados.Count}", null, cancellationToken);

        var elegiveis = FiltroChamados.FiltrarElegiveis(chamados, _configuracaoGlpi.Responsavel);
        Console.WriteLine($"Chamados elegiveis apos filtro: {elegiveis.Count}");
        await _repositorio.RegistrarEventoAsync(execucaoId, "Info", "Filtro", $"Elegiveis: {elegiveis.Count}", null, cancellationToken);

        var novos = 0;
        var alterados = 0;
        var ignorados = 0;
        var comErro = 0;

        foreach (var chamado in elegiveis)
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                var detalhes = await _coletor.ColetarDetalhesAsync(chamado, cancellationToken);
                var hash = HashConteudoChamado.Calcular(detalhes.Descricao);
                var hashAnterior = await _repositorio.ObterUltimoHashAsync(chamado.Numero, cancellationToken);
                var resultado = ResultadoMudancaChamado.Avaliar(hashAnterior, hash);

                if (resultado.FoiAlterado)
                {
                    await _repositorio.SalvarChamadoAsync(detalhes, hash, cancellationToken);

                    var identificacao = IdentificadorSistemaPorPalavrasChave.Identificar(
                        detalhes.Titulo,
                        detalhes.Descricao ?? string.Empty,
                        _sistemas);

                    await _repositorio.PersistirIdentificacaoAsync(chamado.Numero, identificacao, cancellationToken);
                    await _repositorio.RegistrarEventoAsync(execucaoId, "Info", "Persistencia",
                        $"Chamado #{chamado.Numero} salvo. Sistema: {(identificacao.Sistema?.Nome ?? "?")} ({identificacao.Confianca}, {identificacao.Pontuacao}pts)",
                        chamado.Numero, cancellationToken);

                    if (resultado.EhNovo)
                    {
                        novos++;
                        Console.WriteLine($"  [#{chamado.Numero}] NOVO - {chamado.Titulo} -> {FormatarIdentificacao(identificacao)}");
                    }
                    else
                    {
                        alterados++;
                        Console.WriteLine($"  [#{chamado.Numero}] ALTERADO - {chamado.Titulo} -> {FormatarIdentificacao(identificacao)}");
                    }
                }
                else
                {
                    ignorados++;
                    Console.WriteLine($"  [#{chamado.Numero}] sem alteracoes - ignorado");
                }
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                Console.Error.WriteLine($"  [#{chamado.Numero}] ERRO: {ex.Message}");
                await _repositorio.RegistrarEventoAsync(execucaoId, "Erro", "Chamado",
                    $"Erro #{chamado.Numero}: {ex.Message}", chamado.Numero, cancellationToken);
                comErro++;
            }
        }

        var statusFinal = comErro > 0 ? "ConcluidoComErros" : "Concluido";
        await _repositorio.FinalizarExecucaoAsync(execucaoId, statusFinal, chamados.Count, novos + alterados, ignorados, comErro, null, cancellationToken);
        await _repositorio.RegistrarEventoAsync(execucaoId, "Info", "Fim",
            $"Execucao {statusFinal}: {novos} novos, {alterados} alterados, {ignorados} ignorados, {comErro} erros", null, cancellationToken);

        Console.WriteLine();
        Console.WriteLine("=== Resumo ===");
        Console.WriteLine($"  Encontrados : {chamados.Count}");
        Console.WriteLine($"  Elegiveis   : {elegiveis.Count}");
        Console.WriteLine($"  Novos       : {novos}");
        Console.WriteLine($"  Alterados   : {alterados}");
        Console.WriteLine($"  Ignorados   : {ignorados}");
        Console.WriteLine($"  Com erro    : {comErro}");
    }

    private async Task ExecutarAnaliseAsync(CancellationToken cancellationToken)
    {
        Console.WriteLine("=== Iniciando pipeline de analise IA ===");

        await _dbInit.InicializarAsync(cancellationToken);
        Console.WriteLine("Banco inicializado.");

        var execucaoId = await _repositorio.IniciarExecucaoAsync("AnaliseIA", cancellationToken);
        await _repositorio.RegistrarEventoAsync(execucaoId, "Info", "Inicio", "Pipeline de analise IA iniciado", null, cancellationToken);

        var chamados = await _repositorio.ObterChamadosNaoAnalisadosAsync(cancellationToken);
        Console.WriteLine($"Chamados pendentes de analise: {chamados.Count}");
        await _repositorio.RegistrarEventoAsync(execucaoId, "Info", "Consulta", $"Pendentes: {chamados.Count}", null, cancellationToken);

        var analisados = 0;
        var comErro = 0;

        foreach (var chamado in chamados)
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                Console.WriteLine($"  [#{chamado.Numero}] Analisando: {chamado.Titulo}...");

                var resultado = IdentificadorSistemaPorPalavrasChave.Identificar(
                    chamado.Titulo,
                    chamado.Descricao ?? string.Empty,
                    _sistemas);

                var contextoBanco = await EnriquecerContextoBancoAsync(resultado, cancellationToken);

                var contexto = new ContextoAnaliseChamado
                {
                    Chamado = chamado,
                    Identificacao = resultado,
                    ContextoBanco = contextoBanco
                };

                var analise = await _analisador.AnalisarAsync(contexto, cancellationToken);

                await _repositorio.SalvarAnaliseIaAsync(chamado.Numero, analise, cancellationToken);
                await _repositorio.MarcarAnalisadoPorIaAsync(chamado.Numero, cancellationToken);
                await _repositorio.RegistrarEventoAsync(execucaoId, "Info", "AnaliseIA",
                    $"Chamado #{chamado.Numero} analisado. Resumo: {analise.ResumoTecnico[..Math.Min(analise.ResumoTecnico.Length, 120)]}",
                    chamado.Numero, cancellationToken);

                SalvarLogAuditoriaIa(chamado.Numero, resultado, analise);

                analisados++;
                Console.WriteLine($"    IA: {analise.ResumoTecnico}");
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                Console.Error.WriteLine($"  [#{chamado.Numero}] ERRO IA: {ex.Message}");
                await _repositorio.RegistrarEventoAsync(execucaoId, "Erro", "AnaliseIA",
                    $"Erro #{chamado.Numero}: {ex.Message}", chamado.Numero, cancellationToken);
                comErro++;
            }
        }

        var statusFinal = comErro > 0 ? "ConcluidoComErros" : "Concluido";
        await _repositorio.FinalizarExecucaoAsync(execucaoId, statusFinal, chamados.Count, analisados, 0, comErro, null, cancellationToken);
        await _repositorio.RegistrarEventoAsync(execucaoId, "Info", "Fim",
            $"Analise IA {statusFinal}: {analisados} analisados, {comErro} erros", null, cancellationToken);

        Console.WriteLine();
        Console.WriteLine("=== Resumo Analise IA ===");
        Console.WriteLine($"  Pendentes    : {chamados.Count}");
        Console.WriteLine($"  Analisados   : {analisados}");
        Console.WriteLine($"  Com erro     : {comErro}");
    }

    private async Task<string?> EnriquecerContextoBancoAsync(ResultadoIdentificacaoSistema resultado, CancellationToken ct)
    {
        if (resultado.Confianca == NivelConfianca.NaoIdentificado || resultado.Sistema?.Bancos is null)
            return null;

        var alias = resultado.Sistema.Bancos.FirstOrDefault();
        if (string.IsNullOrWhiteSpace(alias))
            return null;

        try
        {
            var mapeamento = await _consultaSql.MapearBancoAsync(alias, string.Empty, ct);
            return $"Alias: {alias}\n{mapeamento}";
        }
        catch
        {
            return null;
        }
    }

    private static void SalvarLogAuditoriaIa(int numeroChamado, ResultadoIdentificacaoSistema identificacao, ResultadoAnaliseIa analise)
    {
        try
        {
            var dir = Path.Combine(AppContext.BaseDirectory, "logs");
            Directory.CreateDirectory(dir);

            var arquivo = Path.Combine(dir, $"ia-{numeroChamado}-{DateTimeOffset.UtcNow:yyyyMMdd-HHmmss}.txt");
            var conteudo = $"""
                === Auditoria Analise IA ===
                Data: {DateTimeOffset.UtcNow:O}
                Chamado: #{numeroChamado}
                Sistema: {(identificacao.Sistema?.Nome ?? "NaoIdentificado")}
                Confianca: {identificacao.Confianca} ({identificacao.Pontuacao}pts)
                Termos: {string.Join("; ", identificacao.TermosEncontrados)}

                --- Resumo Tecnico ---
                {analise.ResumoTecnico}

                --- Perguntas Solicitante ---
                {string.Join("\n", analise.PerguntasSolicitante.Select((p, i) => $"{i + 1}. {p}"))}

                --- Proximos Passos ---
                {string.Join("\n", analise.ProximosPassos.Select((p, i) => $"{i + 1}. {p}"))}
                """;

            File.WriteAllText(arquivo, conteudo);
            Console.WriteLine($"    Auditoria salva: {arquivo}");
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"    ERRO ao salvar auditoria IA: {ex.Message}");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    private static string FormatarIdentificacao(ResultadoIdentificacaoSistema resultado)
    {
        if (resultado.Confianca == NivelConfianca.NaoIdentificado)
            return "sistema nao identificado";

        return $"{(resultado.Sistema?.Nome ?? "?")} ({resultado.Confianca}, {resultado.Pontuacao}pts)";
    }

    private static async Task Main(string[] args)
    {
        _modoAnalise = args.Contains("--analisar");

        var host = Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((ctx, config) =>
            {
                config.AddUserSecrets<Program>(optional: false, reloadOnChange: false);
            })
            .ConfigureServices((ctx, services) =>
            {
                var config = ctx.Configuration;

                var configuracaoGlpi = config.GetSection("Glpi").Get<ConfiguracaoGlpi>();
                if (configuracaoGlpi is null && !_modoAnalise)
                    throw new InvalidOperationException("Secao 'Glpi' nao encontrada em appsettings.json.");
                configuracaoGlpi ??= new ConfiguracaoGlpi
                {
                    UrlBase = new Uri("http://localhost"),
                    UsuarioLogin = "-",
                    SenhaLogin = "-",
                    Responsavel = "-",
                    UserGlpiId = "0",
                    StatusParaColetar = [1]
                };

                if (!_modoAnalise)
                    ValidarConfiguracao(configuracaoGlpi);

                var configuracaoBrowser = config.GetSection("Browser").Get<ConfiguracaoBrowser>()
                    ?? new ConfiguracaoBrowser();

                var configuracaoBanco = config.GetSection("Banco").Get<ConfiguracaoBanco>()
                    ?? new ConfiguracaoBanco();

                var seletores = new SeletoresGlpi();

                var configuracaoIa = ConfiguracaoIa.Carregar(config);
                var agenteIa = FabricaAgente.Criar(configuracaoIa);

                services.AddSingleton(configuracaoGlpi);
                services.AddSingleton(configuracaoBrowser);
                services.AddSingleton(configuracaoBanco);
                var sistemas = ConfiguracaoSistemas.Carregar(config);
                services.AddSingleton(sistemas);
                services.AddSingleton(seletores);
                services.AddSingleton(configuracaoIa);
                services.AddSingleton(agenteIa);
                services.AddSingleton<AnalisadorChamado>();
                services.AddSingleton<InicializadorBanco>(_ => new InicializadorBanco(configuracaoBanco.ConnectionString));
                services.AddSingleton<IRepositorioChamados, RepositorioChamados>(_ =>
                    new RepositorioChamados(configuracaoBanco.ConnectionString));
                services.AddSingleton<IColetorGlpi, ColetorGlpiPlaywright>();

                if (config.GetSection("ConnectionStrings").GetChildren().Any())
                {
                    services.AddSingleton<SqlConnectionCatalog>();
                    services.AddSingleton<SqlReadOnlyGuard>();
                    services.AddSingleton<MetadataCache>();
                    services.AddSingleton<ConsultorSqlServerService>();
                    services.AddSingleton<IConsultaSqlServerSomenteLeitura, AdaptadorConsultaSqlServer>();
                }
                else
                {
                    services.AddSingleton<IConsultaSqlServerSomenteLeitura, ConsultaSqlServerIndisponivel>();
                }

                services.AddHostedService<Program>();
            })
            .Build();

        await host.RunAsync();
    }

    private static void ValidarConfiguracao(ConfiguracaoGlpi config)
    {
        if (string.IsNullOrWhiteSpace(config.Responsavel))
        {
            throw new InvalidOperationException(
                "'Responsavel' obrigatorio. Configure 'Glpi:Responsavel' em appsettings.json.");
        }

        if (string.IsNullOrWhiteSpace(config.UsuarioLogin))
        {
            throw new InvalidOperationException(
                "Credencial 'UsuarioLogin' nao configurada. Use 'dotnet user-secrets set Glpi:UsuarioLogin <valor>'.");
        }

        if (string.IsNullOrWhiteSpace(config.SenhaLogin))
        {
            throw new InvalidOperationException(
                "Credencial 'SenhaLogin' nao configurada. Use 'dotnet user-secrets set Glpi:SenhaLogin <valor>'.");
        }
    }
}
