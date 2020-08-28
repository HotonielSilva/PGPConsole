using BradescoPGP.Common;
using BradescoPGP.Repositorio;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace BradescoPGPConsole.Commands
{
    public class ImportarPortabilidade : AbstractCommand
    {
        public ImportarPortabilidade(ServiceConfig config) : base(config)
        {
        }

        protected override void PrepararDados()
        { }

        protected override void RealizarCarga()
        {
            BradescoPGP.Common.Logging.Log.Information("Iniciando Importação de Portabilidade");

            var count = 0;

            var encarteiramento = default(List<Encarteiramento>);
            var statusIdInicial = default(int);

            using (var db = new PGPEntities())
            {
                encarteiramento = db.Encarteiramento.Distinct().ToList();

                statusIdInicial = db.Status.FirstOrDefault(s => s.Descricao.StartsWith("Não Trat") &&
                s.Evento == Eventos.Portabilidade.ToString()).Id;
            }

            try
            {
                using (var excel = new ExcelPackage(new FileInfo(Config.Caminho)))
                {
                    var sheet = excel.Workbook.Worksheets[2];

                    var Solicitacoes = new List<Solicitacao>();

                    for (int i = 2; i <= sheet.Dimension.Rows; i++)
                    {
                        count = i;
                        var segmento = sheet.Cells[i, 1].Value?.ToString();
                        var lideranca = sheet.Cells[i, 2].Value?.ToString();
                        var consultorMatriz = sheet.Cells[i, 3].Value?.ToString();
                        var consultorPGP = sheet.Cells[i, 4].Value?.ToString();
                        var nomeParticipante = sheet.Cells[i, 5].Value?.ToString();
                        var CPF = sheet.Cells[i, 6].Value?.ToString();

                        if (!string.IsNullOrEmpty(CPF) && CPF.Length > 11)
                            continue;

                        if (string.IsNullOrEmpty(segmento) && string.IsNullOrEmpty(lideranca) && string.IsNullOrEmpty(consultorMatriz) && string.IsNullOrEmpty(consultorPGP)
                        && string.IsNullOrEmpty(nomeParticipante) && string.IsNullOrEmpty(CPF))
                            break;

                        var solicitacao = new Solicitacao();

                        solicitacao.Segmento = segmento;

                        solicitacao.Lideranca = lideranca;

                        solicitacao.ConsultorMatriz = consultorMatriz;

                        solicitacao.ConsultorPGP = consultorPGP;

                        solicitacao.NomeParticipante = nomeParticipante;

                        solicitacao.CPF = CPF;

                        if (sheet.Cells[i, 7].Value != null) solicitacao.SaldoPrevidencia = Convert.ToDecimal(sheet.Cells[i, 7].Value);

                        if (sheet.Cells[i, 8].Value != null) solicitacao.ValorPrevistoSaida = Convert.ToDecimal(sheet.Cells[i, 8].Value);

                        solicitacao.NomeEntidade = sheet.Cells[i, 9].Value?.ToString();

                        if (DateTime.TryParse(sheet.Cells[i, 10].Value?.ToString(), out var dataInicioProcesso)) solicitacao.DataInicioProcesso = dataInicioProcesso;

                        if (DateTime.TryParse(sheet.Cells[i, 11].Value?.ToString(), out var prazoAtendimento)) solicitacao.PrazoAtendimento = prazoAtendimento;

                        if (DateTime.TryParse(sheet.Cells[i, 12].Value?.ToString(), out var dataRef)) solicitacao.DataRef = dataRef;

                        solicitacao.CodIdentificadorProcesso = sheet.Cells[i, 13].Value?.ToString();

                        solicitacao.CodIdentificadorProposta = sheet.Cells[i, 14].Value?.ToString();

                        solicitacao.SUSEPCedente = sheet.Cells[i, 15].Value?.ToString();

                        solicitacao.SUSEPCessionaria = sheet.Cells[i, 16].Value?.ToString();

                        solicitacao.CIDTFDCNPJCedente = sheet.Cells[i, 17].Value?.ToString();

                        solicitacao.CIDTFDCNPJCessionaria = sheet.Cells[i, 18].Value?.ToString();

                        solicitacao.MatriculaConsultor = encarteiramento.FirstOrDefault(s => s.CPF == CPF.Substring(0, CPF.Length - 2))?.Matricula ?? "";

                        solicitacao.StatusId = statusIdInicial;

                        var conta = encarteiramento.FirstOrDefault(s => s.CPF == CPF.Substring(0, CPF.Length - 2))?.CONTA_PRINC;

                        if (conta != null) solicitacao.Conta = int.Parse(conta);

                        var agencia = encarteiramento.FirstOrDefault(s => s.CPF == CPF.Substring(0, CPF.Length - 2))?.AG_PRINC;

                        if (agencia != null) solicitacao.Agencia = int.Parse(agencia);

                        if (!ExisteNaBase(solicitacao))
                            Solicitacoes.Add(solicitacao);

                    }

                    if (Solicitacoes.Count > 0)
                    {
                        using (var db = new PGPEntities())
                        {
                            db.BulkInsert(Solicitacoes);
                        }
                    }

                    BradescoPGP.Common.Logging.Log.Information("Importação de Portabilidade Realizada com sucesso");
                }
            }
            catch (Exception ex)
            {
                Config.TeveFalha = true;
                //Log
                BradescoPGP.Common.Logging.Log.Error("PORTABILIDADE: Erro ao importar portabilidade", ex);
            }
        }

        private bool ExisteNaBase(Solicitacao solicitacao)
        {
            using (var db = new PGPEntities())
            {
                return db.Solicitacao.Any(s =>
                s.DataInicioProcesso == solicitacao.DataInicioProcesso &&
                s.ValorPrevistoSaida == solicitacao.ValorPrevistoSaida &&
                s.CPF == solicitacao.CPF &&
                s.DataConclusao == solicitacao.DataConclusao &&
                s.SaldoPrevidencia == solicitacao.SaldoPrevidencia &&
                s.DataRef == solicitacao.DataRef &&
                s.SUSEPCedente == solicitacao.SUSEPCedente &&
                s.SUSEPCessionaria == solicitacao.SUSEPCessionaria &&
                s.CodIdentificadorProposta == solicitacao.CodIdentificadorProposta);
            }
        }
    }
}
