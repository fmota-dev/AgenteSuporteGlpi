# Spec: Agente Hibrido e Identificacao de Sistema

## Contexto

O coletor GLPI ja persiste chamados reais em SQLite com numero, titulo, descricao,
status, prioridade, responsavel e datas. A proxima fatia do PRD deve iniciar a
pre-analise sem tentar entregar o agente completo de uma vez.

O objetivo desta fatia e criar uma base deterministica e auditavel para o agente:
identificar o sistema provavel por palavras-chave, validar essa identificacao com
os chamados ja coletados e preparar pontos de extensao para IA, repositorios e
SQL Server.

## Decisoes

- O modo inicial sera hibrido: regras deterministicas montam contexto; MAF entra
  depois para resumo, perguntas e hipoteses.
- A identificacao de sistema sera feita primeiro por `appsettings.json`.
- Repositorios Azure DevOps serao usados clonados localmente no MVP.
- O MCP SQL Server publicado em `mcps/ConsultorSQLServer` sera tratado como
  dependencia local sensivel, integrado futuramente por adapter controlado.
- O agente nao deve executar SQL livre no primeiro corte.
- O MVP continua somente leitura sobre GLPI, repositorios e bancos.

## Achados sobre `/mcps`

A pasta `mcps/ConsultorSQLServer` contem agora o codigo-fonte do MCP SQL Server,
alem da configuracao local:

- `ConsultorSQLServer.sln`
- `ConsultorSQLServer.csproj`
- `Program.cs`
- `Tools/ConsultorSqlServerTools.cs`
- `Services/ConsultorSqlServerService.cs`
- `Services/SqlConnectionCatalog.cs`
- `Security/SqlReadOnlyGuard.cs`
- `.mcp/server.json`
- `appsettings.template.json`
- `appsettings.json`

O projeto e `.NET 10`, usa transporte MCP `stdio`, registra tools com
`.WithToolsFromAssembly()` e referencia `ModelContextProtocol` `1.2.0`.

O `appsettings.json` local contem aliases de conexao sensiveis:

- `SrvDb01`
- `SrvDb06`
- `SrvDb11`

O arquivo real nao deve ser commitado nem impresso em logs. A integracao deve
preferir `appsettings.template.json` como referencia e exigir configuracao local.
Antes de qualquer commit amplo da pasta `mcps`, o repositorio deve ignorar
`mcps/**/appsettings.json`, `mcps/**/appsettings.local.json` e artefatos
`bin/`/`obj/`.

O `Program.cs` carrega configuracao nesta ordem:

1. `appsettings.json` ao lado do binario.
2. `appsettings.{Environment}.json`.
3. `appsettings.local.json`.
4. `%USERPROFILE%/.consultor_sql_server/appsettings.json`.
5. variaveis de ambiente.

O servidor inicia via stdio. Se iniciado a partir do root do repositorio, a
configuracao local pode nao ser encontrada como esperado; portanto qualquer
execucao por processo deve usar `WorkingDirectory = mcps/ConsultorSQLServer` ou
apontar configuracao explicitamente.

As tools expostas incluem descoberta (`ListarConexoesConfiguradas`,
`ListarBancos`, `ListarTabelas`, `DescreverTabela`, `MapearBanco`) e consulta
somente leitura (`BuscarTopRegistros`, `ExecutarConsultaSomenteLeitura`,
`ExecutarConsultaPaginada`, `ExplicarConsulta`). O `SqlReadOnlyGuard` aceita
apenas `SELECT`/`WITH`, bloqueia multiplas instrucoes, comentarios e palavras de
escrita, e normaliza limite maximo para `200` linhas.

## Configuracao de Sistemas

Adicionar secao `Sistemas` ao `appsettings.json`:

```json
{
  "Sistemas": [
    {
      "Nome": "Sistema de Pesquisas",
      "Ativo": true,
      "Aliases": ["pesquisas", "sistema de pesquisas"],
      "PalavrasChave": ["pesquisa", "pergunta", "questionario"],
      "Repositorios": ["C:\\dev\\azure\\SistemaPesquisas"],
      "Bancos": ["SrvDb06"]
    }
  ]
}
```

Campos iniciais:

- `Nome`: nome canonico usado nos relatorios.
- `Ativo`: permite desabilitar sem remover configuracao.
- `Aliases`: nomes livres que podem aparecer no chamado.
- `PalavrasChave`: termos de negocio ou tecnicos para pontuacao.
- `Repositorios`: caminhos locais permitidos para busca futura.
- `Bancos`: aliases permitidos do MCP SQL Server.
- `Observacoes`: campo opcional para contexto tecnico ou de negocio.

## Identificacao Deterministica

Criar componentes de dominio:

- `SistemaConfigurado`
- `ConfiguracaoSistemas`
- `ResultadoIdentificacaoSistema`
- `IdentificadorSistemaPorPalavrasChave`

Entrada:

- titulo do chamado;
- descricao coletada;
- prioridade/status como contexto secundario;
- lista de sistemas configurados.

Saida:

- sistema provavel;
- pontuacao;
- nivel de confianca: `Alta`, `Media`, `Baixa`, `NaoIdentificado`;
- termos que causaram match;
- motivo legivel para auditoria.

