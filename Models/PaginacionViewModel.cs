namespace ManejoPresupuesto.Models
{
    // Me ayudará a calcular cuántos registros debo saltar y cuántos debo mostrar en cada página.
    public class PaginacionViewModel
    {
        public int Pagina { get; set; } = 1;
        private int recordsPorPagina = 10;
        private readonly int cantidadMaximaRecordsPorPagina = 50;
        // La propiedad RecordsPorPagina tiene una lógica en el setter
        // para asegurarse de que el número de registros por página
        // no exceda un límite máximo definido por cantidadMaximaRecordsPorPagina.
        public int RecordsPorPagina
        {
            get 
            {
                return recordsPorPagina;
            }
            set 
            {
                recordsPorPagina = value > cantidadMaximaRecordsPorPagina ? cantidadMaximaRecordsPorPagina : value;
            }
        }
        // La propiedad RecordsASaltar calcula cuántos registros se deben saltar
        public int RecordsASaltar => recordsPorPagina * (Pagina - 1);
    }
}
