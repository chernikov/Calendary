using AutoMapper;
using Calendary.Api.Dtos;
using Calendary.Core.Services;
using Calendary.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Calendary.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController : BaseUserController
{
    private readonly IAuthService _authService;
    private readonly IEmailService _emailService;
    private readonly IMapper _mapper;

    public UserController(IUserService userService, IAuthService authService, IEmailService emailService, IMapper mapper) : base(userService)
    {
        _authService = authService;
        _emailService = emailService;
        _mapper = mapper;
    }

    [HttpPost("register")]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(UserDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] UserRegisterDto userLoginDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var existingUser = await _userService.GetUserByEmailAsync(userLoginDto.Email);
        if (existingUser != null)
        {
            var message = new { message = "Користувач з таким email вже існує." };
            return BadRequest(message);
        }

        var user = _mapper.Map<User>(userLoginDto);
        var newUser = await _userService.RegisterUserAsync(user, userLoginDto.Password);

        await SendVerificationCode(newUser);

        var result = _mapper.Map<UserDto>(newUser);
        result.Token = await _authService.GenerateJwtTokenAsync(newUser);
        return Created("", result);
    }

    private async Task SendVerificationCode(User newUser)
    {
        // Generate email confirmation code
        var verificationCode = new Random().Next(100000, 999999).ToString();
        await _userService.CreateVerificationEmailCodeAsync(newUser.Id, verificationCode, DateTime.UtcNow.AddHours(24));

        // Send email confirmation
        // Assuming you have an email service to send emails
        await _emailService.SendVerificationEmailAsync(newUser.Email!, verificationCode);
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


    [HttpGet]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(UserInfoDto))]
    public async Task<IActionResult> Get()
    {

        var user = await CurrentUser.Value;
        if (user is null)
        {
            return Unauthorized();
        }
        var result = _mapper.Map<UserInfoDto>(user);
        return Ok(result);
    }


    [HttpPut]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(UserDto))]
    public async Task<IActionResult> Update([FromBody] UserInfoDto userInfoDto)
    {
        var user = await CurrentUser.Value;
        if (user is null)
        {
            return Unauthorized();
        }

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        if (!string.IsNullOrEmpty(userInfoDto.Email))
        {
            var isValidEmail = await _userService.ValidateEmailAsync(user.Id, userInfoDto.Email);
            if (!isValidEmail)
            {
                var message = new { message = "Користувач з таким email вже є" };
                return BadRequest(message);
            }
        }
        
        var entity = _mapper.Map<User>(userInfoDto);
        var updatedUser = await _userService.UpdateAsync(user.Id, entity);

        var result = _mapper.Map<UserDto>(updatedUser);
        result.Token = await _authService.GenerateJwtTokenAsync(user);
        return Ok(result);
    }
}