using Blog.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Blog.Areas.Admin.Models
{
    public class ClaimsViewModel
    {
        public Role role { set; get; }
        public IList<EditClaim> claims { get; set; }
        public EditClaim claim { get; set; }
    }
}
