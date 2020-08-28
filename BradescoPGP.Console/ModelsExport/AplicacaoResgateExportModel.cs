using BradescoPGP.Repositorio;
using System;

namespace BradescoPGP.Console.ModelsExport
{
    public class AplicacaoResgateExportModel
    {
        public int Id { get; set; }
        public int agencia { get; set; }
        public int conta { get; set; }
        public DateTime data { get; set; }
        public TimeSpan hora { get; set; }
        public string operacao { get; set; }
        public string perif { get; set; }
        public string produto { get; set; }
        public string terminal { get; set; }
        public decimal valor { get; set; }
        public string gerente { get; set; }
        public string advisor { get; set; }
        public string segmento { get; set; }
        public bool? enviado { get; set; }
        public string Especialista { get; set; }
        public string Matricula { get; set; }

        public static AplicacaoResgateExportModel Mapear(AplicacaoResgate aplicacao, string especialista)
        {
            return new AplicacaoResgateExportModel
            {
                advisor = aplicacao.advisor,
                agencia = aplicacao.agencia,
                conta = aplicacao.conta,
                data = aplicacao.data,
                hora = aplicacao.hora,
                operacao = aplicacao.operacao,
                perif = aplicacao.perif,
                produto = aplicacao.produto,
                terminal = aplicacao.terminal,
                valor = aplicacao.valor,
                gerente = aplicacao.gerente,
                segmento = aplicacao.segmento,
                enviado = aplicacao.enviado,
                Especialista = especialista,
                Matricula = aplicacao.MatriculaConsultor
            };
        }
    }
}
