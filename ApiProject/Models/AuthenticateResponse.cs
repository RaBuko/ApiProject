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
        public string Token { get; set; }
        //public string RefreshToken { get; internal set; }

        [JsonConstructor]
        public AuthenticateResponse(int id, string firstName, string lastName, string userName, string token)
        {
            Id = id;
            FirstName = firstName;
            LastName = lastName;
            Username = userName;
            Token = token;
        }

        public AuthenticateResponse(User user, string token)
        {
            Id = user.Id;
            FirstName = user.FirstName;
            LastName = user.LastName;
            Username = user.Username;
            Token = token;
        }
    }
}
