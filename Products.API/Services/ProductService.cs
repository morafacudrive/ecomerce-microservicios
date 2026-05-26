using Products.API.DTOs;
using Products.API.Exceptions;
using Products.API.Models;

namespace Products.API.Services;

public class ProductService
{
    private static readonly List<Product> _products = new();

    public List<ProductResponse> GetAll(string? categoria, string? nombre)
    {
        var query = _products.AsQueryable();

        if (!string.IsNullOrWhiteSpace(categoria))
            query = query.Where(p => p.Categoria.Contains(categoria, StringComparison.OrdinalIgnoreCase));

        if (!string.IsNullOrWhiteSpace(nombre))
            query = query.Where(p => p.Nombre.Contains(nombre, StringComparison.OrdinalIgnoreCase));

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
        ValidateProductRequest(request);

        var existe = _products.Any(p =>
            p.Nombre.Equals(request.Nombre, StringComparison.OrdinalIgnoreCase) &&
            p.Categoria.Equals(request.Categoria, StringComparison.OrdinalIgnoreCase));

        if (existe)
            throw new ConflictException("PRD-003", $"Ya existe un producto con ese nombre en la categoría '{request.Categoria}'.");

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
        ValidateProductRequest(request);

        var product = _products.FirstOrDefault(p => p.Id == id);

        if (product == null)
            throw new NotFoundException("PRD-001", "Producto no encontrado.");

        var existeDuplicado = _products.Any(p =>
            p.Id != id &&
            p.Nombre.Equals(request.Nombre, StringComparison.OrdinalIgnoreCase) &&
            p.Categoria.Equals(request.Categoria, StringComparison.OrdinalIgnoreCase));

        if (existeDuplicado)
            throw new ConflictException("PRD-003", $"Ya existe un producto con ese nombre en la categoría '{request.Categoria}'.");

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

        // PRD-004 queda pendiente para cuando integren con Orders.API.
        // Ejemplo futuro:
        // if (TieneOrdenesActivas(id))
        //     throw new ConflictException("PRD-004", "El producto tiene órdenes activas y no puede eliminarse.");

        _products.Remove(product);
    }

    private static void ValidateProductRequest(CreateProductRequest request)
    {
        if (request == null)
            throw new ValidationException("PRD-002", "Los datos del producto son inválidos.");

        if (string.IsNullOrWhiteSpace(request.Nombre))
            throw new ValidationException("PRD-002", "El nombre del producto es requerido.");

        if (request.Nombre.Length > 100)
            throw new ValidationException("PRD-002", "El nombre del producto no puede superar los 100 caracteres.");

        if (!string.IsNullOrWhiteSpace(request.Descripcion) && request.Descripcion.Length > 500)
            throw new ValidationException("PRD-002", "La descripción no puede superar los 500 caracteres.");

        if (request.Precio <= 0)
            throw new ValidationException("PRD-002", "El precio debe ser mayor a 0.");

        if (request.Stock < 0)
            throw new ValidationException("PRD-002", "El stock debe ser mayor o igual a 0.");

        if (string.IsNullOrWhiteSpace(request.Categoria))
            throw new ValidationException("PRD-002", "La categoría es requerida.");
    }

    private static ProductResponse MapToResponse(Product product)
    {
        return new ProductResponse
        {
            Id = product.Id,
            Nombre = product.Nombre,
            Descripcion = product.Descripcion,
            Precio = product.Precio,
            Stock = product.Stock,
            Categoria = product.Categoria,
            FechaCreacion = product.FechaCreacion
        };
    }
}