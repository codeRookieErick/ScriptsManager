    /*
    ScriptsManager, Administrador de scripts
    Copyright (C) 2020 Erick Mora

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <https://www.gnu.org/licenses/>.

    erickfernandomoraramirez@gmail.com
    erickmoradev@gmail.com
    https://dev.moradev.dev/myportfolio
    */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ScriptsManager.Utils
{
    partial class ScriptsAdministrator
    {
        Dictionary<string, Action> Commands = new Dictionary<string, Action>();

        public void LoadCommands()
        {
            List<(string commandName, MethodInfo method)> methods = this
                .GetType()
                .GetMethods()
                .Where(m => m.IsPublic && m.GetCustomAttribute<IPCCommandAttribute>() != null)
                .Select(m => (commandName: m.GetCustomAttribute<IPCCommandAttribute>().Name, method: m))
                .ToList();

            foreach (var m in methods)
            {
                Commands[m.commandName] = (() => m.method.Invoke(this, null));
            }
        }

        [IPCCommand("in-context")]
        public void SetContext()
        {
            string processName = dataStack.Count > 0 ? dataStack.Pop() : "";
            ScriptProcessManager scriptProcessManager = Managers
                .Select(m => m.manager)
                .FirstOrDefault(n => n.GetValue("name", "name") == processName);

            if (scriptProcessManager == null && currentContext != null)
            {
                OnPrint($"Context not found: '{processName}'");
            }
            else
            {
                currentContext = scriptProcessManager;
                OnPrint($"Context set: '{processName}'");
            }
        }

        [IPCCommand("reset")]
        public void Reset()
        {
            try
            {
                currentContext?.Reset();
                OnPrint($"Process in context reseted");
            }
            catch (Exception e)
            {
                OnPrint($"Fail to reset: '{e.Message}'");
            }
        }

        [IPCCommand("list-processes")]
        public void ListProcesses()
        {
            try
            {
                var processes = this.Managers.Select(m => m.manager.GetValue("name", "")).ToList();
                if (processes.Count > 0)
                {
                    OnPrint("[" + processes.Aggregate((a, b) => $"{a}, {b}") + "]");
                }
                else
                {
                    OnPrint("There is no process running");
                }

            }
            catch (Exception e)
            {
                OnPrint(e.ToString());
            }
        }

        [IPCCommand("manager")]
        public void Manager()
        {

            List<string> commands = typeof(ScriptManagerSignal).GetFields()
                .Where(f => f.IsStatic)
                .Select(f => f.GetValue(null)?.ToString() ?? "")
                .ToList();

            string command = string.Empty;

            while (dataStack.Count > 0)
            {
                command = dataStack.Pop();
                if (command == ScriptManagerSignal.GET_COMMANDS)
                {
                    if (commands.Count > 0)
                    {
                        OnPrint(string.Format(
                            "[{0}]",
                            commands.Aggregate((a, b) => $"{a}, {b}")
                            )); ;
                    }
                }
                else
                {
                    OnAdministratorSignalReceived?.Invoke(this, command);
                    OnPrint($"command {command} sent to manager");
                }
            }
        }
    }
}
