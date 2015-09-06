using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using EntityFramework.Extensions;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Owin.Testing;
using OnlineShop.Data;
using OnlineShop.Models;
using OnlineShop.Services;
using OnlineShop.Services.Models;
using OnlineShop.Tests.DTOs;
using Owin;

namespace OnlineShop.Tests.IntegrationTests
{
    [TestClass]
    public class AdsIntegrationTests
    {
        private static TestServer httpTestServer;
        private static HttpClient httpClient;

        private const string username = "flux";
        private const string password = "12archLicher!@";

        private string accessToken;

        private string AccessToken
        {
            get
            {
                if (this.accessToken == null)
                {
                    var loginResponse = this.Login();
                    if (!loginResponse.IsSuccessStatusCode)
                    {
                        Assert.Fail("Cannot login : "+loginResponse.ReasonPhrase);
                    }
                    var loginData = loginResponse.Content.ReadAsAsync<UserDTO>().Result;

                    this.accessToken = loginData.Access_Token;
                }

                return this.accessToken;
            }
        }

        [AssemblyInitialize]
        public static void AssemblyInit(TestContext context)
        {
            httpTestServer = TestServer.Create(appBuilder =>
            {
                var config = new  HttpConfiguration();
                WebApiConfig.Register(config);
                var startUp = new Startup();

                startUp.Configuration(appBuilder);
                appBuilder.UseWebApi(config);
            });

            httpClient = httpTestServer.HttpClient;
            SeedDb();
        }

        [AssemblyCleanup]
        public static void AssemblyClean()
        {
            if (httpTestServer != null)
            {
                CleanDb();                
                httpTestServer.Dispose();
            }
        }

        [TestMethod]
        public void Login_ShouldBeSuccessful()
        {
            var loginResponse = this.Login();
            Assert.AreEqual(HttpStatusCode.OK, loginResponse.StatusCode);

            var loginData = loginResponse.Content.ReadAsAsync<UserDTO>().Result;
            Assert.AreNotEqual(null, loginData.Access_Token);
        }

        [TestMethod]
        public void CreateNewAd_InvalidType_ShouldReturnBadRequest()
        {
            var context = new OnlineShopContext();
            var category = context.Categories.First();

            var data = new FormUrlEncodedContent(new []
            {
                new KeyValuePair<string, string>("name","OpelAstra"),
                new KeyValuePair<string, string>("description","...."),
                new KeyValuePair<string, string>("price","1565"),
                new KeyValuePair<string, string>("typeId","-1"),
                new KeyValuePair<string, string>("categories[0]",category.Id.ToString()) 
            });

            var response = PostNewAd(data);
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [TestMethod]
        public void CreateNewAd_InvalidNumberOfCategories_ShouldReturnBadRequest()
        {
            var context = new OnlineShopContext();
            var category = context.Categories.First();

            var data = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("name","OpelAstra"),
                new KeyValuePair<string, string>("description","...."),
                new KeyValuePair<string, string>("price","1565"),
                new KeyValuePair<string, string>("typeId","-1"),
                new KeyValuePair<string, string>("categories[0]",category.Id.ToString()),
                new KeyValuePair<string, string>("categories[1]",category.Id.ToString()),
                new KeyValuePair<string, string>("categories[2]",category.Id.ToString()),
                new KeyValuePair<string, string>("categories[3]",category.Id.ToString()) 
            });

            var response = PostNewAd(data);
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [TestMethod]
        public void CreateNewAd_WithoutCategories_ShouldReturnBadRequest()
        {
            var context = new OnlineShopContext();

            var data = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("name","OpelAstra"),
                new KeyValuePair<string, string>("description","...."),
                new KeyValuePair<string, string>("price","1565"),
                new KeyValuePair<string, string>("typeId","-1")
            });

            var response = PostNewAd(data);
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [TestMethod]
        public void CreateNewAd_WithoutName_ShouldReturnBadRequest()
        {
            var context = new OnlineShopContext();
            var category = context.Categories.First();

            var data = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("description","...."),
                new KeyValuePair<string, string>("price","1565"),
                new KeyValuePair<string, string>("typeId","-1"),
                new KeyValuePair<string, string>("categories[0]",category.Id.ToString())
            });

            var response = PostNewAd(data);
            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [TestMethod]
        public void CreateNewAd_ValidAd_ShouldReturnBadRequest()
        {
            var context = new OnlineShopContext();
            var category = context.Categories.First();
            var type = context.AdTypes.First();
            var data = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("name","OpelAstra"),
                new KeyValuePair<string, string>("description","...."),
                new KeyValuePair<string, string>("price","1565"),
                new KeyValuePair<string, string>("typeId", type.Id.ToString()),
                new KeyValuePair<string, string>("categories[0]",category.Id.ToString())
            });

            var response = PostNewAd(data);
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

        private static void CleanDb()
        {
            var context = new OnlineShopContext();

            context.Ads.Delete();
            context.AdTypes.Delete();
            context.Categories.Delete();
            context.Users.Delete();
        }

        private static void SeedDb()
        {
            var context = new OnlineShopContext();
            var userStore = new UserStore<ApplicationUser>(context);
            var userManager = new ApplicationUserManager(userStore);

            var user = new ApplicationUser()
            {
                UserName = username,
                Email = "flux@yahoo.com"
            };

            var result = userManager.CreateAsync(user, password).Result;
            if (!result.Succeeded)
            {
                Assert.Fail(string.Join(Environment.NewLine, result.Errors));
            }

            SeedCategories(context);
            SeedAdTypes(context);
        }

        private static void SeedAdTypes(OnlineShopContext context)
        {
            context.AdTypes.Add(new AdType()
            {
                Name = "Premium",
                Index = 300,
                PricePerDay = 1234
            }); 
            context.AdTypes.Add(new AdType()
            {
                Name = "Diamond",
                Index = 400,
                PricePerDay = 2315
            });
            context.AdTypes.Add(new AdType()
            {
                Name = "Normal",
                Index = 100,
                PricePerDay = 4516
            });
        }

        private static void SeedCategories(OnlineShopContext context)
        {
            context.Categories.Add(new Category()
            {
                Name = "cat1"
            });
            context.Categories.Add(new Category()
            {
                Name = "cat2"
            });
            context.Categories.Add(new Category()
            {
                Name = "cat3"
            });
            context.Categories.Add(new Category()
            {
                Id = 4,
                Name = "cat4"
            });
        }

        private HttpResponseMessage Login()
        {
            var loginData = new FormUrlEncodedContent(new []
            {
                new KeyValuePair<string, string>("username", username),
                new KeyValuePair<string, string>("password", password),
                new KeyValuePair<string, string>("grant_type", "password") 
            });

            var response = httpClient.PostAsync("/Token", loginData).Result;

            return response;
        }

        private HttpResponseMessage PostNewAd(FormUrlEncodedContent data)
        {
            if (!httpClient.DefaultRequestHeaders.Contains("Authorization"))
            {
                httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + this.AccessToken);
            }

            return httpClient.PostAsync("api/ads", data).Result;
        }
    }
}
