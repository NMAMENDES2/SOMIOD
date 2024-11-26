using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SOMIOD.Models
{
    public class Record
    {
        private int id { get; set; }
        private string name { get; set; }
        private string content { get; set; }

        private DateTime creation_datetime { get; set; }
        private int parent {  get; set; }
        
    }
}