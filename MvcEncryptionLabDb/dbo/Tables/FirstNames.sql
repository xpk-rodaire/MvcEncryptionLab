CREATE TABLE [dbo].[FirstNames] (
    [FirstNameId]         INT            IDENTITY (1, 1) NOT NULL,
    [Value]               NVARCHAR (MAX) NULL,
    [Frequency]           REAL           NOT NULL,
    [CumulativeFrequency] REAL           NOT NULL,
    [Rank]                INT            NOT NULL,
    [IsMale]              BIT            NOT NULL,
    CONSTRAINT [PK_dbo.FirstNames] PRIMARY KEY CLUSTERED ([FirstNameId] ASC)
);

