using System.Data;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using AutoMapper;
using BloggingPlatform.config;
using BloggingPlatform.dtos;
using BloggingPlatform.helpers;
using BloggingPlatform.models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Swashbuckle.AspNetCore.Annotations;


namespace BloggingPlatform.controllers
{
    [ApiController]
    [Route("[controller]")]
    [SwaggerTag("Endpoints for user authentication and management.")]
    public class AuthController : ControllerBase
    {
        private readonly DataContextEF _entityFramework;
        private readonly AuthHelper _authHelper;
        private readonly IMapper _mapper;

        public AuthController(IConfiguration config, IMapper mapper)
        {
            _entityFramework = new DataContextEF(config);
            _authHelper = new AuthHelper(config);
            _mapper = mapper;
        }

        [AllowAnonymous]
        [HttpPost("Register")]
        [SwaggerOperation(Summary = "Register a new user", Description = "Creates a new user account with the provided registration details.")]
        [SwaggerResponse(StatusCodes.Status200OK, "User registered successfully.", typeof(CommonFieldsResponseDto<RegisterResponseDto>))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid registration details.")]
        public IActionResult Register(RegisterRequestDto userForRegistration)
        {
            CommonFieldsResponseDto<RegisterResponseDto> commonFieldsResponseDto = new CommonFieldsResponseDto<RegisterResponseDto>();

            if (userForRegistration.Password == userForRegistration.PasswordConfirm)
            {
                User? user = _entityFramework.Users
                .Where(u => u.Email == userForRegistration.Email)
                .FirstOrDefault<User>();
                if (user == null)
                {
                    byte[] passwordSalt = new byte[128 / 8];
                    using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
                    {
                        rng.GetNonZeroBytes(passwordSalt);
                    }

                    byte[] passwordHash = _authHelper.GetPasswordHash(userForRegistration.Password, passwordSalt);


                    User newUser = new User();

                    newUser.FirstName = userForRegistration.FirstName;
                    newUser.LastName = userForRegistration.LastName;
                    newUser.Email = userForRegistration.Email;
                    newUser.PasswordHash = passwordHash;
                    newUser.PasswordSalt = passwordSalt;
                    newUser.Gender = userForRegistration.Gender == "male" ? true : false;
                    newUser.Active = true;
                    newUser.Admin = false;
                    newUser.UserCreated = DateTime.Now;
                    newUser.UserUpdated = DateTime.Now;
                    _entityFramework.Add(newUser);

                    if (_entityFramework.SaveChanges() > 0)
                    {
                        commonFieldsResponseDto.Success = true;
                        commonFieldsResponseDto.Message = "New User Registered Successfully.";
                        commonFieldsResponseDto.Response = _mapper.Map<RegisterResponseDto>(newUser);
                        commonFieldsResponseDto.ResponseList = null;
                        return Ok(commonFieldsResponseDto.GetFilteredResponse());
                    }
                    throw new Exception("Failed to add user.");
                }
                else
                {
                    throw new Exception("User with this email already exists!");
                }
                throw new Exception("Failed to register user.");
            }
            else
            {
                throw new Exception("Passwords do not match!");
            }
        }

        [AllowAnonymous]
        [HttpPost("Login")]
        [SwaggerOperation(Summary = "User login", Description = "Authenticates a user and returns an authentication token.")]
        [SwaggerResponse(StatusCodes.Status200OK, "Login successful, token generated.", typeof(CommonFieldsResponseDto<LoginResponseDto>))]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, "Invalid credentials.")]
        [SwaggerResponse(StatusCodes.Status404NotFound, "User not found.")]
        public IActionResult Login(LoginRequestDto userForLogin)
        {


            User? userForConfirmation = _entityFramework.Users
                .Where(u => u.Email == userForLogin.Email)
                .FirstOrDefault<User>();

            CommonFieldsResponseDto<LoginResponseDto> commonFieldsResponseDto = new CommonFieldsResponseDto<LoginResponseDto>();
            LoginResponseDto loginResponseDto = new LoginResponseDto();

            if (userForConfirmation != null && userForConfirmation.Active)
            {
                byte[] passwordHash = _authHelper.GetPasswordHash(userForLogin.Password, userForConfirmation.PasswordSalt);

                // if (passwordHash == userForConfirmation.PasswordHash) // Won't work

                for (int index = 0; index < passwordHash.Length; index++)
                {
                    if (passwordHash[index] != userForConfirmation.PasswordHash[index])
                    {
                        commonFieldsResponseDto.Success = false;
                        commonFieldsResponseDto.Message = "Incorrect Password";
                        commonFieldsResponseDto.Response = null;
                        commonFieldsResponseDto.ResponseList = null;
                        return Unauthorized(commonFieldsResponseDto.GetFilteredResponse());
                    }
                }

                loginResponseDto.Token = _authHelper.CreateToken(userForConfirmation.UserId);

                commonFieldsResponseDto.Success = true;
                commonFieldsResponseDto.Message = "User with Email: " + userForConfirmation.Email + " is logged in successfully.";
                commonFieldsResponseDto.Response = loginResponseDto;
                commonFieldsResponseDto.ResponseList = null;
                return Ok(commonFieldsResponseDto.GetFilteredResponse());

            }
            else
            {
                commonFieldsResponseDto.Success = false;
                commonFieldsResponseDto.Message = "User with this Email not found.";
                commonFieldsResponseDto.Response = null;
                commonFieldsResponseDto.ResponseList = null;
                return NotFound(commonFieldsResponseDto.GetFilteredResponse());
            }

        }

