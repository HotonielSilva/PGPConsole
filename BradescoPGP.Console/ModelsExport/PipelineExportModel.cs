using BradescoPGP.Repositorio;
using System;

namespace BradescoPGP.Console.ModelsExport
{
    public class PipelineExportModel
    {
        public string Cliente { get; set; }
        public string Especialista { get; set; }
        public int Agencia { get; set; }
        public int Conta { get; set; }
        public bool BradescoPrincipalBanco { get; set; }
        public decimal? ValorMercado { get; set; }
        public DateTime? DataProrrogada { get; set; }
        public decimal ValorDoPipe { get; set; }
        public decimal? ValorAplicado { get; set; }
        public DateTime DataPrevista { get; set; }
        public string Comentario { get; set; }
        public int OrigemId { get; set; }
        public int StatusId { get; set; }
        public int MotivoId { get; set; }
        public string Motivo { get; set; }
        public string Origem { get; set; }
        public string Situacao { get; set; }
        public string Matricula { get; set; }
        public string Equipe { get; set; }

        public static PipelineExportModel Mapear(Pipeline pipeline)
        {
            var pipeViewModel = new PipelineExportModel();

            pipeViewModel.Origem = pipeline.Origem != null ? pipeline.Origem.Descricao : null;
            pipeViewModel.ValorDoPipe = pipeline.ValorDoPipe;
            pipeViewModel.ValorAplicado = pipeline.ValorAplicado ?? null;
            pipeViewModel.DataPrevista = pipeline.DataPrevista;
            pipeViewModel.Comentario = pipeline.Observacoes ?? null;
            pipeViewModel.Cliente = pipeline.NomeCliente;
            pipeViewModel.Especialista = pipeline.Consultor;
            pipeViewModel.BradescoPrincipalBanco = pipeline.BradescoPrincipalBanco;
            pipeViewModel.ValorMercado = pipeline.ValoresNoMercado ?? null;
            pipeViewModel.Agencia = pipeline.Agencia;
            pipeViewModel.Conta = pipeline.Conta;
            pipeViewModel.Motivo = pipeline.Motivo?.Descricao ?? null;
            pipeViewModel.DataProrrogada = pipeline.DataProrrogada ?? null;
            pipeViewModel.Situacao = pipeline.Status?.Descricao ?? null;
            pipeViewModel.Matricula = pipeline.MatriculaConsultor;

            return pipeViewModel;
        }
    }
}
