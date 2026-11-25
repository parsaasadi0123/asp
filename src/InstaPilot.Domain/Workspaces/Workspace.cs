using InstaPilot.Domain.Common;
namespace InstaPilot.Domain.Workspaces;
public sealed class Workspace : Entity<Guid>
{
    public string Name { get; private set; } = string.Empty;
    public string Slug { get; private set; } = string.Empty;
    public Guid CreatedByUserId { get; private set; }
    private Workspace() { }
    public Workspace(Guid id, string name, string slug, Guid createdByUserId) : base(id)
    {
        Rename(name); Slug = NormalizeSlug(slug); CreatedByUserId = createdByUserId;
    }
    public void Rename(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        var value = name.Trim();
        if (value.Length > 120) throw new ArgumentOutOfRangeException(nameof(name));
        Name = value;
    }
    private static string NormalizeSlug(string slug)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(slug);
        var value = slug.Trim().ToLowerInvariant();
        if (value.Length is < 3 or > 80 || value.Any(c => !(char.IsAsciiLetterOrDigit(c) || c == '-'))) throw new ArgumentException("Invalid workspace slug.", nameof(slug));
        return value;
    }
}
