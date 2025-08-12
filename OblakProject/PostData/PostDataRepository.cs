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

            if (followEntity == null)
            {
                var newFollow = new FollowEntity(postId, userEmail);
                var insert = TableOperation.Insert(newFollow);
                await _followsTable.ExecuteAsync(insert);
            }
            else
            {
                var delete = TableOperation.Delete(followEntity);
                await _followsTable.ExecuteAsync(delete);
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
            }
        }
    }
}
