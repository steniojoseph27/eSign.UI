using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace eSign.Models
{
    public partial class Recipient
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Email { get; set; }
        public string Signatories { get; set; }
        public DateTime creationDate { get; set; }
        public string Description { get; set; }
    }
}