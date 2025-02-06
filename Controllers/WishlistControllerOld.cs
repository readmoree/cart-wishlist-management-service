using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using WishlistService.Service;
using System.Collections.Generic;

namespace WishlistService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WishlistController : ControllerBase
    {
        private readonly WishlistServiceClass _service;

        public WishlistController(WishlistServiceClass service)
        {
            _service = service;
        }

        // Get All Books in Wishlist by Specific Customer
        [HttpGet("wishlist/{customerId}")]
        public async Task<IActionResult> GetWishlistBooks(int customerId)
        {
            var books = await _service.GetBooksInWishlist(customerId);
            return books.Count > 0 ? Ok(books) : NotFound("No books found in wishlist.");
        }

        // Add Book to Wishlist
        [HttpPost("/add-wishlist/{customerId}/{bookId}")]
        public async Task<IActionResult> AddBookToWishlist(int customerId, int bookId)
        {
            bool success = await _service.AddBookToWishlist(customerId, bookId);
            return success ? Ok("Book added to wishlist.") : BadRequest("Failed to add book.");
        }

        // Remove Book from Wishlist
        [HttpDelete("/remove-wishlist/{customerId}/{bookId}")]
        public async Task<IActionResult> RemoveBookFromWishlist(int customerId, int bookId)
        {
            bool success = await _service.RemoveBookFromWishlist(customerId, bookId);
            return success ? Ok("Book removed from wishlist.") : NotFound("Book not found in wishlist.");
        }
    }
}
