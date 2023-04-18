using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.Model
{
    public class DadosContratoModel
    {
        public DadosProcessamento DadosProcessamento { get; set; }
        public DadosBoleto DadosBoleto { get; set; }
    }

    public class DadosProcessamento
    {
        public string StatusProcesso { get; set; }
    }

    public class DadosBoleto
    {
        public string Empresa { get; set; }
    }
}
