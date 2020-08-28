using BradescoPGP.Common;
using BradescoPGP.Repositorio;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace BradescoPGPConsole.Commands
{
    class ImportarClusTopTier : AbstractCommand
    {
        private List<Encarteiramento> BackupEncarteiramento { get; set; }

        private List<Usuario> BackupHierarquia { get; set; }

        private List<Clusterizacoes> BackupClusterizacao { get; set; }

        protected override void PrepararDados()
        {
            using (var db = new PGPEntities())
            {
                BackupClusterizacao = db.Clusterizacoes.Where(u => u.AREA.Contains("TOP TIER")).ToList();

                ExecuteQuery("DELETE FROM [dbo].[Clusterizacoes] WHERE AREA LIKE 'TOP TIER%'");
            }
        }

        protected override void RealizarCarga()
        {
            using (var sr = new StreamReader(Config.Caminho))
            {
                var resultClus = ImportarClusterizacao(sr);

                if (!resultClus)
                {
                    Restaurar();

                    DisposeBackup();

                    Config.TeveFalha = true;

                    return;
                }

                Config.TeveFalha = false;
            }
        }

        public ImportarClusTopTier(ServiceConfig config) : base(config)
        {

        }

        private bool ImportarClusterizacao(StreamReader sr)
        {
            try
            {
                sr.DiscardBufferedData();

                sr.BaseStream.Seek(0, SeekOrigin.Begin);

                string linha = null;

                string[] linhaDividida = null;

                int linhacount = 0;

                List<Clusterizacoes> clusterizacaos = new List<Clusterizacoes>();

                while ((linha = sr.ReadLine()) != null)
                {
                    if (linhacount > 0)
                    {
                        linhaDividida = linha.SplitWithQualifier(';', '\"', true);

                        Clusterizacoes clusterizacao = new Clusterizacoes();

                        clusterizacao.CPF_CNPJ = linhaDividida[1];
                        clusterizacao.AGENCIA = int.Parse(linhaDividida[2]);
                        clusterizacao.CONTA = int.Parse(linhaDividida[0].Split('-')[1]);
                        clusterizacao.CodDiretoriaRegional = linhaDividida[6];
                        clusterizacao.DiretoriaRegional = linhaDividida[7];
                        clusterizacao.GER_RELC = !String.IsNullOrEmpty(linhaDividida[10]) ? linhaDividida[10] : string.Empty;
                        clusterizacao.Segmento = linhaDividida[16];
                        clusterizacao.ACAO_PRINCIPAL = linhaDividida[18];
                        clusterizacao.NIVEL_DESENQ_FX_RISCO = linhaDividida[23];
                        clusterizacao.PERFIL_API = linhaDividida[24];
                        clusterizacao.MES_VCTO_API = linhaDividida[25];
                        clusterizacao.SALDO_TOTAL = decimal.Parse(linhaDividida[37]);
                        clusterizacao.SALDO_TOTAL_M3 = decimal.Parse(linhaDividida[39]);
                        clusterizacao.SALDO_DAV_20K = decimal.Parse(linhaDividida[40]);
                        clusterizacao.SALDO_POUPANCA = decimal.Parse(linhaDividida[41]);
                        clusterizacao.SALDO_INVESTS = decimal.Parse(linhaDividida[44]);
                        clusterizacao.SALDO_CDB = decimal.Parse(linhaDividida[45]);
                        clusterizacao.SALDO_ISENTOS = decimal.Parse(linhaDividida[46]);
                        clusterizacao.SALDO_LF = decimal.Parse(linhaDividida[47]);
                        clusterizacao.SALDO_COMPROMISSADAS = decimal.Parse(linhaDividida[48]);
                        var saldoFundosAcoes = decimal.Parse(linhaDividida[49]);
                        var saldoFundosCambial = decimal.Parse(linhaDividida[50]);
                        var saldoFundosMultim = decimal.Parse(linhaDividida[51]);
                        var saldoFundosDI = decimal.Parse(linhaDividida[52]);
                        clusterizacao.SALDO_CORRETORA = decimal.Parse(linhaDividida[53]);
                        clusterizacao.SALDO_CORRETORA_BRA = decimal.Parse(linhaDividida[54]);
                        clusterizacao.SALDO_CORRETORA_AGORA = decimal.Parse(linhaDividida[55]);
                        clusterizacao.SALDO_PREVIDENCIA = decimal.Parse(linhaDividida[56]);

                        var fundos = new List<decimal>
                        {
                            saldoFundosAcoes,saldoFundosCambial,saldoFundosMultim,saldoFundosDI
                        };

                        clusterizacao.SALDO_FUNDOS = fundos.Sum();

                        clusterizacao.ACAO = !String.IsNullOrEmpty(linhaDividida[84]) ? linhaDividida[84] : string.Empty;
                        clusterizacao.AREA = "TOP TIER";

                        clusterizacaos.Add(clusterizacao);

                        if (clusterizacaos.Count == 200000)
                        {
                            using (PGPEntities db = new PGPEntities())
                            {
                                db.BulkInsert(clusterizacaos);
                            }

                            clusterizacaos.Clear();
                        }
                    }

                    linhacount++;
                }

                if (clusterizacaos.Count > 0)
                {
                    using (PGPEntities db = new PGPEntities())
                    {
                        db.BulkInsert(clusterizacaos);
                    }
                }
            }
            catch (Exception ex)
            {
                Config.TeveFalha = true;

                BradescoPGP.Common.Logging.Log.Error("CLUSTERIZAÇÃO TOP TIER: Erro ao importar Cluesterizacoes.", ex);

                return false;
            }

            return true;
        }

        private void Restaurar()
        {
            using (var db = new PGPEntities())
            {
                ExecuteQuery("DELETE FROM [dbo].[Clusterizacoes] WHERE AREA LIKE 'TOP TIER%'");

                db.BulkInsert(BackupClusterizacao);
            }

        }

        private void DisposeBackup()
        {
            BackupClusterizacao.Clear();
        }
    }
}
