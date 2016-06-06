CREATE TABLE [dbo].[LastNames] (
    [LastNameId]          INT            IDENTITY (1, 1) NOT NULL,
    [Value]               NVARCHAR (MAX) NULL,
    [Frequency]           REAL           NOT NULL,
    [CumulativeFrequency] REAL           NOT NULL,
    [Rank]                INT            NOT NULL,
    CONSTRAINT [PK_dbo.LastNames] PRIMARY KEY CLUSTERED ([LastNameId] ASC)
);

