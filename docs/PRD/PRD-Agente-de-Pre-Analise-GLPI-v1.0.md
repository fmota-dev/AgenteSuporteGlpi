# PRD v1.0 — Agente de Pré-Análise GLPI

## 1. Nome do projeto

**Agente de Pré-Análise GLPI**

## 2. Visão geral

O Agente de Pré-Análise GLPI é uma solução de apoio ao diagnóstico técnico de chamados internos atribuídos a desenvolvedores no GLPI.

O sistema deverá consultar chamados novos ou atualizados, inicialmente atribuídos a um único desenvolvedor, analisar descrições, comentários e prints anexados, cruzar informações com repositórios locais/Azure DevOps, banco SQL Server via MCP existente e histórico de análises anteriores, gerando relatórios técnicos completos em HTML e enviando um e-mail consolidado com o resumo da execução.

A proposta do MVP não é automatizar correções nem substituir o desenvolvedor. O objetivo é funcionar como uma camada de triagem inteligente, auditável e somente leitura, reduzindo o tempo de diagnóstico e aumentando a qualidade da investigação inicial.

## 3. Contexto e problema

Atualmente, a resolução de chamados técnicos exige que o desenvolvedor leia o chamado, entenda o contexto, identifique o sistema afetado, interprete prints, busque trechos no código, consulte banco de dados quando necessário e tente relacionar o problema com ocorrências anteriores.

Esse processo é manual, repetitivo e depende muito da memória e experiência individual do desenvolvedor.

Os principais problemas são:

- demora para entender o contexto inicial do chamado;
- dificuldade para identificar rapidamente o sistema afetado;
- tempo gasto procurando possíveis causas no código e no banco;
- retrabalho em chamados recorrentes ou parecidos;
- perda de histórico técnico das análises feitas manualmente;
- baixa padronização na triagem técnica;
- ausência de relatório estruturado com evidências e hipóteses.

## 4. Objetivos

### 4.1 Objetivo principal

Criar uma solução que realize pré-análise técnica automatizada de chamados GLPI, gerando relatórios com resumo, classificação, evidências, hipóteses de causa, nível de confiança, perguntas ao solicitante e próximos passos recomendados.

### 4.2 Objetivos específicos

- Reduzir o tempo médio de diagnóstico inicial.
- Aumentar o acerto na identificação do sistema afetado.
- Apoiar a investigação de erros em sistemas legados.
- Apoiar a investigação de inconsistências de dados.
- Analisar texto, comentários e prints anexados aos chamados.
- Cruzar informações do chamado com código-fonte e banco SQL Server.
- Reutilizar histórico de análises anteriores para identificar chamados semelhantes.
- Gerar relatório HTML individual por chamado.
- Enviar e-mail consolidado com resumo e links dos relatórios.
- Registrar logs completos e histórico técnico em banco.
- Permitir execução manual por número de chamado.
- Manter arquitetura plugável para diferentes modelos de IA.
- Preparar evolução futura para Hangfire, SQL Server, tela administrativa e uso institucional.

## 5. Público-alvo

### 5.1 Usuário inicial

Desenvolvedor responsável por manutenção e evolução de sistemas internos, atuando sobre chamados atribuídos a ele no GLPI.

### 5.2 Usuários futuros

- Outros desenvolvedores do setor.
- Líder técnico.
- Equipe de sustentação.
- Equipe de suporte de sistemas.
- Equipe de infraestrutura.
- Segurança da informação.
- GT de IA ou inovação.
- Gestão interessada em indicadores de retrabalho, recorrência e produtividade.

## 6. Escopo do MVP

O MVP deverá contemplar:

- Aplicação inicial em formato **Console App .NET**.
- Execução agendada por tarefa externa do sistema operacional.
- Consulta de chamados atribuídos ao usuário configurado.
- Filtro por chamados com status:
  - Novo;
  - Em atendimento;
  - Pendente.
- Análise apenas de chamados novos ou atualizados.
- Reanálise apenas quando houver atualização relevante.
- Filtro inicial para:
  - erros em sistemas existentes;
  - inconsistências de dados.
- Identificação do sistema provável por:
  - texto do chamado;
  - palavras-chave configuradas;
  - mapeamento manual de sistemas.
