using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Blog.Data;
using Blog.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Blog.Core;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Blog.CustomAuthorization;

namespace Blog.Areas_Admin_Controllers
{
    [Authorize(Policy ="CanViewPost")]
    public class PostController : Controller
    {
        private readonly BlogDbContext _context;
        private readonly UserManager<User> _usermanager;
        private readonly ILogger<PostController> _logger;
        private readonly IAuthorizationService _authorizationService;
        public PostController(BlogDbContext context, UserManager<User> usermanager,
            ILogger<PostController> logger, IAuthorizationService authorizationService)
        {
            _context = context;
            _usermanager = usermanager;
            _logger = logger;
            _authorizationService = authorizationService;
        }

        //mảng chứa các CategoryID của bài Post
        [BindProperty]
        public int[] SelectedCategories { set; get; }

        public const int ITEMS_PER_PAGE = 4;
        // GET: Post
   
        public async Task<IActionResult> Index([FromQuery] int page)
        {

            if (page == 0)
                page = 1;

            var listPosts = _context.Posts
                .Include(p => p.Author)                // Tải Author
                .Include(p => p.PostCategories)        // Tải các PostCategory
                .ThenInclude(c => c.Category)          // Mỗi PostCateogry tải luôn Categtory
                .OrderByDescending(p => p.DateCreated);

            _logger.LogInformation(page.ToString());

            // Lấy tổng số dòng dữ liệu
            var totalItems = listPosts.Count();
            // Tính số trang hiện thị (mỗi trang hiện thị ITEMS_PER_PAGE mục)
            int totalPages = (int)Math.Ceiling((double)totalItems / ITEMS_PER_PAGE);

            if (page > totalPages)
                return RedirectToAction(nameof(PostController.Index), new { page = totalPages });


            var posts = await listPosts
                            .Skip(ITEMS_PER_PAGE * (page - 1))       // Bỏ qua các trang trước
                            .Take(ITEMS_PER_PAGE)                          // Lấy số phần tử của trang hiện tại
                            .ToListAsync();

            // return View (await listPosts.ToListAsync());
            ViewData["pageNumber"] = page;
            ViewData["totalPages"] = totalPages;

            return View(posts.AsEnumerable());
        }

        // GET: Post/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var post = await _context.Posts
                .Include(p => p.Author)
                .FirstOrDefaultAsync(m => m.PostId == id);
            if (post == null)
            {
                return NotFound();
            }

            return View(post);
        }

        // GET: Post/Create
        [Authorize(Policy = "CanCreatePost")]
        public async Task<IActionResult> CreateAsync()
        {
            // Thông tin về User tạo Post
            var user = await _usermanager.GetUserAsync(User);
            ViewData["userpost"] = $"{user.UserName} {user.FullName}";

            // Phần tử HTML chọn các Category sẽ sử dụng MultiSelectList và gửi nó đến View
            var categories = await _context.Categories.ToListAsync();
            ViewData["categories"] = new MultiSelectList(categories, "Id", "Title");
            return View();
        }

