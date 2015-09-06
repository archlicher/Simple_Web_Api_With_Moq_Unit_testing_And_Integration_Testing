using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using OnlineShop.Data;
using OnlineShop.Data.UnitOfWork;
using OnlineShop.Services.Infrastructure;

namespace OnlineShop.Services.Controllers
{
    public class BaseController : ApiController
    {
        public BaseController(IOnlineShopData context)
        {
            this.Data = context;
        }

        public BaseController()
            :this(new OnlineShopData(new OnlineShopContext()),
                new AspNetUserIdProvider())
        {
            
        }

        public BaseController(IOnlineShopData context, IUserIdProvider userId)
        {
            this.Data = context;
            this.UserId = userId;
        }

        protected IOnlineShopData Data { get; set; }

        protected IUserIdProvider UserId { get; set; }
    }
}