namespace GameStore.Domain.Models;

public abstract class EntityBase
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public string CreatedByUser { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedByUser { get; set; }
    public bool IsDeleted { get; set; }

    public EntityBase()
    {
        Id = Guid.NewGuid();
        CreatedAt = DateTime.UtcNow;
        IsDeleted = false;
    }

    public void Update()
    {
        UpdatedAt = DateTime.UtcNow;
    }

    public void ToggleIsDeleted()
    {
        Update();
        IsDeleted = !IsDeleted;
    }
}
