using Microsoft.Azure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PostData
{
    public class PostDataRepository
    {
        private readonly CloudTable _postTable;
        private readonly CloudTable _votesTable;
        private readonly CloudTable _followsTable;
        private readonly CloudTable _commentsTable;

        public PostDataRepository()
        {
            string connectionString = CloudConfigurationManager.GetSetting("DataConnectionString");
            if (string.IsNullOrEmpty(connectionString))
                throw new InvalidOperationException("Azure storage connection string is missing.");

            var storageAccount = CloudStorageAccount.Parse(connectionString);
            var tableClient = storageAccount.CreateCloudTableClient();

            _postTable = tableClient.GetTableReference("PostTable");
            _postTable.CreateIfNotExistsAsync().GetAwaiter().GetResult();

            _votesTable = tableClient.GetTableReference("VotesTable");
            _votesTable.CreateIfNotExistsAsync().GetAwaiter().GetResult();

            _followsTable = tableClient.GetTableReference("FollowsTable");
            _followsTable.CreateIfNotExistsAsync().GetAwaiter().GetResult();

            _commentsTable = tableClient.GetTableReference("CommentsTable");
            _commentsTable.CreateIfNotExistsAsync().GetAwaiter().GetResult();
        }

        // POSTOVE
        public async Task<IEnumerable<Post>> RetrieveAllPostsAsync()
        {
            var query = new TableQuery<Post>().Where(
                TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, "Post"));
            var posts = new List<Post>();
            TableContinuationToken token = null;

            do
            {
                var segment = await _postTable.ExecuteQuerySegmentedAsync(query, token);
                posts.AddRange(segment.Results);
                token = segment.ContinuationToken;
            } while (token != null);

            return posts;
        }

        public async Task AddPostAsync(Post newPost)
        {
            if (newPost == null) throw new ArgumentNullException(nameof(newPost));

            var insertOperation = TableOperation.Insert(newPost);
            var result = await _postTable.ExecuteAsync(insertOperation);

            if (result.HttpStatusCode < 200 || result.HttpStatusCode >= 300)
                throw new Exception($"Insert failed with status code: {result.HttpStatusCode}");
        }

        public async Task UpdatePostAsync(Post post)
        {
            var updateOperation = TableOperation.Replace(post);
            var result = await _postTable.ExecuteAsync(updateOperation);

            if (result.HttpStatusCode < 200 || result.HttpStatusCode >= 300)
                throw new Exception($"Update failed with status code: {result.HttpStatusCode}");
        }

        public async Task DeletePostAsync(string partitionKey, string rowKey)
        {
            var retrieveOperation = TableOperation.Retrieve<Post>(partitionKey, rowKey);
            var retrievedResult = await _postTable.ExecuteAsync(retrieveOperation);
            var deleteEntity = (Post)retrievedResult.Result;

            if (deleteEntity == null)
                throw new Exception("Entity not found");

            var deleteOperation = TableOperation.Delete(deleteEntity);
            var result = await _postTable.ExecuteAsync(deleteOperation);

            if (result.HttpStatusCode < 200 || result.HttpStatusCode >= 300)
                throw new Exception($"Delete failed with status code: {result.HttpStatusCode}");
        }

        // FOLLOW
        public async Task<bool> IsUserFollowingAsync(string postId, string userEmail)
        {
            var retrieveOperation = TableOperation.Retrieve<FollowEntity>(postId, userEmail.ToLowerInvariant());
            var result = await _followsTable.ExecuteAsync(retrieveOperation);
            return result.Result != null;
        }

        public async Task ToggleFollowAsync(string postId, string userEmail)
        {
            var retrieveOperation = TableOperation.Retrieve<FollowEntity>(postId, userEmail.ToLowerInvariant());
            var result = await _followsTable.ExecuteAsync(retrieveOperation);
            var followEntity = (FollowEntity)result.Result;

            // Retrieve the post
            var postRetrieve = TableOperation.Retrieve<Post>("Post", postId);
            var postResult = await _postTable.ExecuteAsync(postRetrieve);
            var postEntity = (Post)postResult.Result;

            if (followEntity == null)
            {
                // Add follow record
                var newFollow = new FollowEntity(postId, userEmail);
                var insert = TableOperation.Insert(newFollow);
                await _followsTable.ExecuteAsync(insert);

                // Add email to the post's Emails list
                if (postEntity != null)
                {
                    var emails = postEntity.Emails; // get current list
                    if (!emails.Contains(userEmail, StringComparer.OrdinalIgnoreCase))
                    {
                        emails.Add(userEmail);
                        postEntity.Emails = emails; // assign back to trigger setter -> EmailsSerialized updated
                        var replacePost = TableOperation.Replace(postEntity);
                        await _postTable.ExecuteAsync(replacePost);
                        Console.WriteLine(string.Join(", ", postEntity.Emails));
                    }
                }
            }
            else
            {
                // Remove follow record
                var delete = TableOperation.Delete(followEntity);
                await _followsTable.ExecuteAsync(delete);

                // Remove email from the post's Emails list
                if (postEntity != null)
                {
                    var emails = postEntity.Emails;
                    if (emails.RemoveAll(e => e.Equals(userEmail, StringComparison.OrdinalIgnoreCase)) > 0)
                    {
                        postEntity.Emails = emails; // assign back to trigger setter
                        var replacePost = TableOperation.Replace(postEntity);
                        await _postTable.ExecuteAsync(replacePost);
                    }
                }
            }
        }


        // VOTE
        public async Task AddVoteAsync(string postId, string userEmail, bool positive)
        {
            var voteRetrieve = TableOperation.Retrieve<VoteEntity>(postId, userEmail.ToLowerInvariant());
            var voteResult = await _votesTable.ExecuteAsync(voteRetrieve);
            var existingVote = (VoteEntity)voteResult.Result;

            int deltaPositive = 0;
            int deltaNegative = 0;

            if (existingVote == null)
            {
                var newVote = new VoteEntity(postId, userEmail)
                {
                    IsPositive = positive
                };
                var insertVote = TableOperation.Insert(newVote);
                await _votesTable.ExecuteAsync(insertVote);

                if (positive) deltaPositive = 1; else deltaNegative = 1;
            }
            else
            {
                if (existingVote.IsPositive != positive)
                {
                    existingVote.IsPositive = positive;
                    var replaceVote = TableOperation.Replace(existingVote);
                    await _votesTable.ExecuteAsync(replaceVote);

                    if (positive)
                    {
                        deltaPositive = 1;
                        deltaNegative = -1;
                    }
                    else
                    {
                        deltaPositive = -1;
                        deltaNegative = 1;
                    }
                }
                else
                {
                    // Glas je isti, nema promene
                    return;
                }
            }

            var postRetrieve = TableOperation.Retrieve<Post>("Post", postId);
            var postResult = await _postTable.ExecuteAsync(postRetrieve);
            var postEntity = (Post)postResult.Result;

            if (postEntity != null)
            {
                postEntity.PositiveVotes += deltaPositive;
                postEntity.NegativeVotes += deltaNegative;

                var replacePost = TableOperation.Replace(postEntity);
                await _postTable.ExecuteAsync(replacePost);
            }
        }

        // KOMENTARI
        public async Task<IEnumerable<CommentEntity>> GetCommentsForPostAsync(string postId)
        {
            var query = new TableQuery<CommentEntity>().Where(
                TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, postId));
            var comments = new List<CommentEntity>();
            TableContinuationToken token = null;

            do
            {
                var segment = await _commentsTable.ExecuteQuerySegmentedAsync(query, token);
                comments.AddRange(segment.Results);
                token = segment.ContinuationToken;
            } while (token != null);

            return comments.OrderBy(c => c.CreatedAt);
        }

        public async Task AddCommentAsync(CommentEntity comment)
        {
            var insertOperation = TableOperation.Insert(comment);
            var result = await _commentsTable.ExecuteAsync(insertOperation);

            if (result.HttpStatusCode < 200 || result.HttpStatusCode >= 300)
                throw new Exception($"Insert comment failed with status code: {result.HttpStatusCode}");

            // Povećaj broj komentara na postu
            var postRetrieve = TableOperation.Retrieve<Post>("Post", comment.PartitionKey);
            var postResult = await _postTable.ExecuteAsync(postRetrieve);
            var postEntity = (Post)postResult.Result;
            if (postEntity != null)
            {
                postEntity.CommentsCount++;
                var replacePost = TableOperation.Replace(postEntity);
                await _postTable.ExecuteAsync(replacePost);
                await SendCommentNotificationEmails(postEntity, comment);
            }
        }

        private async Task SendCommentNotificationEmails(Post post, CommentEntity comment)
        {
            if (post.Emails == null || post.Emails.Count == 0)
            {
                Console.WriteLine(post.Emails);
                return;
            }
            foreach (var email in post.Emails)
            {
                // Skip the author of the comment if you want
            //    if (email.Equals(comment.AuthorEmail, StringComparison.OrdinalIgnoreCase))
             //       continue;

               
                using (var client = new System.Net.Mail.SmtpClient("smtp.gmail.com", 587))
                {
                    client.Credentials = new System.Net.NetworkCredential("zerobraine.lastloncar@gmail.com", "lfijctaekntyxzux");
                    client.EnableSsl = true;

                    var mail = new System.Net.Mail.MailMessage();
                    mail.From = new System.Net.Mail.MailAddress("zerobraine.lastloncar@gmail.com", "Movie App");
                    mail.To.Add(email);
                    mail.Subject = $"New comment on post {post.Name}";
                    mail.Body = $"User {comment.AuthorEmail} just commented: \"{comment.Content}\"";
                    mail.IsBodyHtml = false;

                    await client.SendMailAsync(mail);
                }
            }
        }

    }
}
