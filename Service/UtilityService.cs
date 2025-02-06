using System.Net.Http;
using System.Text.Json;
using MySql.Data.MySqlClient;

namespace WishlistService.Service
{
    public class UtilityService
    {

        private readonly string _connectionString;
        private readonly HttpClient _httpClient;
        private readonly string _bookServiceUrl;
        private readonly ILogger<WishlistServiceClass> _logger;

        public UtilityService(IConfiguration configuration, HttpClient httpClient, ILogger<WishlistServiceClass> logger)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
            _httpClient = httpClient;
            _bookServiceUrl = configuration["BookServiceUrl"];
            _logger = logger;

        }

        // Make API call to Books microservice
        public async Task<List<object>> GetBookDetails(List<int> bookIds)
        {
            try
            {
                // Convert the list of book IDs into a comma-separated string for the query parameter
                string bookIdsQuery = string.Join(",", bookIds);

                Console.WriteLine(bookIdsQuery);

                // Make the GET request with bookIds as a query parameter
                var response = await _httpClient.GetAsync($"{_bookServiceUrl}/book/public/by-ids?bookIds={bookIdsQuery}");
                Console.WriteLine(response);

                if (response.IsSuccessStatusCode)
                {
                    string jsonResponse = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<List<object>>(jsonResponse);
                }

                _logger.LogWarning($"Books not found for IDs: {string.Join(", ", bookIds)}");
                return new List<object> { new { Message = "Books not found" } };
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError($"Error fetching book details: {ex.Message}");
                return new List<object> { new { Message = "Error retrieving book details" } };
            }
        }

        
        // Checks if a specific book is already in the cart for a customer.
        public async Task<bool> IsBookInCart(int customerId, int bookId)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                string query = "SELECT COUNT(*) FROM Cart WHERE CustomerId = @CustomerId AND BookId = @BookId";

                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@CustomerId", customerId);
                    command.Parameters.AddWithValue("@BookId", bookId);

                    var count = Convert.ToInt32(await command.ExecuteScalarAsync());
                    return count > 0; // Returns true if book is already in the cart
                }
            }
        }

        
        // Checks if a specific book exists in the wishlist for a customer.
        public async Task<bool> IsBookInWishlist(int customerId, int bookId)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                string query = "SELECT COUNT(*) FROM Wishlist WHERE CustomerId = @CustomerId AND BookId = @BookId";

                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@CustomerId", customerId);
                    command.Parameters.AddWithValue("@BookId", bookId);

                    var count = Convert.ToInt32(await command.ExecuteScalarAsync());
                    return count > 0; // Returns true if book is found
                }
            }
        }


        // Add a book to the cart (increase quantity if it exists)
        public async Task<bool> AddBookToCart(int customerId, int bookId)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                // Check if the book is already in the cart
                string checkQuery = "SELECT Quantity FROM Cart WHERE CustomerId = @CustomerId AND BookId = @BookId";

                using (var checkCommand = new MySqlCommand(checkQuery, connection))
                {
                    checkCommand.Parameters.AddWithValue("@CustomerId", customerId);
                    checkCommand.Parameters.AddWithValue("@BookId", bookId);

                    object result = await checkCommand.ExecuteScalarAsync();

                    if (result != null)
                    {
                        int newQuantity = (int)result + 1;

                        // Update quantity if the book is already in the cart
                        string updateQuery = "UPDATE Cart SET Quantity = @Quantity WHERE CustomerId = @CustomerId AND BookId = @BookId";

                        using (var updateCommand = new MySqlCommand(updateQuery, connection))
                        {
                            updateCommand.Parameters.AddWithValue("@Quantity", newQuantity);
                            updateCommand.Parameters.AddWithValue("@CustomerId", customerId);
                            updateCommand.Parameters.AddWithValue("@BookId", bookId);

                            return await updateCommand.ExecuteNonQueryAsync() > 0;
                        }
                    }
                }

                // Insert new book into the cart if it doesn’t exist
                string insertQuery = "INSERT INTO Cart (CustomerId, BookId, Quantity) VALUES (@CustomerId, @BookId, 1)";

                using (var insertCommand = new MySqlCommand(insertQuery, connection))
                {
                    insertCommand.Parameters.AddWithValue("@CustomerId", customerId);
                    insertCommand.Parameters.AddWithValue("@BookId", bookId);

                    return await insertCommand.ExecuteNonQueryAsync() > 0;
                }
            }
        }


        // Add a Book to the wishlist
        public async Task<bool> AddBookToWishlist(int customerId, int bookId)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                string insertQuery = "INSERT INTO Wishlist (CustomerId, BookId) VALUES (@CustomerId, @BookId)";

                using (var insertCommand = new MySqlCommand(insertQuery, connection))
                {
                    insertCommand.Parameters.AddWithValue("@CustomerId", customerId);
                    insertCommand.Parameters.AddWithValue("@BookId", bookId);

                    return await insertCommand.ExecuteNonQueryAsync() > 0;
                }
            }
        }

       
        // Get the quantity of the cart
        public async Task<int> GetCartQuantity(int customerId, int bookId)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                string query = "SELECT Quantity FROM Cart WHERE CustomerId = @CustomerId AND BookId = @BookId";

                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@CustomerId", customerId);
                    command.Parameters.AddWithValue("@BookId", bookId);

                    object result = await command.ExecuteScalarAsync();
                    return result != null ? Convert.ToInt32(result) : 0;
                }
            }
        }


        // Update the cart quantity
        public async Task<bool> UpdateCartQuantity(int customerId, int bookId, int newQuantity)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                string query = "UPDATE Cart SET Quantity = @Quantity WHERE CustomerId = @CustomerId AND BookId = @BookId";

                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@CustomerId", customerId);
                    command.Parameters.AddWithValue("@BookId", bookId);
                    command.Parameters.AddWithValue("@Quantity", newQuantity);

                    int rowsAffected = await command.ExecuteNonQueryAsync();
                    return rowsAffected > 0;
                }
            }
        }


        // Remove book from cart
        public async Task<bool> RemoveBookFromCart(int customerId, int bookId)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                string query = "DELETE FROM Cart WHERE CustomerId = @CustomerId AND BookId = @BookId";

                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@CustomerId", customerId);
                    command.Parameters.AddWithValue("@BookId", bookId);

                    int rowsAffected = await command.ExecuteNonQueryAsync();
                    return rowsAffected > 0;
                }
            }
        }




    }

}
