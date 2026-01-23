using NowPlaying.Models;
using System.Net.Http.Json;
using System.Text.Json;

namespace NowPlaying.Services
{
    public class TMDBService
    {
        private readonly HttpClient _http;
        private readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
        };

        public TMDBService(HttpClient http, IConfiguration config)
        {
            _http = http;
            string? tmdbKey = config["TmdbAccesKey"];

            if (!string.IsNullOrEmpty(tmdbKey))
            {
                _http.BaseAddress = new Uri("https://api.themoviedb.org/3/");
                _http.DefaultRequestHeaders.Authorization = new("Bearer", tmdbKey);
            }
            else
            {
                //deploy to netlify
                _http.BaseAddress = new Uri(_http.BaseAddress + "TMDB/");
            }
        }

        private readonly string imageBaseUrl = "https://image.tmdb.org/t/p/w500";

        public async Task<MovieListResponse> GetNowPlayingMovies()
        {
            string url = "movie/now_playing?region=US&language=en-US";

            MovieListResponse response =
                await _http.GetFromJsonAsync<MovieListResponse>(url, _jsonOptions)
                ?? throw new HttpIOException(HttpRequestError.InvalidResponse, "Now Playing Movies could not be loaded");

            foreach (var movie in response.Results)
            {
                if (string.IsNullOrEmpty(movie.PosterPath))
                {
                    movie.PosterPath = "/images/mw1920_poster.png";
                }
                else
                {
                    movie.PosterPath = $"{imageBaseUrl}{movie.PosterPath}";
                }
            }

            return response;
        }

        public async Task<MovieListResponse> GetPopularMovies()
        {
            string url = "movie/popular?region=US&language=en-US";

            MovieListResponse response =
                await _http.GetFromJsonAsync<MovieListResponse>(url, _jsonOptions)
                ?? throw new HttpIOException(HttpRequestError.InvalidResponse, "Popular Movies could not be loaded");

            foreach (var movie in response.Results)
            {
                if (string.IsNullOrEmpty(movie.PosterPath))
                {
                    movie.PosterPath = "/images/mw1920_poster.png";
                }
                else
                {
                    movie.PosterPath = $"{imageBaseUrl}{movie.PosterPath}";
                }
            }

            return response;
        }

        public async Task<MovieListResponse> SearchMovies(string query)
        {
            string url = $"search/movie?query={query}&include_adult=false&language=en-US";

            MovieListResponse response =
                await _http.GetFromJsonAsync<MovieListResponse>(url, _jsonOptions)
                ?? throw new HttpIOException(HttpRequestError.InvalidResponse, "Search results could not be loaded");

            foreach (var movie in response.Results)
            {
                if (string.IsNullOrEmpty(movie.PosterPath))
                {
                    movie.PosterPath = "/images/mw1920_poster.png";
                }
                else
                {
                    movie.PosterPath = $"{imageBaseUrl}{movie.PosterPath}";
                }
            }

            return response;
        }

        public async Task<MovieDetails> GetMovieById(int movieId)
        {
            string url = $"movie/{movieId}";

            MovieDetails movie = await _http.GetFromJsonAsync<MovieDetails>(url, _jsonOptions)
                ?? throw new HttpIOException(HttpRequestError.InvalidResponse, "Could not retrieve movie details");

            movie.PosterPath = string.IsNullOrEmpty(movie.PosterPath)
                ? "/images/mw1920_poster.png"
                : $"{imageBaseUrl}{movie.PosterPath}";

            movie.BackdropPath = string.IsNullOrEmpty(movie.BackdropPath)
                ? "/images/mw1920_backdrop.jpg"
                : $"{imageBaseUrl}{movie.PosterPath}";

            return movie;
        }

        public async Task<Video?> GetMovieTrailer(int movieId)
        {
            string url = $"movie/{movieId}/videos?language=en-US";

            var videos = await _http.GetFromJsonAsync<MovieVideosResponse>(url, _jsonOptions)
                ?? throw new HttpIOException(HttpRequestError.InvalidResponse, "Could not retrieve movie trailer");

            return videos.Results.FirstOrDefault(v => v.Site!.Contains("YouTube", StringComparison.OrdinalIgnoreCase)
                                                                && v.Type!.Contains("Trailer", StringComparison.OrdinalIgnoreCase));
        }

        public async Task<CreditsResponse> GetMovieCredits(int movieId)
        {
            string url = $"movie/{movieId}/credits?language=en-US";

            CreditsResponse credits = await _http.GetFromJsonAsync<CreditsResponse>(url, _jsonOptions)
                ?? throw new HttpIOException(HttpRequestError.InvalidResponse, "Could not retrieve movie credits");

            foreach (var cast in credits.Cast)
            {
                cast.ProfilePath = string.IsNullOrEmpty(cast.ProfilePath)
                    ? "/images/mw1920_profile.jpg"
                    : $"{imageBaseUrl}{cast.ProfilePath}";
            }

            foreach (var crew in credits.Crew)
            {
                crew.ProfilePath = string.IsNullOrEmpty(crew.ProfilePath)
                    ? "/images/mw1920_profile.jpg"
                    : $"{imageBaseUrl}{crew.ProfilePath}";
            }

            return credits;
        }
    }
}
