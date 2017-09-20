namespace MarkDbModel.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Fix091902 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Operation",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        TargetGuid = c.String(),
                        Action = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            AddColumn("dbo.MarkClient", "ClientGuid", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.MarkClient", "ClientGuid");
            DropTable("dbo.Operation");
        }
    }
}
