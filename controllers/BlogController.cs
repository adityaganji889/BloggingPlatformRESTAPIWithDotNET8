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
[SwaggerTag("Endpoints for managing blogs, including creation, retrieval, updating, and deletion.")]
public class BlogController : ControllerBase
{
    private readonly IBlogService _blogService;

    private readonly IUserService _userService;
    private readonly IMapper _mapper;

    // Constructor with IBlogService injected
    public BlogController(IBlogService blogService, IUserService userService, IMapper mapper)
    {
        _blogService = blogService;
        _userService = userService;
        _mapper = mapper;
    }

    [HttpGet("GetBlogs")]
    [SwaggerOperation(Summary = "Retrieve all blogs", Description = "Fetches a list of all blogs in the system.")]
    [SwaggerResponse(StatusCodes.Status200OK, "Successfully retrieved blogs.", typeof(CommonFieldsResponseDto<BlogResponseDto>))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "No blogs found.")]
    public async Task<IActionResult> GetBlogs()
    {
        CommonFieldsResponseDto<BlogResponseDto> commonFieldsResponseDto = new CommonFieldsResponseDto<BlogResponseDto>();
        IEnumerable<Blog> blogs = await _blogService.GetBlogs();
        List<BlogResponseDto> blogsList = new List<BlogResponseDto>();
        if (blogs.Count() != 0)
        {
            commonFieldsResponseDto.Success = true;
            commonFieldsResponseDto.Message = "All Blogs Fetched Successfully.";
            commonFieldsResponseDto.Response = null;
            commonFieldsResponseDto.ResponseList = _mapper.Map<IEnumerable<BlogResponseDto>>(blogs);
            return Ok(commonFieldsResponseDto.GetFilteredResponse());
        }
        else
        {
            commonFieldsResponseDto.Success = false;
            commonFieldsResponseDto.Message = "No Blogs To Displau.";
            commonFieldsResponseDto.Response = null;
            commonFieldsResponseDto.ResponseList = null;
            return NotFound(commonFieldsResponseDto.GetFilteredResponse());
        }
    }

    [HttpGet("GetBlogs/{authorId}")]
    [SwaggerOperation(Summary = "Retrieve blogs by author", Description = "Fetches all blogs authored by a specific user.")]
    [SwaggerResponse(StatusCodes.Status200OK, "Successfully retrieved author's blogs.", typeof(CommonFieldsResponseDto<BlogResponseDto>))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "No blogs found for this author.")]
    public async Task<IActionResult> GetBlogsByAuthor(int authorId)
    {
        CommonFieldsResponseDto<BlogResponseDto> commonFieldsResponseDto = new CommonFieldsResponseDto<BlogResponseDto>();
        IEnumerable<Blog> blogs = await _blogService.GetBlogsByAuthor(authorId);
        List<BlogResponseDto> blogsList = new List<BlogResponseDto>();
        if (blogs.Count() != 0)
        {
            commonFieldsResponseDto.Success = true;
            commonFieldsResponseDto.Message = "All Blogs of Author with id: " + authorId + " are Fetched Successfully.";
            commonFieldsResponseDto.Response = null;
            commonFieldsResponseDto.ResponseList = _mapper.Map<IEnumerable<BlogResponseDto>>(blogs);
            return Ok(commonFieldsResponseDto.GetFilteredResponse());
        }
        else
        {
            commonFieldsResponseDto.Success = false;
            commonFieldsResponseDto.Message = "No Blogs To Display.";
            commonFieldsResponseDto.Response = null;
            commonFieldsResponseDto.ResponseList = null;
            return NotFound(commonFieldsResponseDto.GetFilteredResponse());
        }
    }

    [HttpGet("GetBlogsOfLoggedInUser")]
    [SwaggerOperation(Summary = "Retrieve blogs of logged-in user", Description = "Fetches all blogs authored by the currently logged-in user.")]
    [SwaggerResponse(StatusCodes.Status200OK, "Successfully retrieved user's blogs.", typeof(CommonFieldsResponseDto<BlogResponseDto>))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "No blogs found for the logged-in user.")]
    public async Task<IActionResult> GetBlogsOfLoggedInUser()
    {
        CommonFieldsResponseDto<BlogResponseDto> commonFieldsResponseDto = new CommonFieldsResponseDto<BlogResponseDto>();
        int authorId = 0;
        string? userIdString = User.FindFirst("userId")?.Value;
        // Try to parse the userIdString to an integer
        if (int.TryParse(userIdString, out int userId))
        {
            authorId = userId;
        }
        IEnumerable<Blog> blogs = await _blogService.GetBlogsByAuthor(authorId);
        List<BlogResponseDto> blogsList = new List<BlogResponseDto>();
        if (blogs.Count() != 0)
        {
            commonFieldsResponseDto.Success = true;
            commonFieldsResponseDto.Message = "All Your Blogs are Fetched Successfully.";
            commonFieldsResponseDto.Response = null;
            commonFieldsResponseDto.ResponseList = _mapper.Map<IEnumerable<BlogResponseDto>>(blogs);
            return Ok(commonFieldsResponseDto.GetFilteredResponse());
        }
        else
        {
            commonFieldsResponseDto.Success = false;
            commonFieldsResponseDto.Message = "No Blogs To Display.";
            commonFieldsResponseDto.Response = null;
            commonFieldsResponseDto.ResponseList = null;
            return NotFound(commonFieldsResponseDto.GetFilteredResponse());
        }
    }

    [HttpGet("GetSingleBlog/{blogId}")]
    [SwaggerOperation(Summary = "Retrieve a single blog", Description = "Fetches details of a blog by its ID.")]
    [SwaggerResponse(StatusCodes.Status200OK, "Successfully retrieved blog details.", typeof(CommonFieldsResponseDto<BlogResponseDto>))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Blog not found.")]
    public async Task<IActionResult> GetSingleBlog(int blogId)
    {
        Blog? blog = await _blogService.GetSingleBlog(blogId);
        CommonFieldsResponseDto<BlogResponseDto> commonFieldsResponseDto = new CommonFieldsResponseDto<BlogResponseDto>();

        if (blog != null)
        {
            commonFieldsResponseDto.Success = true;
            commonFieldsResponseDto.Message = "Blog details with id: " + blogId + " is fetched successfully.";
            commonFieldsResponseDto.ResponseList = null;
            commonFieldsResponseDto.Response = _mapper.Map<BlogResponseDto>(blog);
            return Ok(commonFieldsResponseDto.GetFilteredResponse());
        }
        else
        {
            commonFieldsResponseDto.Success = false;
            commonFieldsResponseDto.Message = "Blog details with id: " + blogId + " is not found.";
            commonFieldsResponseDto.ResponseList = null;
            commonFieldsResponseDto.Response = null;
            return NotFound(commonFieldsResponseDto.GetFilteredResponse());
        }

        throw new Exception("Failed to Get Blog");
    }

    [HttpPost("AddBlog")]
    [SwaggerOperation(Summary = "Add a new blog", Description = "Creates a new blog with the provided details.")]
    [SwaggerResponse(StatusCodes.Status200OK, "Blog added successfully.", typeof(CommonFieldsResponseDto<BlogResponseDto>))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Invalid blog details.")]
    public async Task<IActionResult> AddUser(BlogRequestDto blogRequest)
    {
        Blog blog = new Blog();

        CommonFieldsResponseDto<BlogResponseDto> commonFieldsResponseDto = new CommonFieldsResponseDto<BlogResponseDto>();

        blog.BlogTitle = blogRequest.BlogTitle;
        blog.BlogContent = blogRequest.BlogContent;
        string? userIdString = User.FindFirst("userId")?.Value;
        // Try to parse the userIdString to an integer
        if (int.TryParse(userIdString, out int userId))
        {
            blog.AuthorId = userId;
        }
        blog.BlogCreated = DateTime.Now;
        blog.BlogUpdated = DateTime.Now;
        bool changesDone = false;
        await _blogService.AddEntity<Blog>(blog);
        try
        {
            changesDone = await _blogService.SaveChanges();
            if (changesDone)
            {
                commonFieldsResponseDto.Success = true;
                commonFieldsResponseDto.Message = "New Blog Added Successfully.";
                commonFieldsResponseDto.ResponseList = null;
                commonFieldsResponseDto.Response = null;
                return Ok(commonFieldsResponseDto.GetFilteredResponse());
            }
            else
            {
                return StatusCode(400, "Bad Request");
            }
        }
        catch (Exception ex)
        {
            return StatusCode(500, "Internal server error: " + ex.Message);
        }
    }

    [HttpPut("EditBlog/{blogId}")]
    [SwaggerOperation(Summary = "Edit a blog", Description = "Updates the details of an existing blog.")]
    [SwaggerResponse(StatusCodes.Status200OK, "Blog updated successfully.", typeof(CommonFieldsResponseDto<BlogResponseDto>))]
    [SwaggerResponse(StatusCodes.Status403Forbidden, "You are not authorized to edit this blog.")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Blog not found.")]
    public async Task<IActionResult> EditBlog(int blogId, BlogRequestDto blogRequest)
    {
        Blog? blogDb = await _blogService.GetSingleBlog(blogId);

        CommonFieldsResponseDto<BlogResponseDto> commonFieldsResponseDto = new CommonFieldsResponseDto<BlogResponseDto>();

        int loggedInUserId = 0;
        string? userIdString = User.FindFirst("userId")?.Value;
        // Try to parse the userIdString to an integer
        if (int.TryParse(userIdString, out int userId))
        {
            loggedInUserId = userId;
        }

        User? loggedInUser = await _userService.GetSingleUser(loggedInUserId);

        if (blogDb != null && (loggedInUser.Admin || loggedInUser.UserId.Equals(loggedInUser.UserId)))
        {
            blogDb.BlogTitle = blogRequest.BlogTitle;
            blogDb.BlogContent = blogRequest.BlogContent;
            blogDb.BlogUpdated = DateTime.Now;
            bool changesDone = false;
            try
            {
                changesDone = await _blogService.SaveChanges();
                if (changesDone)
                {
                    commonFieldsResponseDto.Success = true;
                    commonFieldsResponseDto.Message = "Blog details with id:" + blogId + " is Updated Successfully.";
                    commonFieldsResponseDto.ResponseList = null;
                    commonFieldsResponseDto.Response = _mapper.Map<BlogResponseDto>(blogDb);
                    return Ok(commonFieldsResponseDto.GetFilteredResponse());
                }
                else
                {
                    return StatusCode(400, "Bad Request");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }
        else
        {
            return StatusCode(403, "Forbidden: You're trying to access to edit some other author blog.");
        }
    }

    [HttpDelete("DeleteBlog/{blogId}")]
    [SwaggerOperation(Summary = "Delete a blog", Description = "Removes a blog from the system.")]
    [SwaggerResponse(StatusCodes.Status200OK, "Blog deleted successfully.")]
    [SwaggerResponse(StatusCodes.Status403Forbidden, "You are not authorized to delete this blog.")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Blog not found.")]
    public async Task<IActionResult> DeleteBlog(int blogId)
    {
        Blog? blogDb = await _blogService.GetSingleBlog(blogId);

        CommonFieldsResponseDto<BlogResponseDto> commonFieldsResponseDto = new CommonFieldsResponseDto<BlogResponseDto>();

        int loggedInUserId = 0;
        string? userIdString = User.FindFirst("userId")?.Value;
        // Try to parse the userIdString to an integer
        if (int.TryParse(userIdString, out int userId))
        {
            loggedInUserId = userId;
        }

        User? loggedInUser = await _userService.GetSingleUser(loggedInUserId);

        if (blogDb != null && (loggedInUser.Admin || loggedInUser.UserId.Equals(loggedInUser.UserId)))
        {
            bool changesDone = false;
            try
            {
                await _blogService.RemoveEntity<Blog>(blogDb);
                changesDone = await _blogService.SaveChanges();
                if (changesDone)
                {
                    commonFieldsResponseDto.Success = true;
                    commonFieldsResponseDto.Message = "Blog details with id: " + blogId + " is deleted Successfully.";
                    commonFieldsResponseDto.ResponseList = null;
                    commonFieldsResponseDto.Response = null;
                    return Ok(commonFieldsResponseDto.GetFilteredResponse());
                }
                else
                {
                    return StatusCode(400, "Bad Request");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }
        else
        {
            return StatusCode(403, "Forbidden: You're trying to access to delete some other author blog.");
        }
    }
}