- Configuração dos sistemas via `appsettings.json`.
- Consulta a repositórios Azure DevOps previamente clonados ou pastas locais.
- Consulta a banco SQL Server via MCP existente.
- Análise de prints/imagens anexadas.
- Busca de análises anteriores semelhantes no histórico.
- Geração de relatório HTML individual por chamado.
- Organização de relatórios por chamado.
- Envio de e-mail consolidado com resumo e links.
- Persistência em SQLite no MVP.
- Registro de logs completos em banco.
- Registro de tempo, modelo utilizado, tokens/custo quando disponíveis.
- Execução manual por número do chamado via linha de comando.

## 7. Fora do escopo do MVP

Não fazem parte do MVP:

- Alterar chamados no GLPI.
- Adicionar comentários no GLPI.
- Alterar status, prioridade, categoria ou responsável.
- Responder automaticamente ao solicitante.
- Alterar dados em banco.
- Executar scripts de correção.
- Alterar código-fonte.
- Criar branch, commit ou pull request.
- Executar deploy.
- Consultar logs diretamente em servidor de produção.
- Rodar via Hangfire.
- Criar dashboard web.
- Criar tela administrativa.
- Atuar sobre chamados de outros desenvolvedores.
- Resolver solicitações de novas funcionalidades ou novos sistemas.
- Tomar decisões autônomas em produção.

## 8. Escopo futuro

A solução poderá evoluir para:

- Execução via Hangfire.
- Persistência em SQL Server.
- Configuração de sistemas em banco de dados.
- Tela administrativa para configuração de sistemas, palavras-chave, repositórios e bancos.
- Consulta segura a logs com solução definida junto à infraestrutura.
- Suporte a múltiplos desenvolvedores.
- Uso institucional pela equipe.
- Dashboard de análises.
- Indicadores de recorrência, retrabalho e tempo de diagnóstico.
- Comentário privado no GLPI com aprovação humana.
- Rascunho de resposta ao solicitante.
- Integração mais profunda com Azure DevOps.
- Relatórios em versão técnica e versão resumida para gestão/suporte.

## 9. Fontes de dados

### 9.1 GLPI

O agente deverá consultar:

- ID do chamado;
- título;
- descrição;
- status;
- prioridade;
- categoria;
- data de abertura;
- data da última atualização;
- responsável;
- comentários;
- anexos, especialmente imagens;
- solicitante, quando disponível;
- sistema/categoria associada, quando disponível.

### 9.2 Repositórios

O agente deverá consultar repositórios:

- Azure DevOps previamente clonados;
- pastas locais;
- pastas de rede, quando configuradas.

A análise poderá considerar:

- nomes de arquivos;
- nomes de classes;
- controllers;
- services;
- repositories;
- endpoints;
- mensagens de erro;
- termos de negócio;
- procedures versionadas;
- arquivos de configuração permitidos;
- histórico local disponível, se configurado.

### 9.3 Banco de dados

O agente poderá consultar bancos SQL Server por meio do MCP existente.

Premissas:

- O MCP já possui permissões configuradas.
- A operação do agente será somente leitura.
- Nenhuma operação de escrita será permitida.
- As permissões e limites definidos no MCP deverão ser respeitados.
- As consultas executadas deverão ser registradas para auditoria.

### 9.4 Logs

A consulta a logs de produção não fará parte do MVP.

Como os logs ficam em pasta no servidor interno, a estratégia de acesso deverá ser discutida com o time de infraestrutura.

Possíveis soluções futuras:

- cópia controlada de logs para pasta segura;
- API interna para consulta de logs;
- MCP específico de logs com acesso restrito;
- exportação periódica de trechos relevantes;
- indexação segura e auditada dos logs.

### 9.5 Imagens e prints

O agente deverá analisar prints/imagens anexadas para:

- extrair mensagens de erro;
- identificar textos visíveis;
- descrever visualmente o contexto da tela;
- complementar a interpretação do chamado;
- usar imagens principalmente quando houver mensagem de erro visível.

## 10. Configuração de sistemas

No MVP, os sistemas serão configurados via `appsettings.json`.

Cada sistema poderá conter:

- nome;
- aliases;
- status ativo/inativo;
- palavras-chave;
- repositórios associados;
- bancos associados;
- padrões conhecidos de erro;
- observações técnicas;
- observações de negócio;
- caminhos locais permitidos.

Exemplo conceitual:

