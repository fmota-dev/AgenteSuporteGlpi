# Design: Coletor GLPI

## Contexto

Este design cobre a primeira fatia do projeto **Agente de Pre-Analise GLPI**. O PRD completo descreve um agente com IA, historico, relatorios HTML, e-mail consolidado, analise de repositorios, consulta SQL Server via MCP e evolucao futura para uso institucional. Para reduzir risco, a primeira entrega foca somente na coleta real e segura de chamados no GLPI.

Nao existe API GLPI disponivel para este projeto. Por isso, a integracao inicial sera feita por automacao web com Playwright .NET, mantendo operacao somente leitura.

## Objetivo Da Primeira Entrega

Criar um Console App .NET 10 capaz de:

- acessar o GLPI por usuario e senha;
- coletar lista de chamados atribuidos ao usuario configurado;
- abrir cada chamado elegivel e coletar detalhes principais;
- filtrar chamados pelos status do MVP;
- persistir historico inicial em SQLite;
- detectar chamados novos ou alterados;
- gerar resumo minimo no console;
- registrar logs auditaveis por execucao;
- manter Playwright isolado do nucleo de regras;
- testar filtros, parsing e persistencia sem abrir navegador.

## Fora Do Escopo Desta Entrega

- comentarios e acompanhamentos do chamado;
- anexos e prints;
- analise de imagens;
- analise por IA;
- busca em repositorios;
- consulta SQL Server via MCP;
- relatorio HTML final;
- e-mail consolidado;
- alteracao de qualquer dado no GLPI;
- comentario, mudanca de status, prioridade, categoria ou responsavel;
- execucao de scripts de correcao.

## Convencao De Nomenclatura

O codigo deve priorizar pt-BR em nomes de dominio, entidades, servicos, comandos e documentacao. Ingles fica restrito a termos tecnicos, bibliotecas, pacotes, APIs e convencoes inevitaveis do ecossistema .NET e Playwright.

Exemplos desejados:

- `Chamado`
- `ExecucaoColeta`
- `ColetaChamado`
- `FiltroChamados`
- `ColetorGlpi`
- `RepositorioChamados`
- `PersistenciaChamados`
- `ConfiguracaoGlpi`

Exemplos aceitos por serem termos tecnicos:

- `Playwright`
- `Browser`
- `Headless`
- `SQLite`
- `User Secrets`
- `CancellationToken`

## Arquitetura

A primeira entrega deve nascer simples: uma solution `.slnx`, um projeto console e um projeto de testes. A separacao entre automacao web, regras e persistencia sera feita por pastas e namespaces dentro do projeto `AgenteSuporteGlpi`, evitando multi-projeto no MVP.

Projetos previstos:

- `AgenteSuporteGlpi`: entrada CLI, composicao de dependencias, leitura de configuracao, regras puras, automacao Playwright e persistencia SQLite, organizados por pastas internas.
- `AgenteSuporteGlpi.Testes`: testes automatizados de regras, filtros, parsing e persistencia sem abrir navegador.

Pastas principais do projeto console:

- `Chamados`: modelos, filtros, hash, deteccao de mudanca e repositorio de chamados.
- `Contratos`: interfaces entre o fluxo principal e a coleta GLPI.
- `ColetaGlpi`: automacao Playwright somente leitura e parsing HTML.
- `Configuracao`: configuracoes GLPI e Browser.
- `Banco`: configuracao e inicializacao SQLite.
- `Execucoes`: modelos de execucao e eventos.

Fluxo principal:

1. `Program.cs` inicia uma `ExecucaoColeta`.
2. `ColetorGlpi` abre navegador em modo `Headless`.
3. `ColetorGlpi` realiza login no GLPI.
4. `ColetorGlpi` navega ate chamados atribuidos ao usuario configurado.
5. `ColetorGlpi` coleta lista de chamados.
6. Regras em `Chamados` aplicam filtros de status e elegibilidade.
7. `ColetorGlpi` abre cada chamado elegivel e coleta detalhes principais.
8. Regras em `Chamados` calculam hash de conteudo e identificam chamado novo ou alterado.
9. Componentes em `Banco`, `Chamados` e `Execucoes` salvam execucao, chamado, snapshot e eventos no SQLite.
10. `Program.cs` exibe resumo minimo da execucao.

## Coleta GLPI

O login sera por usuario e senha. No desenvolvimento local, os dados sensiveis ficam em User Secrets. O `appsettings.json` pode conter URL, usuario alvo e parametros operacionais, mas nunca deve conter senha.

O navegador roda em modo `Headless` por padrao para permitir agendamento futuro por tarefa externa. Um modo debug com navegador visivel pode ser previsto por configuracao ou flag, mas nao e o padrao.

Dados da lista de chamados:

- numero;
- titulo;
- status;
- prioridade;
- data da ultima atualizacao;
- link do chamado.

Dados dos detalhes principais:

- numero;
- titulo;
- descricao;
- status;
- prioridade;
- categoria, quando disponivel;
- solicitante, quando disponivel;
- responsavel;
- data de abertura;
- data da ultima atualizacao;
- link do chamado.

Comentarios, acompanhamentos e anexos ficam para fatia posterior.

## Regras Iniciais

Status permitidos no MVP:

