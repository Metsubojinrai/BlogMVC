using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Blog.Areas.Admin.Models
{
    public class RoleViewModel
    {
        public string ID { set; get; }
        [BindProperty]
        public InputModel Input { get; set; }
        public class InputModel
        { 
            [Required(ErrorMessage = "Phải nhập tên role")]
            [Display(Name = "Tên Role")]
            [StringLength(100, ErrorMessage = "{0} dài {2} đến {1} ký tự.", MinimumLength = 3)]
            public string Name { set; get; }

            [Display(Name = "Mô tả")]
            [StringLength(100)]
            public string Description { set; get; }
        }
    }
}
