using System.IO;
using System.Text.RegularExpressions;
using System.Xml;
using DocoptNet;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Formatting = Newtonsoft.Json.Formatting;

namespace ConfigChanger
{
  internal class LineProcessor
  {
    public LineProcessor(string? defaultPath, string? extension, bool recursive)
    {
      _sessionState = new SessionState(defaultPath);
      _sessionState.Path = defaultPath;
      _sessionState.Extension = extension;
      _sessionState.Recursive = recursive;
    }


    private Pointer? _prevLine;
    private SessionState _sessionState;

    void DisplayProcess(string path, string xpath, string value)
    {
      IEnumerable<string> files = SearchFiles(path);
      foreach (var file in files)
      {
        Console.ForegroundColor = ConsoleColor.Blue;
        Console.WriteLine(file.Substring(_sessionState.Path.Length + 1));
        Console.ResetColor();

        var fileContent = File.ReadAllText(file);
        if (fileContent.StartsWith("<"))
        {
          bool backup = false;
          var dox = new XmlDocument();
          dox.LoadXml(fileContent);
          XmlNodeList? nodes = null;

          try
          {
            nodes = dox.SelectNodes(xpath);
          }
          catch (Exception ex)
          {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(ex.Message);
            Console.ResetColor();
          }

          if (nodes == null)
          {
            Console.WriteLine("XPath not found!");
            return;
          }

          foreach (var node in nodes)
          {
            if (node is XmlElement element)
            {
              Console.WriteLine(element.OuterXml);
            }
            else if (node is XmlAttribute attribute)
            {
              if (!String.IsNullOrEmpty(value))
              {
                attribute.Value = value;
                if (!backup)
                {
                  var backupPath = MakeBackupPath();
                  var basepath = file.Substring(_sessionState.Path.Length).TrimStart('\\');
                  var newpath = Path.Combine(backupPath, basepath);
                  var newPathDir = Path.GetDirectoryName(newpath);
                  if (!Directory.Exists(newPathDir))
                    Directory.CreateDirectory(newPathDir);

                  File.Copy(file, newpath);
                  backup = true;
                }
              }

              Console.WriteLine(attribute.Value);
            }
          }

          if (backup)
            dox.Save(file);
        }
        else
        {

        }
      }
    }

    private string MakeBackupPath()
    {
      string backupPath = GetBackupPath();

      string backupname = _sessionState.Name + "-" + DateTime.Now.ToString("yyyyyMMdd-HHmmss");
      backupPath = Path.Combine(backupPath, backupname);
      if (!Directory.Exists(backupPath))
      {
        Directory.CreateDirectory(backupPath);
      }
      return backupPath;
    }

    private static string GetBackupPath()
    {
      string backupPath = Path.Combine(Environment.CurrentDirectory, "backup");
      if (!Directory.Exists(backupPath))
      {
        Directory.CreateDirectory(backupPath);
      }

      return backupPath;
    }

    IEnumerable<string> DisplayProcess(string path)
    {
      IEnumerable<string> files = SearchFiles(path);

      foreach (var file in files)
      {
        var filePath = file.Substring(_sessionState.Path.Length);
        Console.WriteLine(filePath);
      }

      return files;
    }

    private IEnumerable<string> SearchFiles(string path)
    {
      string[] extensions = _sessionState.Extension.Split(';', ',', '|');
      SearchOption searchOption = SearchOption.TopDirectoryOnly;
      if (_sessionState.Recursive)
        searchOption = SearchOption.AllDirectories;
      var files = Directory.GetFiles(_sessionState.Path, "*.*", searchOption)
      .Where(x => extensions.Any(y => String.Compare(y, Path.GetExtension(x).TrimStart('.'), true) == 0))
      .Where(x =>
      {
        try
        {
          return Regex.IsMatch(x, path, RegexOptions.IgnoreCase);
        }
        catch
        {
          return false;
        }
      });
      return files;
    }

