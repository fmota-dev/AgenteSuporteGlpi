using System.Diagnostics;

namespace AgenteSuporteGlpi.Web.Servicos;

public sealed class ServicoPipeline
{
    private readonly string _projetoRaiz;
    private readonly object _lock = new();
    private Process? _processoAtual;
    private readonly List<string> _logExecucao = [];
    private bool _executando;

    public bool Executando
    {
        get { lock (_lock) return _executando; }
    }

    public IReadOnlyList<string> Log => _logExecucao.ToArray();

    public ServicoPipeline(string projetoRaiz)
    {
        _projetoRaiz = projetoRaiz;
    }

    public Task ExecutarColetaAsync()
    {
        return ExecutarAsync("Coleta", "");
    }

    public Task ExecutarAnaliseAsync()
    {
        return ExecutarAsync("Analise IA", "-- --analisar");
    }

    private async Task ExecutarAsync(string nome, string argumentos)
    {
        lock (_lock)
        {
            if (_executando)
                throw new InvalidOperationException("Ja existe uma execucao em andamento.");

            _executando = true;
            _logExecucao.Clear();
            _logExecucao.Add($"[{DateTime.Now:HH:mm:ss}] Iniciando {nome}...");
        }

        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = $"run --project \"{_projetoRaiz}\" {argumentos}",
                WorkingDirectory = _projetoRaiz,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            _processoAtual = new Process { StartInfo = startInfo };
            _processoAtual.OutputDataReceived += (_, e) =>
            {
                if (!string.IsNullOrWhiteSpace(e.Data))
                    lock (_lock) _logExecucao.Add(e.Data);
            };
            _processoAtual.ErrorDataReceived += (_, e) =>
            {
                if (!string.IsNullOrWhiteSpace(e.Data))
                    lock (_lock) _logExecucao.Add($"[ERRO] {e.Data}");
            };

            _processoAtual.Start();
            _processoAtual.BeginOutputReadLine();
            _processoAtual.BeginErrorReadLine();

            await _processoAtual.WaitForExitAsync();

            var status = _processoAtual.ExitCode == 0 ? "concluida" : $"concluida com erros (codigo {_processoAtual.ExitCode})";
            lock (_lock) _logExecucao.Add($"[{DateTime.Now:HH:mm:ss}] {nome} {status}.");
        }
        catch (Exception ex)
        {
            lock (_lock) _logExecucao.Add($"[{DateTime.Now:HH:mm:ss}] FALHA: {ex.Message}");
        }
        finally
        {
            lock (_lock)
            {
                _executando = false;
                _processoAtual?.Dispose();
                _processoAtual = null;
            }
        }
    }
}
