namespace MarkDbModel.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class OnlineCache : DbMigration
    {
        public override void Up()
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
            
        }
        
        public override void Down()
        {
            DropTable("dbo.OnlineCache");
        }
    }
}
