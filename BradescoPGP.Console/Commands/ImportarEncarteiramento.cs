
using NPOI.XSSF.UserModel;
using System;
using System.Collections;
using BradescoPGP.Repositorio;
using System.Collections.Generic;
using System.IO;
using BradescoPGP.Common;
//using BradescoPGP.Repositorio;

namespace BradescoPGPConsole.Commands
{
    public class ImportarEncarteiramento : AbstractCommand
    {
        public ImportarEncarteiramento(ServiceConfig config)
            : base(config)
        {
        }

        protected override void PrepararDados()
        {
            TruncarTabela("Encarteiramento");
        }

        protected override void RealizarCarga()
        {
            string linha = null;
            try
            {
                BradescoPGP.Common.Logging.Log.Information("Iniciando importação de Encarteiramento.");

                using (var sr = new StreamReader(Config.Caminho))
                {

                    string[] linhaDividida = null;

                    int linhacount = 0;

                    List<Encarteiramento> encarteiramentos = new List<Encarteiramento>();

                    while ((linha = sr.ReadLine()) != null)
                    {
                        if (linhacount > 0)
                        {
                            linhaDividida = linha.SplitWithQualifier(';', '\"', true);

                            Encarteiramento encarteiramento = new Encarteiramento();

                            encarteiramento.Agencia = linhaDividida[0];
                            encarteiramento.Conta = linhaDividida[1].Split('-')[1];
                            encarteiramento.CPF = linhaDividida[2] != "" ? long.Parse(linhaDividida[2]).ToString() : linhaDividida[2];
                            encarteiramento.AG_PRINC = encarteiramento.Agencia;
                            encarteiramento.CONTA_PRINC = encarteiramento.Conta;
                            encarteiramento.CONSULTOR = linhaDividida[73];
                            encarteiramento.Matricula = linhaDividida[72];
                            encarteiramento.EQUIPE_RESPONSAVEL = linhaDividida[78];
                            encarteiramento.EQUIPE_MESA = linhaDividida[78];
                            encarteiramento.DIR_REG_AG_PRINC = linhaDividida[7];

                            encarteiramentos.Add(encarteiramento);

                            if (encarteiramentos.Count == 200000)
                            {
                                using (var db = new PGPEntities())
                                {
                                    db.BulkInsert(encarteiramentos);
                                    encarteiramentos.Clear();
                                }
                            }
                        }

                        linhacount++;
                    }

                    if (encarteiramentos.Count > 0)
                    {
                        using (PGPEntities db = new PGPEntities())
                        {
                            db.BulkInsert(encarteiramentos);
                        }
                    }
                }

                BradescoPGP.Common.Logging.Log.Information("Importação de Encarteiramento finalizada.");
            }
            catch (Exception ex)
            {
                Config.TeveFalha = true;

                BradescoPGP.Common.Logging.Log.Error("ENCARTEIRAMENTO: Erro ao importar Encarteiramento.", ex);
            }
        }

        private void ComExcelAntigo()
        {
            try
            {
                BradescoPGP.Common.Logging.Log.Information("Iniciando importação de Encarteiramento.");

                using (PGPEntities db = new PGPEntities())
                using (FileStream fs = new FileStream(Config.Caminho, FileMode.Open, FileAccess.Read))
                {
                    XSSFWorkbook workbook = new XSSFWorkbook(fs);
                    XSSFSheet planilha = (XSSFSheet)workbook.GetSheetAt(0);
                    IEnumerator linhas = planilha.GetRowEnumerator();
                    List<Encarteiramento> encarteiramentos = new List<Encarteiramento>();
                    int linhaCount = 0;

                    while (linhas.MoveNext())
                    {
                        Encarteiramento encarteiramento = new Encarteiramento();
                        XSSFRow linhaAtual = (XSSFRow)linhas.Current;
                        if (linhaAtual.RowNum > 0)
                        {
                            encarteiramento.Agencia = linhaAtual.Cells[0].NumericCellValue.ToString();
                            encarteiramento.Conta = linhaAtual.Cells[1].NumericCellValue.ToString();
                            encarteiramento.TIP_CLIENTE = linhaAtual.Cells[3].NumericCellValue.ToString();
                            encarteiramento.CPF = linhaAtual.Cells[4].NumericCellValue.ToString();
                            encarteiramento.DATA = linhaAtual.Cells[5].DateCellValue;
                            encarteiramento.AG_PRINC = linhaAtual.Cells[6].NumericCellValue.ToString();
                            encarteiramento.CONTA_PRINC = linhaAtual.Cells[7].NumericCellValue.ToString();
                            encarteiramento.CONSULTOR = linhaAtual.Cells[8].StringCellValue;
                            encarteiramento.Matricula = linhaAtual.Cells[9].NumericCellValue.ToString();
                            encarteiramento.EQUIPE_RESPONSAVEL = linhaAtual.Cells[10].StringCellValue;
                            encarteiramento.EQUIPE_MESA = linhaAtual.Cells[11].StringCellValue;
                            encarteiramento.DIR_REG_AG_PRINC = linhaAtual.Cells[12].StringCellValue;
                            encarteiramento.GER_REG_AG_PRINC = linhaAtual.Cells[13].StringCellValue;

                            encarteiramentos.Add(encarteiramento);
                            if (encarteiramentos.Count == 100000)
                            {
                                db.BulkInsert(encarteiramentos);
                                encarteiramentos.Clear();
                            }
                        }
                        linhaCount++;
                    }

                    if (encarteiramentos.Count > 0)
                    {
                        db.BulkInsert(encarteiramentos);
                    }
                }
                BradescoPGP.Common.Logging.Log.Information("Importação de Encarteiramento finalizada.");
            }
            catch (Exception ex)
            {
                Config.TeveFalha = true;

                BradescoPGP.Common.Logging.Log.Error("ENCARTEIRAMENTO: Erro ao importar Encarteiramento.", ex);
            }

        }
    }
}
