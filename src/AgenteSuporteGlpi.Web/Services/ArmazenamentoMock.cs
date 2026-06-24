using AgenteSuporteGlpi.Web.Models;

namespace AgenteSuporteGlpi.Web.Services;

public class ArmazenamentoMock
{
    public List<MPerfil> Perfis { get; } =
    [
        new() { Codigo = 1, Nome = "Administrador", CodigoStatus = 1, NomeStatus = "Ativo", DataCriacao = DateTime.Today.AddDays(-60) },
        new() { Codigo = 2, Nome = "Gestor", CodigoStatus = 1, NomeStatus = "Ativo", DataCriacao = DateTime.Today.AddDays(-45) },
        new() { Codigo = 3, Nome = "Colaborador", CodigoStatus = 1, NomeStatus = "Ativo", DataCriacao = DateTime.Today.AddDays(-30) }
    ];

    public List<MUsuario> Usuarios { get; } =
    [
        new()
        {
            Codigo = 1,
            Nome = "Aline Administradora",
            Email = "admin@corp.local",
            SenhaCriptografada = "123456",
            CodigoPerfil = 1,
            NomePerfil = "Administrador",
            CodigoStatus = 1,
            NomeStatus = "Ativo",
            BloqueioHabilitado = true,
            FalhasAcesso = 0,
            DataUltimoAcesso = DateTime.Today.AddHours(-1)
        },
        new()
        {
            Codigo = 2,
            Nome = "Bruno Gestor",
            Email = "gestor@corp.local",
            SenhaCriptografada = "123456",
            CodigoPerfil = 2,
            NomePerfil = "Gestor",
            CodigoStatus = 1,
            NomeStatus = "Ativo",
            BloqueioHabilitado = true,
            FalhasAcesso = 1,
            DataUltimoAcesso = DateTime.Today.AddDays(-1).AddHours(16)
        },
        new()
        {
            Codigo = 3,
            Nome = "Carla Colaboradora",
            Email = "colaborador@corp.local",
            SenhaCriptografada = "123456",
            CodigoPerfil = 3,
            NomePerfil = "Colaborador",
            CodigoStatus = 1,
            NomeStatus = "Ativo",
            BloqueioHabilitado = true,
            FalhasAcesso = 5,
            FimBloqueio = DateTimeOffset.UtcNow.AddHours(12),
            DataUltimoAcesso = DateTime.Today.AddDays(-3).AddHours(10)
        }
    ];

    public ArmazenamentoMock(Criptografia criptografia)
    {
        foreach (var usuario in Usuarios.Where(usuario => !usuario.SenhaCriptografada.StartsWith("AQAAAA", StringComparison.Ordinal)))
        {
            usuario.SenhaCriptografada = criptografia.GerarHashSenha(usuario.SenhaCriptografada);
        }
    }
}
