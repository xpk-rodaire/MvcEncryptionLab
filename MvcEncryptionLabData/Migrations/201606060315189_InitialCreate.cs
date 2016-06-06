namespace MvcEncryptionLabData.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialCreate : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Addresses",
                c => new
                    {
                        AddressId = c.Int(nullable: false, identity: true),
                        AddressLine1IV = c.String(maxLength: 64),
                        AddressLine1Encrypted = c.String(maxLength: 255),
                        AddressLine2 = c.String(),
                        City = c.String(),
                        State = c.String(),
                        Zip = c.String(),
                    })
                .PrimaryKey(t => t.AddressId);
            
            CreateTable(
                "dbo.CaZipCodes",
                c => new
                    {
                        CaZipCodeId = c.Int(nullable: false, identity: true),
                        ZipCode = c.String(),
                        City = c.String(),
                        Population = c.Int(nullable: false),
                        RangeLow = c.Int(nullable: false),
                        RangeHigh = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.CaZipCodeId);
            
            CreateTable(
                "dbo.FirstNames",
                c => new
                    {
                        FirstNameId = c.Int(nullable: false, identity: true),
                        Value = c.String(),
                        Frequency = c.Single(nullable: false),
                        CumulativeFrequency = c.Single(nullable: false),
                        Rank = c.Int(nullable: false),
                        IsMale = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.FirstNameId);
            
            CreateTable(
                "dbo.LastNames",
                c => new
                    {
                        LastNameId = c.Int(nullable: false, identity: true),
                        Value = c.String(),
                        Frequency = c.Single(nullable: false),
                        CumulativeFrequency = c.Single(nullable: false),
                        Rank = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.LastNameId);
            
            CreateTable(
                "dbo.LogItems",
                c => new
                    {
                        LogItemId = c.Int(nullable: false, identity: true),
                        UserName = c.String(maxLength: 35),
                        Target = c.String(maxLength: 255),
                        CreateDateTime = c.DateTime(nullable: false),
                        Text = c.String(),
                        Type = c.Int(nullable: false),
                        ProcessId = c.Guid(nullable: false),
                        ProcessPercentComplete = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.LogItemId);
            
            CreateTable(
                "dbo.People",
                c => new
                    {
                        PersonId = c.Int(nullable: false, identity: true),
                        FirstName = c.String(maxLength: 50),
                        LastNameIV = c.String(nullable: false, maxLength: 64),
                        LastNameEncrypted = c.String(nullable: false, maxLength: 255),
                        SSNIV = c.String(nullable: false, maxLength: 64),
                        SSNEncrypted = c.String(nullable: false, maxLength: 255),
                        SSNSalt = c.String(nullable: false, maxLength: 64),
                        SSNHash = c.String(nullable: false, maxLength: 64),
                        Address_AddressId = c.Int(),
                    })
                .PrimaryKey(t => t.PersonId)
                .ForeignKey("dbo.Addresses", t => t.Address_AddressId)
                .Index(t => t.Address_AddressId);
            
            CreateTable(
                "dbo.SecurityKeys",
                c => new
                    {
                        SecurityKeyId = c.Int(nullable: false, identity: true),
                        SecurityKeySalt = c.String(nullable: false, maxLength: 64),
                        SecurityKeyHash = c.String(nullable: false, maxLength: 64),
                    })
                .PrimaryKey(t => t.SecurityKeyId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.People", "Address_AddressId", "dbo.Addresses");
            DropIndex("dbo.People", new[] { "Address_AddressId" });
            DropTable("dbo.SecurityKeys");
            DropTable("dbo.People");
            DropTable("dbo.LogItems");
            DropTable("dbo.LastNames");
            DropTable("dbo.FirstNames");
            DropTable("dbo.CaZipCodes");
            DropTable("dbo.Addresses");
        }
    }
}
