using AlgoSphere.Domain.Common;

namespace AlgoSphere.Domain.Entities;

public class Forum : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public ICollection<Discussion> Discussions { get; set; } = new List<Discussion>();
}

public class Discussion : BaseEntity
{
    public int ForumId { get; set; }
    public Forum Forum { get; set; } = null!;
    
    public int UserId { get; set; }
    public User User { get; set; } = null!;
    
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public int Views { get; set; }
    
    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
}

public class Comment : BaseEntity
{
    public int DiscussionId { get; set; }
    public Discussion Discussion { get; set; } = null!;
    
    public int UserId { get; set; }
    public User User { get; set; } = null!;
    
    public int? ParentCommentId { get; set; }
    public Comment? ParentComment { get; set; }
    public ICollection<Comment> Replies { get; set; } = new List<Comment>();

    public string Content { get; set; } = string.Empty;
    public string MaterializedPath { get; set; } = string.Empty;
}
