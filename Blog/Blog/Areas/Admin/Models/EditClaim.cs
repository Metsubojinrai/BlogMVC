using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Blog.Areas.Admin.Models
{
    public class EditClaim
    {
        public long Id { set; get; }
        public string ClaimType { set; get; }
        public string ClaimValue { set; get; }
    }
}
