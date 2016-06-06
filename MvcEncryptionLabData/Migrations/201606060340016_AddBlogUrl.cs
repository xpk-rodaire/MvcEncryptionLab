namespace MvcEncryptionLabData.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddBlogUrl : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.LogItems", "UserName", c => c.String(maxLength: 25));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.LogItems", "UserName", c => c.String(maxLength: 35));
        }
    }
}
