using Microsoft.WindowsAzure.Storage.Table;
using System;

namespace PostData
{
    public class Post : TableEntity
    {
        public string Name { get; set; }
        public DateTimeOffset ReleaseDate { get; set; }  // better than string
        public string Genre { get; set; }
        public double IMDBRating { get; set; }            // numeric rating
        public string Synopsis { get; set; }
        public int LengthMinutes { get; set; }            // store length in minutes
        public string ImageUrl { get; set; }
        public string AuthorEmail { get; set; }
        public int Likes { get; set; }
        public int Dislikes { get; set; }

        public string OwnerEmail { get; set; }

        // For comments, consider using JSON string or a separate Comments table
        public string CommentsJson { get; set; }

        public Post(string indexNo)
        {
            PartitionKey = "Post";
            RowKey = indexNo;
        }

        public Post() { }
    }
}
