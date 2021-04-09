using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Blog.CustomAuthorization
{
    public class CanUpdatePostRequirement : IAuthorizationRequirement
    {
        public bool AdminCanUpdate { get; set; }
        public bool OwnerCanUpdate { set; get; }
        public CanUpdatePostRequirement(bool _adminCanUpdate = true, bool _ownerCanupdate = true)
        {
            AdminCanUpdate = _adminCanUpdate;
            OwnerCanUpdate = _ownerCanupdate;
        }
    }
}
