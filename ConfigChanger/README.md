Configuration changer application.

    Usage:
      cfc.exe [-r | --recursive] [-p <path>| --path <path>] [-x <ext>| --extension <ext>]

    Options:
     [-x | --extension]     Specifies configuration files extensions. They can be split with ;. For example config;json
     [-p | --path]          Specifies the working directory.
     [-r | --recursive]     Include the current directory and all its subdirectories in the working directory.
     --version              Shows the application version.


After you entered in the application.

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