Regras iniciais:

- alias exato no titulo vale mais que palavra-chave na descricao;
- multiplos termos do mesmo sistema aumentam pontuacao;
- empate entre sistemas deve retornar `NaoIdentificado` ou `Baixa`, nao escolher
  arbitrariamente;
- sistema inativo e ignorado;
- texto deve ser normalizado sem acentos e case-insensitive.

## Validacao com Chamados Existentes

Usar os chamados reais ja coletados no SQLite como massa de validacao inicial:

- `#31905 Sistema de pesquisas - Criacao de novas perguntas` deve identificar
  `Sistema de Pesquisas` quando configurado.
- `#31619 Agenda` deve identificar o sistema de agenda quando ele for configurado.
- `#31868 Responsavel nao recebe e-mails nem mensagens na agenda` deve identificar
  o sistema de agenda quando ele for configurado.

Essa validacao deve ser automatizada com testes de unidade usando objetos em
memoria. A leitura do SQLite real pode ser usada como validacao operacional, mas
nao deve ser requisito para os testes automatizados.

## Agente MAF

O MAF nao deve controlar coleta, persistencia nem busca de contexto no primeiro
corte. O app monta um `ContextoAnaliseChamado` e passa para o agente.

Modelo de contexto inicial:

- dados do chamado;
- resultado da identificacao do sistema;
- avisos de dados insuficientes;
- fontes usadas;
- historico semelhante futuramente;
- trechos de repositorio futuramente;
- resultados SQL futuramente.

Primeiro uso recomendado de IA:

- resumo tecnico curto;
- perguntas ao solicitante;
- proximos passos;
- explicacao de baixa confianca quando sistema nao for identificado.

Provider e modelo ficam em configuracao `AI`, sem chave real em
`appsettings.json`. O modo inicial deve ser `single`.

## Repositorios DevOps

No MVP, os repositorios devem ser clonados fora do app e referenciados por caminho
local em `Sistemas[].Repositorios`.

O app deve validar:

- caminho existe;
- caminho esta dentro de lista permitida;
- sistema esta ativo;
- falha em repositorio de um sistema nao derruba analise de outros chamados.

Busca em codigo fica para fatia posterior. Esta spec so define o contrato para
configurar caminhos.

## SQL Server via MCP

Criar futuramente uma porta interna:

```csharp
public interface IConsultaSqlServerSomenteLeitura
{
    Task<ResultadoConsultaSql> ConsultarAsync(
        string aliasConexao,
        string perguntaOuConsultaAprovada,
        CancellationToken cancellationToken);
}
```

No primeiro corte, a implementacao pode ser fake ou desabilitada por configuracao.
Quando ativada, ha duas formas possiveis de integracao:

1. **Subprocesso MCP stdio:** iniciar `dotnet run --project
   mcps/ConsultorSQLServer/ConsultorSQLServer.csproj` ou o binario publicado,
   falar protocolo MCP como cliente e chamar as tools.
2. **Referencia direta ao projeto:** referenciar o projeto MCP e chamar
   `ConsultorSqlServerService` por adapter interno, reaproveitando
   `SqlReadOnlyGuard`, `SqlConnectionCatalog` e formatos de resposta sem passar
   por JSON-RPC.

Recomendacao para o app: comecar com **referencia direta ao projeto ou adapter
fake**, porque reduz complexidade de protocolo durante a validacao do agente. O
contrato deve continuar igual para permitir trocar por subprocesso MCP stdio
depois, se fizer sentido.

Quando ativada, a integracao deve respeitar:

- alias de banco permitido pelo sistema identificado;
- somente leitura;
- limite de linhas;
- registro da consulta e resultado resumido;
- nenhum segredo em prompt ou relatorio.

O agente MAF deve receber resultados da consulta, nao credenciais nem acesso
direto ao MCP.

## Relatorio Minimo

Para cada chamado novo ou alterado, gerar futuramente um HTML minimo com:

- identificacao do chamado;
- sistema provavel;
- confianca e motivo;
- dados insuficientes;
- perguntas ao solicitante;
- fontes usadas.

Nesta fatia, a prioridade e persistir o resultado da identificacao e permitir
validar a qualidade do mapeamento antes de gerar o relatorio completo do PRD.

## Fora do Escopo Desta Fatia

- Baixar anexos e imagens.
- OCR ou analise visual.
- Busca real em repositorios.
- Execucao real de consultas SQL pelo agente.
- Relatorio HTML completo.
- Envio de e-mail.
- Multiagentes.
- Alterar GLPI, banco ou codigo-fonte externo.

## Criterios de Aceite

- Configuracao de sistemas carregada do `appsettings.json`.
- Identificador deterministico testado com aliases, palavras-chave, empate e
  sistema inativo.
- Chamados reais conhecidos podem ser representados em testes e identificados
  conforme configuracao.
- Resultado inclui sistema, confianca, pontuacao, termos encontrados e motivo.
- Pipeline principal consegue chamar a identificacao apos salvar/coletar detalhes
  sem alterar a coleta GLPI.
- Nenhum segredo do MCP local aparece em logs, testes, spec ou relatorio.
