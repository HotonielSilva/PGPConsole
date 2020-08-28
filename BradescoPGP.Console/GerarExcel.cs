using BradescoPGP.Common.Logging;
using BradescoPGP.Console.ModelsExport;
using BradescoPGP.Repositorio;
using OfficeOpenXml;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace BradescoPGP.Console
{
    public static class GerarExcel
    {
        public static void Criar<T>(List<T> dados, string nomePlanilha, string nomeArquivo, string destino)
        {
            if (!dados.Any())
                return;

            var file = new ExcelPackage();

            var wk = file.Workbook;

            var colunasExcluidas = new string[] { "Contatos", "Aplicacoes", "MatriculaSupervisor", "enviado" };

            var properites = typeof(T).GetProperties()
                .Where(p => !p.Name.Contains("Id") && !colunasExcluidas.Contains(p.Name)).ToArray();

            var sheet = wk.Worksheets.Add(nomePlanilha);

            int linha = 2;

            var type = typeof(T);

            var produtos = default(List<string>);

            int i = 1;

            //Mesclagem primeira Linha
            sheet.Cells[1, 1].Style.Font.Bold = true;
            sheet.Cells[1, 1].Style.Font.Size = 16;
            sheet.Cells[1, 1].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
            sheet.Cells[1, 1, 1, properites.Length].Merge = true;
            sheet.Cells[1, 1, 1, properites.Length].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);
            sheet.Cells[1, 1, 1, properites.Length].AutoFitColumns();

            //Cabechalhos
            foreach (var p in properites)
            {
                sheet.Cells[linha, i].Value = p.Name;
                sheet.Cells[linha, i].AutoFitColumns();
                i++;
            }

            if (type == typeof(TedExportModel))
            {
                var contatos = typeof(TedsContatos).GetProperties().Where(p => !p.Name.Contains("Id") && !p.Name.Contains("TED")).ToArray();

                //Mesclagem primeira Linha Contatos
                sheet.Cells[1, i].Style.Font.Bold = true;
                sheet.Cells[1, i].Value = "Contatos";
                sheet.Cells[1, i].Style.Font.Size = 16;
                sheet.Cells[1, i].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                sheet.Cells[1, i, 1, i + contatos.Length - 1].Merge = true;
                sheet.Cells[1, i, 1, i + contatos.Length - 1].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);
                sheet.Cells[1, i, 1, i + contatos.Length - 1].AutoFitColumns();

                foreach (var contato in contatos)
                {
                    sheet.Cells[linha, i].Value = contato.Name;
                    sheet.Cells[linha, i].AutoFitColumns();
                    i++;
                }

                using (var db = new PGPEntities())
                {
                    produtos = db.TedsProdutos.Select(s => s.Produto).ToList();

                    //Mesclagem primeira Linha Contatos

                    sheet.Cells[1, i].Style.Font.Bold = true;
                    sheet.Cells[1, i].Value = "Aplicações";
                    sheet.Cells[1, i].Style.Font.Size = 16;
                    sheet.Cells[1, i].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                    sheet.Cells[1, i, 1, i + produtos.Count - 1].Merge = true;
                    sheet.Cells[1, i, 1, i + produtos.Count - 1].Style.Border.BorderAround(OfficeOpenXml.Style.ExcelBorderStyle.Thin);
                    sheet.Cells[1, i, 1, i + produtos.Count - 1].AutoFitColumns();

                    foreach (var produto in produtos)
                    {

                        sheet.Cells[linha, i].Value = produto;
                        sheet.Cells[linha, i].AutoFitColumns();
                        i++;
                    }
                }
            }

            sheet.Cells[linha, 1, linha, properites.Length].Style.Font.Bold = true;
            sheet.Cells[linha, 1, linha, properites.Length].AutoFilter = true;

            //Dados
            foreach (var dado in dados)
            {
                linha++;

                if (type == typeof(VencimentoExportModel))
                {
                    sheet.Cells[1, 1].Value = "Vencimentos";

                    var v = dado as VencimentoExportModel;

                    sheet.Cells[linha, 1].Value = v.Especialista;
                    sheet.Cells[linha, 2].Value = v.Produto;
                    sheet.Cells[linha, 3].Value = v.SaldoAtual;
                    sheet.Cells[linha, 4].Value = v.Agencia;
                    sheet.Cells[linha, 5].Value = v.Conta;
                    sheet.Cells[linha, 6].Value = v.DataVencimento.ToShortDateString();
                    sheet.Cells[linha, 7].Value = v.PercentualIndexador;
                    sheet.Cells[linha, 8].Value = v.Cliente;
                    sheet.Cells[linha, 9].Value = v.Status;
                    sheet.Cells[linha, 10].Value = string.IsNullOrEmpty(v.Matriucla) ? 0 : int.Parse(v.Matriucla);
                    sheet.Cells[linha, 11].Value = v.Equipe;
                }
                else if (type == typeof(TedExportModel))
                {
                    sheet.Cells[1, 1].Value = "TEDs";

                    var v = dado as TedExportModel;
                    sheet.Cells[linha, 1].Value = int.Parse(v.Agencia);
                    sheet.Cells[linha, 2].Value = int.Parse(v.Conta);
                    sheet.Cells[linha, 3].Value = v.NomeCliente;
                    if (int.TryParse(v.MatriculaConsultor, out int matricula))
                    {
                        sheet.Cells[linha, 4].Value = matricula;
                    }
                    else
                    {
                        sheet.Cells[linha, 4].Value = string.Empty;
                    }
                    sheet.Cells[linha, 5].Value = v.NomeConsultor;
                    sheet.Cells[linha, 6].Value = v.NomeSupervisor;
                    sheet.Cells[linha, 7].Value = v.Data.ToShortDateString();
                    sheet.Cells[linha, 8].Value = v.Valor;
                    sheet.Cells[linha, 9].Value = v.ValorAplicado;
                    sheet.Cells[linha, 10].Value = v.Motivo;
                    sheet.Cells[linha, 11].Value = v.MotivoOutrasInstituicoes;
                    sheet.Cells[linha, 12].Value = v.Status;
                    sheet.Cells[linha, 13].Value = v.Equipe;

                    //contatos
                    sheet.Cells[linha, 14].Value = v.Contatos != null && v.Contatos.ContatouCliente.HasValue && v.Contatos.ContatouCliente.Value ? "Sim" : "Não";
                    sheet.Cells[linha, 15].Value = v.Contatos != null && v.Contatos.ContatouGerente.HasValue && v.Contatos.ContatouGerente.Value ? "Sim" : "Não";
                    sheet.Cells[linha, 16].Value = v.Contatos != null && v.Contatos.GerenteSolicitouNaoAtuacao.HasValue && v.Contatos.GerenteSolicitouNaoAtuacao.Value ? "Sim" : "Não";
                    sheet.Cells[linha, 17].Value = v.Contatos != null && v.Contatos.GerenteInvestimentoAtuou.HasValue && v.Contatos.GerenteInvestimentoAtuou.Value ? "Sim" : "Não";
                    sheet.Cells[linha, 18].Value = v.Contatos != null && v.Contatos.EspecialistaAtuou.HasValue && v.Contatos.EspecialistaAtuou.Value ? "Sim" : "Não";
                    sheet.Cells[linha, 19].Value = v.Contatos != null && v.Contatos.ClienteLocalizado.HasValue && v.Contatos.ClienteLocalizado.Value ? "Sim" : "Não";
                    sheet.Cells[linha, 20].Value = v.Contatos != null && v.Contatos.ClienteAceitaConsultoria.HasValue && v.Contatos.ClienteAceitaConsultoria.Value ? "Sim" : "Não";

                    //Aplicacoes
                    var col = 21;
                    foreach (var prod in produtos)
                    {
                        sheet.Cells[linha, col].Value = v.Aplicacoes.Where(a => a.TedsProdutos.Produto == prod).Sum(s => s.ValorAplicado);
                        col++;
                    }
                }
                else if (type == typeof(PipelineExportModel))
                {
                    sheet.Cells[1, 1].Value = "Pipelines";

                    var p = dado as PipelineExportModel;

                    sheet.Cells[linha, 1].Value = p.Cliente;
                    sheet.Cells[linha, 2].Value = p.Especialista;
                    sheet.Cells[linha, 3].Value = p.Agencia;
                    sheet.Cells[linha, 4].Value = p.Conta;
                    sheet.Cells[linha, 5].Value = p.BradescoPrincipalBanco ? "Sim" : "Não";
                    sheet.Cells[linha, 6].Value = p.ValorMercado;
                    sheet.Cells[linha, 7].Value = p.DataProrrogada?.ToShortDateString();
                    sheet.Cells[linha, 8].Value = p.ValorDoPipe;
                    sheet.Cells[linha, 9].Value = p.ValorAplicado;
                    sheet.Cells[linha, 10].Value = p.DataPrevista.ToShortDateString();
                    sheet.Cells[linha, 11].Value = p.Comentario;
                    sheet.Cells[linha, 12].Value = p.Motivo;
                    sheet.Cells[linha, 13].Value = p.Origem;
                    sheet.Cells[linha, 14].Value = p.Situacao;
                    sheet.Cells[linha, 15].Value = int.Parse(p.Matricula);
                    sheet.Cells[linha, 16].Value = p.Equipe;



                    sheet.Cells[linha, 1, linha, properites.Length].AutoFitColumns();
                }
                else if (type == typeof(AplicacaoResgateExportModel))
                {
                    sheet.Cells[1, 1].Value = "Aplicação e Resgate";
                    var aplicacaoes = dado as AplicacaoResgateExportModel;

                    sheet.Cells[linha, 1].Value = aplicacaoes.agencia;
                    sheet.Cells[linha, 2].Value = aplicacaoes.conta;
                    sheet.Cells[linha, 3].Value = aplicacaoes.data.ToShortDateString();
                    var hora = aplicacaoes.hora.Hours < 10 ? $"0{aplicacaoes.hora.Hours}" : $"{aplicacaoes.hora.Hours}";
                    var minutos = aplicacaoes.hora.Minutes < 10 ? $"0{aplicacaoes.hora.Minutes}" : $"{aplicacaoes.hora.Minutes}";
                    sheet.Cells[linha, 4].Value = $"{hora }:{minutos}";
                    sheet.Cells[linha, 5].Value = aplicacaoes.operacao;
                    sheet.Cells[linha, 6].Value = aplicacaoes.perif;
                    sheet.Cells[linha, 7].Value = aplicacaoes.produto;
                    sheet.Cells[linha, 8].Value = aplicacaoes.terminal;
                    sheet.Cells[linha, 9].Value = aplicacaoes.valor;
                    sheet.Cells[linha, 10].Value = aplicacaoes.gerente;
                    sheet.Cells[linha, 11].Value = aplicacaoes.advisor;
                    sheet.Cells[linha, 12].Value = aplicacaoes.segmento;
                    sheet.Cells[linha, 13].Value = aplicacaoes.Especialista;
                    sheet.Cells[linha, 14].Value = int.Parse(aplicacaoes.Matricula);
                }
            }

            //Formatações finais
            for (int col = 1; col < sheet.Dimension.Columns; col++)
            {
                sheet.Column(col).AutoFit();
            }

            sheet.Cells[2, 1, sheet.Dimension.Rows, sheet.Dimension.Columns].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;

            Directory.CreateDirectory(destino);

            var arquivo = Path.Combine(destino, nomeArquivo);

            arquivo = File.Exists(arquivo) ? arquivo.Replace(".xlsx", "") + "Nova.xlsx" : arquivo;

            file.SaveAs(new FileInfo(arquivo));

            Common.Logging.Log.Information($"Foi gerado um arquivo com os dados excluidos para armazenamento em {arquivo}.");
        }
    }
}
