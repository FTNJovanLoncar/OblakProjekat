using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MovieData;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Table;
using Microsoft.WindowsAzure;
using System.Net;

namespace MovieService_WebRole1.Controllers
{
    public class UserController : Controller
    {
        UserDataRepository repo = new UserDataRepository();
        public ActionResult Index()
        {
            return View(repo.RetrieveAllUsers());
        }
    }
}