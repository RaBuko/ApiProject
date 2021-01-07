using ApiProject.Entities;
using System.Text.Json.Serialization;

namespace ApiProject.Models
{
    public class AuthenticateResponse
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Username { get; set; }
        public string JwtToken { get; set; }

        [JsonIgnore]
        public string RefreshToken { get; internal set; }

        [JsonConstructor]
        public AuthenticateResponse(int id, string firstName, string lastName, string username, string jwtToken, string refreshToken)
        {
            Id = id;
            FirstName = firstName;
            LastName = lastName;
            Username = username;
            JwtToken = jwtToken;
            RefreshToken = refreshToken;
        }

        public AuthenticateResponse(User user, string jwtToken, string refreshToken)
        {
            Id = user.Id;
            FirstName = user.FirstName;
            LastName = user.LastName;
            Username = user.Username;
            JwtToken = jwtToken;
            RefreshToken = refreshToken;
        }
    }
}