        [AllowAnonymous]
        [HttpPut("ResetPassword")]
        [SwaggerOperation(Summary = "Reset user password", Description = "Updates the user's password based on provided details.")]
        [SwaggerResponse(StatusCodes.Status200OK, "Password reset successfully.")]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid password reset details.")]
        public IActionResult ResetPassword(ResetPasswordRequestDto userForRegistration)
        {
            CommonFieldsResponseDto<RegisterResponseDto> commonFieldsResponseDto = new CommonFieldsResponseDto<RegisterResponseDto>();
            if (userForRegistration.Password == userForRegistration.PasswordConfirm)
            {
                User? user = _entityFramework.Users
                .Where(u => u.Email == userForRegistration.Email)
                .FirstOrDefault<User>();
                if (user != null)
                {
                    byte[] passwordSalt = new byte[128 / 8];
                    using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
                    {
                        rng.GetNonZeroBytes(passwordSalt);
                    }

                    byte[] passwordHash = _authHelper.GetPasswordHash(userForRegistration.Password, passwordSalt);


                    user.PasswordHash = passwordHash;
                    user.PasswordSalt = passwordSalt;
                    user.UserUpdated = DateTime.Now;
                    if (_entityFramework.SaveChanges() > 0)
                    {
                        commonFieldsResponseDto.Success = true;
                        commonFieldsResponseDto.Message = "Your Password Reset is Successful.";
                        commonFieldsResponseDto.Response = null;
                        commonFieldsResponseDto.ResponseList = null;
                        return Ok(commonFieldsResponseDto.GetFilteredResponse());
                    }
                    throw new Exception("Failed to reset password.");
                }
                else
                {
                    throw new Exception("User with this email doesnot exists!");
                }
                throw new Exception("Failed to reset password.");
            }
            else
            {
                throw new Exception("Passwords do not match!");
            }
        }

        [Authorize] //Protected Route
        [HttpGet("RefreshToken")]
        [SwaggerOperation(Summary = "Refresh authentication token", Description = "Generates a new token for the authenticated user.")]
        [SwaggerResponse(StatusCodes.Status200OK, "Token refreshed successfully.", typeof(CommonFieldsResponseDto<LoginResponseDto>))]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, "User not authenticated.")]
        public IActionResult RefreshToken()
        {

            CommonFieldsResponseDto<LoginResponseDto> commonFieldsResponseDto = new CommonFieldsResponseDto<LoginResponseDto>();

            User? user;

            // Get the userId as a string from the claims
            string? userIdString = User.FindFirst("userId")?.Value;

            // Try to parse the userIdString to an integer
            if (int.TryParse(userIdString, out int userId))
            {
                user = _entityFramework.Users.FirstOrDefault(u => u.UserId == userId);
                // Continue processing with the user object
#pragma warning disable CS8602 // Dereference of a possibly null reference.
                string? token = _authHelper.CreateToken(user.UserId);
                commonFieldsResponseDto.Success = true;
                commonFieldsResponseDto.Message = "Token Refreshed Successfully.";
                LoginResponseDto loginResponseDto = new LoginResponseDto();
                loginResponseDto.Token = token;
                commonFieldsResponseDto.Response = loginResponseDto;
                commonFieldsResponseDto.ResponseList = null;
                return Ok(commonFieldsResponseDto.GetFilteredResponse());
#pragma warning restore CS8602 // Dereference of a possibly null reference.
            }
            else
            {
                commonFieldsResponseDto.Success = true;
                commonFieldsResponseDto.Message = "Could not refresh token";
                commonFieldsResponseDto.Response = null;
                commonFieldsResponseDto.ResponseList = null;
                return BadRequest(commonFieldsResponseDto.GetFilteredResponse());
            }
        }

        [Authorize] //Protected Route
        [HttpGet("GetLoggedInUserInfo")]
        [SwaggerOperation(Summary = "Get logged-in user information", Description = "Fetches the details of the currently logged-in user.")]
        [SwaggerResponse(StatusCodes.Status200OK, "User information retrieved successfully.", typeof(CommonFieldsResponseDto<RegisterResponseDto>))]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, "User not logged in.")]
        public IActionResult GetLoggedInUserInfo()
        {
            User? user;

            CommonFieldsResponseDto<RegisterResponseDto> commonFieldsResponseDto = new CommonFieldsResponseDto<RegisterResponseDto>();

            // Get the userId as a string from the claims
            string? userIdString = User.FindFirst("userId")?.Value;

            // Try to parse the userIdString to an integer
            if (int.TryParse(userIdString, out int userId))
            {
                user = _entityFramework.Users.FirstOrDefault(u => u.UserId == userId);
                // Continue processing with the user object
#pragma warning disable CS8602 // Dereference of a possibly null reference.
                commonFieldsResponseDto.Success = true;
                commonFieldsResponseDto.Message = "Logged in User Info Fetched Successfully.";
                commonFieldsResponseDto.Response = _mapper.Map<RegisterResponseDto>(user);
                commonFieldsResponseDto.ResponseList = null;
                return Ok(commonFieldsResponseDto.GetFilteredResponse());
#pragma warning restore CS8602 // Dereference of a possibly null reference.
            }
            else
            {
                commonFieldsResponseDto.Success = true;
                commonFieldsResponseDto.Message = "User Not Logged In.";
                commonFieldsResponseDto.Response = null;
                commonFieldsResponseDto.ResponseList = null;
                return Unauthorized(commonFieldsResponseDto.GetFilteredResponse());
            }
        }


    }
}