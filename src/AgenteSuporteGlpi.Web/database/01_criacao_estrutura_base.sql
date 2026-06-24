SET NOCOUNT ON;
GO

/*
    Script: 01_criacao_estrutura_base.sql
    Objetivo:
        Criar a estrutura inicial de autenticação e acessos do starter corporativo.

    Convenções:
        Tabela:    TB_[NomePlural][SIGLA]
        PKs:       PK_Cod_[Nome][SIGLA]
        Textos:    TXT_[Nome][SIGLA]
        Datas:     DAT_[Nome][SIGLA]
        Booleanos: BIT_[Nome][SIGLA]
        FKs:       FK_COD_[Nome][SIGLA]
*/

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'TB_StatusSTA')
BEGIN
    CREATE TABLE dbo.TB_StatusSTA
    (
        PK_Cod_StatusSTA        INT IDENTITY(1,1) NOT NULL,
        TXT_NomeSTA             VARCHAR(100) NOT NULL,
        TXT_CorSTA              VARCHAR(30) NULL,
        TXT_TipoSTA             VARCHAR(50) NULL,
        DAT_CriacaoSTA          DATETIME2(0) NOT NULL CONSTRAINT DF_DAT_CriacaoSTA DEFAULT SYSUTCDATETIME(),
        CONSTRAINT PK_TB_StatusSTA PRIMARY KEY CLUSTERED (PK_Cod_StatusSTA),
        CONSTRAINT UQ_TXT_NomeSTA UNIQUE (TXT_NomeSTA)
    );
END;
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'TB_PerfisPER')
BEGIN
    CREATE TABLE dbo.TB_PerfisPER
    (
        PK_Cod_PerfilPER        INT IDENTITY(1,1) NOT NULL,
        TXT_NomePER             VARCHAR(100) NOT NULL,
        FK_COD_StatusPER        INT NOT NULL,
        DAT_CriacaoPER          DATETIME2(0) NOT NULL CONSTRAINT DF_DAT_CriacaoPER DEFAULT SYSUTCDATETIME(),
        CONSTRAINT PK_TB_PerfisPER PRIMARY KEY CLUSTERED (PK_Cod_PerfilPER),
        CONSTRAINT UQ_TXT_NomePER UNIQUE (TXT_NomePER),
        CONSTRAINT FK_TB_PerfisPER_TB_StatusSTA
            FOREIGN KEY (FK_COD_StatusPER) REFERENCES dbo.TB_StatusSTA(PK_Cod_StatusSTA)
    );
END;
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'TB_UsuariosUSU')
BEGIN
    CREATE TABLE dbo.TB_UsuariosUSU
    (
        PK_Cod_UsuarioUSU               INT IDENTITY(1,1) NOT NULL,
        TXT_NomeUSU                     VARCHAR(200) NOT NULL,
        TXT_EmailUSU                    VARCHAR(200) NOT NULL,
        TXT_SenhaCriptografadaUSU       VARCHAR(500) NOT NULL,
        FK_COD_StatusUSU                INT NOT NULL,
        FK_COD_PerfilUSU                INT NOT NULL,
        DAT_CriacaoUSU                  DATETIME2(0) NOT NULL CONSTRAINT DF_DAT_CriacaoUSU DEFAULT SYSUTCDATETIME(),
        DAT_UltimoAcessoUSU             DATETIME2(0) NULL,
        BIT_BloqueioHabilitadoUSU       BIT NOT NULL CONSTRAINT DF_BIT_BloqueioHabilitadoUSU DEFAULT (1),
        NUM_FalhasAcessoUSU             INT NOT NULL CONSTRAINT DF_NUM_FalhasAcessoUSU DEFAULT (0),
        DAT_FimBloqueioUSU              DATETIMEOFFSET(0) NULL,
        CONSTRAINT PK_TB_UsuariosUSU PRIMARY KEY CLUSTERED (PK_Cod_UsuarioUSU),
        CONSTRAINT UQ_TXT_EmailUSU UNIQUE (TXT_EmailUSU),
        CONSTRAINT FK_TB_UsuariosUSU_TB_StatusSTA
            FOREIGN KEY (FK_COD_StatusUSU) REFERENCES dbo.TB_StatusSTA(PK_Cod_StatusSTA),
        CONSTRAINT FK_TB_UsuariosUSU_TB_PerfisPER
            FOREIGN KEY (FK_COD_PerfilUSU) REFERENCES dbo.TB_PerfisPER(PK_Cod_PerfilPER)
    );
END;
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_TB_PerfisPER_FK_COD_StatusPER')
BEGIN
    CREATE NONCLUSTERED INDEX IX_TB_PerfisPER_FK_COD_StatusPER
        ON dbo.TB_PerfisPER(FK_COD_StatusPER);
END;
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_TB_UsuariosUSU_FK_COD_StatusUSU')
BEGIN
    CREATE NONCLUSTERED INDEX IX_TB_UsuariosUSU_FK_COD_StatusUSU
        ON dbo.TB_UsuariosUSU(FK_COD_StatusUSU);
END;
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_TB_UsuariosUSU_FK_COD_PerfilUSU')
BEGIN
    CREATE NONCLUSTERED INDEX IX_TB_UsuariosUSU_FK_COD_PerfilUSU
        ON dbo.TB_UsuariosUSU(FK_COD_PerfilUSU);
END;
GO
