# Agente Suporte GLPI

Projeto-base corporativo em Blazor Server com:

- layout responsivo Senac
- autenticação custom por cookie
- dashboard com `Blazor-ApexCharts`
- módulo `Acessos` com `Usuários` e `Perfis`
- `SweetAlert2`, `Tailwind`, `ClosedXML`, `Dapper` e `SQL Server`

## Como gerar

Se este conteúdo for usado como `dotnet template`, os placeholders são resolvidos automaticamente por parâmetros do template.

Exemplo:

```powershell
dotnet new install C:\Users\fmota\.agents\skills\blazor-senac\starter\template

dotnet new senac-blazor-corp `
  --name "Portal.Corporativo" `
  --namespace "Portal.Corporativo" `
  --nomeSistema "Portal Corporativo" `
  --dominioInicial "Financeiro"
```

Se o conteúdo for copiado manualmente, use o script `starter\gerar-projeto-base.ps1`.

## Placeholders

Substitua estes tokens antes do primeiro build:

- `AgenteSuporteGlpi.Web`
- `AgenteSuporteGlpi.Web`
- `Agente Suporte GLPI`
- `NovoModulo` (opcional, usado nos materiais de scaffold)

## Banco de dados

Os scripts iniciais ficam em [database](./database):

1. `01_criacao_estrutura_base.sql`
2. `02_carga_inicial_base.sql`
3. `03_ajuste_tabela_usuarios_auth_custom.sql`

## Mock e banco real

- `appsettings.Development.json` nasce com `UseMockData = true`
- `appsettings.json` nasce com `UseMockData = false`

## Próximo módulo

Use os arquivos em `../scaffolds/modulo-crud` para duplicar o próximo módulo de negócio de `NovoModulo` ou do domínio que for definido no novo projeto.
