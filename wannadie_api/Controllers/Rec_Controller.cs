using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net;
using MySql.Data.MySqlClient;
using SpotifyAPI.Web;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;



namespace wannadie_api.Controllers
{
    public class Event_by_Genre : ControllerBase
    {
        [HttpGet("genre/{genreName}")]
        public async Task<IActionResult> GetEventsByGenre(string genreName)
        {
            string slug = genreName.Replace(" ", "-");

            string url = $"https://api.seatgeek.com/2/performers?genres.slug={slug}&client_id=MzM5OTE4NTF8MTY4NTQzODQ5OC41ODE1Nzgz";

            using (var httpClient = new HttpClient())
            {
                var response = await httpClient.GetAsync(url);
                if (!response.IsSuccessStatusCode)
                {
                    return StatusCode((int)response.StatusCode, "Error retrieving events from SeatGeek API");
                }

                var content = await response.Content.ReadAsStringAsync();
                return Content(content, "application/json");
            }
        }
    }
    [ApiController]
    [Route("api/[controller]")]
    public class Event_by_Location : ControllerBase
    {
        [HttpGet("city/{location}")]
        public async Task<IActionResult> GetEventsByGenre(string location)
        {
            string clientId = "MzM5OTE4NTF8MTY4NTQzODQ5OC41ODE1Nzgz";

            string slug = location.Replace(" ", "-");

            string url = $"https://api.seatgeek.com/2/events?venue.city={location}&taxonomies.name=concert&client_id={clientId}";

            using (var httpClient = new HttpClient())
            {
                var response = await httpClient.GetAsync(url);
                if (!response.IsSuccessStatusCode)
                {
                    return StatusCode((int)response.StatusCode, "Error retrieving events from SeatGeek API");
                }

                var content = await response.Content.ReadAsStringAsync();
                return Content(content, "application/json");
            }
        }
    }

    [Route("api/superfinder/[controller]")]

    public class Event_by_Artist : ControllerBase
    {
        private const string MapQuestApiKey = "OvuQftOc28AvxVJfHn02vb75BHPivkdh";

        [HttpGet("artist/{artistName}")]
        public async Task<IActionResult> GetEventsByArtist(string artistName)
        {
            string apiKey = "MzM5OTE4NTF8MTY4NTQzODQ5OC41ODE1Nzgz";
            string slug = artistName.Replace(" ", "-");

            string url = $"https://api.seatgeek.com/2/events?performers.slug={slug}&client_id=MzM5OTE4NTF8MTY4NTQzODQ5OC41ODE1Nzgz";

            using (var httpClient = new HttpClient())
            {
                var response = await httpClient.GetAsync(url);
                if (!response.IsSuccessStatusCode)
                {
                    return StatusCode((int)response.StatusCode, "Error retrieving events from SeatGeek API");
                }

                var content = await response.Content.ReadAsStringAsync();
                return Content(content, "application/json");
            }
        }
    }
    [Route("api/base/[controller]")]
    public class GenresController : ControllerBase
    {
        private readonly string connectionString = "Server=34.70.56.25; Port=3306; Database=pipipupu:us-central1:chatgenres; User=chatgenres; Password=supertiddies;";

        [HttpGet]
        public async Task<IActionResult> GetGenres()
        {
            try
            {
                using (var connection = new MySqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    var query = "SELECT id, genres FROM YourTableName";
                    var command = new MySqlCommand(query, connection);
                    var reader = await command.ExecuteReaderAsync();

                    var genresList = new List<Genre>();

                    while (await reader.ReadAsync())
                    {
                        var genre = new Genre
                        {
                            chat_id = reader.GetInt32(reader.GetOrdinal("id")),
                            genres = reader.GetString(reader.GetOrdinal("genres"))
                        };

                        genresList.Add(genre);
                    }

                    return Ok(genresList);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        public class Genre
        {
            public int chat_id { get; set; }
            public string genres { get; set; }
        }
    }
    [ApiController]
    [Route("api/checker/[controller]")]
    public class Genresfinder : ControllerBase
    {
        private readonly string connectionString = "Server=34.70.56.25; Port=3306; Database=superrecs; User=chatgenres; Password=supertiddies;";

        [HttpGet("{chatId}")]
        public async Task<IActionResult> GetGenresByChatId(int chatId)
        {
            try
            {
                using (var connection = new MySqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    var query = "SELECT genres FROM storegen WHERE chat_id = @chatId";
                    var command = new MySqlCommand(query, connection);
                    command.Parameters.AddWithValue("@chatId", chatId);

                    var result = await command.ExecuteScalarAsync();
                    if (result != null)
                    {
                        var genres = result.ToString();
                        return Ok(genres);
                    }
                    else
                    {
                        return NotFound($"No genres found for chat ID: {chatId}");
                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

    }
        [ApiController]
    [Route("api/superfinder/[controller]")]
    public class RecommendationController : ControllerBase
    {
        private const string SpotifyClientId = "48f9cc3719624a9da8cce71daf020f0d";
        private const string SpotifyClientSecret = "5b45f39aaa2b4bcd866ad1afa15cb6f3";
        private readonly string connectionString = "Server=34.70.56.25; Port=3306; Database=superrecs; User=chatgenres; Password=supertiddies;";

        [HttpPost]
        public async Task<IActionResult> GetRecommendation([FromBody] RecommendationRequest request)
        {
            try
            {
                string selectedGenre = await GetRandomGenreByChatId(request.ChatId);
                if (selectedGenre == null)
                {
                    return NotFound($"No genres found for chat ID: {request.ChatId}");
                }

                string recommendation = await GetMusicRecommendation(selectedGenre);

                var response = new RecommendationResponse
                {
                    Recommendation = recommendation
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        private async Task<string> GetRandomGenreByChatId(int chatId)
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                await connection.OpenAsync();

                var query = "SELECT genres FROM storegen WHERE chat_id = @chatId";
                var command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@chatId", chatId);

                var result = await command.ExecuteScalarAsync();
                if (result != null)
                {
                    string genres = result.ToString();
                    List<string> genreList = genres.Split(';').ToList();
                    if (genreList.Count > 0)
                    {
                        Random random = new Random();
                        int index = random.Next(genreList.Count);
                        return genreList[index];
                    }
                }

                return null;
            }
        }

        private async Task<string> GetMusicRecommendation(string genre)
        {
            var config = SpotifyClientConfig.CreateDefault();
            var request = new ClientCredentialsRequest(SpotifyClientId, SpotifyClientSecret);
            var response = await new OAuthClient(config).RequestToken(request);

            var spotifyClient = new SpotifyClient(config.WithToken(response.AccessToken));

            var searchRequest = new SearchRequest(SearchRequest.Types.Playlist, genre);
            var searchResults = await spotifyClient.Search.Item(searchRequest);

            if (searchResults.Playlists.Items.Count > 0)
            {
                var playlistId = searchResults.Playlists.Items[0].Id;

                var playlist = await spotifyClient.Playlists.Get(playlistId);
                if (playlist.Tracks.Items.Count > 0)
                {
                    Random random = new Random();
                    int index = random.Next(playlist.Tracks.Items.Count);
                    var track = playlist.Tracks.Items[index].Track as FullTrack;

                    var songUrl = track.ExternalUrls["spotify"];

                    return $"{track.Name} by {string.Join(", ", track.Artists.Select(a => a.Name))}\nSpotify link: {songUrl}";
                }
            }

            return "No recommendation found.";
        }
    }

    public class RecommendationRequest
    {
        public int ChatId { get; set; }
    }

    public class RecommendationResponse
    {
        public string Recommendation { get; set; }
    }

}
