using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Blog.Models
{
    public class User : IdentityUser<Guid>
    {
        [Key]
        [Column("id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public override Guid Id { get; set; }
        public string FullName { set; get; }
        public string Address { set; get; }
        public DateTime? Birthday { set; get; }
        public string ProfilePicture { get; set; }
    }

    public class Role : IdentityRole<Guid>
    {
        public string Description { get; set; }
    }

    public class UserRole : IdentityUserRole<Guid>
    {

    }
}
