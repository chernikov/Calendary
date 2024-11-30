﻿using Calendary.Model;
using Calendary.Repos.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Calendary.Core.Services;
public interface IPromptService
{
    Task<IEnumerable<Prompt>> GetFullAllAsync(int? themeId, int? ageGender);
    Task<Prompt?> GetByIdAsync(int id);
    Task CreateAsync(Prompt prompt);
    Task UpdateAsync(Prompt prompt);
    Task DeleteAsync(int id);
}

public class PromptService : IPromptService
{
    private readonly IPromptRepository _promptRepository;

    public PromptService(IPromptRepository promptRepository)
    {
        _promptRepository = promptRepository;
    }

    public Task<IEnumerable<Prompt>> GetFullAllAsync(int? themeId, int? ageGender)
        => _promptRepository.GetFullAllAsync(themeId, ageGender); 
   
    public async Task<Prompt?> GetByIdAsync(int id)
    {
        return await _promptRepository.GetByIdAsync(id);
    }

    public async Task CreateAsync(Prompt prompt)
    {
        prompt.Theme = null!;
        await _promptRepository.AddAsync(prompt);
    }

    public async Task UpdateAsync(Prompt prompt)
    {
        var entity = await _promptRepository.GetByIdAsync(prompt.Id);
        if (entity is null)
        {
            return;
        }
        entity.ThemeId = prompt.ThemeId;
        entity.AgeGender = prompt.AgeGender;
        entity.Text = prompt.Text;
        await _promptRepository.UpdateAsync(entity);
    }

    public async Task DeleteAsync(int id)
    {
        await _promptRepository.DeleteAsync(id);
    }

   
}