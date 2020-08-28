using BradescoPGP.Repositorio;
using System;

namespace BradescoPGP.Console.ModelsExport
{
    public class VencimentoExportModel
    {
        public int Id { get; set; }
        public string Especialista { get; set; }
        public string Produto { get; set; }
        public decimal SaldoAtual { get; set; }
        public int Agencia { get; set; }
        public int Conta { get; set; }
        public DateTime DataVencimento { get; set; }
        public double PercentualIndexador { get; set; }
        public string Cliente { get; set; }
        public int? StatusId { get; set; }
        public string Status { get; set; }
        public string Matriucla { get; set; }
        public string Equipe { get; set; }

        public static VencimentoExportModel MapearExcel(Vencimento vencimento, string consultor, string matricula, string equipe)
        {
            var venc = new VencimentoExportModel();

            venc.Id = vencimento.Id;
            venc.Especialista = consultor;
            venc.Produto = vencimento.Nome_produto_sistema_origem;
            venc.SaldoAtual = vencimento.SALDO_ATUAL;
            venc.Agencia = vencimento.Cod_Agencia;
            venc.Conta = vencimento.Cod_Conta_Corrente;
            venc.DataVencimento = vencimento.Dt_Vecto_Contratado;
            venc.PercentualIndexador = vencimento.Perc_Indexador;
            venc.Status = vencimento.Status?.Descricao;
            venc.StatusId = vencimento.Status?.Id;
            venc.Cliente = vencimento.Nm_Cliente_Contraparte;
            venc.Matriucla = matricula;
            venc.Equipe = equipe;

            return venc;
        }
    }
}