        // POST: Post/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("PostId,Title,Description,Slug,Content,Published")] Post post)
        {
            var user = await _usermanager.GetUserAsync(User);
            ViewData["userpost"] = $"{user.UserName} {user.FullName}";

            if (SelectedCategories.Length == 0)
            {
                ModelState.AddModelError(String.Empty, "Phải ít nhất một chuyên mục");
            }

            if (ModelState["Slug"].ValidationState == ModelValidationState.Invalid)
            {
                post.Slug = Utils.GenerateSlug(post.Title); //phương thức tĩnh, chuyển đổi Title thành Url
                ModelState.SetModelValue("Slug", new ValueProviderResult(post.Slug));
                // Thiết lập và kiểm tra lại Model
                ModelState.Clear();
                TryValidateModel(post);
            }

            bool SlugExisted = await _context.Posts.Where(p => p.Slug == post.Slug).AnyAsync();
            if (SlugExisted)
            {
                ModelState.AddModelError(nameof(post.Slug), "Slug đã có trong Database");
            }

            if (ModelState.IsValid)
            {
                //Tạo Post
                var newpost = new Post()
                {
                    AuthorId = user.Id,
                    Title = post.Title,
                    Slug = post.Slug,
                    Content = post.Content,
                    Description = post.Description,
                    Published = post.Published,
                    DateCreated = DateTime.Now,
                    DateUpdated = DateTime.Now
                };
                _context.Add(newpost);
                await _context.SaveChangesAsync();

                // Chèn thông tin về PostCategory của bài Post
                foreach (var selectedCategory in SelectedCategories)
                {
                    _context.Add(new PostCategory() { PostID = newpost.PostId, CategoryID = selectedCategory });
                }
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            //phần tử HTML chọn các Category sẽ sử dụng MultiSelectList và gửi nó đến View
            var categories = await _context.Categories.ToListAsync();
            ViewData["categories"] = new MultiSelectList(categories, "Id", "Title", SelectedCategories);
            return View(post);
        }

        // GET: Post/Edit/5
        [Authorize(Policy = "CanUpdatePost")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // var post = await _context.Posts.FindAsync (id);
            var post = await _context.Posts.Where(p => p.PostId == id)
                .Include(p => p.Author)
                .Include(p => p.PostCategories)
                .ThenInclude(c => c.Category).FirstOrDefaultAsync();
            if (post == null)
            {
                return NotFound();
            }

            ViewData["userpost"] = $"{post.Author.UserName} {post.Author.FullName}";
            ViewData["datecreate"] = post.DateCreated.ToShortDateString();

            // Danh mục chọn
            var selectedCates = post.PostCategories.Select(c => c.CategoryID).ToArray();
            var categories = await _context.Categories.ToListAsync();
            ViewData["categories"] = new MultiSelectList(categories, "Id", "Title", selectedCates);
            return View(post);
        }

        // POST: Post/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("PostId,Title,Description,Slug,Content,Published")] Post post)
        {
            if (id != post.PostId)
            {
                return NotFound();
            }

            // Kiểm tra nhóm Admin hoặc chủ sở hữu Post thì có quyênn
            var rs = await _authorizationService.AuthorizeAsync(User, post,
                                                                new CanUpdatePostRequirement(true, true));
            if (!rs.Succeeded)
            {
                return Forbid();
            }
            // Có quyền

            // Phát sinh Slug theo Title
            if (ModelState["Slug"].ValidationState == ModelValidationState.Invalid)
            {
                post.Slug = Utils.GenerateSlug(post.Title);
                ModelState.SetModelValue("Slug", new ValueProviderResult(post.Slug));
                // Thiết lập và kiểm tra lại Model
                ModelState.Clear();
                TryValidateModel(post);
            }

            if (SelectedCategories.Length == 0)
            {
                ModelState.AddModelError(String.Empty, "Phải ít nhất một chuyên mục");
            }

            bool SlugExisted = await _context.Posts.Where(p => p.Slug == post.Slug && p.PostId != post.PostId).AnyAsync();
            if (SlugExisted)
            {
                ModelState.AddModelError(nameof(post.Slug), "Slug đã có trong Database");
            }

            if (ModelState.IsValid)
            {

                // Lấy nội dung từ DB
                var postUpdate = await _context.Posts.Where(p => p.PostId == id)
                    .Include(p => p.PostCategories)
                    .ThenInclude(c => c.Category).FirstOrDefaultAsync();
                if (postUpdate == null)
                {
                    return NotFound();
                }

                // Cập nhật nội dung mới
                postUpdate.Title = post.Title;
                postUpdate.Description = post.Description;
                postUpdate.Content = post.Content;
                postUpdate.Slug = post.Slug;
                postUpdate.DateUpdated = DateTime.Now;

                // Các danh mục không có trong selectedCategories
                var listcateremove = postUpdate.PostCategories
                                               .Where(p => !SelectedCategories.Contains(p.CategoryID))
                                               .ToList();
                listcateremove.ForEach(c => postUpdate.PostCategories.Remove(c));

                // Các ID category chưa có trong postUpdate.PostCategories
                var listCateAdd = SelectedCategories
                                    .Where(
                                        id => !postUpdate.PostCategories.Where(c => c.CategoryID == id).Any()
                                    ).ToList();

                listCateAdd.ForEach(id => {
                    postUpdate.PostCategories.Add(new PostCategory()
                    {
                        PostID = postUpdate.PostId,
                        CategoryID = id
                    });
                });

                try
                {

                    _context.Update(postUpdate);

                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PostExists(post.PostId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }

            var categories = await _context.Categories.ToListAsync();
            ViewData["categories"] = new MultiSelectList(categories, "Id", "Title", SelectedCategories);
            return View(post);
        }

        // GET: Post/Delete/5
        [Authorize(Policy = "CanDeletePost")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var post = await _context.Posts
                .Include(p => p.Author)
                .FirstOrDefaultAsync(m => m.PostId == id);
            if (post == null)
            {
                return NotFound();
            }
            return View(post);
        }

        // POST: Post/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var post = await _context.Posts.FindAsync(id);
            _context.Posts.Remove(post);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PostExists(int id)
        {
            return _context.Posts.Any(e => e.PostId == id);
        }
    }
}
