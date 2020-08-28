using System;
using System.Collections.Generic;
using System.IO;
using NPOI.XSSF.UserModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using BradescoPGP.Repositorio;
using BradescoPGP.Common.Logging;
using OfficeOpenXml;
using System.Text.RegularExpressions;
using System.Data.SqlClient;
using System.Reflection;

namespace BradescoPGPConsole.Commands
{
    public class ImportarQualitativo : AbstractCommand
    {

        public ImportarQualitativo(ServiceConfig config)
            : base(config)
        {
        }

        protected override void PrepararDados()
        {
            TruncarTabela("Qualitativo");
        }

        protected override void RealizarCarga()
        {
            try
            {
                BradescoPGP.Common.Logging.Log.Information("Iniciando importação de Qualitativo.");

                List<Qualitativo> qualitativos = new List<Qualitativo>();

                #region comentario
                ////using (var db = new PGPEntities())
                //using (FileStream arquivo = new FileStream(Config.Caminho, FileMode.Open, FileAccess.Read))
                //{
                //    XSSFWorkbook workbook = new XSSFWorkbook(arquivo);

                //    XSSFSheet planilha = (XSSFSheet)workbook.GetSheet("RESUMO TOTAL");

                //    IEnumerator linhas = planilha.GetRowEnumerator();
                //    int linhaInicial = 4;

                //    while (linhas.MoveNext())
                //    {
                //        var linhaAtual = (XSSFRow)linhas.Current;

                //        if (linhaAtual.RowNum >= linhaInicial)
                //        {
                //            if (linhaAtual.Cells.Count > 1 && !linhaAtual.Cells[1].StringCellValue.ToUpper().Contains("EQUIPE") &&
                //                !String.IsNullOrEmpty(linhaAtual.Cells[1].StringCellValue))
                //            {
                //                var qualitativo = new Qualitativo();
                //                using (var db = new PGPEntities())
                //                {
                //                    var usuarios = db.Usuario.ToList();

                //                    var celzero = linhaAtual.Cells[0].StringCellValue;
                //                    int inicio = celzero.IndexOf('(') + 1;
                //                    var user = celzero.Substring(inicio).Replace(")", "");
                //                    var consultor = usuarios.FirstOrDefault(x => x.NomeUsuario == user.ToLower());
                //                    qualitativo.MatriculaConsultor = consultor != null ? consultor.Matricula : user;
                //                    qualitativo.NomeConsultor = consultor != null ? consultor.Nome : celzero.Substring(0, celzero.IndexOf('(') - 1);
                //                }

                //                qualitativo.OBJETIVOTOTAL = (int)linhaAtual.Cells[2].NumericCellValue;
                //                qualitativo.DENTRODACARTEIRA = (int)linhaAtual.Cells[3].NumericCellValue;
                //                qualitativo.FORADACARTEIRA = (int)linhaAtual.Cells[4].NumericCellValue;
                //                qualitativo.TOTALCONTATOS = (int)linhaAtual.Cells[5].NumericCellValue;
                //                qualitativo.PORCENTAGEMATINGIMENTO = linhaAtual.Cells[6].NumericCellValue.ToString();
                //                qualitativo.GIRODECARTEIRAOBJETIVO = (int)linhaAtual.Cells[7].NumericCellValue;
                //                qualitativo.GIRODECARTEIRAOREALIZADO = (int)linhaAtual.Cells[8].NumericCellValue;
                //                qualitativo.PORCENTAGEMATINGIMENTOGIRO = linhaAtual.Cells[9].NumericCellValue.ToString();
                //                qualitativo.REVISAOFINANCEIRAOBJETIVO = linhaAtual.Cells[10].NumericCellValue.ToString();
                //                qualitativo.REVISAOFINANCEIRAREALIZADO = (int)linhaAtual.Cells[11].NumericCellValue;
                //                qualitativo.PORCENTAGEMATINGIMENTOREVISAO = linhaAtual.Cells[12].NumericCellValue.ToString();
                //                qualitativo.CADASTROAPIOBJETIVO = linhaAtual.Cells[13].NumericCellValue.ToString();
                //                qualitativo.CADASTROAPIREALIZADO = (int)linhaAtual.Cells[14].NumericCellValue;
                //                qualitativo.PORCENTAGEMATINGIMENTOCADASTROAPI = linhaAtual.Cells[15].NumericCellValue.ToString();

                //                qualitativos.Add(qualitativo);
                //            }
                //        }
                //    }
                //    using (var db = new PGPEntities())
                //    {
                //        //Inseri registros no banco
                //        db.BulkInsert(qualitativos);
                //    }
                //}
                #endregion

                using (var excel = new ExcelPackage(new FileInfo(Config.Caminho)))
                {
                    var planilha = excel.Workbook.Worksheets["RESUMO TOTAL"];

                    for (int linha = 5; linha < planilha.Dimension.Rows; linha++)
                    {
                        if (!String.IsNullOrEmpty(planilha.Cells[linha, 2].Value?.ToString()))
                        {
                            if(!planilha.Cells[linha, 2].Value.ToString().Contains("EQUIPE"))
                            {
                                var qualitativo = new Qualitativo();
                                using (var db = new PGPEntities())
                                {
                                    var usuarios = db.Usuario.ToList();

                                    var celzero = planilha.Cells[linha, 1].Value.ToString();
                                    int inicio = celzero.IndexOf('(') + 1;
                                    var user = celzero.Substring(inicio).Replace(")", "");
                                    var consultor = usuarios.FirstOrDefault(x => x.NomeUsuario == user.ToLower());
                                    qualitativo.NomeConsultor = consultor != null ? consultor.Nome : celzero.Substring(0, celzero.IndexOf('(') - 1);
                                }

                                qualitativo.OBJETIVOTOTAL = int.Parse(Math.Ceiling(decimal.Parse(planilha.Cells[linha, 3].Value.ToString())).ToString());
                                qualitativo.DENTRODACARTEIRA = int.Parse(Math.Ceiling(decimal.Parse(planilha.Cells[linha, 4].Value.ToString())).ToString());
                                qualitativo.FORADACARTEIRA = int.Parse(Math.Ceiling(decimal.Parse(planilha.Cells[linha, 5].Value.ToString())).ToString());
                                qualitativo.TOTALCONTATOS = int.Parse(Math.Ceiling(decimal.Parse(planilha.Cells[linha, 6].Value.ToString())).ToString());
                                qualitativo.PORCENTAGEMATINGIMENTO = decimal.Parse(planilha.Cells[linha, 7].Value.ToString()).ToString();
                                qualitativo.GIRODECARTEIRAOBJETIVO = int.Parse(Math.Ceiling(decimal.Parse(planilha.Cells[linha, 8].Value.ToString())).ToString());
                                qualitativo.GIRODECARTEIRAOREALIZADO = int.Parse(Math.Ceiling(decimal.Parse(planilha.Cells[linha, 9].Value.ToString())).ToString());
                                qualitativo.PORCENTAGEMATINGIMENTOGIRO = decimal.Parse(planilha.Cells[linha, 10].Value.ToString()).ToString();
                                qualitativo.REVISAOFINANCEIRAOBJETIVO = Math.Ceiling(decimal.Parse(planilha.Cells[linha, 11].Value.ToString())).ToString();
                                qualitativo.REVISAOFINANCEIRAREALIZADO = int.Parse(Math.Ceiling(decimal.Parse(planilha.Cells[linha, 12].Value.ToString())).ToString());
                                qualitativo.PORCENTAGEMATINGIMENTOREVISAO = decimal.Parse(planilha.Cells[linha, 13].Value.ToString()).ToString();
                                qualitativo.CADASTROAPIOBJETIVO = Math.Ceiling(decimal.Parse(planilha.Cells[linha, 14].Value.ToString())).ToString();
                                qualitativo.CADASTROAPIREALIZADO = double.Parse(Math.Ceiling(decimal.Parse(planilha.Cells[linha, 15].Value.ToString())).ToString());
                                qualitativo.PORCENTAGEMATINGIMENTOCADASTROAPI = decimal.Parse(planilha.Cells[linha, 16].Value.ToString()).ToString();

                                qualitativos.Add(qualitativo);
                            }
                        }
                        else
                        {
                            break;
                        }
                    }

                    using (var db = new PGPEntities())
                    {
                        //Inseri registros no banco
                        db.BulkInsert(qualitativos);
                    }
                }

                BradescoPGP.Common.Logging.Log.Information("Importação de Qualitativo finalizada.");
            }
            catch (Exception ex)
            {
                Config.TeveFalha = true;

                BradescoPGP.Common.Logging.Log.Error("QUALITATIVO: Erro ao importar Qualitativo.", ex);
            }
        }
    }
}
