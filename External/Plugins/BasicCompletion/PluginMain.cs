using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Timers;
using System.Windows.Forms;
using PluginCore;
using PluginCore.Controls;
using PluginCore.Helpers;
using PluginCore.Localization;
using PluginCore.Managers;
using PluginCore.Utilities;
using ScintillaNet;
using Timer = System.Timers.Timer;

namespace BasicCompletion
{
    public class PluginMain : IPlugin
    {
        readonly ISet<string> updateTable = new HashSet<string>();
        readonly IDictionary<string, List<string>> baseTable = new Dictionary<string, List<string>>();
        readonly IDictionary<string, string[]> fileTable = new Dictionary<string, string[]>();
        Timer updateTimer;
        bool isActive;
        bool isSupported;
        string settingFilename;
        Settings settingObject;
        string[] projKeywords;

        #region Required Properties

        /// <summary>
        /// Api level of the plugin
        /// </summary>
        public int Api => 1;

        /// <summary>
        /// Name of the plugin
        /// </summary> 
        public string Name { get; } = nameof(BasicCompletion);

        /// <summary>
        /// GUID of the plugin
        /// </summary>
        public string Guid { get; } = "c5564dec-5288-4bbb-b286-a5678536698b";

        /// <summary>
        /// Author of the plugin
        /// </summary> 
        public string Author { get; } = "FlashDevelop Team";

        /// <summary>
        /// Description of the plugin
        /// </summary> 
        public string Description { get; set; } = "Adds global basic code completion support to FlashDevelop.";

        /// <summary>
        /// Web address for help
        /// </summary> 
        public string Help { get; } = "www.flashdevelop.org/community/";

        /// <summary>
        /// Object that contains the settings
        /// </summary>
        [Browsable(false)]
        public object Settings => settingObject;

        #endregion
        
        #region Required Methods
        
        /// <summary>
        /// Initializes the plugin
        /// </summary>
        public void Initialize()
        {
            InitTimer();
            InitBasics();
            LoadSettings();
            AddEventHandlers();
        }
        
        /// <summary>
        /// Disposes the plugin
        /// </summary>
        public void Dispose() => SaveSettings();

        /// <summary>
        /// Handles the incoming events
        /// </summary>
        public void HandleEvent(object sender, NotifyEvent e, HandlingPriority priority)
        {
            var sci = PluginBase.MainForm.CurrentDocument?.SciControl;
            if (sci is null) return;
            switch (e.Type)
            {
                case EventType.Keys:
                    if (isSupported)
                    {
                        switch (((KeyEvent) e).Value)
                        {
                            case Keys.Control | Keys.Space:
                                var items = GetCompletionListItems(sci.ConfigurationLanguage, sci.FileName);
                                if (!items.IsNullOrEmpty())
                                {
                                    items.Sort();
                                    var word = sci.GetWordLeft(sci.CurrentPos - 1, false);
                                    CompletionList.Show(items, false, word);
                                    e.Handled = true;
                                }
                                break;
                            case Keys.Control | Keys.Space | Keys.Alt:
                                PluginBase.MainForm.CallCommand("InsertSnippet", "null");
                                e.Handled = true;
                                break;
                        }
                    }
                    break;
                case EventType.UIStarted:
                    isSupported = false;
                    isActive = true;
                    break;
                case EventType.UIClosing:
                    isActive = false;
                    break;
                case EventType.FileSwitch:
                    isSupported = false;
                    break;
                case EventType.Completion:
                    if (!e.Handled && isActive)
                    {
                        isSupported = true;
                        e.Handled = true;
                    }
                    HandleFile(sci);
                    break;
                case EventType.SyntaxChange:
                case EventType.ApplySettings:
                    HandleFile(sci);
                    break;
                case EventType.FileSave:
                    var te = (TextEvent) e;
                    if (te.Value == sci.FileName && isSupported) AddDocumentKeywords(sci);
                    else if (DocumentManager.FindDocument(te.Value) is not null)
                        updateTable.Add(te.Value);
                    break;
                case EventType.Command:
                    var de = (DataEvent) e;
                    if (de.Action == "ProjectManager.Project" && de.Data is IProject project)
                    {
                        LoadProjectKeywords(project);
                    }
                    break;
            }
        }

