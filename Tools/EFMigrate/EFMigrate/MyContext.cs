using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EFMigrate
{
    public class MyContext : DbContext
    {
        public MyContext()
            : base("MyContext")
        {

        }

        public DbSet<User> Users { get; set; }
    }

    public class MyConfiguration : DbMigrationsConfiguration<MyContext>
    {
        public MyConfiguration()
        {
            AutomaticMigrationsEnabled = true;
            AutomaticMigrationDataLossAllowed = true;
        }

        protected override void Seed(MyContext context)
        {
            base.Seed(context);
        }
    }
}
