using System;
using System.Collections.Generic;
using System.IO;
using BradescoPGP.Repositorio;
using BradescoPGP.Common;
using System.Text;
using System.Configuration;
using System.Data.SqlClient;

namespace BradescoPGPConsole.Commands
{
    public class ImportarCaptacaoLiquida : AbstractCommand
    {
        public ImportarCaptacaoLiquida(ServiceConfig config)
            : base(config)
        {
        }

        protected override void RealizarCarga()
        {
            int linhacount = 0;

            try
            {
                BradescoPGP.Common.Logging.Log.Information("Iniciando importação de Captação Liquida.");

                using (var sr = new StreamReader(Config.Caminho, Encoding.GetEncoding("ISO8859-1")))
                {
                    string linha = null;

                    string[] linhaDividida = null;

                    List<CaptacaoLiquida> captacaoLiquida = new List<CaptacaoLiquida>();


                    while ((linha = sr.ReadLine()) != null)
                    {
                        linhaDividida = linha.SplitWithQualifier(';', '\"', true);

                        var agenciaConta = linhaDividida[0];

                        if (linhacount > 0)
                        {
                            var capLiq = new CaptacaoLiquida();

                            capLiq.Agencia = agenciaConta.Split('-')[0];

                            capLiq.Conta = agenciaConta.Split('-')[1];

                            capLiq.CodAgencia = linhaDividida[1];

                            capLiq.Ag_Conta = agenciaConta;

                            capLiq.Diretoria = linhaDividida[2];

                            capLiq.TipoPessoa = linhaDividida[3];

                            capLiq.GerenciaRegional = linhaDividida[4];

                            capLiq.MatriculaCordenador = linhaDividida[5];

                            capLiq.CordenadorPGP = linhaDividida[6];

                            capLiq.MatriculaConsultor = linhaDividida[7];

                            capLiq.Consultor = linhaDividida[8];

                            capLiq.Produto = linhaDividida[10];

                            capLiq.MesDataBase = linhaDividida[11];

                            if (DateTime.TryParse(linhaDividida[12], out var dataBase))capLiq.DataBase = dataBase;

                            if(capLiq.DataBase <= DateTime.MinValue || capLiq.DataBase >= DateTime.MaxValue)
                            {
                                capLiq.DataBase = null;
                            }
                                
                            if (decimal.TryParse(linhaDividida[13], out decimal valorAplic)) capLiq.ValorAplicacao = valorAplic; //5

                            if (decimal.TryParse(linhaDividida[14], out decimal valorResg)) capLiq.ValorResgate = valorResg; //6

                            if (decimal.TryParse(linhaDividida[15], out decimal vlNET)) capLiq.ValorNET = vlNET;

                            captacaoLiquida.Add(capLiq);

                            if (captacaoLiquida.Count == 200000)
                            {
                                using (PGPEntities db = new PGPEntities())
                                {
                                    db.BulkInsert(captacaoLiquida);
                                }

                                captacaoLiquida.Clear();
                            }
                        }

                        linhacount++;
                    }

                    if (captacaoLiquida.Count > 0)
                    {
                        using (PGPEntities db = new PGPEntities())
                        {
                            db.BulkInsert(captacaoLiquida);
                        }
                    }
                }

                BradescoPGP.Common.Logging.Log.Information("Importação de Captação Liquida finalizada.");
            }
            catch (Exception ex)
            {
                Config.TeveFalha = true;

                BradescoPGP.Common.Logging.Log.Error("Captação Liquida: Erro ao importar Captação Liquida.", ex);
            }

            //Caminho do Dinheiro
            var camDinInst = ImportarCaminhoDinheiro.ObterInstancia();

            if (!Config.TeveFalha && camDinInst.EstaPronto())
            {
                camDinInst.Importar();

                ObterCaptacaoLiquidaExcel();
            }
        }

        protected override void PrepararDados()
        {
            //TruncarTabela("CaptacaoLiquidaResumo");
            TruncarTabela("CaptacaoLiquida");
        }

