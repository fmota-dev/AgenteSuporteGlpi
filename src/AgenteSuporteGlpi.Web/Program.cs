using ApexCharts;
using AgenteSuporteGlpi.Web.Components;
using AgenteSuporteGlpi.Web.Servicos;

var construtor = WebApplication.CreateBuilder(args);

var connectionString = construtor.Configuration.GetSection("Banco:ConnectionString").Value
    ?? "Data Source=../AgenteSuporteGlpi/dados/agente-suporte-glpi.db";

construtor.Services.AddSingleton(new ServicoDashboard(connectionString));
construtor.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
construtor.Services.AddApexCharts();

var aplicacao = construtor.Build();

if (!aplicacao.Environment.IsDevelopment())
{
    aplicacao.UseExceptionHandler("/Error", createScopeForErrors: true);
}

aplicacao.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
aplicacao.MapStaticAssets();
aplicacao.UseAntiforgery();

aplicacao.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

aplicacao.Run();
