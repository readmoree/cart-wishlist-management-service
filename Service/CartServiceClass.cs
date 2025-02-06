using CartService.Entity;
using MySql.Data.MySqlClient;
using System.Collections.Generic;
using System.Data;
using System.Net.Http;
using System.Threading.Tasks;
using WishlistService.Service;
using System.Text.Json;
using WishlistService.DatabaseContext;
using Mysqlx.Crud;


namespace CartService.Service { 

    public class CartServiceClass
    {

        private readonly string _connectionString;
        private readonly HttpClient _httpClient;
        private readonly string _bookServiceUrl;
        private readonly ILogger<WishlistServiceClass> _logger;
        private readonly UtilityService _utilityService;
        

        public CartServiceClass(IConfiguration configuration, HttpClient httpClient, ILogger<WishlistServiceClass> logger, UtilityService utilityService)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
            _httpClient = httpClient;
            _bookServiceUrl = configuration["BookServiceUrl"];
            _logger = logger;
            _utilityService = utilityService;
            

        }

        // Get all books in the cart for a specific customer with book details and quantity
        public async Task<dynamic> GetBooksInCart(int customerId)
        {
            var bookIds = new List<int>();
            var quantities = new List<int>();

            // Step 1: Get all BookIds and Quantities from the Cart for the given customer
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                string query = "SELECT BookId, Quantity FROM Cart WHERE CustomerId = @CustomerId";

                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@CustomerId", customerId);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        // Collect all the book IDs and their quantities
                        while (await reader.ReadAsync())
                        {
                            int bookId = reader.GetInt32("BookId");
                            int quantity = reader.GetInt32("Quantity");

                            // Store book IDs and quantities separately for further processing
                            bookIds.Add(bookId);
                            quantities.Add(quantity);
                        }
                    }
                }
            }

            // Step 2: If no books are found in the cart
            if (bookIds.Count == 0)
            {
                return new { Message = "No books found in the cart." };
            }

            // Step 3: Fetch book details using the Wishlist service's method
            var bookDetails = await _utilityService.GetBookDetails(bookIds);

            // Step 4: Create the combined result with quantities and books in a single response
            var combinedResult = new
            {
                quantitiesOfBooks = quantities,
                books = bookDetails
            };

            // Step 5: Return the combined result
            return combinedResult;
        }


        // Add a book to the cart
        public async Task<bool> AddBookToCart(int customerId, int bookId)
        {
            return await _utilityService.AddBookToCart(customerId, bookId);

        }


        // Remove a book from the cart (delete if exists)
        public async Task<bool> RemoveBookFromCart(int customerId, int bookId)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                // Check if the book exists in the cart
                string checkQuery = "SELECT 1 FROM Cart WHERE CustomerId = @CustomerId AND BookId = @BookId";

                using (var checkCommand = new MySqlCommand(checkQuery, connection))
                {
                    checkCommand.Parameters.AddWithValue("@CustomerId", customerId);
                    checkCommand.Parameters.AddWithValue("@BookId", bookId);

                    object result = await checkCommand.ExecuteScalarAsync();

                    if (result == null)
                    {
                        return false; // Book not found in cart
                    }

                    // Remove the book from the cart regardless of the quantity
                    string deleteQuery = "DELETE FROM Cart WHERE CustomerId = @CustomerId AND BookId = @BookId";

                    using (var deleteCommand = new MySqlCommand(deleteQuery, connection))
                    {
                        deleteCommand.Parameters.AddWithValue("@CustomerId", customerId);
                        deleteCommand.Parameters.AddWithValue("@BookId", bookId);

                        return await deleteCommand.ExecuteNonQueryAsync() > 0;
                    }
                }
            }
        }


        // Transfer a book from cart to wishlist

        public async Task<bool> TransferBookToWislist(int customerId, int bookId)
        {
            // Check if the book already exists in the Wishlist
            bool bookInWishlist = await _utilityService.IsBookInWishlist(customerId, bookId);
            if (bookInWishlist)
            {
                return false; // Book is already in wishlist
            }

            // Check if the book exists in the cart
            bool bookInCart = await _utilityService.IsBookInCart(customerId, bookId);
            if (!bookInCart)
            {
                return false; // Book not exists  in cart
            }

            // Remove the book from the cart
            bool removeFromCart = await RemoveBookFromCart(customerId, bookId);
            if (!removeFromCart)
            {
                return false; // If removal from cart failed
            }

            // Add the book to wishlist
            bool addToWishlist = await _utilityService.AddBookToWishlist(customerId, bookId);
            return addToWishlist; // Return the result of adding to the wishlist
        }


        // Deletes all Book from Cart
        public async Task<bool> DeleteAllBookFromCart(int customerId)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                string query = "DELETE FROM Cart WHERE CustomerId = @CustomerId";

                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@CustomerId", customerId);

                    int rowsAffected = await command.ExecuteNonQueryAsync();
                    return rowsAffected > 0;
                }
            }
        }

    }
}
