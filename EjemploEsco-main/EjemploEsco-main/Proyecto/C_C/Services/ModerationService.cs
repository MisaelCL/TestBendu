using System;
using System.Collections.Generic;
using System.Linq;
using C_C.Model;

namespace C_C.Services
{
    public class ModerationService
    {
        private readonly List<ReporteModel> _reportes = new List<ReporteModel>();

        public void Reportar(Guid reportanteId, Guid reportadoId, string motivo)
        {
            var reporte = new ReporteModel
            {
                Id = Guid.NewGuid(),
                ReportanteId = reportanteId,
                ReportadoId = reportadoId,
                Motivo = motivo,
                CreadoEl = DateTime.UtcNow
            };

            _reportes.Add(reporte);
        }

        public IEnumerable<ReporteModel> ObtenerReportes(Guid reportadoId)
        {
            return _reportes.Where(reporte => reporte.ReportadoId == reportadoId);
        }
    }
}
