using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Xml;
using System.IO;
using System.Threading;
using System.Reflection;
using System.Windows.Forms;

namespace ScriptsManager.Utils
{
    public partial class ScriptProcessManager
    {
        public enum ProcessStatus
        {
            Running,
            Stopped
        }

        ProcessStatus status = ProcessStatus.Stopped;
        public ProcessStatus Status
        {
            get => status; 
            set
            {
                this.status = value;
                this.StatusChanged?.Invoke(this, this.status);
            }
        }
        public event EventHandler<string> ValueChanged;
        public event EventHandler<ProcessStatus> StatusChanged;
        public event EventHandler<string> MessageReceived;
        public event EventHandler<string> DebugMessageReceived;
        public event EventHandler<Control> ControlAdded;
        public event EventHandler<Control> ControlRemoved;
        public event EventHandler<string> DataReceived;
        public event EventHandler<(string title, string message)> NotificationSended;
        public Process Process { get; private set; }

        Dictionary<string, string> Values { get; }
            = new Dictionary<string, string>();
        Dictionary<string, (string value, bool overrideValue)> EnvironmentVariables { get; }
            = new Dictionary<string, (string value, bool overrideValue)>();

        Dictionary<string, string> ListenToSignals { get; }
            = new Dictionary<string, string>();

        Dictionary<string, Action> Commands { get; }
            = new Dictionary<string, Action>();

        public void SetValue(string name, string value)
        {
            Values[name] = value;
            this.ValueChanged?.Invoke(this, name);
        }
        public string GetValue(string valueName, string defaultValue = null, List<string> queue = null)
        {
            queue = queue ?? new List<string>();
            if (queue.Contains(valueName)) return string.Empty;
            string value = Values.ContainsKey(valueName) ? Values[valueName] : defaultValue ?? string.Empty;
            foreach (var match in Regex.Matches(value, @"\$\{([\d\w]+)\}").OfType<Match>())
            {
                string key = match.Value;
                string cleanKey = match.Groups.OfType<Group>().LastOrDefault()?.Value ?? "";
                string replacement = GetValue(cleanKey, "", queue.Union(new List<string> { valueName }).ToList());
                value = value.Replace(key, replacement);
            }
            return value;
        }
        public Color GetColor(string color)
        {
            string[] argv;
            if ((argv = GetValue(color).Split(',')).Length >= 3)
            {
                argv = argv
                    .Select(a => a.Trim())
                    .Where(a => int.TryParse(a, out _))
                    .ToArray();

                if (argv.Length == 3) return Color.FromArgb(
                        int.Parse(argv[0]),
                        int.Parse(argv[1]),
                        int.Parse(argv[2])
                    );

                if (argv.Length == 4) return Color.FromArgb(
                        int.Parse(argv[0]),
                        int.Parse(argv[1]),
                        int.Parse(argv[2]),
                        int.Parse(argv[3])
                    );

            }
            return Color.Empty;
        }

        public Color TextColor
        {
            get => GetColor("textColor");
        }
        public Color BackgroundColor
        {
            get => GetColor("backgroundColor");
        }
        public Image Icon
        {
            get
            {
                try
                {
                    return Image.FromFile(".\\icons\\" + GetValue("icon", "default.png"));
                }
                catch (Exception e)
                {
                    return new Bitmap(10, 10);
                }
            }
        }
        void LoadCommands()
        {
            this
                .GetType()
                .GetMethods()
                .Where(m =>
                    m.GetCustomAttribute<IPCCommandAttribute>() != null
                    && m.IsPublic
                    )
                .ToList()
                .ForEach(m =>
                {
                    string methodName = m.GetCustomAttribute<IPCCommandAttribute>().Name;
                    Action body = () => m.Invoke(this, null);
                    Commands[methodName] = body;
                });
        }
        MyIpc Ipc { get; set; } = null;
        public ScriptProcessManager()
        {
            LoadCommands();
        }


