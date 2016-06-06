CREATE TABLE [dbo].[Addresses] (
    [AddressId]             INT            IDENTITY (1, 1) NOT NULL,
    [AddressLine1IV]        NVARCHAR (64)  NULL,
    [AddressLine1Encrypted] NVARCHAR (255) NULL,
    [AddressLine2]          NVARCHAR (MAX) NULL,
    [City]                  NVARCHAR (MAX) NULL,
    [State]                 NVARCHAR (MAX) NULL,
    [Zip]                   NVARCHAR (MAX) NULL,
    CONSTRAINT [PK_dbo.Addresses] PRIMARY KEY CLUSTERED ([AddressId] ASC)
);

