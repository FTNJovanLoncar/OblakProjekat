using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PostData
{
    public class Post:TableEntity
    {

        public string Name { get; set; }
        public string ReleaseDate { get; set; }
        public string Genre { get; set; }
        public string IMDBRating { get; set; }
        public string Synopsis { get; set; }
        public string LengthTime { get; set; }
        public string ImageUrl { get; set; }


        public Post(string IndexNo)
        {
            PartitionKey = "Post";
            RowKey = IndexNo;
        }

        public Post() { }

    }
}
