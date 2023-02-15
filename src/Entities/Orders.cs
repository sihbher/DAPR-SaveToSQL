using System;
using System.ComponentModel.DataAnnotations;

namespace DaprSaveToSql.Entities
{
    public class Orders
    {
        [Key]
        public int IDPlant { get; set; }
        [Key]
        public int IDOrder { get; set; }
        public double Amount { get; set; }
        public double Price { get; set; }
        public bool Dispatched { get; set; }
        public DateTime CreatedTimeStamp { get; set; }
        public DateTime LastUpdatedTimeStamp { get; set; }
    }
}