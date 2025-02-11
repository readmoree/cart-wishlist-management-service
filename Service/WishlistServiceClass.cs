using MySql.Data.MySqlClient;
using System.Collections.Generic;
using System.Data;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using WishlistService.Entity;
using Microsoft.Extensions.Configuration;
using System.Text;
using CartService.Service;

namespace WishlistService.Service
{
    public class WishlistServiceClass
    {
        private readonly string _connectionString;
        private readonly HttpClient _httpClient;
        private readonly string _bookServiceUrl;
        private readonly ILogger<WishlistServiceClass> _logger;
        private readonly UtilityService _utilityService;


        public WishlistServiceClass(IConfiguration configuration, HttpClient httpClient, ILogger<WishlistServiceClass> logger, UtilityService utilityService)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
            _httpClient = httpClient;
            _bookServiceUrl = configuration["BookServiceUrl"];
            _logger = logger;
            _utilityService = utilityService;


        }

        // Add a Book to Wishlist
        public async Task<bool> AddBookToWishlist(int customerId, int bookId)
        {
            return await _utilityService.AddBookToWishlist(customerId, bookId);

        }


        // Get all books in the wishlist for a specific customer with book details
        public async Task<List<object>> GetBooksInWishlist(int customerId)
        {
            var bookIds = new List<int>();

            // Fetch all Book IDs from Wishlist for the given customer
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                string query = "SELECT BookId FROM Wishlist WHERE CustomerId = @CustomerId";

                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@CustomerId", customerId);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            bookIds.Add(reader.GetInt32("BookId"));
                        }
                    }
                }
            }

            // If no books exist in the wishlist, return an empty list
            if (bookIds.Count == 0)
            {
                return new List<object>();
            }

            // Call the Books microservice with the list of book IDs
            var bookDetails = await _utilityService.GetBookDetails(bookIds);

            return bookDetails;
        }


        // Remove a book from the wishlist
        public async Task<bool> RemoveBookFromWishlist(int customerId, int bookId)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                string deleteQuery = "DELETE FROM Wishlist WHERE CustomerId = @CustomerId AND BookId = @BookId";

                using (var command = new MySqlCommand(deleteQuery, connection))
                {
                    command.Parameters.AddWithValue("@CustomerId", customerId);
                    command.Parameters.AddWithValue("@BookId", bookId);

                    return await command.ExecuteNonQueryAsync() > 0;
                }
            }
        }


        // Transfers a book from the wishlist to the cart for a specific customer.
        public async Task<bool> TransferBookToCart(int customerId, int bookId)
        {
            // Step 1: Check if the book exists in the Wishlist
            bool bookInWishlist = await _utilityService.IsBookInWishlist(customerId, bookId);
            if (!bookInWishlist)
            {
                return false; // Book not found in wishlist
            }

            // Step 2: Check if the book is already in the Cart
            int existingQuantity = await _utilityService.GetCartQuantity(customerId, bookId);

            if (existingQuantity > 0)
            {
                // Step 3: If book exists in cart, update the quantity
                int newQuantity = existingQuantity + 1;
                bool updateCart = await _utilityService.UpdateCartQuantity(customerId, bookId, newQuantity);
                return updateCart; // Return true if the quantity update is successful
            }
            else
            {
                // Step 4: If book is not in cart, add it as a new entry
                bool addToCart = await _utilityService.AddBookToCart(customerId, bookId,1);
                return addToCart; // Return true if adding to the cart is successful
            }
        }



    }
}
