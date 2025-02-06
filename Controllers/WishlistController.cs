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

            if (books.Count > 0)
            {
                return Ok(new
                {
                    Status = "Success",
                    Message = "Books retrieved successfully.",
                    Data = books
                });
            }
            else
            {
                return NotFound(new
                {
                    Status = "Error",
                    Message = "No books found in wishlist."
                });
            }
        }


        // Add Book to Wishlist
        [HttpPost("add-wishlist/{customerId}/{bookId}")]
        public async Task<IActionResult> AddBookToWishlist(int customerId, int bookId)
        {
            bool success = await _service.AddBookToWishlist(customerId, bookId);

            if (success)
            {
                return Ok(new
                {
                    Status = "Success",
                    Message = "Book added to wishlist.",
                    Data = new { CustomerId = customerId, BookId = bookId }
                });
            }
            else
            {
                return BadRequest(new
                {
                    Status = "Error",
                    Message = "Failed to add book. Book may already be in the wishlist or an error occurred."
                });
            }
        }


        // Remove Book from Wishlist
        [HttpDelete("remove-wishlist/{customerId}/{bookId}")]
        public async Task<IActionResult> RemoveBookFromWishlist(int customerId, int bookId)
        {
            bool success = await _service.RemoveBookFromWishlist(customerId, bookId);

            if (success)
            {
                return Ok(new
                {
                    Status = "Success",
                    Message = "Book removed from wishlist.",
                    Data = new { CustomerId = customerId, BookId = bookId }
                });
            }
            else
            {
                return NotFound(new
                {
                    Status = "Error",
                    Message = "Book not found in wishlist."
                });
            }
        }

        /// <summary>
        /// API endpoint to transfer a book from wishlist to cart for a specific customer.
        /// </summary>
        /// <param name="customerId">ID of the customer.</param>
        /// <param name="bookId">ID of the book to transfer.</param>
        /// <returns>Returns a response with status message and relevant data if successful, or error if the book is not found in wishlist or already in cart.</returns>
        [HttpPost("transfer-to-cart/{customerId}/{bookId}")]
        public async Task<IActionResult> TransferBookToCart(int customerId, int bookId)
        {
            bool success = await _service.TransferBookToCart(customerId, bookId);

            // If the transfer was successful, return status "Success"
            if (success)
            {
                return Ok(new
                {
                    Status = "Success",
                    Message = "Book transferred from wishlist to cart.",
                    Data = new { CustomerId = customerId, BookId = bookId }
                });
            }
            // If transfer failed (book not found or already in cart), return "Error"
            else
            {
                return NotFound(new
                {
                    Status = "Error",
                    Message = "Book not found in wishlist or already in cart."
                });
            }
        }


    }
}
