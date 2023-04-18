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
                foreach (var contratosAgrupadorPorStatus in contratos.GroupBy(x => x.DadosProcessamento.StatusProcesso))
                {
                    _momento = "Montagem dos dados para reembolso";
                    // TODO: Analisar a construção deste método
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
                            // Enviar Tratamento Manual
                        }
                        else
                        {
                            contrato.DadosProcessamento.StatusProcesso = dadosParaReembolso.StatusDestino;
                            contrato.DadosAnalise.NumeroOcorrenciaFQ = oprAbrirFQ.Data.StatusAberturaFQ = "AberturaEfetuada" ? oprAbrirFQ.Data.NumeroOcorrencia : contrato.DadosAnalise.NumeroOcrrenciaFQ;

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
            var contratosAgrupadosInconsistencia = contratos.GroupBy(x => x.DadosProcessamento.StatusProcesso);
            var boleto = contratos.FirstOrDefault();

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
                            .Substring(2, boleto.DadosBoleto.NumeroBoleto.Length -2) + boleto.DadosBoleto.CodigoCampanha;
                        dadosParaReembolso.ValorInicial = boleto.DadosBoleto.ValorRecebido.Replace("R$ ", "").ConvertDecimal();
                        break;

                    case "OcorrenciaErroDuplicidade":
                        dadosParaReembolso.StatusDestino = "CancelamentoRenegociacaoReembolsoEfetuado";
                        if (boleto.DadosBoleto.Empresa == "01")
                            dadosParaReembolso.IdAssunto = 1574;
                        if (boleto.DadosBoleto.Empresa == "14")
                            dadosParaReembolso.IdAssunto = 1564;
                        dadosParaReembolso.Indicador = boleto.DadosBoleto.NumeroBoleto
                            .Substring(2, boleto.DadosBoleto.NumeroBoleto.Length - 2) + boleto.DadosBoleto.CodigoCampanha;
                        dadosParaReembolso.ValorInicial = boleto.DadosBoleto.ValorRecebido.Replace("R$ ", "").ConvertDecimal();
                        break;

                    case "NecessarioCancelarAnaliseValorRecebido":
                        dadosParaReembolso.StatusDestino = "CancelamentoRenegociacaoReembolsoEfetuado";
                        if (boleto.DadosBoleto.Empresa == "01")
                            dadosParaReembolso.IdAssunto = 1574;
                        if (boleto.DadosBoleto.Empresa == "14")
                            dadosParaReembolso.IdAssunto = 1564;
                        dadosParaReembolso.Indicador = boleto.DadosBoleto.NumeroBoleto
                            .Substring(2, boleto.DadosBoleto.NumeroBoleto.Length - 2) + boleto.DadosBoleto.CodigoCampanha;
                        dadosParaReembolso.ValorInicial = boleto.DadosBoleto.ValorRecebido.Replace("R$ ", "").ConvertDecimal();
                        break;

                    case "CancelamentoParcelaFlexEntradaFlex":
                        dadosParaReembolso.StatusDestino = "CancelamentoRenegociacaoReembolsoEfetuado";
                        if (boleto.DadosBoleto.Empresa == "01")
                            dadosParaReembolso.IdAssunto = 1574;
                        if (boleto.DadosBoleto.Empresa == "14")
                            dadosParaReembolso.IdAssunto = 1564;
                        dadosParaReembolso.Indicador = boleto.DadosBoleto.NumeroBoleto
                            .Substring(2, boleto.DadosBoleto.NumeroBoleto.Length - 2) + boleto.DadosBoleto.CodigoCampanha;
                        dadosParaReembolso.ValorInicial = boleto.DadosBoleto.ValorRecebido.Replace("R$ ", "").ConvertDecimal();
                        break;

                    case "ReembolsarParcelado":
                        dadosParaReembolso.StatusDestino = "CancelamentoRenegociacaoReembolsoEfetuado";
                        if (boleto.DadosBoleto.Empresa == "01")
                            dadosParaReembolso.IdAssunto = 1574;
                        if (boleto.DadosBoleto.Empresa == "14")
                            dadosParaReembolso.IdAssunto = 1564;
                        dadosParaReembolso.Indicador = boleto.DadosBoleto.NumeroBoleto
                            .Substring(2, boleto.DadosBoleto.NumeroBoleto.Length - 2) + boleto.DadosBoleto.CodigoCampanha;
                        dadosParaReembolso.ValorInicial = boleto.DadosBoleto.ValorRecebido.Replace("R$ ", "").ConvertDecimal();
                        break;

                    case "EncaminharDevolucaoReembolsoValorCL":
                        dadosParaReembolso.StatusDestino = "CancelamentoRenegociacaoReembolsoEfetuado";
                        if (boleto.DadosBoleto.Empresa == "01")
                            dadosParaReembolso.IdAssunto = 1574;
                        if (boleto.DadosBoleto.Empresa == "14")
                            dadosParaReembolso.IdAssunto = 1564;
                        dadosParaReembolso.Indicador = boleto.DadosBoleto.NumeroBoleto
                            .Substring(2, boleto.DadosBoleto.NumeroBoleto.Length - 2) + boleto.DadosBoleto.CodigoCampanha;
                        dadosParaReembolso.ValorInicial = boleto.DadosBoleto.ValorRecebido.Replace("R$ ", "").ConvertDecimal();
                        break;

                    case "EncaminharDevolucaoReembolsoValorF5":
                        dadosParaReembolso.StatusDestino = "CancelamentoRenegociacaoReembolsoEfetuado";
                        if (boleto.DadosBoleto.Empresa == "01")
                            dadosParaReembolso.IdAssunto = 1574;
                        if (boleto.DadosBoleto.Empresa == "14")
                            dadosParaReembolso.IdAssunto = 1564;
                        dadosParaReembolso.Indicador = boleto.DadosBoleto.NumeroBoleto
                            .Substring(2, boleto.DadosBoleto.NumeroBoleto.Length - 2) + boleto.DadosBoleto.CodigoCampanha;
                        dadosParaReembolso.ValorInicial = boleto.DadosBoleto.ValorRecebido.Replace("R$ ", "").ConvertDecimal();
                        break;

                    case "EncaminharDevolucaoReembolsoValorSF":
                        dadosParaReembolso.StatusDestino = "CancelamentoRenegociacaoReembolsoEfetuado";
                        if (boleto.DadosBoleto.Empresa == "01")
                            dadosParaReembolso.IdAssunto = 1574;
                        if (boleto.DadosBoleto.Empresa == "14")
                            dadosParaReembolso.IdAssunto = 1564;
                        dadosParaReembolso.Indicador = boleto.DadosBoleto.NumeroBoleto
                            .Substring(2, boleto.DadosBoleto.NumeroBoleto.Length - 2) + boleto.DadosBoleto.CodigoCampanha;
                        dadosParaReembolso.ValorInicial = boleto.DadosBoleto.ValorRecebido.Replace("R$ ", "").ConvertDecimal();
                        break;
                }
            }
            else
            {

            }


            return dadosParaReembolso;
        }

        public OperationResult AbrirFQPJ(ParametrosBase parametros, DadosContratoModel boleto)
        {
            var retorno = new OperationResult();
            return retorno;
        }

    }
}
