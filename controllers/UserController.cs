using AutoMapper;
using BloggingPlatform.dtos;
using BloggingPlatform.models;
using BloggingPlatform.services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace BloggingPlatform.Controllers;


[Authorize]
[ApiController]
[Route("[controller]")]
[SwaggerTag("Endpoints for managing users, including retrieval, modification, and role management.")]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;

    private readonly IMapper _mapper;

    public UserController(IUserService userService, IMapper mapper)
    {
        _userService = userService;
        _mapper = mapper;
    }

    [HttpGet("GetUsers")]
    [SwaggerOperation(Summary = "Retrieve all users", Description = "Fetches a list of all users in the system.")]
    [SwaggerResponse(StatusCodes.Status200OK, "Successfully retrieved users.", typeof(CommonFieldsResponseDto<RegisterResponseDto>))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "No users found.")]
    public async Task<IActionResult> GetUsers()
    {
        IEnumerable<User> users = await _userService.GetUsers();
        CommonFieldsResponseDto<RegisterResponseDto> commonFieldsResponseDto = new CommonFieldsResponseDto<RegisterResponseDto>();
        if (users?.Count() != 0)
        {
            commonFieldsResponseDto.Success = true;
            commonFieldsResponseDto.Message = "All Users Details Fetched Successfully.";
            commonFieldsResponseDto.Response = null;
            commonFieldsResponseDto.ResponseList = _mapper.Map<IEnumerable<RegisterResponseDto>>(users);
            return Ok(commonFieldsResponseDto.GetFilteredResponse());
        }
        else
        {
            commonFieldsResponseDto.Success = false;
            commonFieldsResponseDto.Message = "No Users To Display.";
            commonFieldsResponseDto.Response = null;
            commonFieldsResponseDto.ResponseList = null;
            return NotFound(commonFieldsResponseDto.GetFilteredResponse());
        }
    }

    [HttpGet("GetSingleUser/{userId}")]
    [SwaggerOperation(Summary = "Retrieve a single user", Description = "Fetches details of a user by their ID.")]
    [SwaggerResponse(StatusCodes.Status200OK, "Successfully retrieved user details.", typeof(CommonFieldsResponseDto<RegisterResponseDto>))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "User not found.")]
    public async Task<IActionResult> GetSingleUser(int userId)
    {
        User? user = await _userService.GetSingleUser(userId);

        CommonFieldsResponseDto<RegisterResponseDto> commonFieldsResponseDto = new CommonFieldsResponseDto<RegisterResponseDto>();

        if (user != null)
        {
            commonFieldsResponseDto.Success = true;
            commonFieldsResponseDto.Message = "User Details with id: " + user.UserId + " is Fetched Successfully.";
            commonFieldsResponseDto.Response = _mapper.Map<RegisterResponseDto>(user);
            commonFieldsResponseDto.ResponseList = null;
            return Ok(commonFieldsResponseDto.GetFilteredResponse());
        }
        else
        {
            commonFieldsResponseDto.Success = false;
            commonFieldsResponseDto.Message = "User Details with id: " + userId + " is Not Found.";
            commonFieldsResponseDto.Response = null;
            commonFieldsResponseDto.ResponseList = null;
            return NotFound(commonFieldsResponseDto.GetFilteredResponse());
        }

        throw new Exception("Failed to Get User");
    }

    [HttpPut("EditUser")]
    [SwaggerOperation(Summary = "Edit user details", Description = "Updates the details of a user.")]
    [SwaggerResponse(StatusCodes.Status200OK, "Successfully updated user details.", typeof(CommonFieldsResponseDto<RegisterResponseDto>))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "User not found.")]
    public async Task<IActionResult> EditUser(RegisterRequestDto user, [FromQuery] int? userId)
    {
        User? userDb = null;

        CommonFieldsResponseDto<RegisterResponseDto> commonFieldsResponseDto = new CommonFieldsResponseDto<RegisterResponseDto>();

        int loggedInUserId = 0;
        string? userIdString = User.FindFirst("userId")?.Value;
        // Try to parse the userIdString to an integer
        if (int.TryParse(userIdString, out int loggedUserId))
        {
            loggedInUserId = loggedUserId;
        }

        User? loggedInUser = await _userService.GetSingleUser(loggedInUserId);

        if (userId != null)
        {
            userDb = await _userService.GetSingleUser(userId.Value);
        }
        else
        {
            userDb = await _userService.GetSingleUser(loggedInUserId);
        }

        if (userDb != null && (loggedInUser.Admin || userDb.UserId.Equals(loggedInUser.UserId)))
        {
            userDb.FirstName = user.FirstName;
            userDb.LastName = user.LastName;
            userDb.Email = user.Email;
            userDb.Gender = user.Gender == "Male" ? true : false;
            userDb.UserUpdated = DateTime.Now;
            bool changesDone = false;
            try
            {
                changesDone = await _userService.SaveChanges();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            if (changesDone)
            {
                commonFieldsResponseDto.Success = true;
                if (userDb.UserId.Equals(loggedInUser.UserId))
                {
                    commonFieldsResponseDto.Message = "Your User Profile with id: " + loggedInUser.UserId + " is Updated Successfully.";
                }
                else
                {
                    commonFieldsResponseDto.Message = "User Details with id: " + userId + " is Updated Successfully.";
                }
                commonFieldsResponseDto.Response = _mapper.Map<RegisterResponseDto>(userDb);
                commonFieldsResponseDto.ResponseList = null;
                return Ok(commonFieldsResponseDto.GetFilteredResponse());
            }
            throw new Exception("Failed to Update User");
        }
        else
        {
            commonFieldsResponseDto.Success = false;
            commonFieldsResponseDto.Message = "User Details with id: " + userId + " is Not Found.";
            commonFieldsResponseDto.Response = null;
            commonFieldsResponseDto.ResponseList = null;
            return NotFound(commonFieldsResponseDto.GetFilteredResponse());
        }

        throw new Exception("Failed to Get User");
    }

    [HttpDelete("DeleteUser/{userId}")]
    [SwaggerOperation(Summary = "Delete a user", Description = "Removes a user from the system.")]
    [SwaggerResponse(StatusCodes.Status200OK, "Successfully deleted user.")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "User not found.")]
    public async Task<IActionResult> DeleteUser(int userId)
    {
        User? userDb = await _userService.GetSingleUser(userId);

        CommonFieldsResponseDto<RegisterResponseDto> commonFieldsResponseDto = new CommonFieldsResponseDto<RegisterResponseDto>();

        int loggedInUserId = 0;
        string? userIdString = User.FindFirst("userId")?.Value;
        // Try to parse the userIdString to an integer
        if (int.TryParse(userIdString, out int loggedUserId))
        {
            loggedInUserId = loggedUserId;
        }

        User? loggedInUser = await _userService.GetSingleUser(loggedInUserId);

        if (userDb != null && (loggedInUser.Admin || loggedInUserId != userId))
        {
            await _userService.RemoveEntity<User>(userDb);

            bool changesDone = false;
            try
            {
                changesDone = await _userService.SaveChanges();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            if (changesDone)
            {
                commonFieldsResponseDto.Success = true;
                commonFieldsResponseDto.Message = "User Details with id: " + userId + " is Deleted Successfully.";
                commonFieldsResponseDto.Response = null;
                commonFieldsResponseDto.ResponseList = null;
                return Ok(commonFieldsResponseDto.GetFilteredResponse());
            }

            throw new Exception("Failed to Delete User");
        }
        else
        {
            commonFieldsResponseDto.Success = false;
            commonFieldsResponseDto.Message = "User Details with id: " + userId + " is Not Found.";
            commonFieldsResponseDto.Response = null;
            commonFieldsResponseDto.ResponseList = null;
            return NotFound(commonFieldsResponseDto.GetFilteredResponse());
        }

        throw new Exception("Failed to Get User");
    }

    [HttpPut("ToggleUserAdminRights/{userId}")]
    [SwaggerOperation(Summary = "Toggle admin rights", Description = "Enables or disables admin rights for a user.")]
    [SwaggerResponse(StatusCodes.Status200OK, "Successfully toggled admin rights.")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "User not found.")]
    public async Task<IActionResult> ToggleUserAdminRights(int userId)
    {
        User? userDb = await _userService.GetSingleUser(userId);

        CommonFieldsResponseDto<RegisterResponseDto> commonFieldsResponseDto = new CommonFieldsResponseDto<RegisterResponseDto>();

        int loggedInUserId = 0;
        string? userIdString = User.FindFirst("userId")?.Value;
        // Try to parse the userIdString to an integer
        if (int.TryParse(userIdString, out int loggedUserId))
        {
            loggedInUserId = loggedUserId;
        }

        User? loggedInUser = await _userService.GetSingleUser(loggedInUserId);

        if (userDb != null && (loggedInUser.Admin || loggedInUserId != userId))
        {
            userDb.Admin = !userDb.Admin;
            userDb.UserUpdated = DateTime.Now;
            bool changesDone = false;
            try
            {
                changesDone = await _userService.SaveChanges();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            if (changesDone)
            {
                commonFieldsResponseDto.Success = true;
                commonFieldsResponseDto.Message = "Admin Rights of id: " + userId + " is Updated Successfully.";
                commonFieldsResponseDto.Response = _mapper.Map<RegisterResponseDto>(userDb);
                commonFieldsResponseDto.ResponseList = null;
                return Ok(commonFieldsResponseDto.GetFilteredResponse());
            }

            throw new Exception("Failed to User Admin Rights.");
        }
        else
        {
            commonFieldsResponseDto.Success = false;
            commonFieldsResponseDto.Message = "User Details with id: " + userId + " is Not Found.";
            commonFieldsResponseDto.Response = null;
            commonFieldsResponseDto.ResponseList = null;
            return NotFound(commonFieldsResponseDto.GetFilteredResponse());
        }

        throw new Exception("Failed to Get User");
    }

    [HttpPut("ToggleUserActiveRights/{userId}")]
    [SwaggerOperation(Summary = "Toggle user active status", Description = "Enables or disables a user's active status.")]
    [SwaggerResponse(StatusCodes.Status200OK, "Successfully toggled active status.")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "User not found.")]
    public async Task<IActionResult> ToggleUserActiveRights(int userId)
    {
        User? userDb = await _userService.GetSingleUser(userId);

        CommonFieldsResponseDto<RegisterResponseDto> commonFieldsResponseDto = new CommonFieldsResponseDto<RegisterResponseDto>();

        int loggedInUserId = 0;
        string? userIdString = User.FindFirst("userId")?.Value;
        // Try to parse the userIdString to an integer
        if (int.TryParse(userIdString, out int loggedUserId))
        {
            loggedInUserId = loggedUserId;
        }

        User? loggedInUser = await _userService.GetSingleUser(loggedInUserId);

        if (userDb != null && (loggedInUser.Admin || loggedInUserId != userId))
        {
            userDb.Active = !userDb.Active;
            userDb.UserUpdated = DateTime.Now;
            bool changesDone = false;
            try
            {
                changesDone = await _userService.SaveChanges();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            if (changesDone)
            {
                commonFieldsResponseDto.Success = true;
                commonFieldsResponseDto.Message = "User Active Rights of id: " + userId + " is Updated Successfully.";
                commonFieldsResponseDto.Response = _mapper.Map<RegisterResponseDto>(userDb);
                commonFieldsResponseDto.ResponseList = null;
                return Ok(commonFieldsResponseDto.GetFilteredResponse());
            }

            throw new Exception("Failed to Update User Active Status.");
        }
        else
        {
            commonFieldsResponseDto.Success = false;
            commonFieldsResponseDto.Message = "User Details with id: " + userId + " is Not Found.";
            commonFieldsResponseDto.Response = null;
            commonFieldsResponseDto.ResponseList = null;
            return NotFound(commonFieldsResponseDto.GetFilteredResponse());
        }

        throw new Exception("Failed to Get User");
    }
}
