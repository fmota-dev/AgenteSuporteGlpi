SET NOCOUNT ON;
GO

/*
    Script: 03_ajuste_tabela_usuarios_auth_custom.sql
    Objetivo:
        Garantir que a TB_UsuariosUSU esteja alinhada ao auth custom do starter.
*/

IF OBJECT_ID('dbo.TB_UsuariosUSU', 'U') IS NULL
BEGIN
    RAISERROR('A tabela dbo.TB_UsuariosUSU não foi encontrada.', 16, 1);
    RETURN;
END;
GO

IF COL_LENGTH('dbo.TB_UsuariosUSU', 'DAT_UltimoAcessoUSU') IS NULL
BEGIN
    ALTER TABLE dbo.TB_UsuariosUSU ADD DAT_UltimoAcessoUSU DATETIME2(0) NULL;
END;
GO

IF COL_LENGTH('dbo.TB_UsuariosUSU', 'BIT_BloqueioHabilitadoUSU') IS NULL
BEGIN
    ALTER TABLE dbo.TB_UsuariosUSU ADD BIT_BloqueioHabilitadoUSU BIT NOT NULL CONSTRAINT DF_BIT_BloqueioHabilitadoUSU_AJUSTE DEFAULT (1);
END;
GO

IF COL_LENGTH('dbo.TB_UsuariosUSU', 'NUM_FalhasAcessoUSU') IS NULL
BEGIN
    ALTER TABLE dbo.TB_UsuariosUSU ADD NUM_FalhasAcessoUSU INT NOT NULL CONSTRAINT DF_NUM_FalhasAcessoUSU_AJUSTE DEFAULT (0);
END;
GO

IF COL_LENGTH('dbo.TB_UsuariosUSU', 'DAT_FimBloqueioUSU') IS NULL
BEGIN
    ALTER TABLE dbo.TB_UsuariosUSU ADD DAT_FimBloqueioUSU DATETIMEOFFSET(0) NULL;
END;
GO

PRINT 'TB_UsuariosUSU ajustada para o auth custom com sucesso.';
GO
