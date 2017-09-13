using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MarkDbModel.Entity;


namespace MarkDbModel
{
    public class ApplicationDbContext : DbContext
    {
        public static string ConnectionString = "Data Source=127.0.0.1;Initial Catalog=MarkSystem;user=sa;password=keyman;";

        public ApplicationDbContext() : base(ConnectionString) { }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();
        }


        #region DataTable
        public DbSet<MarkClient> MarkClient { get; set; }

        public DbSet<Command> Command { get; set; }

        public DbSet<CommandGroup> CommandGroup { get; set; }

        public DbSet<OnlineCache> OnlineCache { get; set; }

        #endregion DataTable
    }
}
