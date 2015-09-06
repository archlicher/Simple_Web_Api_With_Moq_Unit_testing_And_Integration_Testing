using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using OnlineShop.Data.Repository;
using OnlineShop.Models;

namespace OnlineShop.Tests.UnitTests
{
    public class MockContainer
    {
        public Mock<IRepository<Ad>> AdRepositoryMock { get; set; }

        public Mock<IRepository<AdType>> AdTypeRepositoryMock { get; set; }
        
        public Mock<IRepository<Category>> CategoryRepositoryMock { get; set; }

        public Mock<IRepository<ApplicationUser>> UserRepositoryMock { get; set; }

        public void PrepapreMocks()
        {
            this.SetupFakeCategories();

            this.SetupFakeAds();

            this.SetupFakeAdTypes();

            this.SetupFakeUsers();
        }

        private void SetupFakeAds()
        {
            var adTypes = new List<AdType>()
            {
                new AdType() {Name = "Normal", Index = 100},
                new AdType() {Name = "Premium", Index = 200}
            };

            var fakeAds = new List<Ad>()
            {
                new Ad()
                {
                    Id = 5,
                    Name = "Audi A6",
                    Type = adTypes[0],
                    PostedOn = DateTime.Now.AddDays(-6),
                    Owner = new ApplicationUser() {UserName = "archlicher", Id = "123"},
                    Price = 432
                },
                new Ad()
                {
                    Id = 10,
                    Name = "Bmw 3",
                    Type = adTypes[1],
                    PostedOn = DateTime.Now.AddDays(-5),
                    Owner = new ApplicationUser() {UserName = "archlicher", Id = "123"},
                    Price = 4325
                }
            };

            this.AdRepositoryMock = new Mock<IRepository<Ad>>();
            this.AdRepositoryMock.Setup(r => r.All()).Returns(fakeAds.AsQueryable());
            this.AdRepositoryMock.Setup(r => r.Find(It.IsAny<int>()))
                .Returns((int id) =>
                {
                    var fakeAd = fakeAds.FirstOrDefault(a => a.Id == id);

                    return fakeAd;
                });
        }

        private void SetupFakeAdTypes()
        {
            var fakeAdTypes = new List<AdType>()
            {
                new AdType() {Id = 1, Name = "Normal", Index = 100},
                new AdType() {Id = 2, Name = "Premium", Index = 200}
            };

            this.AdTypeRepositoryMock = new Mock<IRepository<AdType>>();
            this.AdTypeRepositoryMock.Setup(r => r.All()).Returns(fakeAdTypes.AsQueryable());
            this.AdTypeRepositoryMock.Setup(r => r.Find(It.IsAny<int>()))
                .Returns((int id) => { return fakeAdTypes.FirstOrDefault(at => at.Id == id); });
        }

        private void SetupFakeCategories()
        {
            var fakeCategories = new List<Category>()
            {
                new Category() {Id = 1, Name = "Cat1"},
                new Category() {Id = 2, Name = "Cat2"},
                new Category() {Id = 3, Name = "Cat3"}
            };

            this.CategoryRepositoryMock = new Mock<IRepository<Category>>();
            this.CategoryRepositoryMock.Setup(r => r.All()).Returns(fakeCategories.AsQueryable());
            this.CategoryRepositoryMock.Setup(r => r.Find(It.IsAny<int>()))
                .Returns((int id) => { return fakeCategories.FirstOrDefault(at => at.Id == id); });
        }

        private void SetupFakeUsers()
        {
            var fakeUsers = new List<ApplicationUser>()
            {
                new ApplicationUser() {UserName = "archlicher", Id = "123"},
                new ApplicationUser() {UserName = "flux", Id = "124"}
            };

            this.UserRepositoryMock = new Mock<IRepository<ApplicationUser>>();
            this.UserRepositoryMock.Setup(r => r.All()).Returns(fakeUsers.AsQueryable());
            this.UserRepositoryMock.Setup(r => r.Find(It.IsAny<int>()))
                .Returns((int id) => { return fakeUsers.FirstOrDefault(at => at.Id.Equals(id.ToString())); });
        }
    }
}
