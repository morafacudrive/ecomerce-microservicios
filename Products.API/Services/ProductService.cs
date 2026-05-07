using Products.API.DTOs;
using Products.API.Exceptions;
using Products.API.Models;

namespace Products.API.Services;

public class ProductService
{
    // Por ahora usamos una lista en memoria (después la cátedra provee la persistencia)
    private static List<Product> _products = new();

    public List<ProductResponse> GetAll(string? categoria, string? nombre)
    {
        var query = _products.AsQueryable();

        if (!string.IsNullOrEmpty(categoria))
            query = query.Where(p => p.Categoria == categoria);

        if (!string.IsNullOrEmpty(nombre))
            query = query.Where(p => p.Nombre.Contains(nombre));

        return query.Select(MapToResponse).ToList();
    }

    public ProductResponse GetById(Guid id)
    {
        var product = _products.FirstOrDefault(p => p.Id == id);
        if (product == null)
            throw new NotFoundException("PRD-001", "Producto no encontrado.");

        return MapToResponse(product);
    }

    public ProductResponse Create(CreateProductRequest request)
    {
        // Validar duplicado
        var existe = _products.Any(p => p.Nombre == request.Nombre && p.Categoria == request.Categoria);
        if (existe)
            throw new BusinessRuleException("PRD-003", $"Ya existe un producto con ese nombre en la categoría '{request.Categoria}'.");

        var product = new Product
        {
            Id = Guid.NewGuid(),
            Nombre = request.Nombre,
            Descripcion = request.Descripcion,
            Precio = request.Precio,
            Stock = request.Stock,
            Categoria = request.Categoria,
            FechaCreacion = DateTime.UtcNow
        };

        _products.Add(product);
        return MapToResponse(product);
    }

    public ProductResponse Update(Guid id, CreateProductRequest request)
    {
        var product = _products.FirstOrDefault(p => p.Id == id);
        if (product == null)
            throw new NotFoundException("PRD-001", "Producto no encontrado.");

        product.Nombre = request.Nombre;
        product.Descripcion = request.Descripcion;
        product.Precio = request.Precio;
        product.Stock = request.Stock;
        product.Categoria = request.Categoria;

        return MapToResponse(product);
    }

    public void Delete(Guid id)
    {
        var product = _products.FirstOrDefault(p => p.Id == id);
        if (product == null)
            throw new NotFoundException("PRD-001", "Producto no encontrado.");

        _products.Remove(product);
    }

    private static ProductResponse MapToResponse(Product p) => new()
    {
        Id = p.Id,
        Nombre = p.Nombre,
        Descripcion = p.Descripcion,
        Precio = p.Precio,
        Stock = p.Stock,
        Categoria = p.Categoria,
        FechaCreacion = p.FechaCreacion
    };
}