        private void ObterCaptacaoLiquidaExcel()
        {
            BradescoPGP.Common.Logging.Log.Information("Iniciando o processo de resumo Captação Líquida");
            try
            {
                using (var con = new SqlConnection(ConfigurationManager.ConnectionStrings["PGP"].ConnectionString))
                {
                    con.Open();

                    using (var cmd = new SqlCommand("sp_CapLiqResumo", con))
                    {
                        //Aceita o paramentro do mes base referente
                        cmd.CommandType = System.Data.CommandType.StoredProcedure;

                        if (cmd.ExecuteNonQuery() <= 0)
                        {
                            BradescoPGP.Common.Logging.Log.Debug("CAPTAÇÃO LIQUIDA: Nenhum registro encontrado referente ao mes atual para gerar o resumo da captação");
                        }
                    }
                }

                BradescoPGP.Common.Logging.Log.Information("Processo de resumo Captação Líquida finalizado.");
            }
            catch (Exception e)
            {
                Config.TeveFalha = true;

                BradescoPGP.Common.Logging.Log.Error("CAPTAÇÃO LIQUIDA: Erro na importação", e);
            }
        }

    }

    #region Versao Antiga

    //private void VersaoAntiga()
    //{
    //    try
    //    {
    //        BradescoPGP.Common.Logging.Log.Information("Iniciando importação de CaptacaoLiquida.");

    //        using (FileStream file = new FileStream(Config.Caminho, FileMode.Open, FileAccess.Read))
    //        {
    //            var wk = new XSSFWorkbook(file);

    //            var plan = wk.GetSheet("D_BASE_PGP");

    //            IEnumerator linhas = plan.GetRowEnumerator();

    //            List<CaptacaoLiquidaResumo> captacoesLiquidas = new List<CaptacaoLiquidaResumo>();

    //            var formatNegative = new NumberFormatInfo();

    //            formatNegative.NegativeSign = "-";

    //            string matricula = null;

    //            while (linhas.MoveNext())
    //            {
    //                var linhaAtual = (XSSFRow)linhas.Current;

    //                if (linhaAtual.RowNum > 1)
    //                {
    //                    //Verifica se é a ultima linha
    //                    if (linhaAtual.Cells[0].CellType == CellType.String && linhaAtual.Cells[0].StringCellValue == "Total Geral")
    //                    {
    //                        break;
    //                    }

    //                    var captacao = new CaptacaoLiquidaResumo();
    //                    //Verifica se tem a matricula na linha atual, caso não os dados são do mesmo consultor.

    //                    if (linhaAtual.Cells.Count == 13)
    //                    {
    //                        matricula = linhaAtual.Cells[0].NumericCellValue.ToString();

    //                        captacao.MatriculaConsultor = matricula;
    //                        captacao.ProdutoMacro = linhaAtual.Cells[1].StringCellValue;
    //                        captacao.VL_NET = decimal.Parse(linhaAtual.Cells[3].NumericCellValue.ToString(), NumberStyles.Float);
    //                    }
    //                    else
    //                    {
    //                        captacao.MatriculaConsultor = matricula;
    //                        captacao.ProdutoMacro = linhaAtual.Cells[0].StringCellValue;
    //                        captacao.VL_NET = decimal.Parse(linhaAtual.Cells[2].NumericCellValue.ToString(), NumberStyles.Float);
    //                    }

    //                    captacoesLiquidas.Add(captacao);

    //                    if (linhaAtual.RowNum == 100000)
    //                    {
    //                        using (PGPEntities db = new PGPEntities())
    //                        {
    //                            db.BulkInsert(captacoesLiquidas);
    //                            captacoesLiquidas.Clear();
    //                        }
    //                    }
    //                }
    //            }
    //            if (captacoesLiquidas.Count > 0)
    //            {
    //                using (PGPEntities db = new PGPEntities())
    //                {
    //                    db.BulkInsert(captacoesLiquidas);
    //                }
    //            }
    //        }

    //        BradescoPGP.Common.Logging.Log.Information("Importação de CaptacaoLiquida finalizada.");
    //    }
    //    catch (Exception ex)
    //    {
    //        BradescoPGP.Common.Logging.Log.Error("CAPTAÇÃO LIQUIDA: Erro ao importar CaptacaoLiquida.", ex);
    //    }
    //}
    #endregion
}