```json
{
  "Sistemas": [
    {
      "Nome": "Sistema Legado A",
      "Ativo": true,
      "Aliases": ["Sistema A", "Modulo A"],
      "PalavrasChave": ["matrícula", "turma", "inscrição", "erro ao salvar"],
      "Repositorios": [
        "C:\\Repos\\SistemaA"
      ],
      "Bancos": [
        "BancoSistemaA"
      ],
      "Observacoes": "Sistema legado mantido pela equipe de desenvolvimento."
    }
  ]
}
```

Futuramente, essa configuração deverá migrar para banco de dados e, depois, para uma tela administrativa.

## 11. Critérios para seleção de chamados

O agente deverá analisar apenas chamados:

- atribuídos ao usuário configurado;
- com status Novo, Em atendimento ou Pendente;
- novos ou atualizados desde a última análise;
- classificados como erro em sistema existente ou inconsistência de dados;
- relacionados, preferencialmente, a sistemas configurados.

## 12. Atualizações relevantes

Um chamado deverá ser reanalisado quando houver:

- novo comentário;
- novo anexo;
- alteração no título;
- alteração na descrição;
- alteração de status.

Chamados sem atualização relevante não deverão gerar nova análise.

## 13. Tratamento de sistemas não identificados

O agente deverá trabalhar com uma lista de sistemas configurados/permitidos.

Quando o sistema for identificado, a análise completa será executada.

Quando o sistema não for identificado, o agente deverá:

- gerar relatório mínimo;
- indicar que o sistema não foi identificado;
- explicar quais informações estão faltando;
- sugerir perguntas para o solicitante;
- incluir o chamado no e-mail consolidado como não analisado por falta de contexto suficiente.

## 14. Fluxo principal

1. A execução é iniciada manualmente ou por agendamento.
2. O sistema registra uma nova execução.
3. O sistema consulta os chamados atribuídos ao usuário no GLPI.
4. O sistema filtra chamados com status Novo, Em atendimento ou Pendente.
5. O sistema identifica chamados novos ou atualizados.
6. O sistema ignora chamados já analisados sem atualização relevante.
7. O sistema classifica o tipo do chamado.
8. O sistema filtra chamados elegíveis ao MVP.
9. O sistema tenta identificar o sistema afetado.
10. Se o sistema não for identificado, gera relatório mínimo.
11. Se o sistema for identificado, analisa texto, comentários e imagens.
12. O sistema busca análises antigas semelhantes.
13. O sistema busca evidências nos repositórios configurados.
14. O sistema consulta banco SQL Server via MCP, quando necessário e permitido.
15. O agente gera hipóteses de causa.
16. O agente calcula nível de confiança.
17. O agente recomenda próximos passos.
18. O agente gera perguntas ao solicitante.
19. O agente gera sugestão de resposta técnica quando houver dados suficientes.
20. O sistema gera relatório HTML individual.
21. O sistema salva metadados, relatório, resumo, fontes e logs no banco.
22. O sistema salva arquivos na estrutura de execução.
23. O sistema envia e-mail consolidado.
24. O sistema encerra a execução com estatísticas e logs.

## 15. Execução manual

O MVP deverá permitir análise manual de um chamado específico via linha de comando.

Exemplo:

```bash
analisador-glpi --ticket 12345
```

Esse modo deverá permitir reanálise sob demanda e investigação de chamados urgentes fora dos horários programados.

## 16. Periodicidade

No MVP, a execução agendada ocorrerá duas vezes ao dia:

- 11:00;
- 15:00.

Os horários deverão ser parametrizáveis.

No MVP, o agendamento poderá ser feito via tarefa externa do sistema operacional.

No futuro, a execução deverá migrar para Hangfire.

## 17. Organização de arquivos

### 17.1 Relatórios por chamado

Os relatórios HTML deverão seguir a estrutura:

```text
/relatorios/chamado-12345/analise-2026-06-22-1100.html
```

Essa estrutura permite manter múltiplas análises do mesmo chamado.

### 17.2 Pasta de execuções

O sistema deverá manter uma pasta de execuções para facilitar auditoria e debug.

Exemplo:

```text
/execucoes/2026-06-22/1100/
  resumo-execucao.json
  email-consolidado.html
  logs.txt
```

## 18. Saídas do sistema

### 18.1 Relatório HTML individual

Cada chamado analisado deverá gerar um relatório HTML individual, visualmente limpo, corporativo e fácil de compartilhar com devs.

