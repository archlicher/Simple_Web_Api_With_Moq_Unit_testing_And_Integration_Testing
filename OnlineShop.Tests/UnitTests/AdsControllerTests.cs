using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading;
using System.Web.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using OnlineShop.Data.UnitOfWork;
using OnlineShop.Models;
using OnlineShop.Services.Controllers;
using OnlineShop.Services.Infrastructure;
using OnlineShop.Services.Models;

namespace OnlineShop.Tests.UnitTests
{
    [TestClass]
    public class AdsControllerTests
    {
        private MockContainer mockContainer;

        [TestInitialize]
        public void TestInitialize()
        {
            this.mockContainer = new MockContainer();
            this.mockContainer.PrepapreMocks();
        }

        [TestMethod]
        public void TestGetAllAds_ShouldReturnAllAdsSortedByIndex()
        {
            var fakeAds = mockContainer.AdRepositoryMock.Object.All();

            var mockContext = new Mock<IOnlineShopData>();
            mockContext.Setup(r => r.Ads.All()).Returns(fakeAds.AsQueryable());

            var adsController = new AdsController(mockContext.Object);
            this.SetupController(adsController);

            var result = adsController.GetAds().ExecuteAsync(CancellationToken.None).Result;
            Assert.AreEqual(HttpStatusCode.OK, result.StatusCode);
            var adResponse = result.Content.ReadAsAsync<IEnumerable<AdsViewModel>>()
                .Result
                .OrderByDescending(a => a.Type)
                .ThenByDescending(a => a.PostedOn)
                .Select(a => new {a.Id})
                .ToList();
            var fa = fakeAds
                .OrderByDescending(a => a.Type.Index)
                .ThenByDescending(a => a.PostedOn)
                .Select(a => new { a.Id })
                .ToList();
            Assert.AreEqual(2, adResponse.Count);
            Assert.AreEqual(2, fa.Count);
            CollectionAssert.AreEqual(fa, adResponse);
        }

        [TestMethod]
        public void CreateAd_ShouldAddNewlyCreatedAd()
        {
            var ads = new List<Ad>();

            var fakeUser = this.mockContainer.UserRepositoryMock.Object.All().FirstOrDefault();
            if (fakeUser == null)
            {
                Assert.Fail("Cannot perform test - no users available.");
            }

            this.mockContainer.AdRepositoryMock
                .Setup(r => r.Add(It.IsAny<Ad>()))
                .Callback((Ad ad) =>
                {
                    ad.Owner = fakeUser;
                    ads.Add(ad);
                });

            var fakeCat = mockContainer.CategoryRepositoryMock.Object.All();
            var fakeAdTypes = mockContainer.AdTypeRepositoryMock.Object.All();
            var fakeUsers = mockContainer.UserRepositoryMock.Object.All();

            var mockContext = new Mock<IOnlineShopData>();
            mockContext.Setup(r => r.Ads.All()).Returns(ads.AsQueryable());
            mockContext.Setup(r => r.Ads.Add(It.IsAny<Ad>())).Callback((Ad ad) => ads.Add(ad));

            mockContext.Setup(r => r.Categories.All()).Returns(fakeCat.AsQueryable());
            mockContext.Setup(r => r.AdTypes.All()).Returns(fakeAdTypes.AsQueryable());
            mockContext.Setup(r => r.Users.All()).Returns(fakeUsers.AsQueryable());

            var mockIdProvider = new Mock<IUserIdProvider>();
            mockIdProvider.Setup(r => r.GetUserId()).Returns(fakeUser.Id);

            var adsController = new AdsController(mockContext.Object, mockIdProvider.Object);
            this.SetupController(adsController);

            var randomName = Guid.NewGuid().ToString();
            var newAd = new CreateAdBindingModel()
            {
                Name = randomName,
                Price = 555,
                TypeId = 1,
                Description = "Testing testing",
                Categories = new[] { 1, 2, 3 }
            };

            var response = adsController.CreateAd(newAd).ExecuteAsync(CancellationToken.None).Result;
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            mockContext.Verify(c => c.SaveChanges(), Times.Once);

            Assert.AreEqual(1, ads.Count);
            Assert.AreEqual(randomName, ads.First().Name);

        }

        [TestMethod]
        public void TestCloseAds_ShouldCloseAnAd()
        {
            var fakeAds = mockContainer.AdRepositoryMock.Object.All();
            var openAd = fakeAds.FirstOrDefault(ad => ad.Status == AdStatus.Open);

            if (openAd == null)
            {
                Assert.Fail("No open ads - cannot perform test");
            }

            var adId = openAd.Id;
            var mockContext = new Mock<IOnlineShopData>();
            mockContext.Setup(r => r.Ads).Returns(this.mockContainer.AdRepositoryMock.Object);

            var mockIdProvider = new Mock<IUserIdProvider>();
            mockIdProvider.Setup(i => i.GetUserId()).Returns(openAd.OwnerId);
            
            var adsController = new AdsController(mockContext.Object, mockIdProvider.Object);
            SetupController(adsController);

            var response = adsController.CloseAd(adId).ExecuteAsync(CancellationToken.None).Result;

            Assert.AreEqual(HttpStatusCode.OK,response.StatusCode);
            mockContext.Verify(c => c.SaveChanges(), Times.Once);
            var closedAd = mockContext.Object.Ads.Find(adId);
            Assert.AreNotEqual(null, closedAd.ClosedOn);
            Assert.AreEqual(AdStatus.Closed, closedAd.Status);
        }

        [TestMethod]
        public void CloseAd_FromNonOwner()
        {
            var openAd = mockContainer.AdRepositoryMock.Object.All().FirstOrDefault(ad => ad.Status == AdStatus.Open);

            if (openAd == null)
            {
                Assert.Fail("No ads with open status available");
            }

            var adId = openAd.Id;
            var mockContext = new Mock<IOnlineShopData>();
            mockContext.Setup(r => r.Ads).Returns(this.mockContainer.AdRepositoryMock.Object);

            var mockIdProvider = new Mock<IUserIdProvider>();
            mockIdProvider.Setup(r => r.GetUserId())
                .Returns(this.mockContainer.UserRepositoryMock.Object.All().FirstOrDefault(u => u.Id != openAd.OwnerId).Id);

            var adsController = new AdsController(mockContext.Object, mockIdProvider.Object);
            SetupController(adsController);

            var resposnse = adsController.CloseAd(adId).ExecuteAsync(CancellationToken.None).Result;
            Assert.AreEqual(HttpStatusCode.BadRequest, resposnse.StatusCode);
            mockContext.Verify(c => c.SaveChanges(), Times.Never);
            var stillOpenAd = mockContext.Object.Ads.Find(adId);
            Assert.AreEqual(AdStatus.Open, stillOpenAd.Status);

        }

        private void SetupController(BaseController controller)
        {
            controller.Request = new HttpRequestMessage();
            controller.Configuration = new HttpConfiguration();
        }
    }
}
