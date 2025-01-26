using AutoMapper;
using Calendary.Api.Dtos.Admin;
using Calendary.Api.Tools;
using Calendary.Core.Services;
using Calendary.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Calendary.Api.Controllers.Admin;

[Authorize(Roles = "Admin")]
[ApiController]
[Route("api/admin/user")]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IMapper _mapper;

    public UserController(IUserService userService, IMapper mapper)
    {
        _userService = userService;
        _mapper = mapper;
    }

    // GET: api/admin/user
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var users = await _userService.GetAllForAdminAsync();
        var userDtos = _mapper.Map<List<AdminUserDto>>(users);
        return Ok(userDtos);
    }

    // GET: api/admin/user/{identity}
    [HttpGet("{identity}")]
    public async Task<IActionResult> Get(Guid identity)
    {
        var user = await _userService.GetUserByIdentityAsync(identity);
        if (user == null)
        {
            return NotFound();
        }

        var userDto = _mapper.Map<AdminUserDto>(user);
        return Ok(userDto);
    }

    // POST: api/admin/user
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] AdminCreateUserDto userDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var user = _mapper.Map<User>(userDto);


        var createdUser = await _userService.RegisterUserAsync(user, StringExtensions.GenerateRandom());
        var createdUserDto = _mapper.Map<AdminUserDto>(createdUser);

        return CreatedAtAction(nameof(Get), new { id = createdUser.Id }, createdUserDto);
    }

    // PUT: api/admin/user/{identity}
    [HttpPut("{identity}")]
    public async Task<IActionResult> Update(Guid identity, [FromBody] AdminUserDto userDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var user = await _userService.GetUserByIdentityAsync(identity);
        if (user == null)
        {
            return NotFound();
        }

        var updateUser = _mapper.Map<User>(userDto);
        var updatedUser = await _userService.UpdateAsync(user.Id, updateUser);

        var updatedUserDto = _mapper.Map<AdminUserDto>(updatedUser);
        return Ok(updatedUserDto);
    }
}
