CREATE TABLE [dbo].[CaZipCodes] (
    [CaZipCodeId] INT            IDENTITY (1, 1) NOT NULL,
    [ZipCode]     NVARCHAR (MAX) NULL,
    [City]        NVARCHAR (MAX) NULL,
    [Population]  INT            NOT NULL,
    [RangeLow]    INT            NOT NULL,
    [RangeHigh]   INT            NOT NULL,
    CONSTRAINT [PK_dbo.CaZipCodes] PRIMARY KEY CLUSTERED ([CaZipCodeId] ASC)
);

