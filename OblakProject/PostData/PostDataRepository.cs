using Microsoft.Azure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PostData
{
    public class PostDataRepository
    {
        private readonly CloudTable _table;

        public PostDataRepository()
        {
            string connectionString = CloudConfigurationManager.GetSetting("DataConnectionString");
            if (string.IsNullOrEmpty(connectionString))
                throw new InvalidOperationException("Azure storage connection string is missing.");

            var storageAccount = CloudStorageAccount.Parse(connectionString);
            var tableClient = storageAccount.CreateCloudTableClient();
            _table = tableClient.GetTableReference("PostTable");
            _table.CreateIfNotExistsAsync().GetAwaiter().GetResult(); // initialize table
        }

        public async Task<IEnumerable<Post>> RetrieveAllPostsAsync()
        {
            var query = new TableQuery<Post>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, "Post"));
            var posts = new List<Post>();
            TableContinuationToken token = null;

            do
            {
                var queryResult = await _table.ExecuteQuerySegmentedAsync(query, token);
                posts.AddRange(queryResult.Results);
                token = queryResult.ContinuationToken;
            } while (token != null);

            return posts;
        }

        public async Task AddPostAsync(Post newPost)
        {
            if (newPost == null) throw new ArgumentNullException(nameof(newPost));

            var insertOperation = TableOperation.Insert(newPost);
            var result = await _table.ExecuteAsync(insertOperation);

            if (result.HttpStatusCode < 200 || result.HttpStatusCode >= 300)
                throw new Exception($"Insert failed with status code: {result.HttpStatusCode}");
        }

        public async Task UpdatePostAsync(Post post)
        {
            var updateOperation = TableOperation.Replace(post);
            var result = await _table.ExecuteAsync(updateOperation);

            if (result.HttpStatusCode < 200 || result.HttpStatusCode >= 300)
                throw new Exception($"Update failed with status code: {result.HttpStatusCode}");
        }


        public async Task DeletePostAsync(string partitionKey, string rowKey)
        {
            var retrieveOperation = TableOperation.Retrieve<Post>(partitionKey, rowKey);
            var retrievedResult = await _table.ExecuteAsync(retrieveOperation);
            var deleteEntity = (Post)retrievedResult.Result;

            if (deleteEntity == null)
                throw new Exception("Entity not found");

            var deleteOperation = TableOperation.Delete(deleteEntity);
            var result = await _table.ExecuteAsync(deleteOperation);

            if (result.HttpStatusCode < 200 || result.HttpStatusCode >= 300)
                throw new Exception($"Delete failed with status code: {result.HttpStatusCode}");
        }



    }
}