        public void Start(bool reset = false)
        {
            if (Status == ProcessStatus.Running && !reset) return;
            Kill();
            Ipc = MyIpc.Create(ProcessData);
            this.Process = null;
            Process localProcess = new Process();
            localProcess.StartInfo = new ProcessStartInfo
            {
                FileName = GetValue("filename", null),
                Arguments = GetValue("arguments", ""),
                WorkingDirectory = GetValue("workingDirectory", Directory.GetCurrentDirectory()),
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                UseShellExecute = false
            };
            foreach (var env in EnvironmentVariables)
            {
                if (env.Value.overrideValue || !localProcess.StartInfo.EnvironmentVariables.ContainsKey(env.Key))
                {
                    localProcess.StartInfo.EnvironmentVariables[env.Key] = env.Value.value;
                }
            }
            localProcess.StartInfo.EnvironmentVariables["MY_IPC_SERVER_PORT"] = Ipc.ClientPort.ToString();
            localProcess.StartInfo.EnvironmentVariables["MY_IPC_CLIENT_PORT"] = Ipc.ServerPort.ToString();
            localProcess.Start();
            this.Process = localProcess;
            localProcess.Exited += (o, e) => {
                Kill();
            };
            Status = ProcessStatus.Running;
        }

        private void ProcessData(string data)
        {
            data.Split(';')
                .Where(s => !string.IsNullOrEmpty(s))
                .ToList()
                .ForEach(l => DataReceived?.Invoke(this, l));
        }

        public void Send(string data)
        {
            Ipc.Send(data);
        }



        public void Load(XmlNode processNode)
        {
            XmlDocument xmlDocument = new XmlDocument();
            try
            {
                foreach (var a in processNode.Attributes.OfType<XmlAttribute>())
                {
                    Values[a.Name] = a.Value;
                }
                foreach (XmlNode environmentVariable in processNode.SelectNodes("./environment/var"))
                {
                    string name = environmentVariable.SelectSingleNode("@name")?.Value ?? "";
                    string value = environmentVariable.SelectSingleNode("@value")?.Value ?? "";
                    string overrideValue = environmentVariable.SelectSingleNode("@override")?.Value ?? "false";
                    EnvironmentVariables[name] = (value: value, overrideValue: overrideValue.ToUpper() == "TRUE");
                }
                foreach (XmlNode signal in processNode.SelectNodes("./listen/signal"))
                {
                    string name = signal.SelectSingleNode("@name")?.Value ?? "";
                    string action = signal.SelectSingleNode("@action")?.Value ?? "";
                    if (Commands.ContainsKey(action))
                    {
                        ListenToSignals[name] = action;
                    }
                }

                if (bool.TryParse(GetValue("autorun", "false"), out bool autorun) && autorun)
                {
                    Start();
                }
            }
            catch (Exception e)
            {

            }
        }

        public static IEnumerable<ScriptProcessManager> LoadProcess(string path)
        {

            XmlDocument xmlDocument = new XmlDocument();
            try
            {
                xmlDocument.Load(path);

            }
            catch (Exception e)
            {

            }
            foreach (XmlNode processNode in xmlDocument.DocumentElement.SelectNodes("//process"))
            {
                ScriptProcessManager result = new ScriptProcessManager();
                result.Load(processNode);
                yield return result;
            }
        }

        #region Interpreter
        Stack<Action> commandsStack = new Stack<Action>();
        Stack<string> dataStack = new Stack<string>();

        public void Execute(string request)
        {
            foreach (Match match in Regex.Matches(request, @"'[^']+'|[\w\-\d]+"))
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
                catch (Exception e)
                {

                }
            }
            dataStack.Clear();
        }

        #endregion

        #region Controls
        public Dictionary<string, Control> Controls { get; } = new Dictionary<string, Control>();
        #endregion
    }

    [AttributeUsage(AttributeTargets.Method, Inherited = true)]
    public class IPCCommandAttribute : Attribute
    {
        public string Name;
        public IPCCommandAttribute(string name)
        {
            this.Name = name;
        }
    }
}
