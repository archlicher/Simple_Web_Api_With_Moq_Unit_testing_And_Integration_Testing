using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Web;
using Microsoft.Ajax.Utilities;
using OnlineShop.Models;

namespace OnlineShop.Services.Models
{
    public class AdsViewModel
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public decimal Price { get; set; }

        public string Type { get; set; }

        public DateTime PostedOn { get; set; }

        public UserViewModel Owner { get; set; }

        public IEnumerable<CategoriesViewModel> Categories { get; set; }

        public static Expression<Func<Ad, AdsViewModel>> Create
        {
            get
            {
                return ad => new AdsViewModel()
                {
                    Categories = ad.Categories.Select(c => new CategoriesViewModel()
                    {
                        Id = c.Id,
                        Name = c.Name
                    }),
                    Owner = new UserViewModel() {Id = ad.OwnerId, Username = ad.Owner.UserName},
                    Name = ad.Name,
                    Description = ad.Description,
                    Price = ad.Price,
                    Type = ad.Type.Name,
                    Id = ad.Id,
                    PostedOn = ad.PostedOn
                };
            }
        }
    }
}