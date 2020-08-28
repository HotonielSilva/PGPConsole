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
using BradescoPGP.Common;

namespace BradescoPGPConsole.Commands
{
    public class ImportarHierarquias : AbstractCommand
    {
        public ImportarHierarquias(ServiceConfig config)
            : base(config)
        {
        }

        protected override void PrepararDados()
        {
            ExecuteQuery("DELETE FROM [dbo].[Usuario] WHERE PerfilId > 1");
        }

        protected override void RealizarCarga()
        {

            int indexLinhaAtual = 0;

            try
            {
                BradescoPGP.Common.Logging.Log.Information("Iniciando importação de Hierarquias.");

                using (StreamReader sd = new StreamReader(Config.Caminho, Encoding.UTF8))
                {
                    var encarteiramento = new List<string>();

                    using (var db = new PGPEntities())
                    {
                        encarteiramento = db.Encarteiramento.Select(s => s.Matricula).Distinct().ToList();
                    }

                    string linhaAtual = null;

                    string[] linhaDividida = null;

                    var Usuarios = new List<Usuario>();

                    while ((linhaAtual = sd.ReadLine()) != null)
                    {
                        if (indexLinhaAtual > 0)
                        {
                            linhaDividida = linhaAtual.SplitWithQualifier(';', '\"', true);

                            var matriculasInclusas = Usuarios.Select(s => s.Matricula).ToList();

                            var matricula = linhaDividida[72];

                            if (!matriculasInclusas.Contains(matricula))
                            {

                                var especialista = new Usuario();

                                especialista.Nome = linhaDividida[73];

                                especialista.Matricula = linhaDividida[72];

                                especialista.NomeSupervisor = linhaDividida[75];

                                especialista.MatriculaSupervisor = linhaDividida[74];

                                especialista.Equipe = linhaDividida[78];

                                especialista.PerfilId = encarteiramento.Contains(especialista.Matricula) ? 3 : 2;

                                especialista.NomeUsuario = CodNunToCodLetra(especialista.Matricula);

                                Usuarios.Add(especialista);

                                matriculasInclusas = Usuarios.Select(s => s.Matricula).ToList();

                                if (!matriculasInclusas.Contains(especialista.MatriculaSupervisor))
                                {
                                    var gestor = new Usuario();

                                    gestor.Nome = especialista.NomeSupervisor;

                                    gestor.Matricula = especialista.MatriculaSupervisor;

                                    gestor.NomeSupervisor = linhaDividida[77];

                                    gestor.MatriculaSupervisor = linhaDividida[76];

                                    gestor.Equipe = linhaDividida[78];

                                    gestor.PerfilId = 2;

                                    gestor.NomeUsuario = CodNunToCodLetra(gestor.Matricula);

                                    Usuarios.Add(gestor);
                                }

                                if (Usuarios.Count == 100000)
                                {
                                    using (var db = new PGPEntities())
                                    {
                                        db.BulkInsert(Usuarios.OrderBy(s => s.Nome));

                                        Usuarios.Clear();
                                    }
                                }
                            }
                        }
                        indexLinhaAtual++;
                    }

                    if (Usuarios.Count > 0)
                    {
                        using (var db = new PGPEntities())
                        {
                            db.BulkInsert(Usuarios.OrderBy(s => s.Nome).ToList());
                        }
                    }
                }

                BradescoPGP.Common.Logging.Log.Information("Importação de Hierarquias finalizada.");

            }
            catch (Exception ex)
            {
                Config.TeveFalha = true;

                BradescoPGP.Common.Logging.Log.Error("HIERARQUIA: Erro ao importar Hierarquias.", ex);
            }
        }

        /// <summary>
        /// Retornar o código Funcional do usuário com a letra no lugar do número
        /// </summary>
        /// <param name="CodFunc"></param>
        /// <returns></returns>
        private string CodNunToCodLetra(string CodFunc)
        {
            try
            {
                string Cod = CodFunc.ToUpper().Trim().Substring(0, 1);
                string CodLetra = string.Empty;

                switch (Cod)
                {
                    case "1":
                        CodLetra = "A";
                        break;
                    case "2":
                        CodLetra = "B";
                        break;
                    case "3":
                        CodLetra = "C";
                        break;
                    case "4":
                        CodLetra = "D";
                        break;
                    case "5":
                        CodLetra = "E";
                        break;
                    case "6":
                        CodLetra = "F";
                        break;
                    case "7":
                        CodLetra = "G";
                        break;
                    case "8":
                        CodLetra = "H";
                        break;
                    case "9":
                        CodLetra = "I";
                        break;
                    default:
                        return CodFunc.ToUpper();
                }

                CodLetra = CodLetra + CodFunc.Trim().Substring(1);
                return CodLetra.ToUpper();
            }
            catch
            {
                return CodFunc.ToUpper();
            }
        }

        private void ImportAntigo()
        {
            try
            {
                BradescoPGP.Common.Logging.Log.Information("Iniciando importação de Hierarquias.");

                using (FileStream fs = new FileStream(Config.Caminho, FileMode.Open, FileAccess.Read))
                {
                    var matriculasUsuariosBase = default(List<string>);
                    using (PGPEntities db = new PGPEntities())
                    {
                        matriculasUsuariosBase = db.Usuario.Select(u => u.Matricula).ToList();
                    }

                    XSSFWorkbook wb = new XSSFWorkbook(fs);
                    XSSFSheet sheet = (XSSFSheet)wb.GetSheetAt(0);
                    IEnumerator linhas = sheet.GetRowEnumerator();
                    List<Usuario> Usuario = new List<Usuario>();

                    while (linhas.MoveNext())
                    {
                        XSSFRow linhaAtual = (XSSFRow)linhas.Current;

                        //Pula linha de cabeçalho
                        if (linhaAtual.RowNum > 0)
                        {
                            //Cria novo usuario apenas se não existir na base de dados.
                            if (!matriculasUsuariosBase.Contains(linhaAtual.Cells[1].NumericCellValue.ToString()))
                            {
                                Usuario hierarquia = new Usuario();

                                hierarquia.Nome = linhaAtual.Cells[0].StringCellValue;
                                hierarquia.Matricula = linhaAtual.Cells[1].NumericCellValue.ToString();
                                hierarquia.NomeSupervisor = linhaAtual.Cells[2].StringCellValue;
                                hierarquia.MatriculaSupervisor = linhaAtual.Cells[3].NumericCellValue.ToString();
                                hierarquia.Equipe = linhaAtual.Cells[4].StringCellValue;
                                hierarquia.PerfilId = linhaAtual.Cells[5].StringCellValue == "Especialista" ? 3 :
                                    linhaAtual.Cells[5].StringCellValue == "Gestor" ? 2 : 1;
                                hierarquia.NomeUsuario = linhaAtual.Cells[6].StringCellValue;

                                Usuario.Add(hierarquia);
                            }
                        }
                    }

                    if (Usuario.Count > 0)
                    {
                        using (PGPEntities db = new PGPEntities())
                        {
                            db.BulkInsert(Usuario);
                        }

                        BradescoPGP.Common.Logging.Log.Information($"Foram cadastrados {Usuario.Count} usuarios novos.");
                    }
                    else
                    {
                        BradescoPGP.Common.Logging.Log.Information("Nenhum usuario novo para cadastrar.");
                    }
                }

                BradescoPGP.Common.Logging.Log.Information("Importação de Hierarquias finalizada.");
            }
            catch (Exception ex)
            {
                Config.TeveFalha = true;

                BradescoPGP.Common.Logging.Log.Error("HIERARQUIA: Erro ao importar Hierarquias.", ex);
            }
        }
    }
}
