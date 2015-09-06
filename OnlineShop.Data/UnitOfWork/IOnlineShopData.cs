using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OnlineShop.Data.Repository;
using OnlineShop.Models;

namespace OnlineShop.Data.UnitOfWork
{
    public interface IOnlineShopData
    {
        IRepository<Ad> Ads { get; }

        IRepository<AdType> AdTypes { get; }

        IRepository<Category> Categories { get; }

        IRepository<ApplicationUser> Users { get; } 

        int SaveChanges();
    }
}
