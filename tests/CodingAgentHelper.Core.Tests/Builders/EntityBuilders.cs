namespace CodingAgentHelper.Core.Tests.Builders;

using Domain.Entities;

/// <summary>
/// Builder for creating Standard entities in tests
/// </summary>
public class StandardBuilder
{
    private string _title = "Test Standard";
    private string _description = "Test Description";
    private string _category = "Testing";
    private StandardStatus _status = StandardStatus.Active;
    private StandardPriority _priority = StandardPriority.Medium;
    private readonly List<string> _tags = new();

    public StandardBuilder WithTitle(string title)
    {
        _title = title;
        return this;
    }

    public StandardBuilder WithDescription(string description)
    {
        _description = description;
        return this;
    }

    public StandardBuilder WithCategory(string category)
    {
        _category = category;
        return this;
    }

    public StandardBuilder WithStatus(StandardStatus status)
    {
        _status = status;
        return this;
    }

    public StandardBuilder WithPriority(StandardPriority priority)
    {
        _priority = priority;
        return this;
    }

    public StandardBuilder WithTag(string tag)
    {
        _tags.Add(tag);
        return this;
    }

    public Standard Build()
    {
        var standard = new Standard(_title, _description, _category);
        
        if (_status != StandardStatus.Active)
            standard.UpdateStatus(_status);

        if (_priority != StandardPriority.Medium)
            standard.UpdatePriority(_priority);

        foreach (var tag in _tags)
            standard.AddTag(tag);

        return standard;
    }
}

/// <summary>
/// Builder for creating Category entities in tests
/// </summary>
public class CategoryBuilder
{
    private string _name = "Test Category";
    private string _description = "Test Category Description";

    public CategoryBuilder WithName(string name)
    {
        _name = name;
        return this;
    }

    public CategoryBuilder WithDescription(string description)
    {
        _description = description;
        return this;
    }

    public Category Build()
    {
        return new Category(_name, _description);
    }
}
