using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;
using MovieData;
using PostData;

namespace MovieService_WebRole1.Controllers
{
    public class PostController : Controller
    {
        private readonly PostDataRepository _postRepo = new PostDataRepository();
        private readonly UserDataRepository _userRepo = new UserDataRepository();

        // GET: Post/Index
        public async Task<ActionResult> Index()
        {
            var posts = await _postRepo.RetrieveAllPostsAsync();
            var user = await _userRepo.GetUserByEmailAsync(User.Identity.Name);

            ViewBag.IsAuthor = (user != null && user.UserRole == UserRole.Author);

            return View(posts);
        }


        // GET: Post/Create
        public async Task<ActionResult> Create()
        {
            var user = await _userRepo.GetUserByEmailAsync(User.Identity.Name);
            if (user == null || user.UserRole != UserRole.Author)
            {
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden, "Samo autori mogu kreirati diskusije.");
            }
            return View();
        }

        // POST: Post/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(Post post)
        {
            var user = await _userRepo.GetUserByEmailAsync(User.Identity.Name);
            if (user == null || user.UserRole != UserRole.Author)
            {
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden, "Samo autori mogu kreirati diskusije.");
            }

            if (!ModelState.IsValid)
            {
                return View(post);
            }

            post.RowKey = Guid.NewGuid().ToString();
            post.PartitionKey = "Post";

            // dodaj email autora u post
            post.AuthorEmail = user.Email;

            await _postRepo.AddPostAsync(post);

            return RedirectToAction("Index");
        }

        // GET: Post/Edit/{id}
        public async Task<ActionResult> Edit(string id)
        {
            var user = await _userRepo.GetUserByEmailAsync(User.Identity.Name);
            var posts = await _postRepo.RetrieveAllPostsAsync();
            var post = posts.FirstOrDefault(p => p.RowKey == id);

            if (post == null)
                return HttpNotFound();

            if (post.AuthorEmail != user.Email)
                return new HttpStatusCodeResult(401);

            return View(post);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(Post updatedPost)
        {
            var user = await _userRepo.GetUserByEmailAsync(User.Identity.Name);
            if (user == null || updatedPost.AuthorEmail != user.Email)
                return new HttpStatusCodeResult(401);

            // Prvo dohvatimo originalni post iz baze
            var posts = await _postRepo.RetrieveAllPostsAsync();
            var originalPost = posts.FirstOrDefault(p => p.RowKey == updatedPost.RowKey);

            if (originalPost == null)
                return HttpNotFound();

            // Ažuriramo samo polja koja korisnik može menjati
            originalPost.Name = updatedPost.Name;
            originalPost.Genre = updatedPost.Genre;
            originalPost.ReleaseDate = updatedPost.ReleaseDate;
            originalPost.IMDBRating = updatedPost.IMDBRating;
            originalPost.Synopsis = updatedPost.Synopsis;

            // Sada koristimo originalPost koji ima ETag da izvršimo update
            await _postRepo.UpdatePostAsync(originalPost);

            return RedirectToAction("Index");
        }


        // GET: Post/Delete/{id}
        public async Task<ActionResult> Delete(string id)
        {
            var user = await _userRepo.GetUserByEmailAsync(User.Identity.Name);
            var posts = await _postRepo.RetrieveAllPostsAsync();
            var post = posts.FirstOrDefault(p => p.RowKey == id);

            if (post == null)
                return HttpNotFound();

            if (post.AuthorEmail != user.Email)
                return new HttpStatusCodeResult(401);

            return View(post);
        }

        // POST: Post/Delete/{id}
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(string id)
        {
            var posts = await _postRepo.RetrieveAllPostsAsync();
            var post = posts.FirstOrDefault(p => p.RowKey == id);

            if (post != null)
            {
                await _postRepo.DeletePostAsync(post.PartitionKey, post.RowKey);
            }

            return RedirectToAction("Index");
        }
    }
}
