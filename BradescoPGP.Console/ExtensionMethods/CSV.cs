using System;
using System.Collections.Generic;
using System.Linq;

namespace BradescoPGPConsole.ExtensionMethods
{
    public static class CSV
    {


        public static string GerarString<T>(this List<T> dados, string separador = ",")
        {
            string result = string.Empty;

            if (dados.Any())
            {
                dados.ForEach(e =>
                {
                    result += $"{e}{separador} ";
                });

                var idsx = result.LastIndexOf(separador);

                result = result.Substring(0, idsx);
            }
            return result;
        }


        public static string Criar<T>(string sepadaor, IEnumerable<T> valores, string[] colunasExluidas = null)
        {
            var csv = string.Empty;

            var enumerableTipo = typeof(T);


            var cabecalho = colunasExluidas != null ?
                enumerableTipo.GetProperties()
                .Where(s => !colunasExluidas.Contains(s.Name) && !s.PropertyType.IsClass)
                .Select(s => s.Name).ToList() :
                enumerableTipo.GetProperties()
                .Where(s => !s.PropertyType.IsAbstract)
                .Select(s => s.Name).ToList();


            csv += string.Join(sepadaor, cabecalho) + Environment.NewLine;

            foreach (var valor in valores)
            {
                for (int i = 0; i < cabecalho.Count; i++)
                {
                    if (i == cabecalho.Count - 1)
                        csv += valor.GetType().GetProperty(cabecalho[i]).GetValue(valor).ToString().Trim().Replace(Environment.NewLine, " ") + Environment.NewLine;
                    else
                        csv += valor.GetType().GetProperty(cabecalho[i]).GetValue(valor).ToString().Trim().Replace(Environment.NewLine, " ") + sepadaor;
                }

            }

            return csv;
        }


        public static string Criar(string sepadaor, IEnumerable<string> valores)
        {
            var csv = string.Empty;

            var valorList = valores.ToList();

            for (int i = 0; i < valorList.Count; i++)
            {
                if (i == valorList.Count - 1)
                    csv += valorList[i] + Environment.NewLine;
                else
                    csv += valorList[i] + sepadaor;
            }



            return csv;
        }
    }
}
