using System.ComponentModel.DataAnnotations;

namespace GameStore.API.Contracts.Requests;

public class OrderRequest
{
    [Required]
    public Guid CustomerId { get; set; }

    [Required]
    public List<Guid> ProductIds { get; set; } = new();

    public DateTime? OrderDate { get; set; }
}