    public void ProcessLine(string[]? args)
    {
      const string usage = @"Configuration changer application.

    Usage:
      > search <path> [<xpath>] [<value>]
      > ls (sessions | configs | backups)
      > show <id>
      > add
      > set <id>
      > remove <id>
      > save [<name>] [--path-only]
      > open <name>
      > cls
      > apply
      > restore <name>
      > exit

    Options:
      search <path> [<xpath>] [<value>]
                        Searching through the files and folder via regular expression ability.
                        <path> indicates directories with REGEX filtering ability.
                        <xpath> indicates address of elements and attributes in the XML based files.
                        <value> indicates new replacement.
                        
      ls (sessions | configs | backups)
                        sessions  Show saved session lists.
                        configs   Show saved configuration elements in the current session.
                        backups   Show backup-ed files.
      add               Adding recently searched item to the configuration element in the current sessions.
      set <id>          Setting the value of a searched elements in the current session.
      remove <id>       Removing a searched elements in the current session.
      save [<name>] [--path-only]
                        Saving the current session into the disk.
                        <name> indicates name of the session file.
                        [--path-only] indicates that the replacement value of configuration elements do not be saved.
      open <name>       Opening a saved session file.
      cls               Clearing the screen.
      apply             Applying the current session with replacement values and taking backup.
      restore <name>    Restoring a backup-ed session.
      exit              Exiting from the application.

    ";

      if (args != null && args.Length > 0)
      {
        IDictionary<string, ValueObject>? arguments = null;
        try
        {
          arguments = new Docopt().Apply(usage, args, version: "Configuration Changer 1.0", exit: false);
        }
        catch (Exception ex)
        {
          Console.ForegroundColor = ConsoleColor.Red;
          Console.WriteLine(ex.Message);
          Console.ResetColor();
          return;
        }

        if (arguments["search"].IsTrue)
        {
          Search((string)arguments["<path>"].Value,
          arguments["<xpath>"]?.Value.ToString(),
          arguments["<value>"]?.Value.ToString());
        }
        else if (arguments["add"].IsTrue)
        {
          Add();
        }
        else if (arguments["ls"].IsTrue)
        {
          if (arguments["configs"].IsTrue)
            List();
          else if (arguments["sessions"].IsTrue)
            ShowSessions();
          else if (arguments["backups"].IsTrue)
            ShowBackups();
        }
        else if (arguments["remove"].IsTrue)
        {
          Remove((string)arguments["<id>"].Value);
        }
        else if (arguments["save"].IsTrue)
        {
          Save(arguments["<name>"]?.Value.ToString(), arguments["--path-only"].IsTrue);
        }
        else if (arguments["open"].IsTrue)
        {
          OpenSession((string)arguments["<name>"].Value);
        }
        else if (arguments["set"].IsTrue)
        {
          SetValue((string)arguments["<id>"].Value);
        }
        else if (arguments["cls"].IsTrue)
        {
          ClearScreen();
        }
        else if (arguments["show"].IsTrue)
        {
          Show((string)arguments["<id>"].Value);
        }
        else if (arguments["apply"].IsTrue)
        {
          Apply();
        }
        else if (arguments["restore"].IsTrue)
        {
          Restore((string)arguments["<name>"].Value);
        }

      }
    }

    private void Restore(string backupname)
    {
      string backupMainPath = GetBackupPath();
      string backuppath = Path.Combine(backupMainPath, backupname);
      if (Directory.Exists(backuppath))
      {
        var files = Directory.GetFiles(backuppath, "*.*", SearchOption.AllDirectories);
        foreach (var file in files)
        {
          string filepath = _sessionState.Path;
          var fileRelativePath = file.Substring(backupMainPath.Length + backupname.Length + 1).TrimStart('\\');
          var fileOriginPath = Path.Combine(filepath, fileRelativePath);
          File.Copy(file, fileOriginPath, true);
          Console.WriteLine(file);
        }
      }
    }

    private void ShowBackups()
    {
      string backuppath = GetBackupPath();
      var backupdir = new DirectoryInfo(backuppath);
      var backups = backupdir.GetDirectories();
      foreach (var backup in backups)
      {
        Console.WriteLine(backup.Name);
      }
    }

    private void Apply()
    {
      for (int i = 0; i < _sessionState.Pointers.Count; i++)
      {
        DisplayProcess(_sessionState.Pointers[i].FilePath, _sessionState.Pointers[i].XPath, _sessionState.Pointers[i].Value);
      }
    }

    private void Show(string id)
    {
      int index = _sessionState.FindIndex(id);
      if (index >= 0)
      {
        DisplayProcess(_sessionState.Pointers[index].FilePath, _sessionState.Pointers[index].XPath, null);
      }
    }

    private void ClearScreen()
    {
      Console.Clear();
    }

