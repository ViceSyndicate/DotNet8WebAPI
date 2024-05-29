using DotNet8WebAPI.Entity;
using DotNet8WebAPI.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace DotNet8WebAPI.Services
{
    public class UserService : IUserService
    {
        private readonly AppSettings _AppSettings;
        private readonly OurHeroDbContext db;
        public UserService(IOptions<AppSettings> appSettings, OurHeroDbContext _db)
        {
            _AppSettings = appSettings.Value;
            db = _db;
        }

        public async Task<AuthenticateResponse?> Authenticate(AuthenticateRequest model)
        {
            // Get Single user from db where the models username and password matches.
            var user = await db.Users.SingleOrDefaultAsync(x => x.Username == model.Username && x.Password == model.Password);
            if (user == null) return null;
            var token = await generateJwtToken(user);

            return new AuthenticateResponse(user, token);
        }

        public async Task<IEnumerable<User>> GetAll()
        {
            return await db.Users.Where(x => x.isActive == true).ToListAsync();
        }
        public async Task<User?> GetById(int id)
        {
            return await db.Users.FirstOrDefaultAsync(x => x.Id == id);
        }
        public async Task<User?> AddAndUpdateUser(User userObj)
        {
            bool isSuccess = false;
            if (userObj.Id > 0)
            {
                // obj = object/user to update.
                var obj = await db.Users.FirstOrDefaultAsync(c => c.Id == userObj.Id);

                obj.FirstName = userObj.FirstName;
                obj.LastName = userObj.LastName;
                db.Users.Update(obj);
                // SaveChangesAsync() returns then number of state entries written to db.
                // If 0 we didn't update anything.
                isSuccess = await db.SaveChangesAsync() > 0;
            }
            return isSuccess ? userObj : null;
        }

        // Helper method 
        private async Task<string> generateJwtToken(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var token = await Task.Run(() =>
            {
                var key = Encoding.ASCII.GetBytes(_AppSettings.Secret);
                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(new[] { new Claim("id", user.Id.ToString()) }),
                    Expires = DateTime.UtcNow.AddDays(7),
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
                };
                return tokenHandler.CreateToken(tokenDescriptor);
            });
            return tokenHandler.WriteToken(token);
        }
    }
}
