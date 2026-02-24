using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using CineClub.Data;
using CineClub.Models;
using Microsoft.AspNetCore.Http; // JSON dönütleri için gerekebilir

namespace CineClub.Controllers
{
    public class ReviewController : Controller
    {
        private readonly CineDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public ReviewController(CineDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }


        // 4. Create API endpoint.
        [HttpGet("/api/reviews/{movieId}")]
        public async Task<IActionResult> GetReviewsByMovieApi(int movieId)
        {
            // we access reviews related to movie that we get id
            var reviews = await _context.Reviews
                .Where(r => r.MovieId == movieId)
                .ToListAsync();

            var results = new List<object>();

            foreach (var r in reviews)
            {
                // we access UserName troughout UserId
                var user = await _userManager.FindByIdAsync(r.UserId ?? string.Empty);

                // JSON : review, review rate, review date, username
                results.Add(new
                {
                    review = r.Content,
                    reviewRate = r.Rating,
                    reviewDate = r.CreatedAtUtc,
                    username = user?.UserName ?? "Anonim"
                });
            }

            // JSON list
            return Json(results);
        }
      
        // GET: Review
        public async Task<IActionResult> Index()
        {
            var cineDbContext = _context.Reviews.Include(r => r.Movie);
            return View(await cineDbContext.ToListAsync());
        }

        // GET: Review/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var review = await _context.Reviews
                .Include(r => r.Movie)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (review == null)
            {
                return NotFound();
            }
            
            //to display user name 
            string? userName = null;
            if (!string.IsNullOrEmpty(review.UserId))
            {
                var user = await _userManager.FindByIdAsync(review.UserId);
                userName = user?.UserName;
            }

            ViewBag.UserName = userName;

            return View(review);
        }



        // GET: Review/Create
        public IActionResult Create()
        {
            ViewData["MovieId"] = new SelectList(_context.Movies, "Id", "Title");
            return View();
        }

        // POST: Review/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Content,Rating,CreatedAtUtc,MovieId")] Review review)
        {
            if (ModelState.IsValid)
            {
                // Get current user's (logged-in) Id
                var userId = _userManager.GetUserId(User);
                review.UserId = userId;
                review.CreatedAtUtc = DateTime.UtcNow;


                var user = await _userManager.GetUserAsync(User);
                var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");

                if (!isAdmin)
                {
                    // check normal user is have a review or not
                    var hasComment = _context.Reviews
                        .Any(r => r.MovieId == review.MovieId && r.UserId == userId);

                    if (hasComment)
                    {
                        // show message for error
                        ModelState.AddModelError("", "You can already add review.");
                        ViewData["MovieId"] = new SelectList(_context.Movies, "Id", "Title", review.MovieId);
                        return View(review);
                    }
                }



                _context.Add(review);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["MovieId"] = new SelectList(_context.Movies, "Id", "Title", review.MovieId);
            return View(review);
        }









        // GET: Review/Edit/5
        [Authorize]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var review = await _context.Reviews.FindAsync(id);
            if (review == null)
            {
                return NotFound();
            }
            
            var currentUserId = _userManager.GetUserId(User);
            if (review.UserId != currentUserId && !User.IsInRole("Admin"))
            {
                return Forbid(); 
            }

            ViewData["MovieId"] = new SelectList(_context.Movies, "Id", "Title", review.MovieId);
            return View(review);
        }

        // POST: Review/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        
        public async Task<IActionResult> Edit(int id, [Bind("Id,Content,Rating,CreatedAtUtc,MovieId")] Review review)
        {
            if (id != review.Id)
            {
                return NotFound();
            }

            var reviewFromDb = await _context.Reviews.FindAsync(id);
            if (reviewFromDb == null)
            {
                return NotFound();
            }

            var currentUserId = _userManager.GetUserId(User);
            if (reviewFromDb.UserId != currentUserId && !User.IsInRole("Admin"))
            {
                return Forbid();
            }

            if (ModelState.IsValid) // control the rating-content
            {
                try
                {
                    //we assign the value that get user to db entity
                    reviewFromDb.Content = review.Content;
                    reviewFromDb.Rating  = review.Rating;

                    reviewFromDb.UpdateTime = DateTime.Now;

                    //we update entity and save to ef core
                    _context.Update(reviewFromDb);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ReviewExists(review.Id))
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

            ViewData["MovieId"] = new SelectList(_context.Movies, "Id", "Title", review.MovieId);
            return View(review);
        }




        // GET: Review/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var review = await _context.Reviews
                .Include(r => r.Movie)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (review == null)
            {
                return NotFound();
            }

            var currentUserId = _userManager.GetUserId(User);
            if (review.UserId != currentUserId && !User.IsInRole("Admin"))
            {
                return Forbid();
            }

            return View(review);
        }

        // POST: Review/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var review = await _context.Reviews.FindAsync(id);
            if (review != null)
            {
                _context.Reviews.Remove(review);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ReviewExists(int id)
        {
            return _context.Reviews.Any(e => e.Id == id);
        }
    }
}
