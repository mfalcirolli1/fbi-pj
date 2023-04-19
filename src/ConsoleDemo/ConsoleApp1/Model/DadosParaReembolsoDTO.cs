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
        public string Origem { get; set; }
        public string NomeDesc { get; set; }
        public string Cpf { get; set; }
        public string TipoPessoa { get; set; }
        public string Operacao { get; set; }
        public string NumContrato { get; set; }
        public string Funcional { get; set; }
        public string Status { get; set; }
        public int IdEmpresa { get; set; }
        public string Banco { get; set; }
        public string IdTipo { get; set; }
        public string Agencia { get; set; }
        public string Conta { get; set; }

    }
}