O relatório deverá ter:

- cabeçalho com dados do chamado;
- resumo executivo;
- detalhamento técnico;
- cards ou seções visuais;
- indicação de nível de confiança;
- fontes utilizadas;
- hipóteses e evidências;
- perguntas ao solicitante;
- próximos passos.

### 18.2 E-mail consolidado

A cada execução, o sistema deverá enviar um e-mail consolidado para uma lista configurável no `appsettings.json`.

No MVP, a lista começará apenas com o e-mail do desenvolvedor responsável.

O e-mail deverá conter:

- data/hora da execução;
- quantidade de chamados encontrados;
- quantidade de chamados analisados;
- quantidade de chamados ignorados;
- chamados sem sistema identificado;
- resumo dos chamados analisados;
- nível de confiança de cada análise;
- severidade técnica recomendada;
- links ou caminhos dos relatórios HTML;
- alertas de falha ou limitações.

O e-mail não deverá conter o relatório completo, apenas resumo e links.

### 18.3 Histórico em banco

No MVP, o histórico será salvo em SQLite.

Em produção, a persistência deverá migrar para SQL Server.

O sistema deverá salvar:

- metadados da execução;
- chamados encontrados;
- chamados analisados;
- chamados ignorados;
- versão da análise;
- resumo da análise;
- relatório completo;
- caminho do HTML;
- classificação;
- sistema provável;
- nível de confiança;
- severidade técnica recomendada;
- fontes utilizadas;
- ferramentas utilizadas;
- consultas realizadas;
- arquivos analisados;
- imagens analisadas;
- prompts completos com dados sensíveis mascarados;
- modelo utilizado;
- tempo de execução;
- tokens/custo quando disponíveis;
- erros ocorridos;
- data e hora.

## 19. Estrutura obrigatória do relatório

Cada relatório deverá conter:

1. Identificação do chamado.
2. Resumo executivo.
3. Classificação do chamado.
4. Sistema provável afetado.
5. Severidade técnica recomendada.
6. Nível de confiança da análise.
7. Fontes utilizadas.
8. Evidências encontradas.
9. Hipóteses de causa.
10. Arquivos e repositórios relacionados.
11. Consultas de banco realizadas.
12. Logs relacionados.
13. Riscos ou dados insuficientes.
14. Perguntas para o solicitante.
15. Próximos passos recomendados.
16. Sugestão de resposta técnica, sem publicar no GLPI.
17. Histórico da análise.

O relatório deverá conter resumo executivo e detalhamento técnico no mesmo HTML.

## 20. Nível de confiança

O agente deverá indicar nível de confiança para cada análise.

Classificação sugerida:

- **Alta:** há evidências fortes e convergentes, como mensagem de erro identificada, trecho de código relacionado e dados compatíveis.
- **Média:** há indícios relevantes, mas faltam dados para confirmação total.
- **Baixa:** o chamado possui pouca informação, não há evidências suficientes ou a causa provável é incerta.

O nível de confiança deverá evitar que o relatório pareça mais conclusivo do que realmente é.

## 21. Severidade técnica recomendada

Além da prioridade registrada no GLPI, o agente deverá recomendar uma severidade técnica.

Essa recomendação não altera o chamado automaticamente.

Classificação sugerida:

- **Baixa:** erro isolado, baixo impacto ou com contorno simples.
- **Média:** afeta fluxo relevante, mas não impede operação geral.
- **Alta:** impede operação importante ou afeta vários usuários.
- **Crítica:** afeta processo essencial, múltiplas unidades ou operação institucional sensível.

A severidade deverá ser apresentada como recomendação, não como decisão final.

## 22. Sugestão de resposta técnica

O agente poderá gerar sugestão de resposta técnica para o GLPI somente quando houver dados suficientes.

A sugestão não deverá ser publicada automaticamente.

A resposta sugerida poderá conter:

- resumo do entendimento do problema;
- informações adicionais necessárias;
- próximos passos de investigação;
- orientação para o solicitante;
- indicação de que a análise está em andamento.

## 23. Perguntas para o solicitante

O relatório deverá sempre conter uma seção de perguntas para o solicitante.

Exemplos:

