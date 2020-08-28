using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BradescoPGPConsole
{
    public class InvokerImports
    {
        private readonly List<ICommand> _commands;

        public InvokerImports(List<ICommand> commands)
        {
            _commands = commands;
        }
        public void IniciarImportacoes()
        {
            foreach (var commnd in _commands)
            {
                if (commnd.EstaPronto())
                {
                    ThreadPool.QueueUserWorkItem(Execute, commnd);
                }
            }
        }

        private void Execute(object state)
        {
            ((ICommand)state).Importar();
        }
    }
}
