using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using CartService.Service;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using WishlistService.Service;

namespace CartService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CartController : ControllerBase
    {
        private readonly CartServiceClass _service;
        private readonly UtilityService _utilityService;

        public CartController(CartServiceClass service, UtilityService utilityService)
        {
            _service = service; 
            _utilityService = utilityService;
        }

       

        // Get all books in the cart for a specific customer
        [HttpGet("cart/{customerId}")]
        public async Task<IActionResult> GetCartBooks(int customerId)
        {
            Console.WriteLine("INSIDE GET CART METHOD");
            var books = await _service.GetBooksInCart(customerId);

            if (books != null)
            {
                // Formatted response when books are found
                var response = new
                {
                    status = "success",
                    message = "Books found in the cart.",
                    data = books
                };

                return Ok(response);
            }
            else
            {
                // Formatted response when no books are found
                var response = new
                {
                    status = "error",
                    message = "No books found in cart.",
                    data = new List<dynamic>()
                };

                return NotFound(response);
            }
        }


        // Add book to the cart
        [HttpPost("add-cart/{customerId}/{bookId}/{quantity}")]
        public async Task<IActionResult> AddBookToCart(int customerId, int bookId,int quantity)
        {
            bool success = await _service.AddBookToCart(customerId, bookId,quantity);

            if (success)
            {
                // Successful response with data and success status
                var response = new
                {
                    status = "success",
                    message = "Book added to cart.",
                    data = new { customerId, bookId }  // You can customize the data as needed
                };
                return Ok(response);
            }
            else
            {
                // Error response with error status and message
                var response = new
                {
                    status = "error",
                    message = "Failed to add book to cart.",
                    error = "Unable to process the request" // You can customize the error message
                };
                return BadRequest(response);
            }
        }


        // Remove book from the cart
        [HttpDelete("remove-cart/{customerId}/{bookId}")]
        public async Task<IActionResult> RemoveBookFromCart(int customerId, int bookId)
        {
            bool success = await _service.RemoveBookFromCart(customerId, bookId);

            if (success)
            {
                // Successful response with data and success status
                var response = new
                {
                    status = "success",
                    message = "Book removed from cart.",
                    data = new { customerId, bookId }  // You can customize the data as needed
                };
                return Ok(response);
            }
            else
            {
                // Error response with error status and message
                var response = new
                {
                    status = "error",
                    message = "Failed to remove book from cart.",
                    error = "Book not found in cart" // You can customize the error message
                };
                return NotFound(response);
            }
        }


        // Transfers book from cart to wishlist
        [HttpPost("transfer-to-wishlist/{customerId}/{bookId}")]
        public async Task<IActionResult> TransferBookToWishlist(int customerId, int bookId)
        {
            bool success = await _service.TransferBookToWislist(customerId, bookId);

            // If the transfer was successful, return status "Success"
            if (success)
            {
                return Ok(new
                {
                    Status = "Success",
                    Message = "Book transferred from cart to wishlist",
                    Data = new { CustomerId = customerId, BookId = bookId }
                });
            }
            // If transfer failed (book not found or already in cart), return "Error"
            else
            {
                return NotFound(new
                {
                    Status = "Error",
                    Message = "Book not found in cart or already in wishlist."
                });
            }
        }


        // Update the quantity of the cart
        [HttpPut("update-quantity/{customerId}/{bookId}/{quantity}")]
        public async Task<IActionResult> UpdateCartQuantity(int customerId, int bookId, int quantity)
        {
            if (quantity <= 0)
            {
                return BadRequest(new { status = "error", message = "Invalid quantity. Quantity must be greater than 0." });
            }

            // Perform the update logic
            bool updateSuccess = await _utilityService.UpdateCartQuantity(customerId, bookId, quantity);

            if (updateSuccess)
            {
                return Ok(new { status = "success", message = "Cart updated successfully." });
            }
            else
            {
                return StatusCode(500, new { status = "error", message = "Failed to update cart." });
            }
        }


        // Delete all books from Cart
        [HttpDelete("delete-all-cart/{customerId}")]
        public async Task<IActionResult> DeleteAllBookFromCart(int customerId)
        {
            bool success = await _service.DeleteAllBookFromCart(customerId);

            if (success)
            {
                // Successful response with data and success status
                var response = new
                {
                    status = "success",
                    message = "All Books removed from cart."
                };
                return Ok(response);
            }
            else
            {
                // Error response with error status and message
                var response = new
                {
                    status = "error",
                    message = "Failed to remove all book from cart.",
                    error = "Book not found in cart" // You can customize the error message
                };
                return NotFound(response);
            }
        }




    }
}
