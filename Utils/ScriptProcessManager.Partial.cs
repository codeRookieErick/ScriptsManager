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
            }catch(Exception e)
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
