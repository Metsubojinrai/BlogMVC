using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Blog
{
    [ViewComponent]
    public class RowTreeCategory : ViewComponent
    {
        public RowTreeCategory()
        {

        }

        public IViewComponentResult Invoke(dynamic data)
        {
            return View(data);
        }
    }
}
