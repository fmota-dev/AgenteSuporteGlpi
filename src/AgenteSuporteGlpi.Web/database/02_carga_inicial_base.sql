SET NOCOUNT ON;
GO

/*
    Script: 02_carga_inicial_base.sql
    Objetivo:
        Popular a base inicial do starter com status, perfis e usuários demonstrativos.

    Observação:
        As senhas saem como texto puro "123456" para simplificar a primeira carga.
        No primeiro login bem-sucedido, o auth custom pode regravar o valor em hash.
*/

IF NOT EXISTS (SELECT 1 FROM dbo.TB_StatusSTA WHERE PK_Cod_StatusSTA = 1)
BEGIN
    SET IDENTITY_INSERT dbo.TB_StatusSTA ON;

    INSERT INTO dbo.TB_StatusSTA
    (
        PK_Cod_StatusSTA,
        TXT_NomeSTA,
        TXT_CorSTA,
        TXT_TipoSTA
    )
    VALUES
        (1, N'Ativo', N'#16A34A', N'Cadastro'),
        (2, N'Inativo', N'#64748B', N'Cadastro');

    SET IDENTITY_INSERT dbo.TB_StatusSTA OFF;
END;
GO

IF NOT EXISTS (SELECT 1 FROM dbo.TB_PerfisPER WHERE TXT_NomePER = N'Administrador')
BEGIN
    INSERT INTO dbo.TB_PerfisPER (TXT_NomePER, FK_COD_StatusPER, DAT_CriacaoPER)
    VALUES
        (N'Administrador', 1, SYSUTCDATETIME()),
        (N'Gestor', 1, SYSUTCDATETIME()),
        (N'Colaborador', 1, SYSUTCDATETIME());
END;
GO

IF NOT EXISTS (SELECT 1 FROM dbo.TB_UsuariosUSU WHERE TXT_EmailUSU = N'admin@corp.local')
BEGIN
    INSERT INTO dbo.TB_UsuariosUSU
    (
        TXT_NomeUSU,
        TXT_EmailUSU,
        TXT_SenhaCriptografadaUSU,
        FK_COD_StatusUSU,
        FK_COD_PerfilUSU,
        DAT_CriacaoUSU,
        DAT_UltimoAcessoUSU,
        BIT_BloqueioHabilitadoUSU,
        NUM_FalhasAcessoUSU,
        DAT_FimBloqueioUSU
    )
    SELECT
        N'Aline Administradora',
        N'admin@corp.local',
        N'123456',
        1,
        p.PK_Cod_PerfilPER,
        SYSUTCDATETIME(),
        DATEADD(HOUR, -1, SYSUTCDATETIME()),
        1,
        0,
        NULL
    FROM dbo.TB_PerfisPER p
    WHERE p.TXT_NomePER = N'Administrador';
END;
GO

IF NOT EXISTS (SELECT 1 FROM dbo.TB_UsuariosUSU WHERE TXT_EmailUSU = N'gestor@corp.local')
BEGIN
    INSERT INTO dbo.TB_UsuariosUSU
    (
        TXT_NomeUSU,
        TXT_EmailUSU,
        TXT_SenhaCriptografadaUSU,
        FK_COD_StatusUSU,
        FK_COD_PerfilUSU,
        DAT_CriacaoUSU,
        DAT_UltimoAcessoUSU,
        BIT_BloqueioHabilitadoUSU,
        NUM_FalhasAcessoUSU,
        DAT_FimBloqueioUSU
    )
    SELECT
        N'Bruno Gestor',
        N'gestor@corp.local',
        N'123456',
        1,
        p.PK_Cod_PerfilPER,
        SYSUTCDATETIME(),
        DATEADD(DAY, -1, SYSUTCDATETIME()),
        1,
        1,
        NULL
    FROM dbo.TB_PerfisPER p
    WHERE p.TXT_NomePER = N'Gestor';
END;
GO

IF NOT EXISTS (SELECT 1 FROM dbo.TB_UsuariosUSU WHERE TXT_EmailUSU = N'colaborador@corp.local')
BEGIN
    INSERT INTO dbo.TB_UsuariosUSU
    (
        TXT_NomeUSU,
        TXT_EmailUSU,
        TXT_SenhaCriptografadaUSU,
        FK_COD_StatusUSU,
        FK_COD_PerfilUSU,
        DAT_CriacaoUSU,
        DAT_UltimoAcessoUSU,
        BIT_BloqueioHabilitadoUSU,
        NUM_FalhasAcessoUSU,
        DAT_FimBloqueioUSU
    )
    SELECT
        N'Carla Colaboradora',
        N'colaborador@corp.local',
        N'123456',
        1,
        p.PK_Cod_PerfilPER,
        SYSUTCDATETIME(),
        DATEADD(DAY, -3, SYSUTCDATETIME()),
        1,
        5,
        DATEADD(HOUR, 12, SYSUTCDATETIME())
    FROM dbo.TB_PerfisPER p
    WHERE p.TXT_NomePER = N'Colaborador';
END;
GO
