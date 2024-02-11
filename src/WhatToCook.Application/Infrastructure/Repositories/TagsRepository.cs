using WhatToCook.Application.Domain;

namespace WhatToCook.Application.Infrastructure.Repositories;

public interface ITagsRepository
{
    Task<IEnumerable<Tag>> Create(IEnumerable<string> tagsNames);

    IEnumerable<Tag> GetTagsByNames(string[] names);
}

internal class TagsRepository : ITagsRepository
{
    private readonly DatabaseContext context;

    public TagsRepository(DatabaseContext context) => this.context = context;

    public IEnumerable<Tag> GetTagsByNames(string[] names)
    {
        var namesLowerCased = names.Select(x => x.ToLowerInvariant());
        return context.Tags.Where(x => namesLowerCased.Contains(x.Name.ToLower()));
    }

    public async Task<IEnumerable<Tag>> Create(IEnumerable<string> tagsNames)
    {
        var tags = tagsNames.Select(x => new Tag(x));
        await context.Tags.AddRangeAsync(tags);
        await context.SaveChangesAsync();

        return tags;
    }
}