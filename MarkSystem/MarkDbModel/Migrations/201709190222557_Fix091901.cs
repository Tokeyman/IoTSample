namespace MarkDbModel.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Fix091901 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.MarkClient", "Description", c => c.String());
            AddColumn("dbo.MarkClient", "Online", c => c.Boolean(nullable: false));
            AddColumn("dbo.MarkClient", "Status", c => c.String());
            AddColumn("dbo.MarkClient", "DataCache", c => c.String());
            DropTable("dbo.OnlineCache");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.OnlineCache",
                c => new
                    {
                        ClientGuid = c.String(nullable: false, maxLength: 128),
                        Ip = c.String(),
                        Port = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.ClientGuid);
            
            DropColumn("dbo.MarkClient", "DataCache");
            DropColumn("dbo.MarkClient", "Status");
            DropColumn("dbo.MarkClient", "Online");
            DropColumn("dbo.MarkClient", "Description");
        }
    }
}
