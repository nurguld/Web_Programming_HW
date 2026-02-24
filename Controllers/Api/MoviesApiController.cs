using CineClub.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace CineClub.Controllers.Api;
    
[ApiController]
[Route("api/movies")] 
public class MoviesApiController : ControllerBase
{
    private readonly CineDbContext _context;

    public MoviesApiController(CineDbContext context)
    {
        _context = context;
    }

    // GET: /api/movies
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var movies = await _context.Movies
            .Include(m => m.Genre)
            .OrderBy(m => m.Title)
            .Select(m => new
            {
                id = m.Id,
                title = m.Title,
                releaseYear = m.ReleaseYear,
                genreName = m.Genre.Name
            })
            .ToListAsync();

        return Ok(movies); // JSON list
    }

    // GET: /api/movies/5
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var movie = await _context.Movies
            .Include(m => m.Genre)
            .Where(m => m.Id == id)
            .Select(m => new
            {
                id = m.Id,
                title = m.Title,
                description = m.Description,
                releaseYear = m.ReleaseYear,
                genreName = m.Genre.Name
            })
            .FirstOrDefaultAsync();

        if (movie == null)
        {
            return NotFound(); // 404
        }

        return Ok(movie); // JSON object
    }

    // GET: /api/movies/search?q=inception
    [HttpGet("search")]
    public async Task<IActionResult> Search(string? q)
    {
        if (string.IsNullOrWhiteSpace(q))
        {
            return Ok(Array.Empty<object>());
        }

        q = q.Trim();

        var movies = await _context.Movies
            .Include(m => m.Genre)
            //.Where(m => m.Title.Contains(q))
            .Where(m => EF.Functions.Like(m.Title, $"%{q}%"))
            .OrderBy(m => m.Title)
            .Take(10)
            .Select(m => new
            {
                id = m.Id,
                title = m.Title,
                releaseYear = m.ReleaseYear,
                genreName = m.Genre.Name
            })
            .ToListAsync();

        return Ok(movies); // JSON
    }



    // GET: /api/movies/top-rated
    [HttpGet("top-rated")]
    public async Task<IActionResult> GetTopRated()
    {
        var topRatedMovies = await _context.Movies
            .Include(m => m.Reviews) // include all reviews
            .Select(m => new
            {
                movieName = m.Title,
                // if we dont have rating we assign 0
                rate = m.Reviews.Any() ? m.Reviews.Average(r => r.Rating) : 0,
                releaseDate = m.ReleaseYear
            })
            .OrderByDescending(m => m.rate) // list according to calculating average rate
            .Take(10) // take top 10
            .ToListAsync();

        return Ok(topRatedMovies);
    }


}