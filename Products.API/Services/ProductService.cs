using Products.API.Data;
using Products.API.DTOs;
using Products.API.Exceptions;
using Products.API.Models;

namespace Products.API.Services;

public class ProductService(ProductRepository repo)
{
    public async Task<List<ProductResponse>> GetAllAsync(string? categoria, string? nombre)
    {
        var products = await repo.GetAllAsync(categoria, nombre);
        return products.Select(MapToResponse).ToList();
    }

    public async Task<ProductResponse> GetByIdAsync(Guid id)
    {
        var product = await repo.GetByIdAsync(id.ToString());

        if (product == null)
            throw new NotFoundException("PRD-001", "Producto no encontrado.");

        return MapToResponse(product);
    }

    public async Task<ProductResponse> CreateAsync(CreateProductRequest request)
    {
        ValidateProductRequest(request);

        var existe = await repo.GetByNombreYCategoriaAsync(request.Nombre, request.Categoria);

        if (existe != null)
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

        await repo.CreateAsync(product);

        return MapToResponse(product);
    }

    public async Task<ProductResponse> UpdateAsync(Guid id, CreateProductRequest request)
    {
        ValidateProductRequest(request);

        var product = await repo.GetByIdAsync(id.ToString());

        if (product == null)
            throw new NotFoundException("PRD-001", "Producto no encontrado.");

        var duplicado = await repo.GetByNombreYCategoriaAsync(request.Nombre, request.Categoria);

        if (duplicado != null && duplicado.Id != id)
            throw new ConflictException("PRD-003", $"Ya existe un producto con ese nombre en la categoría '{request.Categoria}'.");

        product.Nombre = request.Nombre;
        product.Descripcion = request.Descripcion;
        product.Precio = request.Precio;
        product.Stock = request.Stock;
        product.Categoria = request.Categoria;

        await repo.UpdateAsync(product);

        return MapToResponse(product);
    }

    public async Task DeleteAsync(Guid id)
    {
        var product = await repo.GetByIdAsync(id.ToString());

        if (product == null)
            throw new NotFoundException("PRD-001", "Producto no encontrado.");

        await repo.DeleteAsync(id.ToString());
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