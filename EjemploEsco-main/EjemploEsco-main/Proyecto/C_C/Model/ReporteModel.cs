using System;

namespace C_C.Model
{
    public class ReporteModel
    {
        public Guid Id { get; set; }

        public Guid ReportanteId { get; set; }

        public Guid ReportadoId { get; set; }

        public string Motivo { get; set; }

        public DateTime CreadoEl { get; set; }
    }
}
