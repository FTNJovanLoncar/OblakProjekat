using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Microsoft.Azure;
using MovieData;
using System.Threading.Tasks;

namespace MovieService_WebRole1
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            InitBlobs();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            //SeedAuthorsAtStartup();
        }

        public void InitBlobs()
        {
            try
            {
                // read account configuration settings
                var storageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("DataConnectionString"));

                // create blob container for images
                CloudBlobClient blobStorage = storageAccount.CreateCloudBlobClient();
                CloudBlobContainer container = blobStorage.GetContainerReference("vezba");
                container.CreateIfNotExists();

                // configure container for public access
                var permissions = container.GetPermissions();
                permissions.PublicAccess = BlobContainerPublicAccessType.Container;
                container.SetPermissions(permissions);
            }
            catch (WebException)
            {

            }
        }

        private void SeedAuthorsAtStartup()
        {
            var repo = new UserDataRepository(); // same type as in your controller

            var authors = new List<User>
    {
        new User("author1@example.com")
        {
            Name = "Author One",
            Password = "password123",
            Email = "author1@example.com",
            Country = "USA",
            City = "New York",
            Address = "123 Main Street",
            Gender = "Male",
            ImageUrl = "https://example.com/images/author1.jpg",
            UserRole = UserRole.Author
        },
        new User("author2@example.com")
        {
            Name = "Author Two",
            Password = "password456",
            Email = "author2@example.com",
            Country = "UK",
            City = "London",
            Address = "456 Baker Street",
            Gender = "Female",
            ImageUrl = "https://example.com/images/author2.jpg",
            UserRole = UserRole.Author
        }
    };

            foreach (var author in authors)
            {
                var existing = Task.Run(() => repo.GetUserByEmailAsync(author.Email)).Result;
                if (existing == null)
                {
                    Task.Run(() => repo.AddStudent(author)).Wait();
                }
            }
        }


    }
}
