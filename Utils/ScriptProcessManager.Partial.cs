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
using System.Windows.Forms;

namespace ScriptsManager.Utils
{
    partial class ScriptProcessManager
    {
        void SendDebug(string message)
        {
            this.DebugMessageReceived?.Invoke(this, message);
        }

        [IPCCommand("reset")]
        public void Reset() => Start(true);
        
        [IPCCommand("kill")]
        public void Kill()
        {
            if (Status == ProcessStatus.Stopped) return;
            Status = ProcessStatus.Stopped;
            try
            {
                if (!(Process?.HasExited ?? true))
                {
                    Process.Kill();
                    Process.WaitForExit();
                }
            }
            finally { }

            try
            {
                Ipc?.Kill();
            }
            finally { }
        }

        [IPCCommand("create-control")]
        public void CreateControl()
            {
            string controlName = dataStack.Pop();
            if (Controls.ContainsKey(controlName)) return;
            string typeName = dataStack.Pop();
            TypeInfo typeInfo = GetType(typeName);
            if(typeInfo != default)
            {
                ConstructorInfo construct = typeInfo.GetConstructor(new Type[0]);
                if (construct != default)
                {
                    Control control = construct.Invoke(null) as Control; 
                    if(control != null)
                    {
                        Controls[controlName] = control;
                        ControlAdded?.Invoke(this, control);
                    }
                }
            }
        }

        [IPCCommand("change-control")]
        public void ChangeControl()
        {
            string command = dataStack.Pop();
            string controlName = dataStack.Pop();
            if (!Controls.ContainsKey(controlName)) return;

            string[] parameters = command.Substring(command.IndexOf("set-")+4).Split(new char[] { '=' }, 2);
            if (parameters.Length != 2) return;

            PropertyInfo propertyInfo = Controls[controlName]
                .GetType()
                .GetProperties()
                .FirstOrDefault(p => p.CanWrite && p.Name == parameters[0]);

            if (propertyInfo == default) return;

            try
            {
                object value = Convert.ChangeType(parameters[1], propertyInfo.PropertyType);
                propertyInfo.SetValue(Controls[controlName], value);
            }catch(Exception)
            {

            }
        }

        [IPCCommand("remove-control")]
        public void RemoveControl()
        {
            string controlName = dataStack.Pop();
            if (Controls.ContainsKey(controlName))
            {
                ControlRemoved?.Invoke(this, Controls[controlName]);
                Controls.Remove(controlName);
            }
        }

        [IPCCommand("print")]
        public void Print()
        {
            string message = $"{DateTime.Now.ToString("hh:mm:ss")} => {dataStack.Pop()}";
            MessageReceived?.Invoke(this, message);
        }

        [IPCCommand("notify")]
        public void Clear()
        {
            string text = dataStack.Pop();
            string title = dataStack.Count > 0 ? dataStack.Pop() : string.Empty;
            NotificationSended?.Invoke(this, (title, text));
        }


        TypeInfo GetType(string name)
        {
            List<TypeInfo> types = 
                Assembly.LoadFrom("./Controls.dll")
                .DefinedTypes
                .Where(t => t.IsSubclassOf(typeof(Control)))
                .ToList();
            return types.FirstOrDefault(t => t.Name == name);
        }
    }
}
