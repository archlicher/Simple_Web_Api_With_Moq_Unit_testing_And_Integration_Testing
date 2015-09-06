using OnlineShop.Services.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using Microsoft.AspNet.Identity;
using OnlineShop.Data;
using OnlineShop.Data.UnitOfWork;
using OnlineShop.Models;
using OnlineShop.Services.Infrastructure;

namespace OnlineShop.Services.Controllers
{
    [Authorize]
    public class AdsController : BaseController
    {

        public AdsController()
        {
        }

        public AdsController(IOnlineShopData onlineShopData)
            : base(onlineShopData)
        {
        }

        public AdsController(IOnlineShopData onlineShopData, IUserIdProvider userId)
            :base(onlineShopData, userId)
        {
        }

        [AllowAnonymous]
        public IHttpActionResult GetAds()
        {
            var ads = this.Data.Ads.All()
                .OrderByDescending(a => a.Type.Index)
                .ThenByDescending(a => a.PostedOn)
                .Select(AdsViewModel.Create);

            return this.Ok(ads);
        }

        [HttpPost]
        public IHttpActionResult CreateAd(CreateAdBindingModel model)
        {
            var userId = this.UserId;
            
            if (userId == null) return this.NotFound();

            if (model == null) return this.BadRequest("No data sent");
            
            if (!this.ModelState.IsValid) return this.BadRequest(this.ModelState);

            if (model.Categories == null || model.Categories.Count() > 3)
            {
                return this.BadRequest("Categories should be atleast 1 and no more than 3");
            }

            if (model.Categories.Select(cat => this.Data.Categories.All().FirstOrDefault(c => c.Id == cat)).Any(category => category == null))
            {
                return this.BadRequest("Categories do not exist");
            }

            if (this.Data.AdTypes.All().FirstOrDefault(t => t.Id == model.TypeId) == null)
            {
                return this.BadRequest("Unknown or missing type of ad");
            }

            ICollection<Category> categories = new HashSet<Category>();

            foreach (var cat in model.Categories)
            {
                categories.Add(this.Data.Categories.Find(cat));
            }

            var ad = new Ad()
            {
                Name = model.Name,
                Description = model.Description,
                TypeId = model.TypeId,
                Price = model.Price,
                Categories = categories,
                OwnerId = userId.GetUserId(),
                PostedOn = DateTime.Now,
                Status = AdStatus.Open
            };

            this.Data.Ads.Add(ad);
            this.Data.SaveChanges();

            return this.Ok(ad.Id);
        }

        [HttpPut]
        [Route("api/ads/{id}/close")]
        public IHttpActionResult CloseAd(int id)
        {
            var userId = this.UserId;

            if (userId == null) return this.BadRequest("You need to be logged in");

            var ad = this.Data.Ads.All().FirstOrDefault(a => a.Id == id);

            if (ad == null)
            {
                return this.NotFound();
            }

            if (ad.OwnerId != userId.GetUserId())
            {
                return this.BadRequest();
            }

            ad.Status = AdStatus.Closed;
            ad.ClosedOn = DateTime.Now;
            this.Data.SaveChanges();

            var result = this.Data.Ads.All()
                .Where(a => a.Id == id)
                .Select(AdsViewModel.Create)
                .FirstOrDefault();

            return this.Ok(result);
        }
    }
}