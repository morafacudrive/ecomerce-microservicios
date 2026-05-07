using System;

/// <summary>
/// Summary description for Class1
/// </summary>
public class ProductResponse
{
	public ProductResponse()
	{

        public Guid Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public decimal Precio { get; set; }
        public int Stock { get; set; }
        public string Categoria { get; set; } = string.Empty;
        public DateTime FechaCreacion { get; set; }
}
}
