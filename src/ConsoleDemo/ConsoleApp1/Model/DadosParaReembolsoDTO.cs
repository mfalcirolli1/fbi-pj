using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.Model
{
    public class DadosParaReembolsoDTO
    {
        public int IdAssunto { get; set; }
        public string Indicador { get; set; }
        public string StatusDestino { get; set; }
        public decimal ValorInicial { get; set; }

    }
}
