using System.Data;
using System.Net.Mail;
using System.Security.Cryptography;
using AutoMapper;
using BloggingPlatform.config;
using BloggingPlatform.dtos;
using BloggingPlatform.helpers;
using BloggingPlatform.models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;
using System.Text.Json; //Render SMTP Mail changes

namespace BloggingPlatform.controllers
{
    [ApiController]
    [Route("[controller]")]
    [SwaggerTag("Endpoints for user authentication and management.")]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly DataContextEF _entityFramework;
        private readonly AuthHelper _authHelper;
        private readonly IMapper _mapper;
        private readonly SmtpClient _smtpClient;
        private readonly HttpClient _httpClient; //Render SMTP Mail changes

        public AuthController(IConfiguration config, IMapper mapper, SmtpClient smtpClient, HttpClient httpClient) //Render SMTP Mail changes
        {
            _config = config;
            _entityFramework = new DataContextEF(config);
            _authHelper = new AuthHelper(config);
            _mapper = mapper;
            _smtpClient = smtpClient;
            _httpClient = httpClient; //Render SMTP Mail changes
        }

        [AllowAnonymous]
        [HttpPost("Register")]
        [SwaggerOperation(Summary = "Register a new user", Description = "Creates a new user account with the provided registration details.")]
        [SwaggerResponse(StatusCodes.Status200OK, "User registered successfully.", typeof(CommonFieldsResponseDto<RegisterResponseDto>))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid registration details.")]
        public async Task<IActionResult> Register(RegisterRequestDto userForRegistration)
        {
            CommonFieldsResponseDto<RegisterResponseDto> commonFieldsResponseDto = new CommonFieldsResponseDto<RegisterResponseDto>();

            if (userForRegistration.Password == userForRegistration.PasswordConfirm)
            {
                User? user = await _entityFramework.Users
                .Where(u => u.Email == userForRegistration.Email)
                .FirstOrDefaultAsync<User>();
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
                    newUser.Gender = userForRegistration.Gender == "Male" ? true : false;
                    newUser.Active = false;
                    newUser.Admin = false;
                    newUser.UserCreated = DateTime.Now;
                    newUser.UserUpdated = DateTime.Now;
                    await _entityFramework.AddAsync(newUser);

                    if (await _entityFramework.SaveChangesAsync() > 0)
                    {
                        int newUserId = newUser.UserId;
                        Otp newOtp = new Otp();
                        var otp = new Random().Next(100000, 999999).ToString(); // Generate a 6-digit OTP
                        newOtp.UserId = newUserId;
                        newOtp.OtpValue = otp; // Assuming you have an OTP field in your User entity
                        newOtp.OtpCreated = DateTime.Now;
                        newOtp.OtpUpdated = DateTime.Now;
                        newOtp.OtpExpiration = DateTime.Now.AddMinutes(10); // OTP valid for 10 minutes
                        await _entityFramework.AddAsync(newOtp);
                        // Save user with the new OTP
                        if (await _entityFramework.SaveChangesAsync() > 0)
                        {
                            var smtpSettings = _config.GetSection("SmtpSettings");

                            string? email = smtpSettings["User"];
                            if (string.IsNullOrEmpty(email))
                            {
                                throw new InvalidOperationException("SMTP User email is not configured.");
                            }
                            // Send the OTP via email
                            // var mailMessage = new MailMessage
                            // {
                            //     From = new MailAddress(email), // Replace with your sender email
                            //     Subject = "Verify Email For Blogging API Auth",
                            //     Body = "Your OTP to verify email is: " + otp + ", will expire in next 10 mins.",
                            //     IsBodyHtml = true,
                            // };

                            // mailMessage.To.Add(userForRegistration.Email);

                            // await _smtpClient.SendMailAsync(mailMessage);
                            //Render SMTP Mail changes
                            var apiUrl = _config["EmailApi:Url"];

                            var requestBody = new
                            {
                                user = new
                                {
                                    _id = newUser.UserId,
                                    email = userForRegistration.Email
                                },
                                otp = otp,
                                mailType = "verifyemailotp",
                                appType = ".NET8 Blogging API Auth"
                            };

                            var jsonContent = new StringContent(
                                JsonSerializer.Serialize(requestBody),
                                System.Text.Encoding.UTF8,
                                "application/json"
                            );

                            var request = new HttpRequestMessage(HttpMethod.Post, apiUrl);
                            request.Content = jsonContent;

                            var response = await _httpClient.SendAsync(request);

                            // Read the response content as string
                            var responseContent = await response.Content.ReadAsStringAsync();

                            if (!response.IsSuccessStatusCode)
                            {
                                var error = await response.Content.ReadAsStringAsync();
                                throw new Exception($"Failed to send OTP. Response: {error}");
                            }

                            // Parse JSON without a DTO
                            var jsonDoc = JsonDocument.Parse(responseContent);
                            var root = jsonDoc.RootElement;

                            // Check if 'success' is true
                            bool success = root.GetProperty("success").GetBoolean();
                            if (!success)
                            {
                                string message = root.GetProperty("message").GetString() ?? "No message returned";
                                throw new Exception($"Email API failed: {message}");
                            }

                            // Extract the OTP token from 'data'
                            string token = root.GetProperty("data").GetString() ?? "";
                            if (string.IsNullOrEmpty(token))
                            {
                                throw new Exception("Token not returned from email API.");
                            }

                            //At this point, 'token' contains the OTP or token from API
                            Console.WriteLine($"Token received from 3rd Party API: {token}");

                            //Render SMTP Mail changes

                            commonFieldsResponseDto.Success = true;
                            commonFieldsResponseDto.Message = "New User is Registered and OTP is sent to email: " + userForRegistration.Email + " Successfully.";
                            commonFieldsResponseDto.Response = _mapper.Map<RegisterResponseDto>(newUser);
                            commonFieldsResponseDto.ResponseList = null;
                            return Ok(commonFieldsResponseDto.GetFilteredResponse());
                        }

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
        [HttpPut("VerifyEmail")]
        [SwaggerOperation(Summary = "Verify Email Of Newly Registered User/Newly Updated Email", Description = "Updates the user's active status to true based on email verification.")]
        [SwaggerResponse(StatusCodes.Status200OK, "User email verified successfully.")]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid user email details.")]
        public async Task<IActionResult> VerifyEmail(VerifyEmailRequestDto userForRegistration)
        {
            CommonFieldsResponseDto<RegisterResponseDto> commonFieldsResponseDto = new CommonFieldsResponseDto<RegisterResponseDto>();

            User? user = await _entityFramework.Users
            .Where(u => u.Email == userForRegistration.Email)
            .FirstOrDefaultAsync<User>();
            if (user != null)
            {
                // Check OTP validity
                Otp? otp = await _entityFramework.Otps.Where(o => o.UserId == user.UserId).FirstOrDefaultAsync<Otp>();
                // otp.UserId != user.UserId || otp.OtpValue != userForRegistration.OtpValue || otp.OtpExpiration < DateTime.Now
#pragma warning disable CS8602 // Dereference of a possibly null reference.
                if (otp.UserId != user.UserId || otp.OtpValue != userForRegistration.OtpValue || otp.OtpExpiration < DateTime.Now)
                {
                    return BadRequest("OTP validation failed.");
                }
#pragma warning restore CS8602 // Dereference of a possibly null reference.

                user.Active = true;
                user.UserUpdated = DateTime.Now;

                _entityFramework.Remove(otp);

                if (await _entityFramework.SaveChangesAsync() > 0)
                {
                    commonFieldsResponseDto.Success = true;
                    commonFieldsResponseDto.Message = "User with Email: " + user.Email + " is verified successfully.";
                    commonFieldsResponseDto.Response = null;
                    commonFieldsResponseDto.ResponseList = null;
                    return Ok(commonFieldsResponseDto.GetFilteredResponse());
                }
                throw new Exception("Failed to verify email.");
            }
            else
            {
                throw new Exception("User with this email doesnot exists!");
            }
            throw new Exception("Failed to verify email.");
        }

        [AllowAnonymous]
        [HttpPost("Login")]
        [SwaggerOperation(Summary = "User login", Description = "Authenticates a user and returns an authentication token.")]
        [SwaggerResponse(StatusCodes.Status200OK, "Login successful, token generated.", typeof(CommonFieldsResponseDto<LoginResponseDto>))]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, "Invalid credentials.")]
        [SwaggerResponse(StatusCodes.Status404NotFound, "User not found.")]
        public async Task<IActionResult> Login(LoginRequestDto userForLogin)
        {


            User? userForConfirmation = await _entityFramework.Users
                .Where(u => u.Email == userForLogin.Email)
                .FirstOrDefaultAsync<User>();

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
        public async Task<IActionResult> ResetPassword(ResetPasswordRequestDto userForRegistration)
        {
            CommonFieldsResponseDto<RegisterResponseDto> commonFieldsResponseDto = new CommonFieldsResponseDto<RegisterResponseDto>();
            if (userForRegistration.Password == userForRegistration.PasswordConfirm)
            {
                User? user = await _entityFramework.Users
                .Where(u => u.Email == userForRegistration.Email)
                .FirstOrDefaultAsync<User>();
                if (user != null)
                {
                    // Check OTP validity
                    Otp? otp = await _entityFramework.Otps.Where(o => o.UserId == user.UserId).FirstOrDefaultAsync<Otp>();
                    // otp.UserId != user.UserId || otp.OtpValue != userForRegistration.OtpValue || otp.OtpExpiration < DateTime.Now
#pragma warning disable CS8602 // Dereference of a possibly null reference.
                    if (otp.UserId != user.UserId || otp.OtpValue != userForRegistration.OtpValue || otp.OtpExpiration < DateTime.Now)
                    {
                        return BadRequest("OTP validation failed.");
                    }
#pragma warning restore CS8602 // Dereference of a possibly null reference.

                    byte[] passwordSalt = new byte[128 / 8];
                    using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
                    {
                        rng.GetNonZeroBytes(passwordSalt);
                    }

                    byte[] passwordHash = _authHelper.GetPasswordHash(userForRegistration.Password, passwordSalt);


                    user.PasswordHash = passwordHash;
                    user.PasswordSalt = passwordSalt;
                    user.UserUpdated = DateTime.Now;

                    _entityFramework.Remove(otp);

                    if (await _entityFramework.SaveChangesAsync() > 0)
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

        // Your GenerateOtp method
        [AllowAnonymous]
        [HttpPost("GenerateOtp")]
        [SwaggerOperation(Summary = "Generate OTP for password reset/verify newly updated email", Description = "Sends an OTP to the user's email for password reset/newly updated email verification.")]
        [SwaggerResponse(StatusCodes.Status200OK, "OTP sent successfully.")]
        [SwaggerResponse(StatusCodes.Status404NotFound, "User not found.")]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid request parameters.")]
        public async Task<IActionResult> GenerateOtp([FromBody] OtpRequestDto otpRequest)
        {
            CommonFieldsResponseDto<string> commonFieldsResponseDto = new CommonFieldsResponseDto<string>();

            var user = await _entityFramework.Users
                .Where(u => u.Email == otpRequest.Email)
                .FirstOrDefaultAsync();

            if (user == null)
            {
                commonFieldsResponseDto.Success = false;
                commonFieldsResponseDto.Message = "User with this Email Doesnot Exists.";
                commonFieldsResponseDto.Response = null;
                commonFieldsResponseDto.ResponseList = null;
                return NotFound(commonFieldsResponseDto.GetFilteredResponse());
            }
            // Fetch all old OTPs for the user
            List<Otp> existingOtps = await _entityFramework.Otps
                .Where(o => o.UserId == user.UserId)
                .ToListAsync();

            if (existingOtps.Any())
            {
                foreach (var otpvals in existingOtps)
                {
                    _entityFramework.Remove(otpvals);
                }
                await _entityFramework.SaveChangesAsync();
            }

            Otp newOtp = new Otp();
            var otp = new Random().Next(100000, 999999).ToString(); // Generate a 6-digit OTP
            newOtp.UserId = user.UserId;
            newOtp.OtpValue = otp; // Assuming you have an OTP field in your User entity
            newOtp.OtpCreated = DateTime.Now;
            newOtp.OtpUpdated = DateTime.Now;
            newOtp.OtpExpiration = DateTime.Now.AddMinutes(10); // OTP valid for 10 minutes
            await _entityFramework.AddAsync(newOtp);
            // Save user with the new OTP
            await _entityFramework.SaveChangesAsync();

            var smtpSettings = _config.GetSection("SmtpSettings");

            string? email = smtpSettings["User"];

            if (string.IsNullOrEmpty(email))
            {
                throw new InvalidOperationException("SMTP User email is not configured.");
            }
            // Send the OTP via email
            // var mailMessage = new MailMessage
            // {
            //     From = new MailAddress(email), // Replace with your sender email
            //     Subject = "Password Reset/Verify Newly Updated Email For Blogging API Auth",
            //     Body = "Your OTP for reset password/verify newly updated email is: " + otp + ", will expire in next 10 mins.",
            //     IsBodyHtml = true,
            // };

            // mailMessage.To.Add(user.Email);

            // await _smtpClient.SendMailAsync(mailMessage);

            //Render SMTP Mail changes
                            var apiUrl = _config["EmailApi:Url"];

                            var requestBody = new
                            {
                                user = new
                                {
                                    _id = user.UserId,
                                    email = user.Email
                                },
                                otp = otp,
                                mailType = "generateOTP",
                                appType = ".NET8 Blogging API Auth".Trim()
                            };

                            var jsonString = JsonSerializer.Serialize(requestBody);
                            Console.WriteLine("JSON being sent to external API:");
                            Console.WriteLine(jsonString);

                            var jsonContent = new StringContent(
                                jsonString,
                                System.Text.Encoding.UTF8,
                                "application/json"
                            );

                            var request = new HttpRequestMessage(HttpMethod.Post, apiUrl);
                            Console.WriteLine($"request: {request}");
                            request.Content = jsonContent;
                            Console.WriteLine($"request.Content: {jsonContent}");

                            var response = await _httpClient.SendAsync(request);

                            // Read the response content as string
                            var responseContent = await response.Content.ReadAsStringAsync();

                            if (!response.IsSuccessStatusCode)
                            {
                                var error = await response.Content.ReadAsStringAsync();
                                throw new Exception($"Failed to send OTP. Response: {error}");
                            }

                            // Parse JSON without a DTO
                            var jsonDoc = JsonDocument.Parse(responseContent);
                            Console.WriteLine($"jsonDoc: {jsonDoc}");
                            var root = jsonDoc.RootElement;
                            Console.WriteLine($"Root element: {root}");

                            // Check if 'success' is true
                            bool success = root.GetProperty("success").GetBoolean();
                            if (!success)
                            {
                                string message = root.GetProperty("message").GetString() ?? "No message returned";
                                throw new Exception($"Email API failed: {message}");
                            }

                            // Extract the OTP token from 'data'
                            string token = root.GetProperty("data").GetString() ?? "";
                            if (string.IsNullOrEmpty(token))
                            {
                                throw new Exception("Token not returned from email API.");
                            }

                            //At this point, 'token' contains the OTP or token from API
                            Console.WriteLine($"Token received from 3rd Party API: {token}");

                            //Render SMTP Mail changes

            commonFieldsResponseDto.Success = true;
            commonFieldsResponseDto.Message = "OTP to email:" + user.Email + " is sent successfully.";
            commonFieldsResponseDto.Response = null;
            commonFieldsResponseDto.ResponseList = null;

            return Ok(commonFieldsResponseDto.GetFilteredResponse());
        }

        [Authorize] //Protected Route
        [HttpGet("RefreshToken")]
        [SwaggerOperation(Summary = "Refresh authentication token", Description = "Generates a new token for the authenticated user.")]
        [SwaggerResponse(StatusCodes.Status200OK, "Token refreshed successfully.", typeof(CommonFieldsResponseDto<LoginResponseDto>))]
        [SwaggerResponse(StatusCodes.Status401Unauthorized, "User not authenticated.")]
        public async Task<IActionResult> RefreshToken()
        {

            CommonFieldsResponseDto<LoginResponseDto> commonFieldsResponseDto = new CommonFieldsResponseDto<LoginResponseDto>();

            User? user;

            // Get the userId as a string from the claims
            string? userIdString = User.FindFirst("userId")?.Value;

            // Try to parse the userIdString to an integer
            if (int.TryParse(userIdString, out int userId))
            {
                user = await _entityFramework.Users.FirstOrDefaultAsync(u => u.UserId == userId);
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
        public async Task<IActionResult> GetLoggedInUserInfo()
        {
            User? user;

            CommonFieldsResponseDto<RegisterResponseDto> commonFieldsResponseDto = new CommonFieldsResponseDto<RegisterResponseDto>();

            // Get the userId as a string from the claims
            string? userIdString = User.FindFirst("userId")?.Value;

            // Try to parse the userIdString to an integer
            if (int.TryParse(userIdString, out int userId))
            {
                user = await _entityFramework.Users.FirstOrDefaultAsync(u => u.UserId == userId);
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