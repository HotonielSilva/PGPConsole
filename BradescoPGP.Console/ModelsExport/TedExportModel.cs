using BradescoPGP.Repositorio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BradescoPGP.Console.ModelsExport
{
    public class TedExportModel
    {
        public string Agencia { get; set; }
        public string Conta { get; set; }
        public string NomeCliente { get; set; }
        public string MatriculaConsultor { get; set; }
        public string NomeConsultor { get; set; }
        public string MatriculaSupervisor { get; set; }
        public string NomeSupervisor { get; set; }
        public DateTime Data { get; set; }
        public decimal Valor { get; set; }
        public decimal? ValorAplicado { get; set; }
        public string Motivo { get; set; }
        public string MotivoOutrasInstituicoes { get; set; }
        public string Status { get; set; }
        public string Equipe { get; set; }
        public TedsContatos Contatos { get; set; }
        public ICollection<TedsAplicacoes> Aplicacoes { get; set; }

        public static TedExportModel Mapear(TED entity, string equipe = null)
        {
            return new TedExportModel
            {
                Motivo = entity.TedsMotivos?.Motivo,
                MotivoOutrasInstituicoes = entity.TedsMotivoOutrasInst?.Motivo,
                Agencia = entity.Agencia,
                Conta = entity.Conta,
                Data = entity.Data,
                MatriculaConsultor = entity.MatriculaConsultor,
                MatriculaSupervisor = entity.MatriculaSupervisor,
                NomeCliente = entity.NomeCliente,
                NomeSupervisor = entity.NomeSupervisor,
                Status = entity.Status.Descricao,
                Valor = entity.Valor,
                ValorAplicado = entity.TedsAplicacoes.Sum(s => s.ValorAplicado),
                NomeConsultor = entity.NomeConsultor,
                Equipe = equipe ?? string.Empty,
                Aplicacoes = entity.TedsAplicacoes,
                Contatos = entity.TedsContatos
        
            };
        }
    }
}
