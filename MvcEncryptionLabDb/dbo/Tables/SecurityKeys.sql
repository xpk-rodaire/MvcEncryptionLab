CREATE TABLE [dbo].[SecurityKeys] (
    [SecurityKeyId]   INT           IDENTITY (1, 1) NOT NULL,
    [SecurityKeySalt] NVARCHAR (64) NOT NULL,
    [SecurityKeyHash] NVARCHAR (64) NOT NULL,
    CONSTRAINT [PK_dbo.SecurityKeys] PRIMARY KEY CLUSTERED ([SecurityKeyId] ASC)
);

