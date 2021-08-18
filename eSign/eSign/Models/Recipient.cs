namespace eSign.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class Recipient
    {
        public long Id { get; set; }
        public string Title { get; set; }
        public string Email { get; set; }
        public string EnvelopeID { get; set; }
        public string Signatories { get; set; }
        public string Description { get; set; }
        public byte[] Documents { get; set; }
        public string Status { get; set; }
        public Nullable<System.DateTime> CreationDate { get; set; }
        public Nullable<System.DateTime> UpdateOn { get; set; }

        public string documentURL { get; set; }
    }
}
