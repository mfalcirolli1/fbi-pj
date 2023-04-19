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
        public DadosAnalise DadosAnalise { get; set; }
        public DadosContrato DadosContrato { get; set; }
    }

    public class DadosAnalise
    {
        public string ValorDevolvido { get; set; }
        public string ValorParaDevolucao { get; set; }
        public string NumeroOcorrenciaFQ { get; set; }
    }

    public class DadosProcessamento
    {
        public string StatusProcesso { get; set; }
    }

    public class DadosContrato
    {
        public string NumeroCL { get; set; }
        public string Contrato { get; set; }
    }

    public class DadosBoleto
    {
        public string Empresa { get; set; }
        public string CodigoCampanha { get; set; }
        public string ValorRecebido { get; set; }
        public string NumeroBoleto { get; set; }
        public string AgenciaContaAtiva { get; set; }
    }
}
