using Microsoft.AspNet.Identity.EntityFramework;
using OnlineShop.Data.Migrations;
using OnlineShop.Models;

namespace OnlineShop.Data
{
    using System;
    using System.Data.Entity;
    using System.Linq;

    public class OnlineShopContext : IdentityDbContext<ApplicationUser>
    {
        // Your context has been configured to use a 'OnlineShopContext' connection string from your application's 
        // configuration file (App.config or Web.config). By default, this connection string targets the 
        // 'OnlineShop.Data.OnlineShopContext' database on your LocalDb instance. 
        // 
        // If you wish to target a different database and/or database provider, modify the 'OnlineShopContext' 
        // connection string in the application configuration file.
        public OnlineShopContext()
            : base("OnlineShopContext")
        {
            Database.SetInitializer(new MigrateDatabaseToLatestVersion<OnlineShopContext, Configuration>());
        }


        public static OnlineShopContext Create()
        {
            return new OnlineShopContext();
        }
        public virtual DbSet<Ad> Ads { get; set; }
        public virtual DbSet<AdType> AdTypes { get; set; }
        public virtual DbSet<Category> Categories { get; set; } 

        // Add a DbSet for each entity type that you want to include in your model. For more information 
        // on configuring and using a Code First model, see http://go.microsoft.com/fwlink/?LinkId=390109.

        // public virtual DbSet<MyEntity> MyEntities { get; set; }
    }

    //public class MyEntity
    //{
    //    public int Id { get; set; }
    //    public string Name { get; set; }
    //}
}