    private void SetValue(string id)
    {
      int index = _sessionState.FindIndex(id);
      if (index >= 0)
      {
        Console.WriteLine("Enter the configure new value:");
        var value = Console.ReadLine();
        _sessionState.Pointers[index].Value = value;

        Console.WriteLine(value);
      }
    }

    private void Search(string? path, string? xpath, string? value)
    {
      _prevLine = new Pointer
      {
        ID = 0,
        FilePath = path,
        XPath = xpath,
        Value = value
      };
      if (!String.IsNullOrEmpty(path) && String.IsNullOrEmpty(xpath))
      {
        DisplayProcess(path);
      }
      if (!String.IsNullOrEmpty(path) && !String.IsNullOrEmpty(xpath))
      {
        DisplayProcess(path, xpath, null);
      }
    }

    private void OpenSession(string fileName)
    {
      if (!String.IsNullOrEmpty(fileName))
      {
        string path = Path.Combine(Environment.CurrentDirectory, "sessions");
        var sessionPath = Path.Combine(path, fileName + ".json");
        if (File.Exists(sessionPath))
        {

          var json = File.ReadAllText(sessionPath);
          DefaultContractResolver contractResolver = new DefaultContractResolver
          {
            NamingStrategy = new CamelCaseNamingStrategy()
          };
          var settings = new JsonSerializerSettings
          {
            ContractResolver = contractResolver,
            Formatting = Formatting.Indented
          };
          _sessionState = JsonConvert.DeserializeObject<SessionState>(json, settings);
        }
        else
        {
          Console.WriteLine("File not found!");
        }
      }
    }

    private void ShowSessions()
    {
      string path = Path.Combine(Environment.CurrentDirectory, "sessions");
      if (Directory.Exists(path))
      {
        var sessions = Directory.GetFiles(path, "*.json");
        foreach (var session in sessions)
          Console.WriteLine(Path.GetFileNameWithoutExtension(session));
      }
    }

    private void Save(string fileName, bool savePathOnly)
    {
      if (!String.IsNullOrEmpty(fileName))
        _sessionState.Name = fileName;
      fileName = _sessionState.Name;
      if (!String.IsNullOrEmpty(fileName))
      {

        fileName = Path.GetFileName(fileName) + ".json";
        string path = Path.Combine(Environment.CurrentDirectory, "sessions");
        if (!Directory.Exists(path))
          Directory.CreateDirectory(path);
        string filePath = Path.Combine(path, fileName);

        DefaultContractResolver contractResolver = new DefaultContractResolver
        {
          NamingStrategy = new CamelCaseNamingStrategy()
        };
        var settings = new JsonSerializerSettings
        {
          ContractResolver = contractResolver,
          Formatting = Formatting.Indented
        };

        SessionState newSession = _sessionState;
        if (savePathOnly)
        {
          newSession = new SessionState()
          {
            Extension = _sessionState.Extension,
            Name = _sessionState.Name,
            Path = _sessionState.Path,
            Recursive = _sessionState.Recursive
          };
          foreach (var pointer in _sessionState.Pointers)
          {
            newSession.Pointers.Add(new Pointer
            {
              FilePath = pointer.FilePath,
              ID = pointer.ID,
              XPath = pointer.XPath,
            });
          }
        }
        var json = JsonConvert.SerializeObject(newSession, settings);
        File.WriteAllText(filePath, json);
      }
      else
      {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("Please choose a name for the session.");
        Console.ResetColor();
      }
    }

    private void Remove(string id)
    {
      int index = _sessionState.FindIndex(id);
      if (index >= 0)
      {
        _sessionState.Pointers.RemoveAt(index);
        Console.WriteLine(id);
      }
    }

    private void List()
    {
      foreach (var entry in _sessionState.Pointers)
      {
        Console.ForegroundColor = ConsoleColor.Blue;
        Console.Write(entry.ID);
        Console.Write(" ");
        Console.ResetColor();
        Console.Write(entry.FilePath);
        Console.Write("\t");
        Console.Write(entry.XPath);
        Console.Write("\t");
        Console.WriteLine(entry.Value);
      }
    }

    private void Add()
    {
      int bigest = _sessionState.Pointers.Count > 0 ? _sessionState.Pointers.Max(x => x.ID) : 0;
      _sessionState.Pointers.Add(new Pointer
      {
        ID = bigest + 1,
        FilePath = _prevLine.FilePath,
        Value = _prevLine.Value,
        XPath = _prevLine.XPath,
      });
    }
  }
}
