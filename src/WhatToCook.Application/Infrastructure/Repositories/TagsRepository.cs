using Microsoft.EntityFrameworkCore;
using WhatToCook.Application.Domain;

namespace WhatToCook.Application.Infrastructure.Repositories;

public interface ITagsRepository
{
    Task<List<Tag>> Create(IEnumerable<string> tagsNames);

    Task<List<Tag>> GetTagsByNames(string[] names);
}

internal class TagsRepository : ITagsRepository
{
    private readonly DatabaseContext context;

    public TagsRepository(DatabaseContext context) => this.context = context;

    public async Task<List<Tag>> GetTagsByNames(string[] names)
    {
        IEnumerable<string> namesLowerCased = names.Select(x => x.ToLowerInvariant());
        return await context.Tags.Where(x => namesLowerCased.Contains(x.Name.ToLower())).ToListAsync();
    }

    public async Task<List<Tag>> Create(IEnumerable<string> tagsNames)
    {
        List<Tag> tags = tagsNames.Select(x => new Tag(x)).ToList();
        await context.Tags.AddRangeAsync(tags);
        _ = await context.SaveChangesAsync();

        return tags;
    }
}