- Qual usuário foi afetado?
- Em qual tela ocorreu o erro?
- O erro ocorre sempre ou apenas em determinado registro?
- Qual o horário aproximado do erro?
- Qual matrícula, código ou identificador do registro?
- O problema ocorre com outros usuários?
- Houve alguma alteração recente no processo?
- É possível enviar novo print com a mensagem completa?

Quando não houver dúvidas relevantes, a seção poderá indicar que não há perguntas adicionais no momento.

## 24. Fontes utilizadas

O relatório deverá apresentar obrigatoriamente as fontes utilizadas na análise.

Exemplos de fontes:

- chamado GLPI;
- comentários do chamado;
- imagem anexada;
- arquivo de código;
- repositório;
- tabela consultada;
- consulta executada via MCP;
- análise anterior semelhante;
- configuração do sistema.

Essa seção é essencial para aumentar confiança, auditabilidade e revisão humana.

## 25. Autonomia

No MVP, o agente terá autonomia apenas para:

- ler dados permitidos;
- analisar informações;
- gerar relatório;
- enviar e-mail;
- salvar histórico.

O agente não terá autonomia para:

- alterar GLPI;
- alterar banco;
- alterar código;
- executar scripts;
- tomar decisão de negócio;
- interagir com solicitantes;
- fechar chamados;
- abrir pull requests.

## 26. Modelo de IA

A solução deverá ser plugável para permitir troca de modelo ou provedor.

Opções previstas:

- Azure OpenAI;
- OpenAI;
- Ollama;
- Foundry Local.

A arquitetura deverá evitar acoplamento direto com um único provedor.

Deverá existir uma abstração para o serviço de IA, permitindo troca futura sem reescrever o fluxo principal.

## 27. Segurança e auditoria

O sistema deverá operar com foco em segurança, rastreabilidade e revisão humana.

Premissas:

- Operação somente leitura no MVP.
- Respeito às permissões do GLPI, repositórios e MCP de banco.
- Registro de fontes utilizadas.
- Registro de ferramentas chamadas.
- Registro de consultas executadas.
- Registro de prompts com dados sensíveis mascarados.
- Registro de modelo usado, tempo de execução e tokens/custo quando disponíveis.
- Nenhuma ação automática de correção.
- Nenhuma alteração em produção.
- Relatórios compartilháveis com desenvolvedores.

## 28. Requisitos funcionais

### RF01 — Consultar chamados atribuídos

O sistema deverá consultar chamados atribuídos ao usuário configurado no GLPI.

### RF02 — Filtrar por status

O sistema deverá considerar apenas chamados com status Novo, Em atendimento ou Pendente.

### RF03 — Detectar chamados novos ou atualizados

O sistema deverá identificar chamados novos ou com atualização relevante desde a última análise.

### RF04 — Ignorar chamados já analisados

O sistema deverá ignorar chamados já analisados quando não houver atualização relevante.

### RF05 — Classificar chamados

O sistema deverá classificar chamados por tipo, como erro em sistema existente, inconsistência de dados, dúvida, melhoria ou solicitação.

### RF06 — Filtrar chamados elegíveis

O sistema deverá analisar apenas chamados elegíveis conforme escopo do MVP.

### RF07 — Identificar sistema provável

O sistema deverá identificar o sistema provável com base no texto do chamado, palavras-chave e mapeamento manual.

### RF08 — Tratar sistema não identificado

O sistema deverá gerar relatório mínimo quando não conseguir identificar o sistema afetado.

### RF09 — Analisar imagens anexadas

O sistema deverá analisar imagens anexadas para extrair mensagens de erro e contexto visual.

### RF10 — Buscar análises semelhantes

O sistema deverá buscar análises anteriores semelhantes no histórico.

### RF11 — Buscar evidências em repositórios

O sistema deverá buscar arquivos, classes, mensagens, endpoints e termos relacionados nos repositórios configurados.

### RF12 — Consultar banco via MCP

O sistema poderá consultar bancos SQL Server por meio do MCP existente, respeitando as permissões configuradas.

### RF13 — Gerar hipóteses de causa

O agente deverá gerar hipóteses de causa com base nas evidências encontradas.

### RF14 — Gerar próximos passos

O agente deverá sugerir próximos passos técnicos para investigação.

### RF15 — Gerar perguntas para o solicitante

O agente deverá gerar perguntas relevantes para complementar a análise do chamado.

### RF16 — Gerar sugestão de resposta técnica

O agente deverá gerar sugestão de resposta técnica quando houver dados suficientes.

