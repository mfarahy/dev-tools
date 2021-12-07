
using ConfigChanger;
using DocoptNet;

class MainClass
{
  static void Main(string[] args)
  {
    const string usage = @"Configuration changer application.

    Usage:
      cfc.exe [-r | --recursive] [-p <path>| --path <path>] [-x <ext>| --extension <ext>]

    Options:
     [-x| --extension] Specify configuration files extensions. They can be split with ;.
     [-p| --path] Specify the working directory.
      [-r | --recursive]         Include the current directory and all its subdirectories.
      --version   Show version.

    ";
    var arguments = new Docopt().Apply(usage, args, version: "Configuration Changer 1.0", exit: false);

    string? line = "";
    LineProcessor processor = new LineProcessor(
    arguments["<path>"].Value?.ToString(),
    arguments["<ext>"].Value?.ToString(), 
    (bool)arguments["-r"].Value || (bool)arguments["--recursive"].Value);
    do
    {
      ShowPrompt();
      line = Console.ReadLine();
      processor.ProcessLine(line?.Split(' '));

    } while (String.Compare(line, "exit", true) != 0);

  }




  static void ShowPrompt()
  {
    Console.WriteLine(">");
    (int x, int y) = Console.GetCursorPosition();
    Console.SetCursorPosition(x + 1, y - 1);
  }

}

