# Uso Local Do Coletor GLPI

## Objetivo

Executar coleta somente leitura de chamados GLPI atribuídos ao responsável configurado.

## Configuração Não Sensível

Arquivo: `src/AgenteSuporteGlpi/appsettings.json`

- `Glpi:UrlBase`
- `Glpi:Responsavel`
- `Glpi:LimiteChamadosPorExecucao`
- `Browser:Headless`
- `Browser:TimeoutMilissegundos`
- `Banco:ConnectionString`

## Configuração Privada Local

Use User Secrets no projeto Console:

```powershell
dotnet user-secrets init --project src/AgenteSuporteGlpi/AgenteSuporteGlpi.csproj
dotnet user-secrets set "Glpi:UsuarioLogin" "seu-usuario" --project src/AgenteSuporteGlpi/AgenteSuporteGlpi.csproj
dotnet user-secrets set "Glpi:SenhaLogin" "sua-senha" --project src/AgenteSuporteGlpi/AgenteSuporteGlpi.csproj
```

## Execução

```powershell
dotnet run --project src/AgenteSuporteGlpi/AgenteSuporteGlpi.csproj
```

## Segurança

O coletor não deve comentar, salvar, alterar status, prioridade, categoria ou responsável. Se o GLPI exigir captcha, MFA, troca de senha ou confirmação inesperada, a execução deve abortar.
