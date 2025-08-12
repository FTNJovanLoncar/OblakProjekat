using Microsoft.WindowsAzure.Storage.Table;
using System;

namespace PostData
{
    public class Post : TableEntity
    {
        public Post() { }

        public Post(string indexNo)
        {
            PartitionKey = "Post";
            RowKey = indexNo;
        }

        public string Name { get; set; }
        public string Genre { get; set; }
        public DateTime ReleaseDate { get; set; }  // changed to DateTime (from DateTimeOffset)
        public double IMDBRating { get; set; }
        public string Synopsis { get; set; }
        public string AuthorEmail { get; set; }
        public int PositiveVotes { get; set; }
        public int NegativeVotes { get; set; }
        public int CommentsCount { get; set; }

        [IgnoreProperty]
        public bool IsFollowedByCurrentUser { get; set; }
    }

    public class VoteEntity : TableEntity
    {
        public VoteEntity() { }

        public VoteEntity(string postId, string userEmail)
        {
            PartitionKey = postId;
            RowKey = userEmail.ToLowerInvariant();
        }

        public bool IsPositive { get; set; }
    }

    public class FollowEntity : TableEntity
    {
        public FollowEntity() { }

        public FollowEntity(string postId, string userEmail)
        {
            PartitionKey = postId;
            RowKey = userEmail.ToLowerInvariant();
        }
    }

    public class CommentEntity : TableEntity
    {
        public CommentEntity() { }

        public CommentEntity(string postId, string commentId)
        {
            PartitionKey = postId;      // postId kao PartitionKey
            RowKey = commentId;         // jedinstveni komentar ID (Guid)
        }

        public string AuthorEmail { get; set; }
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
