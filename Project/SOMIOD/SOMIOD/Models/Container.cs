using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Web;

namespace SOMIOD.Models
{
    public class Container
    {
        private int id { get; set; }
        private string name { get; set; }
        private String content { get; set; }
        private DateTime creation_datetime { get; set; }
        private int parent {  get; set; }
    }
}