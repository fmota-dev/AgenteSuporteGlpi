using Microsoft.JSInterop;

namespace AgenteSuporteGlpi.Web.Services;

public class ServicoAlertas(IJSRuntime js)
{
    public Task ExibirSucessoAsync(string mensagem)
        => ExibirToastAsync("success", mensagem);

    public Task ExibirErroAsync(string mensagem)
        => ExibirToastAsync("error", mensagem);

    public Task ExibirAvisoAsync(string mensagem)
        => ExibirToastAsync("warning", mensagem);

    public Task ExibirInformacaoAsync(string mensagem)
        => ExibirToastAsync("info", mensagem);

    public async Task<bool> ConfirmarAsync(string titulo, string texto)
        => await js.InvokeAsync<bool>("SenacUI.confirmar", titulo, texto);

    private async Task ExibirToastAsync(string icone, string mensagem)
        => await js.InvokeVoidAsync("SenacUI.toast", icone, mensagem);
}
