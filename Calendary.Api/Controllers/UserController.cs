using AutoMapper;
using Calendary.Api.Dtos;
using Calendary.Core;
using Calendary.Model;
using Microsoft.AspNetCore.Mvc;

namespace Calendary.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IMapper _mapper;

        public UsersController(IUserService userService, IMapper mapper)
        {
            _userService = userService;
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
            return Created("", result);
        }
    }
}