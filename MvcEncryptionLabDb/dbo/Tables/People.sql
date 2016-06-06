CREATE TABLE [dbo].[People] (
    [PersonId]          INT            IDENTITY (1, 1) NOT NULL,
    [FirstName]         NVARCHAR (50)  NULL,
    [LastNameIV]        NVARCHAR (64)  NOT NULL,
    [LastNameEncrypted] NVARCHAR (255) NOT NULL,
    [SSNIV]             NVARCHAR (64)  NOT NULL,
    [SSNEncrypted]      NVARCHAR (255) NOT NULL,
    [SSNSalt]           NVARCHAR (64)  NOT NULL,
    [SSNHash]           NVARCHAR (64)  NOT NULL,
    [Address_AddressId] INT            NULL,
    CONSTRAINT [PK_dbo.People] PRIMARY KEY CLUSTERED ([PersonId] ASC),
    CONSTRAINT [FK_dbo.People_dbo.Addresses_Address_AddressId] FOREIGN KEY ([Address_AddressId]) REFERENCES [dbo].[Addresses] ([AddressId])
);


GO
CREATE NONCLUSTERED INDEX [IX_Address_AddressId]
    ON [dbo].[People]([Address_AddressId] ASC);

