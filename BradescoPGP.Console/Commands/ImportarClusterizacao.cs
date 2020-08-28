using BradescoPGP.Repositorio;
using System;
using System.Collections.Generic;
using System.IO;
using BradescoPGP.Common;

namespace BradescoPGPConsole.Commands
{
    public class ImportarClusterizacao : AbstractCommand
    {

        public ImportarClusterizacao(ServiceConfig config)
            : base(config)
        {
        }

        protected override void PrepararDados()
        {
            TruncarTabela("Clusterizacoes");
        }

        protected override void RealizarCarga()
        {
            try
            {
                BradescoPGP.Common.Logging.Log.Information("Iniciando importação de Cluesterizacoes.");

                using (var sr = new StreamReader(Config.Caminho))
                {
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

                            clusterizacao.AGENCIA = int.Parse(linhaDividida[0]);
                            clusterizacao.CONTA = int.Parse(linhaDividida[1].Split('-')[1]);
                            clusterizacao.CPF_CNPJ = linhaDividida[3];
                            clusterizacao.SALDO_CORRETORA_BRA = decimal.Parse(linhaDividida[48]);
                            clusterizacao.SALDO_CORRETORA_AGORA = decimal.Parse(linhaDividida[49]);
                            clusterizacao.SALDO_CORRETORA = decimal.Parse(linhaDividida[47]);
                            clusterizacao.SALDO_PREVIDENCIA = decimal.Parse(linhaDividida[50]);
                            clusterizacao.SALDO_POUPANCA = decimal.Parse(linhaDividida[37]);
                            clusterizacao.SALDO_INVESTS = decimal.Parse(linhaDividida[38]);
                            clusterizacao.SALDO_DAV_20K = decimal.Parse(linhaDividida[36]);
                            clusterizacao.SALDO_COMPROMISSADAS = decimal.Parse(linhaDividida[42]);
                            clusterizacao.SALDO_ISENTOS = decimal.Parse(linhaDividida[40]);
                            clusterizacao.SALDO_LF = decimal.Parse(linhaDividida[41]);
                            clusterizacao.SALDO_CDB = decimal.Parse(linhaDividida[39]);
                            clusterizacao.SALDO_TOTAL = decimal.Parse(linhaDividida[34]);
                            clusterizacao.SALDO_TOTAL_M3 = decimal.Parse(linhaDividida[35]);
                            clusterizacao.PERFIL_API = linhaDividida[21];
                            clusterizacao.MES_VCTO_API = linhaDividida[22];
                            clusterizacao.NIVEL_DESENQ_FX_RISCO = linhaDividida[20];
                            clusterizacao.COD_GER_RELC = !String.IsNullOrEmpty(linhaDividida[70]) ? linhaDividida[70] : string.Empty;
                            clusterizacao.GER_RELC = !String.IsNullOrEmpty(linhaDividida[71]) ? linhaDividida[71] : string.Empty;
                            clusterizacao.Segmento = linhaDividida[13];
                            clusterizacao.CodDiretoriaRegional = linhaDividida[6];
                            clusterizacao.DiretoriaRegional = linhaDividida[7];

                            
                            clusterizacao.SALDO_FUNDOS = 0;



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
                BradescoPGP.Common.Logging.Log.Information("Importação de Cluesterizacoes finalizada.");
            }
            catch (Exception ex)
            {
                Config.TeveFalha = true;

                BradescoPGP.Common.Logging.Log.Error("CLUSTERIZAÇÃO: Erro ao importar Cluesterizacoes.", ex);
            }
        }
    }
}