- Novo;
- Em atendimento;
- Pendente.

Regras de processamento:

- coletar somente chamados atribuidos ao usuario configurado;
- ignorar chamados fora dos status permitidos;
- detectar mudanca por `DataUltimaAtualizacao` e hash do conteudo coletado;
- nao reprocessar chamado sem alteracao;
- registrar chamado com erro sem derrubar a execucao inteira;
- preservar historico por execucao.

## Persistencia SQLite

SQLite sera a fonte oficial do historico desde a primeira entrega.

Tabelas iniciais:

### `ExecucoesColeta`

Campos principais:

- identificador;
- inicio;
- fim;
- status;
- quantidade encontrada;
- quantidade coletada;
- quantidade ignorada;
- quantidade com erro;
- mensagem de erro geral, quando houver.

### `Chamados`

Campos principais:

- numero;
- titulo atual;
- status atual;
- prioridade atual;
- responsavel;
- solicitante;
- categoria;
- data de abertura;
- data da ultima atualizacao;
- link.

### `ColetasChamado`

Campos principais:

- identificador;
- identificador da execucao;
- numero do chamado;
- descricao coletada;
- hash do conteudo;
- status da coleta;
- indicador de chamado novo;
- indicador de chamado alterado;
- erro da coleta, quando houver;
- data da coleta.

### `EventosExecucao`

Campos principais:

- identificador;
- identificador da execucao;
- data e hora;
- nivel;
- etapa;
- mensagem;
- numero do chamado, quando aplicavel.

## Configuracao

`appsettings.json` deve conter configuracoes nao sensiveis:

- URL do GLPI;
- usuario alvo ou responsavel;
- status permitidos;
- caminho do SQLite;
- caminho de logs;
- modo do navegador, com `Headless` como padrao;
- timeouts;
- limite maximo de chamados por execucao;
- parametros de navegacao e seletores quando fizer sentido externalizar.

Dados sensiveis no desenvolvimento:

- usuario de login;
- senha de login.

Esses dados devem usar User Secrets. Para agendamento futuro, a alternativa prevista e usar variaveis de ambiente ou secret store equivalente.

## Seguranca Operacional

A automacao deve operar somente leitura.

Acoes proibidas:

- salvar alteracoes;
- comentar chamado;
- alterar status;
- alterar prioridade;
- alterar categoria;
- alterar responsavel;
- executar script;
- baixar ou enviar arquivo nesta primeira entrega;
- acionar qualquer fluxo de aprovacao ou notificacao.

Falhas seguras:

- se aparecer troca de senha obrigatoria, abortar execucao;
- se aparecer captcha, abortar execucao;
- se aparecer MFA ou confirmacao externa, abortar execucao;
- se aparecer modal inesperado com acao mutavel, abortar ou registrar erro sem clicar em confirmacao;
- se seletores quebrarem, registrar etapa e erro.

Logs devem mascarar dados sensiveis. Screenshots e traces devem existir somente em modo debug e precisam ser tratados como potencialmente sensiveis.

## Testes

Testes automatizados nao devem abrir navegador.

Cobertura inicial esperada:

- filtro de status;
- filtro por responsavel;
- deteccao de chamado novo;
- deteccao de chamado alterado;
- calculo de hash de conteudo;
- persistencia de execucao;
- persistencia de chamado;
- persistencia de snapshot de coleta;
- registro de erro por chamado sem falhar a execucao inteira;
- parsing de dados a partir de HTML estatico ou snapshot salvo.

A automacao Playwright contra GLPI real sera validada manualmente em execucao controlada, com limite baixo de chamados.

## Criterios De Aceite

A primeira entrega sera aceita quando:

- projeto .NET 10 estiver criado com app console unico, pastas internas por responsabilidade e projeto de testes;
- login GLPI funcionar com credenciais vindas de User Secrets;
- navegador rodar `Headless` por padrao;
- lista de chamados atribuidos for coletada;
- detalhes principais de chamados elegiveis forem coletados;
- filtros de status forem aplicados;
- SQLite salvar execucao, chamados, coletas e eventos;
- chamado sem alteracao for ignorado em execucao posterior;
- falha em um chamado nao interromper a execucao inteira;
- resumo minimo aparecer no console;
- testes do nucleo, parsing e persistencia passarem;
- nenhuma acao de escrita no GLPI existir no fluxo implementado.

## Proximas Fatias

Depois desta entrega, a evolucao sugerida e:

1. Coletar comentarios e acompanhamentos.
2. Coletar e registrar anexos/prints.
3. Gerar relatorio HTML minimo por chamado.
4. Integrar IA plugavel para pre-analise.
5. Buscar evidencias em repositorios locais.
6. Consultar SQL Server via MCP existente.
7. Gerar e-mail consolidado.

## Decisoes Registradas

- Primeira fatia: GLPI primeiro.
- GLPI sem API: usar Playwright .NET.
- Login atual: usuario e senha.
- Coleta minima: lista + detalhes.
- Versao alvo: .NET 10.
- Persistencia inicial: SQLite desde o inicio.
- Browser: `Headless` por padrao.
- Dados sensiveis locais: User Secrets.
- Testes: sem abrir navegador.
- Nomenclatura: pt-BR para dominio; ingles somente para termos tecnicos.
