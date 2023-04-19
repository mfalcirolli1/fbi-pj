using Business.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    public class SistemaReembolso
    {
        public string? _momento = string.Empty;

        public OperationResult ProcessarReembolsoPJ(ParametrosBase parametros, List<DadosContratoModel> contratos, bool verificarNumeroContrato, decimal valorParaReembolso = 0)
        {
            var retorno = new OperationResult();

            try
            {
                // TODO: Contratos dever ser agrupados por status de processamento?
                foreach (var contratosAgrupadorPorStatus in contratos.GroupBy(x => x.DadosProcessamento.StatusProcesso))
                {
                    _momento = "Montagem dos dados para reembolso";
                    // TODO: Analisar a construção do método GerarDadosParaReembolsoDTOPJ
                    var dadosParaReembolso = GerarDadosParaReembolsoDTOPJ(contratosAgrupadorPorStatus.ToList(), parametros);
                    var funcionalParaLogarReembolso = "";

                    _momento = "Efetuando reembolso";
                    var oprEfetuarReembolso = EfetuarReembolso(parametros, dadosParaReembolso, funcionalParaLogarReembolso, verificarNumeroContrato);

                    if (oprEfetuarReembolso.Situation != "Success")
                        return retorno;

                    _momento = "Registrando FQ reembolso";
                    var oprAbrirFQ = AbrirFQPJ(parametros, contratosAgrupadorPorStatus.FirstOrDefault());

                    if (oprAbrirFQ.Situation != "Success")
                        return retorno;

                    foreach (var contrato in contratosAgrupadorPorStatus.ToList())
                    {
                        if (oprAbrirFQ.Data.StatusAberturaFQ == "AberturaComErro")
                        {
                            // Enviar para Tratamento Manual
                        }
                        else
                        {
                            contrato.DadosProcessamento.StatusProcesso = dadosParaReembolso.StatusDestino;
                            contrato.DadosAnalise.NumeroOcorrenciaFQ = oprAbrirFQ.Data.StatusAberturaFQ == "AberturaEfetuada" ? oprAbrirFQ.Data.NumeroOcorrencia : contrato.DadosAnalise.NumeroOcorrenciaFQ;

                            // TODO: Considerando que o valor do reembolso é composto por vários contratos ou o boleto inteiro,
                            // o campo ValorDevolvido deverá receber o valor individual de cada contrato ou o valor total do reembolso?
                            contrato.DadosAnalise.ValorDevolvido = dadosParaReembolso.ValorInicial.ToString();

                            // Update Registro
                        }
                    }

                    // Commit
                }

                return retorno;
            }
            catch (Exception ex)
            {
                retorno.Situation = ex.Message;
                return retorno;
            }
        }

        public OperationResult EfetuarReembolso(ParametrosBase parametros, DadosParaReembolsoDTO dados, string funcional, bool verificarNumeroContrato)
        {
            var retorno = new OperationResult();
            return retorno;
        }

        public DadosParaReembolsoDTO GerarDadosParaReembolsoDTOPJ(List<DadosContratoModel> contratos, ParametrosBase parametros, decimal valorParaReembolso = 0)
        {
            var dadosParaReembolso = new DadosParaReembolsoDTO();
            var boleto = contratos.FirstOrDefault();

            /*
                  TODO: Mano, aqui neste caso, eu não deveria agrupar os contratos por boleto e a partir dai fazer a iteração pra cada boleto,
                        mesmo já estando agrupado por status?
                        Pq se eu der apenas um 'contratos.FirstOrDefault()' vou pegar apenas o primeiro boleto da lista e acabar ignorando os demais

                  Tipo assim:

                  var dadosParaReembolso = new DadosParaReembolsoDTO();     
                  var boletos = contratos.Orderby(x => x.DadosBoleto.NumeroBoleto).ToList();
                  foreach (var boleto in boletos)
                  {
                        if (valorParaReembolso.Equals(0))
                        {
                            ....
                            ...
                            ..
                            .
                        }
                  }
             */

            if (valorParaReembolso.Equals(0))
            {
                switch (boleto.DadosProcessamento.StatusProcesso)
                {
                    case "OcorrenciaCancelamentoRenegociacao":
                        dadosParaReembolso.StatusDestino = "CancelamentoRenegociacaoReembolsoEfetuado";
                        if (boleto.DadosBoleto.Empresa == "01")
                            dadosParaReembolso.IdAssunto = 1574;
                        if (boleto.DadosBoleto.Empresa == "14")
                            dadosParaReembolso.IdAssunto = 1564;
                        dadosParaReembolso.Indicador = boleto.DadosBoleto.NumeroBoleto
                            .Substring(2, boleto.DadosBoleto.NumeroBoleto.Length - 2)
                            + boleto.DadosBoleto.CodigoCampanha;
                        dadosParaReembolso.ValorInicial = boleto.DadosBoleto.ValorRecebido.Replace("R$ ", "").ConvertDecimal(); // DO -> boleto.BOL_VALORRECEBIDO.Replace("R$ ", "").ConvertDecimal();
                        break;

                    case "OcorrenciaErroDuplicidade":
                        dadosParaReembolso.StatusDestino = "ErroDuplicidadeReembolsoEfetuado";
                        if (boleto.DadosBoleto.Empresa == "01")
                            dadosParaReembolso.IdAssunto = 1574;
                        if (boleto.DadosBoleto.Empresa == "14")
                            dadosParaReembolso.IdAssunto = 1564;
                        dadosParaReembolso.Indicador = boleto.DadosBoleto.NumeroBoleto
                            .Substring(2, boleto.DadosBoleto.NumeroBoleto.Length - 2)
                            + boleto.DadosBoleto.CodigoCampanha;
                        dadosParaReembolso.ValorInicial = contratos.Sum(x => x.DadosBoleto.ValorRecebido.Replace("R$ ", "").ConvertDecimal()); // DO -> boleto.BOL_VALORRECEBIDO.Replace("R$ ", "").ConvertDecimal();
                        break;

                    case "NecessarioCancelarAnaliseValorRecebido":
                        dadosParaReembolso.StatusDestino = "CancelamentoValorRecebidoReembolsoEfetuado";
                        if (boleto.DadosBoleto.Empresa == "01")
                            dadosParaReembolso.IdAssunto = 1574;
                        if (boleto.DadosBoleto.Empresa == "14")
                            dadosParaReembolso.IdAssunto = 1564;
                        dadosParaReembolso.Indicador = boleto.DadosBoleto.NumeroBoleto
                            .Substring(2, boleto.DadosBoleto.NumeroBoleto.Length - 2)
                            + boleto.DadosBoleto.CodigoCampanha;
                        dadosParaReembolso.ValorInicial = boleto.DadosBoleto.ValorRecebido.Replace("R$ ", "").ConvertDecimal(); // DO -> boleto.BOL_VALORRECEBIDO.Replace("R$ ", "").ConvertDecimal();
                        break;

                    case "CancelamentoParcelaFlexEntradaFlex":
                        dadosParaReembolso.StatusDestino = "ParcelaFlexEntradaFlexReembolsoEfetuado";
                        if (boleto.DadosBoleto.Empresa == "01")
                            dadosParaReembolso.IdAssunto = 1574;
                        if (boleto.DadosBoleto.Empresa == "14")
                            dadosParaReembolso.IdAssunto = 1564;
                        dadosParaReembolso.Indicador = boleto.DadosBoleto.NumeroBoleto
                            .Substring(2, boleto.DadosBoleto.NumeroBoleto.Length - 2)
                            + boleto.DadosBoleto.CodigoCampanha;
                        dadosParaReembolso.ValorInicial = boleto.DadosBoleto.ValorRecebido.Replace("R$ ", "").ConvertDecimal(); // DO -> boleto.BOL_VALORRECEBIDO.Replace("R$ ", "").ConvertDecimal();
                        break;

                    case "ReembolsarParcelado":
                        dadosParaReembolso.StatusDestino = "DevolucaoSistemaReembolsoEfetuada";
                        if (boleto.DadosBoleto.Empresa == "01")
                            dadosParaReembolso.IdAssunto = 1574;
                        if (boleto.DadosBoleto.Empresa == "14")
                            dadosParaReembolso.IdAssunto = 1564;
                        dadosParaReembolso.Indicador = boleto.DadosBoleto.NumeroBoleto
                            .Substring(2, boleto.DadosBoleto.NumeroBoleto.Length - 2)
                            + boleto.DadosBoleto.CodigoCampanha;
                        dadosParaReembolso.ValorInicial = boleto.DadosBoleto.ValorRecebido.Replace("R$ ", "").ConvertDecimal(); // DO -> boleto.BOL_VALORRECEBIDO.Replace("R$ ", "").ConvertDecimal();
                        break;

                    case "EncaminharDevolucaoReembolsoValorCL":
                        dadosParaReembolso.StatusDestino = "DevolucaoSistemaReembolsoEfetuada";
                        dadosParaReembolso.IdAssunto = 1435;
                        dadosParaReembolso.Indicador = boleto.DadosContrato.NumeroCL.Substring(2, boleto.DadosContrato.NumeroCL.Length - 2);
                        dadosParaReembolso.ValorInicial = contratos.Sum(x => x.DadosAnalise.ValorParaDevolucao.ConvertDecimal()); // DO -> valorSomado
                        break;

                    case "EncaminharDevolucaoReembolsoValorF5":
                        dadosParaReembolso.StatusDestino = "DevolucaoSistemaReembolsoEfetuada";
                        dadosParaReembolso.IdAssunto = 1738; //1949
                        dadosParaReembolso.Indicador = boleto.DadosContrato.Contrato;
                        dadosParaReembolso.ValorInicial = contratos.Sum(x => x.DadosAnalise.ValorParaDevolucao.ConvertDecimal()); // DO -> valorSomado
                        break;

                    case "EncaminharDevolucaoReembolsoValorSF":
                        dadosParaReembolso.StatusDestino = "DevolucaoSistemaReembolsoEfetuada";
                        dadosParaReembolso.IdAssunto = 1408;
                        dadosParaReembolso.Indicador = boleto.DadosContrato.Contrato;
                        dadosParaReembolso.ValorInicial = contratos.Sum(x => x.DadosAnalise.ValorParaDevolucao.ConvertDecimal()); // DO -> valorSomado
                        break;
                }
            }
            else
            {
                if (boleto.DadosBoleto.Empresa == "01")
                    dadosParaReembolso.IdAssunto = 1574;
                if (boleto.DadosBoleto.Empresa == "14")
                    dadosParaReembolso.IdAssunto = 1564;
                dadosParaReembolso.Indicador = boleto.DadosBoleto.NumeroBoleto
                    .Substring(2, boleto.DadosBoleto.NumeroBoleto.Length - 2)
                    + boleto.DadosBoleto.CodigoCampanha;
                dadosParaReembolso.ValorInicial = valorParaReembolso;
            }

            dadosParaReembolso.Origem = "O";
            dadosParaReembolso.NomeDesc = "DadosCliente.Nome";
            dadosParaReembolso.Cpf = "DadosCliente.CpfCnpj";
            dadosParaReembolso.TipoPessoa = "J";
            dadosParaReembolso.Operacao = "DadosContrato.Operacao";
            dadosParaReembolso.NumContrato = "DadosContrato.Contrato";
            dadosParaReembolso.Funcional = "0338764";
            dadosParaReembolso.Status = string.Empty;
            dadosParaReembolso.IdEmpresa = 341;
            dadosParaReembolso.Banco = "341";

            if (string.IsNullOrEmpty(boleto.DadosBoleto.AgenciaContaAtiva))
            {
                dadosParaReembolso.IdTipo = "O";
                dadosParaReembolso.Agencia = "1000";
                dadosParaReembolso.Conta = "0";
            }
            else
            {
                dadosParaReembolso.IdTipo = "C";
                dadosParaReembolso.Agencia = boleto.DadosBoleto.AgenciaContaAtiva.Split('.').FirstOrDefault();
                dadosParaReembolso.Conta = boleto.DadosBoleto.AgenciaContaAtiva.Split('.').FirstOrDefault();
            }

            return dadosParaReembolso;
        }

        public OperationResult<DadosFQ> AbrirFQPJ(ParametrosBase parametros, DadosContratoModel boleto)
        {
            var retorno = new OperationResult<DadosFQ>();
            return retorno;
        }

    }

    public static class Ext
    {
        public static decimal ConvertDecimal(this string number)
        {
            return 0.00M;
        }
    }
}