        /// <summary>
        /// Handles the completion and config for a file
        /// </summary>
        void HandleFile(ScintillaControl sci)
        {
            if (isSupported)
            {
                var language = sci.ConfigurationLanguage;
                if (!baseTable.ContainsKey(language)) AddBaseKeywords(language);
                if (!fileTable.ContainsKey(sci.FileName)) AddDocumentKeywords(sci);
                if (updateTable.Remove(sci.FileName)) AddDocumentKeywords(sci); // Need to update after save?
                updateTimer.Stop();
            }
            else updateTable.Remove(sci.FileName); // Not supported saved, remove
        }

        #endregion

        #region Custom Methods

        /// <summary>
        /// Initializes the update timer
        /// </summary>
        void InitTimer()
        {
            updateTimer = new Timer {SynchronizingObject = (Form) PluginBase.MainForm, Interval = 500};
            updateTimer.Elapsed += UpdateTimerElapsed;
        }

        /// <summary>
        /// After the timer elapses, update doc keywords
        /// </summary>
        void UpdateTimerElapsed(object sender, ElapsedEventArgs e)
        {
            if (isSupported && PluginBase.MainForm.CurrentDocument?.SciControl is {} sci)
                AddDocumentKeywords(sci);
        }

        /// <summary>
        /// Initializes important variables
        /// </summary>
        void InitBasics()
        {
            var path = Path.Combine(PathHelper.DataDir, nameof(BasicCompletion));
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            settingFilename = Path.Combine(path, "Settings.fdb");
            Description = TextHelper.GetString("Info.Description");
        }

        /// <summary>
        /// Adds the required event handlers
        /// </summary> 
        void AddEventHandlers()
        {
            UITools.Manager.OnCharAdded += SciControlCharAdded;
            UITools.Manager.OnTextChanged += SciControlTextChanged;
            EventManager.AddEventHandler(this, EventType.Completion, HandlingPriority.Low);
            EventManager.AddEventHandler(this, EventType.Keys | EventType.FileSave | EventType.ApplySettings | EventType.SyntaxChange | EventType.FileSwitch | EventType.Command | EventType.UIStarted | EventType.UIClosing);
        }

        /// <summary>
        /// Loads the plugin settings
        /// </summary>
        void LoadSettings()
        {
            settingObject = new Settings();
            if (!File.Exists(settingFilename)) SaveSettings();
            else settingObject = ObjectSerializer.Deserialize(settingFilename, settingObject);
        }

        /// <summary>
        /// Saves the plugin settings
        /// </summary>
        void SaveSettings() => ObjectSerializer.Serialize(settingFilename, settingObject);

        /// <summary>
        /// Adds base keywords from config file to hashtable
        /// </summary>
        void AddBaseKeywords(string language)
        {
            var keywords = new List<string>();
            var lang = ScintillaControl.Configuration.GetLanguage(language);
            foreach (var usekeyword in lang.usekeywords)
            {
                var kc = ScintillaControl.Configuration.GetKeywordClass(usekeyword.cls);
                if (kc?.val is null) continue;
                var entry = Regex.Replace(kc.val, @"\t|\n|\r", " ");
                var words = entry.Split(new[]{' '}, StringSplitOptions.RemoveEmptyEntries);
                foreach (var word in words)
                {
                    if (word.Length > 3 && !keywords.Contains(word) && !word.StartsWithOrdinal("\x5E"))
                    {
                        keywords.Add(word);
                    }
                }
            }
            baseTable[language] = keywords;
        }

        /// <summary>
        /// Load the current project's keywords from completion file
        /// </summary>
        void LoadProjectKeywords(IProject project)
        {
            var projDir = Path.GetDirectoryName(project.ProjectPath);
            var complFile = Path.Combine(projDir, "COMPLETION");
            if (!File.Exists(complFile)) return;
            try
            {
                var text = File.ReadAllText(complFile);
                var matches = Regex.Matches(text, "[A-Za-z0-9_$]{2,}");
                var words = new Dictionary<int, string>();
                for (var i = 0; i < matches.Count; i++)
                {
                    var word = matches[i].Value;
                    var hash = word.GetHashCode();
                    if (words.ContainsKey(hash)) continue;
                    words.Add(hash, word);
                }
                projKeywords = words.Values.ToArray();
            }
            catch { /* No errors please... */ }
        }