### RF17 — Gerar relatório HTML

O sistema deverá gerar relatório HTML individual por chamado analisado.

### RF18 — Enviar e-mail consolidado

O sistema deverá enviar e-mail consolidado com resumo da execução e links para relatórios.

### RF19 — Salvar histórico em banco

O sistema deverá salvar logs e histórico completo das análises em banco.

### RF20 — Permitir análise manual

O sistema deverá permitir análise manual de um chamado específico via linha de comando.

### RF21 — Registrar fontes utilizadas

O sistema deverá registrar e exibir as fontes utilizadas na análise.

### RF22 — Registrar uso do modelo

O sistema deverá registrar modelo usado, duração, tokens e custo quando disponíveis.

### RF23 — Registrar prompts mascarados

O sistema deverá salvar prompts completos com dados sensíveis mascarados.

### RF24 — Gerar pasta de execução

O sistema deverá gerar pasta de execução com resumo, e-mail consolidado e logs da execução.

## 29. Requisitos não funcionais

### RNF01 — Segurança

O sistema deverá respeitar permissões de acesso configuradas para GLPI, banco e repositórios.

### RNF02 — Somente leitura

O MVP deverá operar em modo somente leitura sobre GLPI, banco e repositórios.

### RNF03 — Auditabilidade

Toda análise deverá ser rastreável, registrando fontes consultadas, ferramentas utilizadas e resultado produzido.

### RNF04 — Plugabilidade de modelo

A solução deverá permitir troca futura do modelo de IA ou provedor sem reescrever o núcleo da aplicação.

### RNF05 — Configuração externa

Sistemas, horários, caminhos, destinatários e integrações deverão ser parametrizáveis.

### RNF06 — Execução agendada

O sistema deverá suportar execução agendada.

### RNF07 — Tolerância a falhas

Falhas na análise de um chamado não deverão interromper a análise dos demais.

### RNF08 — Histórico persistente

O histórico das análises deverá ser mantido em banco para consulta futura.

### RNF09 — Baixo acoplamento

O agente deverá ser separado das integrações, permitindo evolução independente dos conectores.

### RNF10 — Operação local ou interna

A arquitetura deverá considerar a possibilidade de execução com modelo local ou em ambiente interno.

### RNF11 — Rastreabilidade

Cada relatório deverá permitir rastrear quais dados foram usados para gerar a análise.

### RNF12 — Compartilhamento técnico

Os relatórios poderão ser compartilhados com outros desenvolvedores do time.

### RNF13 — Observabilidade

O sistema deverá registrar logs de execução, falhas, duração e resultados.

### RNF14 — Evolução para produção

A arquitetura deverá permitir migração de SQLite para SQL Server e de Console App para Hangfire.

## 30. Critérios de aceite do MVP

O MVP será considerado aceitável quando:

- Conseguir consultar chamados atribuídos ao usuário no GLPI.
- Filtrar apenas chamados Novo, Em atendimento e Pendente.
- Detectar chamados novos ou atualizados.
- Ignorar chamados já analisados sem atualização relevante.
- Identificar sistema provável usando texto, palavras-chave e configuração.
- Gerar relatório mínimo quando o sistema não for identificado.
- Analisar prints/imagens anexadas ao chamado.
- Consultar repositórios locais configurados.
- Consultar banco SQL Server via MCP existente.
- Buscar análises semelhantes no histórico.
- Gerar relatório HTML individual por chamado.
- Salvar metadados, resumo, relatório completo e logs no banco.
- Salvar prompts mascarados.
- Registrar fontes utilizadas.
- Registrar modelo, duração e tokens/custo quando disponíveis.
- Gerar pasta de execução.
- Enviar e-mail consolidado com resumo e links.
- Permitir execução manual por número de chamado.
- Operar sem alterar GLPI, banco ou código-fonte.

## 31. Indicadores de sucesso

- Redução do tempo médio de diagnóstico.
- Aumento da taxa de identificação correta do sistema afetado.
- Aumento do acerto nas hipóteses iniciais.
- Redução de retrabalho investigativo.
- Geração de histórico técnico reutilizável.
- Redução de análises repetidas para problemas recorrentes.
- Maior padronização na triagem técnica.
- Maior rastreabilidade das evidências usadas.
- Melhoria na qualidade das respostas técnicas ao solicitante.

## 32. Riscos e cuidados

