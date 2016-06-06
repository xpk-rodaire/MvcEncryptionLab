namespace MvcEncryptionLabData.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class FakeField : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.LogItems", "FakeFIeld", c => c.String(nullable: false));

            Sql("UPDATE dbo.LogItems SET FakeFIeld = 'This is a fake value'"); 
        }
        
        public override void Down()
        {
            DropColumn("dbo.LogItems", "FakeFIeld");
        }
    }
}
