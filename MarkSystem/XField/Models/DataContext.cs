using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace XField.Models
{
    #region Entity

    public class Blog
    {
        public string Id { get; set; }
        public string Content { get; set; }

        public virtual ICollection<Comment> Comments { get; set; }
        public virtual ICollection<Tag> Tags { get; set; }
    }

    public class Comment
    {
        public string Id { get; set; }
        public string Content { get; set; }

        public string BlogId { get; set; }

        [ForeignKey("BlogId")]
        public virtual Blog Blog { get; set; }
    }

    public class Tag
    {
        public string Id { get; set; }
        public string Name { get; set; }

        public string BlogId { get; set; }

        [ForeignKey("BlogId")]
        public virtual Blog Blog { get; set; }
    }
    #endregion Entity

    /* 显示定义 外键
     * 测试EF的级联删除功能，
     * 期望情况下，删除Blog会删除这个Blog下的所有Comment，但是不会删除Tag
     * 会将对应的Tag外键设定为Null
     */ 
    public class DataContext : DbContext
    {
        public static string ConnectionString = "Data Source=127.0.0.1;Initial Catalog=XField;user=sa;password=keyman;";
        public DataContext() : base(ConnectionString) { }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();
            base.OnModelCreating(modelBuilder);
        }

        #region DataTable
        public DbSet<Blog> Blog { get; set; }
        public DbSet<Comment> Comment { get; set; }
        public DbSet<Tag> Tag { get; set; }
        #endregion DataTables
    }
}