        /// <summary>
        /// Adds document keywords from config file to hashtable
        /// </summary>
        void AddDocumentKeywords(ScintillaControl sci)
        {
            var textLang = sci.ConfigurationLanguage;
            var language = ScintillaControl.Configuration.GetLanguage(textLang);
            if (language.characterclass is null) return;
            var wordCharsRegex = "[" + language.characterclass.Characters + "]{2,}";
            var matches = Regex.Matches(sci.Text, wordCharsRegex);
            var words = new Dictionary<int, string>();
            for (var i = 0; i < matches.Count; i++)
            {
                var word = matches[i].Value;
                var hash = word.GetHashCode();
                if (words.ContainsKey(hash)) continue;
                words.Add(hash, word);
            }
            fileTable[sci.FileName] = words.Values.ToArray();
        }

        /// <summary>
        /// Gets the completion list items combining base and doc keywords
        /// </summary>
        List<ICompletionListItem> GetCompletionListItems(string lang, string file)
        {
            var words = new List<string>();
            if (baseTable.TryGetValue(lang, out var list)) words.AddRange(list);
            if (fileTable.TryGetValue(file, out var fileWords))
            {
                foreach (var it in fileWords)
                {
                    if (!words.Contains(it)) words.Add(it);
                }
            }
            if (PluginBase.CurrentProject is not null && projKeywords is not null)
            {
                foreach (var it in projKeywords)
                {
                    if (!words.Contains(it)) words.Add(it);
                }
            }
            return words.Select(it => new CompletionItem(it)).ToList<ICompletionListItem>();
        }

        /// <summary>
        /// Shows the completion list automatically after typing three chars
        /// </summary>
        void SciControlCharAdded(ScintillaControl sci, int value)
        {
            if (!isSupported || settingObject.DisableAutoCompletion) return;
            var lang = sci.ConfigurationLanguage;
            var characters = ScintillaControl.Configuration.GetLanguage(lang).characterclass.Characters;
            // Do not autocomplete in word
            if (characters.Contains(sci.CurrentChar)) return;
            // Autocomplete after typing word chars only
            if (!characters.Contains((char)value)) return;
            var word = sci.GetWordLeft(sci.CurrentPos - 1, false);
            if (word is null || word.Length < 3) return;
            var items = GetCompletionListItems(lang, sci.FileName);
            if (items.IsNullOrEmpty()) return;
            items.Sort();
            CompletionList.Show(items, true, word);
            var insert = settingObject.AutoInsertType;
            if (insert == AutoInsert.Never || (insert == AutoInsert.CPP && (sci.Lexer != 3/*CPP*/ || sci.PositionIsOnComment(sci.CurrentPos)) || lang == "text"))
            {
                CompletionList.DisableAutoInsertion();
            }
        }

        /// <summary>
        /// Starts the timer for the document keywords updating
        /// </summary>
        void SciControlTextChanged(ScintillaControl sci, int position, int length, int linesAdded)
        {
            if (!isSupported) return;
            updateTimer.Stop();
            updateTimer.Interval = Math.Max(500, sci.Length / 10);
            updateTimer.Start();
        }

        #endregion
    }

    #region Extra Classes

    /// <summary>
    /// Simple completion list item
    /// </summary>
    internal class CompletionItem : ICompletionListItem, IComparable, IComparable<ICompletionListItem>
    {
        public CompletionItem(string label) => Label = label;

        public string Label { get; }

        public string Description => TextHelper.GetString("Info.CompletionItemDesc");

        public Bitmap Icon => (Bitmap)PluginBase.MainForm.FindImage("315");

        public string Value => Label;

        int IComparable.CompareTo(object obj)
            => string.Compare(Label, ((ICompletionListItem) obj).Label, true);

        int IComparable<ICompletionListItem>.CompareTo(ICompletionListItem other)
            => string.Compare(Label, other.Label, true);
    }

    #endregion
}