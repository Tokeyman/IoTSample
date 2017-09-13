namespace MarkDbModel.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Initialize : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Command",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Index = c.Int(nullable: false),
                        TimeSpan = c.Time(nullable: false, precision: 7),
                        CommandContext = c.String(),
                        IsRepeat = c.Boolean(nullable: false),
                        CommandGroupId = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.CommandGroup", t => t.CommandGroupId)
                .Index(t => t.CommandGroupId);
            
            CreateTable(
                "dbo.CommandGroup",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Description = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.MarkClient",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Ip = c.String(),
                        Port = c.Int(nullable: false),
                        CommandGroupId = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.CommandGroup", t => t.CommandGroupId)
                .Index(t => t.CommandGroupId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.MarkClient", "CommandGroupId", "dbo.CommandGroup");
            DropForeignKey("dbo.Command", "CommandGroupId", "dbo.CommandGroup");
            DropIndex("dbo.MarkClient", new[] { "CommandGroupId" });
            DropIndex("dbo.Command", new[] { "CommandGroupId" });
            DropTable("dbo.MarkClient");
            DropTable("dbo.CommandGroup");
            DropTable("dbo.Command");
        }
    }
}
