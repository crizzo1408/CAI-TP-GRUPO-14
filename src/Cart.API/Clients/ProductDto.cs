namespace Cart.API.Clients;

/// <summary>
/// Lo que Cart necesita de la respuesta de GET /api/products/{id} de Products.API.
/// Es un DTO propio: Cart no referencia el proyecto Products (el contrato es el de la consigna).
/// </summary>
public class ProductDto
{
    public Guid Id { get; set; }
    public string Nombre { get; set; }
    public decimal Precio { get; set; }
    public int Stock { get; set; }
}
