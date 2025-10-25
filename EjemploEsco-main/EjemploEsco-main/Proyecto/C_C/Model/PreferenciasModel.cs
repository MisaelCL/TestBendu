using System;

namespace C_C.Model
{
    public class PreferenciasModel
    {
        public Guid Id { get; set; }

        public Guid UserId { get; set; }

        public int EdadMinima { get; set; }

        public int EdadMaxima { get; set; }

        public string CarreraObjetivo { get; set; }

        public string InteresesDeseados { get; set; }

        public int DistanciaMaximaKm { get; set; }
    }
}
