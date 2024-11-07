using AutoMapper;
using Calendary.Api.Dtos;
using Calendary.Core.Services;
using Calendary.Model;
using Microsoft.AspNetCore.Mvc;

namespace Calendary.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IAuthService _authService;
    private readonly IMapper _mapper;

    public UsersController(IUserService userService, IAuthService authService, IMapper mapper)
    {
        _userService = userService;
        _authService = authService;
        _mapper = mapper;
    }

    [HttpPost("register")]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(UserDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]

    public async Task<IActionResult> Register([FromBody] UserLoginDto userLoginDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var user = _mapper.Map<User>(userLoginDto);
        var newUser = await _userService.RegisterUserAsync(user, userLoginDto.Password);
        var result = _mapper.Map<UserDto>(newUser);
        result.Token = await _authService.GenerateJwtTokenAsync(user);
        return Created("", result);
    }


    [HttpPost("login")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(UserDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] UserLoginDto userLoginDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var user = await _userService.LoginAsync(userLoginDto.Email, userLoginDto.Password);
        if (user is null)
        {
            return Unauthorized();
        }

        var userDto = _mapper.Map<UserDto>(user);
        userDto.Token = await _authService.GenerateJwtTokenAsync(user);
        return Ok(userDto);
    }
}