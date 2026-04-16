using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using UserService.Data;
using UserService.Model;
using UserService.Model.Request;

namespace UserService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : Controller
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly IConfiguration _configuration;
        public UserController(ApplicationDbContext applicationDbContext, IConfiguration configuration)
        {
            this._applicationDbContext = applicationDbContext;
            _configuration = configuration;
        }

        [HttpGet]
        public async Task<IActionResult> GetUsersAsync()
        {
            try
            {
                var users = this._applicationDbContext.Users.ToList();
                var response = new ApiResponse<List<User>>
                {
                    Status = "OK",
                    Data = users,
                    Message = "Users fetched successfully"
                };
                return Ok(response);
            }
            catch (Exception ex)
            {
                var errorResponse = new ApiResponse<string>
                {
                    Status = "ERROR",
                    Data = null,
                    Message = ex.Message
                };

                return StatusCode(500, errorResponse);
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateUSersAsync(UserRequest request)
        {
            try
            {
                var existUser = await _applicationDbContext.Users.FirstOrDefaultAsync(x => x.Email == request.Email);
                if (existUser != null)
                {
                    var response = new ApiResponse<string>
                    {
                        Status = "ERROR",
                        Data = null,
                        Message = $"User already exists with this email: {request.Email}"
                    };
                    return BadRequest(response);
                }
                var user = new User
                {
                    Email = request.Email,
                    Name = request.Name,
                    Password = request.Password
                };

                await _applicationDbContext.AddAsync(user);
                await _applicationDbContext.SaveChangesAsync();
                var successResponse = new ApiResponse<User>
                {
                    Status = "OK",
                    Data = user,
                    Message = "User created successfully"
                };
                return Ok(successResponse);
            }
            catch (Exception ex)
            {
                var errorResponse = new ApiResponse<string>
                {
                    Status = "ERROR",
                    Data = null,
                    Message = ex.Message
                };

                return StatusCode(500, errorResponse);
            }
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUserAsync(UserRequest request, long id)
        {
            try
            {
                var user = await _applicationDbContext.Users.FirstOrDefaultAsync(x => x.Id == id);
                if (user == null)
                {
                    var response = new ApiResponse<string>
                    {
                        Status = "ERROR",
                        Data = null,
                        Message = "User not found"
                    };
                    return BadRequest(response);
                }

                var existUser = await _applicationDbContext.Users.AnyAsync(x => x.Email == request.Email);
                if (existUser)
                {
                    return BadRequest(new ApiResponse<string>
                    {
                        Status = "ERROR",
                        Data = null,
                        Message = "Email already used by another user"
                    });
                }
                user.Name = request.Name;
                user.Email = request.Email;
                user.Password = request.Password;
                user.ModifiedAt = DateTime.UtcNow;

                await _applicationDbContext.SaveChangesAsync();

                return Ok(new ApiResponse<User>
                {
                    Status = "OK",
                    Data = user,
                    Message = "User updated successfully"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<string>
                {
                    Status = "ERROR",
                    Data = null,
                    Message = ex.Message
                });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            try
            {
                var user = await _applicationDbContext.Users.FirstOrDefaultAsync(x => x.Id == id);
                if (user == null)
                {
                    var response = new ApiResponse<string>
                    {
                        Status = "ERROR",
                        Data = null,
                        Message = "User not found"
                    };
                    return BadRequest(response);
                }
                user.ModifiedAt = DateTime.UtcNow;
                user.Status = "D";
                await _applicationDbContext.SaveChangesAsync();

                return Ok(new ApiResponse<string>
                {
                    Status = "OK",
                    Data = null,
                    Message = "User deleted successfully"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<string>
                {
                    Status = "ERROR",
                    Data = null,
                    Message = ex.Message
                });
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequest request)
        {
            try
            {
                var user = await _applicationDbContext.Users
                    .FirstOrDefaultAsync(x => x.Email == request.Email);

                if (user == null || user.Password != request.Password)
                {
                    return Unauthorized(new ApiResponse<string>
                    {
                        Status = "ERROR",
                        Data = null,
                        Message = "Invalid email or password"
                    });
                }

                var token = GenerateJwtToken(user);

                return Ok(new ApiResponse<string>
                {
                    Status = "OK",
                    Data = token,
                    Message = "Login successful"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<string>
                {
                    Status = "ERROR",
                    Data = null,
                    Message = ex.Message
                });
            }
        }

        private string GenerateJwtToken(User user)
        {
            var jwtSettings = _configuration.GetSection("Jwt");

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtSettings["Key"])
            );

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.Name, user.Email),
                new Claim("UserId", user.Id.ToString())
            };

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: DateTime.Now.AddHours(1),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
