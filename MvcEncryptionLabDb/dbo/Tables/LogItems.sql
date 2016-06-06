CREATE TABLE [dbo].[LogItems] (
    [LogItemId]              INT              IDENTITY (1, 1) NOT NULL,
    [UserName]               NVARCHAR (25)    NULL,
    [Target]                 NVARCHAR (255)   NULL,
    [CreateDateTime]         DATETIME         NOT NULL,
    [Text]                   NVARCHAR (MAX)   NULL,
    [Type]                   INT              NOT NULL,
    [ProcessId]              UNIQUEIDENTIFIER NOT NULL,
    [ProcessPercentComplete] INT              NOT NULL,
    CONSTRAINT [PK_dbo.LogItems] PRIMARY KEY CLUSTERED ([LogItemId] ASC)
);