### 32.1 Risco: análise parecer mais conclusiva do que realmente é

Mitigação:

- sempre exibir nível de confiança;
- destacar dados insuficientes;
- apresentar hipóteses como hipóteses;
- manter revisão humana obrigatória.

### 32.2 Risco: acesso indevido a dados

Mitigação:

- operação somente leitura;
- uso do MCP já permissionado;
- registrar consultas;
- mascarar dados sensíveis em prompts e logs.

### 32.3 Risco: dependência de modelo externo

Mitigação:

- arquitetura plugável;
- suporte previsto a Azure OpenAI, OpenAI, Ollama e Foundry Local;
- possibilidade de execução local/interna.

### 32.4 Risco: chamados mal descritos

Mitigação:

- relatório mínimo para sistema não identificado;
- perguntas obrigatórias ao solicitante;
- indicação explícita de contexto insuficiente.

### 32.5 Risco: excesso de ruído no e-mail

Mitigação:

- e-mail consolidado;
- relatório completo separado em HTML;
- filtro de chamados novos/atualizados.

## 33. Arquitetura conceitual

Componentes principais:

1. **Scheduler/Executor**
   - No MVP: execução por Console App e agendamento externo.
   - Futuro: Hangfire.

2. **GLPI Connector**
   - Consulta chamados, comentários, status e anexos.

3. **Ticket Classifier**
   - Classifica tipo do chamado e elegibilidade.

4. **System Resolver**
   - Identifica sistema provável com base em texto, palavras-chave e configuração.

5. **Image Analyzer**
   - Extrai mensagens de erro e contexto visual de prints.

6. **Repository Analyzer**
   - Busca arquivos, classes, endpoints e mensagens de erro nos repositórios.

7. **Database MCP Client**
   - Consulta SQL Server via MCP existente.

8. **History Analyzer**
   - Busca análises anteriores semelhantes.

9. **AI Orchestrator**
   - Coordena prompts, ferramentas, evidências e geração da análise.

10. **Report Generator**
    - Gera HTML individual e resumo do e-mail.

11. **Email Sender**
    - Envia e-mail consolidado para destinatários configurados.

12. **Audit/History Store**
    - Salva execuções, análises, fontes, prompts mascarados e logs.

## 34. Modelo de dados conceitual

Entidades sugeridas para o MVP:

- `Execucao`
- `ChamadoAnalisado`
- `Analise`
- `FonteUtilizada`
- `ConsultaExecutada`
- `ArquivoAnalisado`
- `ImagemAnalisada`
- `PromptRegistrado`
- `ErroExecucao`
- `ConfiguracaoSistema`

## 35. Roadmap sugerido

### Fase 1 — MVP local

- Console App.
- SQLite.
- appsettings.json.
- GLPI Connector.
- Repositórios locais.
- MCP SQL Server existente.
- Relatório HTML.
- E-mail consolidado.
- Histórico e logs.

### Fase 2 — Robustez

- Melhorias no classificador.
- Busca mais avançada em histórico.
- Melhor template HTML.
- Métricas de diagnóstico.
- Comparação de análises semelhantes.
- Melhorias de mascaramento.

### Fase 3 — Produção interna

- Migração para SQL Server.
- Hangfire.
- Configuração em banco.
- Melhor controle de permissões.
- Observabilidade.
- Validação com líder técnico, infraestrutura e segurança.

### Fase 4 — Uso institucional

- Tela administrativa.
- Múltiplos desenvolvedores.
- Dashboard.
- Indicadores gerenciais.
- Integração segura com logs.
- Rascunho de resposta no GLPI com aprovação humana.

## 36. Decisões finais confirmadas

- Nome: **Agente de Pré-Análise GLPI**.
- Banco do MVP: **SQLite configurável no appsettings.json**.
- Banco em produção: **SQL Server**.
- Visual do HTML: **bonito, legível e compartilhável com devs, sem exagero**.
- Pasta de execuções: **sim**.
- E-mail: **lista configurável, começando apenas com o desenvolvedor responsável**.
- Registrar custo/tempo/modelo: **sim**.
- Salvar prompts: **sim, com dados sensíveis mascarados**.
- Mostrar fontes utilizadas: **obrigatório**.
- Severidade técnica: **recomendação do agente, sem alterar o GLPI**.
- Formatos desejados do PRD: **Markdown e HTML**.
