using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SOMIOD.Models
{
    public class Notification
    {
        public int id {  get; set; }
        public string name {  get; set; }

        public DateTime creation_datetime { get; set; }

        public int parent {  get; set; }

        public int @event { get; set; }

        public string endpoint { get; set; }
        public bool enabled { get; set; }
    }
}