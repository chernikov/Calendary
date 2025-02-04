﻿using Microsoft.AspNetCore.Mvc;
using Calendary.Model;
using Calendary.Core.Services;
using AutoMapper;
using Calendary.Api.Dtos;

namespace Calendary.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ImageController : BaseUserController
{
    private readonly string _imageDirectory = "wwwroot/uploads";
    private readonly IImageService imageService;
    private readonly ICalendarService calendarService;
    private readonly IMapper mapper;

    public ImageController(IUserService userService, 
        IImageService imageService, 
        ICalendarService calendarService, 
        IMapper mapper) : base(userService)
    {
        if (!Directory.Exists(_imageDirectory))
        {
            Directory.CreateDirectory(_imageDirectory);
        }

        this.imageService = imageService;
        this.calendarService = calendarService;
        this.mapper = mapper;
    }


    [HttpGet("{calendarId}")]
    public async Task<IActionResult> GetImagesByCalendarId(int calendarId)
    {
        var user = await CurrentUser.Value;

        if (user is null)
        {
            return Unauthorized();
        }
        var calendar = await calendarService.GetByIdAsync(calendarId);
        if (calendar is null || calendar.UserId != user.Id)
        {
            return NotFound();
        }

        var images = await imageService.GetAllByCalendarIdAsync(calendarId);
        var result = mapper.Map<IEnumerable<ImageDto>>(images);
        return Ok(result);
    }


    [HttpPost("upload/{calendarId}")]
    public async Task<IActionResult> UploadImage(int calendarId, [FromQuery] short month, IFormFile file)
    {
        if (file is null || file.Length == 0)
        {
            return BadRequest("File isn't uploaded");
        }

        string imageUrl = await SaveImage(file);
        var image = new Image { ImageUrl = imageUrl, CalendarId = calendarId, MonthNumber = month };
        await imageService.SaveAsync(image);

        return Ok(new { imageUrl });
    }

    [HttpPost("batchupload/{calendarId}")]
    public async Task<IActionResult> BatchUploadImage(int calendarId, IFormFile file)
    {
        if (file is null || file.Length == 0)
        {
            return BadRequest("File isn't uploaded");
        }

        var nextMonth = await imageService.GetNotFilledMonthAsync(calendarId);
        if (nextMonth != -1)
        {
            string imageUrl = await SaveImage(file);

            var image = new Image { ImageUrl = imageUrl, CalendarId = calendarId, MonthNumber = nextMonth };

            await imageService.SaveAsync(image);

            return Ok(new { imageUrl });
        }
        return BadRequest("All months are filled");
    }


    private async Task<string> SaveImage(IFormFile file)
    {
        var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
        var filePath = Path.Combine(_imageDirectory, fileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }
        var imageUrl = $"/uploads/{fileName}";
        return imageUrl;
    }


    [HttpDelete("{imageId}")]
    public async Task<IActionResult> DeleteImage(int imageId)
    {
        var user = await CurrentUser.Value;
        if (user is null)
        {
            return Unauthorized();
        }

        var image = await imageService.GetByIdAsync(imageId);
        if (image is null)
        {
            return NotFound();
        }

        var calendar = await calendarService.GetByIdAsync(image.CalendarId);
        if (calendar is null || calendar.UserId != user.Id)
        {
            return NotFound();
        }
        await imageService.DeleteAsync(image);

        return NoContent();
    }
}
