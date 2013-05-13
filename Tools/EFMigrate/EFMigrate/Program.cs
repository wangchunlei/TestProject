using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Data.Entity.Migrations.Infrastructure;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EFMigrate
{
    class Program
    {
        static void Main(string[] args)
        {
            Database.SetInitializer(new MigrateDatabaseToLatestVersion<MyContext, MyConfiguration>());
            Database.SetInitializer(new DropCreateDatabaseAlways<MyContext>());
            //var migrator = new DbMigrator(new DbMigrationsConfiguration(){});
            //var dbms = migrator.GetDatabaseMigrations();
            //var dbls = migrator.GetLocalMigrations();
            //var dbps = migrator.GetPendingMigrations();
            //migrator.Update();

            //var migratorConfig = new MyConfiguration();
            //    //{
            //    //    AutomaticMigrationDataLossAllowed = true,
            //    //    AutomaticMigrationsEnabled = true,
            //    //};

            //var migrator = new DbMigrator(migratorConfig);

            //var scriptor = new MigratorScriptingDecorator(migrator);
            //var script = scriptor.ScriptUpdate()
            //migrator.Update();
            using (var context = new MyContext())
            {
                context.Database.Initialize(true);
                //for (int i = 0; i < 100; i++)
                //{
                //     var user = new User()
                //    {
                //        Name = "1",
                //        Code = "2",
                //        Address2 = "3",
                //        Age = 4,
                //        Phone1 = "5",
                //        UserID = i.ToString()
                //    };
                //    context.Users.Add(user);
                //}
                //context.SaveChanges();
            }
        }
    }
}
