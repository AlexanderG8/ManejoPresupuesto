namespace ManejoPresupuesto.Models
{
    // Me ayudará a enviar la información de paginación a la vista, incluyendo el número de página actual,
    public class PaginacionRespuesta
    {
        public int Pagina { get; set; } = 1;
        public int RecordsPorPagina { get; set; } = 10;
        public int CantidadTotalRecords { get; set; }
        // La propiedad CantidadTotalDePaginas calcula el número total de páginas
        // necesarias para mostrar todos los registros, redondeando hacia arriba
        // para asegurarse de que cualquier registro adicional se muestre en una página adicional.
        public int CantidadTotalDePaginas => (int)Math.Ceiling((double)CantidadTotalRecords / RecordsPorPagina);
        // La propiedad BaseURL se utiliza para construir las URLs de paginación en la vista, p
        // ermitiendo que los enlaces de paginación apunten a la ruta correcta.
        public string BaseURL { get; set; }
    }
    // Además, la clase genérica PaginacionRespuesta<T> me permitirá enviar la lista de elementos que corresponden a la página actual.

    public class PaginacionRespuesta<T> : PaginacionRespuesta 
    {
        public IEnumerable<T> Elementos { get; set; }
    }
}
