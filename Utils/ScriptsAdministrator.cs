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
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ScriptsManager.Utils
{
    public partial class ScriptsAdministrator:IDisposable
    {
        public static class ScriptManagerSignal
        {
            public const string BRING_FRONT            = "BRING-FRONT";
            public const string GET_COMMANDS           = "GET-COMMANDS";
            public const string STOP_PRESENTATION      = "STOP-PRESENTATION";
            public const string START_PRESENTATION     = "START-PRESENTATION";
            public const string SHOW_NEXT              = "SHOW-NEXT";
            public const string KILL_MANAGER           = "KILL-MANAGER";
            public const string RESTART_MANAGER        = "RESTART-MANAGER";
        }

        public event EventHandler<(string message, bool output)> Print;
        public event EventHandler<ScriptProcessManager> OnScriptAdded;
        public event EventHandler<ScriptProcessManager> OnScriptRemoved;
        public event EventHandler<string> OnAdministratorSignalReceived;

        public SocketsLayer SocketsLayer { get; }
        ScriptProcessManager currentContext = null;
        public List<BackgroundTask> BackgroundTasks { get; } = new List<BackgroundTask>();
        public string DefinitionsPath { get; private set; }
        public ScriptsAdministrator(int receivePort, int sendPort, string definitionsPath)
        {
            DefinitionsPath = definitionsPath;
            SocketsLayer = new SocketsLayer(receivePort, sendPort, null, OnDataReceived);
            LoadCommands();
            LoadRecurrentTasks();
        }

        void LoadRecurrentTasks()
        {
            BackgroundTasks.Add(new BackgroundTask(CheckForDefinitionsUpdates, (int)BackgroundTask.Mode.Interactive));
        }

        #region RecurrentTasks
        List<(string path, ScriptProcessManager manager)> Managers = new List<(string path, ScriptProcessManager manager)>();
        
        /// <summary>
        /// Determina si algun script ha sido agregado o eliminado (Ojo, no evalua por los cambios dentro del script)
        /// TODO: Evaluar los cambios en el script
        /// </summary>
        public void CheckForDefinitionsUpdates()
        {
            lock (Managers)
            {
                string[] newFiles = Directory.GetFiles(DefinitionsPath, "*.xml");
                string[] oldFiles = Managers.Select(m => m.path).Distinct().ToArray();
                string[] addedFiles = newFiles.Where(f => !oldFiles.Contains(f)).ToArray();
                string[] removedFiles = oldFiles.Where(f => !newFiles.Contains(f)).ToArray();

                var RemovedManagers = Managers.Where(m => removedFiles.Contains(m.path)).ToList();

                foreach (var manager in RemovedManagers)
                {
                    Managers.Remove(manager);
                    this.OnScriptRemoved?.Invoke(this, manager.manager);
                }

                foreach (string path in addedFiles)
                {
                    foreach (var manager in ScriptProcessManager.LoadProcess(path))
                    {
                        Managers.Add((path, manager));
                        this.OnScriptAdded?.Invoke(this, manager);
                    }
                }
            }
        }
        #endregion

        /// <summary>
        /// Esto es para la administracion remota
        /// </summary>
        /// <param name="buffer"></param>
        #region SocketMethods
        public void OnDataReceived(byte[] buffer)
        {
            OnCommandReceived(Encoding.UTF8.GetString(buffer));
        }
        public void SendData(byte[] data)
        {
            SocketsLayer.Send(data);
        }
        #endregion
        #region CommandsInterpreter
        Stack<string> dataStack = new Stack<string>();
        Stack<Action> commandsStack = new Stack<Action>();
        public void OnCommandReceived(string data)
        {
            Print?.Invoke(this, (GetPrefix(false) + data, output: false));
            foreach (Match match in Regex.Matches(data, @"'[^']+'|[\w\-\d]+"))
            {
                string token = match.Value.Replace("'", "");
                if (Commands.ContainsKey(token))
                {
                    commandsStack.Push(Commands[token]);
                }
                else
                {
                    dataStack.Push(token);
                }
            }
            while (commandsStack.Count > 0)
            {
                try
                {
                    commandsStack.Pop()();
                }
                catch (Exception)
                {

                }
            }
            dataStack.Clear();
            ///Interpreter here
        }
        public string GetPrefix(bool output)
        {
            string contextName = currentContext?.GetValue("name", "") ?? "";
            return
                string.Format(
                    "{0} '{1}' {2} ",
                    DateTime.Now.ToString("hh:mm:ss"),
                    contextName,
                    output ? ">>" : "#"
                );
        }
        public void OnPrint(string data)
        {
            Print?.Invoke(this, (GetPrefix(true) + data, output: true));
            SendData(Encoding.UTF8.GetBytes(GetPrefix(true) + data));
        }
        #endregion
        public void Dispose()
        {
            foreach (var action in BackgroundTasks)
            {
                action.Dispose();
            }
            foreach(var manager in Managers)
            {
                try
                {
                    manager.manager?.Kill();
                }catch(Exception)
                {

                }
            }
        }
    }
}
