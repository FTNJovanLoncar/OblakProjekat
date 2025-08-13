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

        public async Task<ActionResult> Index()
        {
            var posts = await _postRepo.RetrieveAllPostsAsync();
            var user = await _userRepo.GetUserByEmailAsync(User.Identity.Name);

            ViewBag.IsAuthor = (user != null && user.UserRole == UserRole.Author);

            if (user != null)
            {
                foreach (var post in posts)
                {
                    post.IsFollowedByCurrentUser = await _postRepo.IsUserFollowingAsync(post.RowKey, user.Email);
                }
            }

            return View(posts);
        }

        public async Task<ActionResult> Create()
        {
            var user = await _userRepo.GetUserByEmailAsync(User.Identity.Name);
            if (user == null || user.UserRole != UserRole.Author)
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden, "Samo autori mogu kreirati diskusije.");

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(Post post)
        {
            var user = await _userRepo.GetUserByEmailAsync(User.Identity.Name);
            if (user == null || user.UserRole != UserRole.Author)
                return new HttpStatusCodeResult(HttpStatusCode.Forbidden, "Samo autori mogu kreirati diskusije.");

            if (!ModelState.IsValid)
                return View(post);

            post.RowKey = Guid.NewGuid().ToString();
            post.PartitionKey = "Post";
            post.AuthorEmail = user.Email;

            await _postRepo.AddPostAsync(post);

            return RedirectToAction("Index");
        }

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

            var posts = await _postRepo.RetrieveAllPostsAsync();
            var originalPost = posts.FirstOrDefault(p => p.RowKey == updatedPost.RowKey);

            if (originalPost == null)
                return HttpNotFound();

            originalPost.Name = updatedPost.Name;
            originalPost.Genre = updatedPost.Genre;
            originalPost.ReleaseDate = updatedPost.ReleaseDate;
            originalPost.IMDBRating = updatedPost.IMDBRating;
            originalPost.Synopsis = updatedPost.Synopsis;

            await _postRepo.UpdatePostAsync(originalPost);

            return RedirectToAction("Index");
        }

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

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(string id)
        {
            var posts = await _postRepo.RetrieveAllPostsAsync();
            var post = posts.FirstOrDefault(p => p.RowKey == id);

            if (post != null)
                await _postRepo.DeletePostAsync(post.PartitionKey, post.RowKey);

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Upvote(string postId)
        {
            var user = await _userRepo.GetUserByEmailAsync(User.Identity.Name);
            if (user == null)
                return new HttpStatusCodeResult(401);

            await _postRepo.AddVoteAsync(postId, user.Email, true);
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Downvote(string postId)
        {
            var user = await _userRepo.GetUserByEmailAsync(User.Identity.Name);
            if (user == null)
                return new HttpStatusCodeResult(401);

            await _postRepo.AddVoteAsync(postId, user.Email, false);
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ToggleFollow(string postId)
        {
            var user = await _userRepo.GetUserByEmailAsync(User.Identity.Name);
            if (user == null)
                return new HttpStatusCodeResult(401);

            await _postRepo.ToggleFollowAsync(postId, user.Email);
            
            return RedirectToAction("Index");
        }

        // KOMENTARI - prikaz svih komentara i polje za novi komentar
        public async Task<ActionResult> Comments(string id)
        {
            var user = await _userRepo.GetUserByEmailAsync(User.Identity.Name);
            if (user == null)
                return new HttpStatusCodeResult(401);

            var comments = await _postRepo.GetCommentsForPostAsync(id);

            ViewBag.PostId = id;
            ViewBag.UserEmail = user.Email;

            return View(comments);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AddComment(string postId, string content)
        {
            var user = await _userRepo.GetUserByEmailAsync(User.Identity.Name);
            if (user == null)
                return new HttpStatusCodeResult(401);

            if (string.IsNullOrWhiteSpace(content))
            {
                TempData["Error"] = "Komentar ne može biti prazan.";
                return RedirectToAction("Comments", new { id = postId });
            }

            var comment = new CommentEntity(postId, Guid.NewGuid().ToString())
            {
                AuthorEmail = user.Email,
                Content = content,
                CreatedAt = DateTime.UtcNow
            };

            await _postRepo.AddCommentAsync(comment);
         
            return RedirectToAction("Comments", new { id = postId });
        }
    }
}
