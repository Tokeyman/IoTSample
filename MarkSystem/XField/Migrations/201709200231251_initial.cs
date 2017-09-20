namespace XField.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class initial : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Blog",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Content = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Comment",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Content = c.String(),
                        BlogId = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Blog", t => t.BlogId)
                .Index(t => t.BlogId);
            
            CreateTable(
                "dbo.Tag",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Name = c.String(),
                        BlogId = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Blog", t => t.BlogId)
                .Index(t => t.BlogId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Tag", "BlogId", "dbo.Blog");
            DropForeignKey("dbo.Comment", "BlogId", "dbo.Blog");
            DropIndex("dbo.Tag", new[] { "BlogId" });
            DropIndex("dbo.Comment", new[] { "BlogId" });
            DropTable("dbo.Tag");
            DropTable("dbo.Comment");
            DropTable("dbo.Blog");
        }
    }
}
