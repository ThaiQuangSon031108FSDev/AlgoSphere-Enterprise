using AlgoSphere.Domain.Common;

namespace AlgoSphere.Domain.Entities;

public class Topic : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int OrderIndex { get; set; }
    
    public int CategoryId { get; set; }
    public Category Category { get; set; } = null!;
    public ICollection<Exercise> Exercises { get; set; } = new List<Exercise>();
}

public class Category : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public ICollection<Topic> Topics { get; set; } = new List<Topic>();
}
