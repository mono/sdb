# SDB: Mono Soft Debugger Client

SDB is a command line client for Mono's soft debugger, a cooperative debugger
that is part of the Mono VM. It tries to be similar in command syntax to tools
such as GDB and LLDB.

## Building

Building and using SDB requires a basic POSIX-like environment, the
`libreadline` library, and an installed Mono framework.

First, clone the submodules:

	$ git submodule update --init --recursive

To build, run:

    $ make

This compiles SDB and its dependencies, and puts everything in the `bin`
directory. This directory can be moved around freely; when executed, the `sdb`
script will set up the environment so all the dependency assemblies are found.

You could, for example, just add the `bin` directory to your `PATH`:

    $ export PATH=`pwd`/bin:$PATH
    $ sdb
    Welcome to the Mono soft debugger (sdb 1.0.5058.39468)
    Type 'help' for a list of commands or 'quit' to exit
    (sdb)

The following variables can be set in your environment or on the Make command
line to affect the build:

    * `CD`: Path to the `cd` POSIX utility.
    * `CHMOD`: Path to the `chmod` POSIX utility.
    * `CP`: Path to the `cp` POSIX utility.
    * `MCS`: Which MCS executable to use.
    * `MCS_FLAGS`: Flags to pass to MCS.
    * `MKDIR`: Path to the `mkdir` POSIX utility.
    * `PKG_CONFIG`: Path to the `pkg-config` utility.
    * `SED`: Path to the `sed` POSIX utility.
    * `XBUILD`: Which XBuild executable to use.
    * `XBUILD_FLAGS`: Flags to pass to XBuild.

Additionally, `MODE` can be set to `Debug` (default) or `Release` to indicate
the kind of build desired.

## Usage

Running a program is simple:

    $ cat test.cs
    using System;
    using System.Diagnostics;

    static class Program
    {
        static void Main()
        {
            var str = "Foo!";

            Foo(str);
        }

        static void Foo(string str)
        {
            Console.WriteLine(str);

            Bar();
        }

        static void Bar()
        {
            Debugger.Break();
        }
    }
    $ mcs -debug test.cs
    $ sdb
    Welcome to the Mono soft debugger (sdb 1.0.5060.15368)
    Type 'help' for a list of commands or 'quit' to exit

    (sdb) exec test.exe
    Inferior process '5234' ('test.exe') started
    Foo!
    Inferior process '5234' ('test.exe') suspended

A stack trace can be generated with `bt`:

    (sdb) bt
    #0 [0x00000001] Program.Bar at /home/alexrp/Projects/tests/cs/test.cs:22
    #1 [0x00000007] Program.Foo at /home/alexrp/Projects/tests/cs/test.cs:17
    #2 [0x00000008] Program.Main at /home/alexrp/Projects/tests/cs/test.cs:10

We can select a frame and inspect locals:

    (sdb) f up
    #1 [0x00000007] Program.Foo at /home/alexrp/Projects/tests/cs/test.cs:17
    (sdb) eval str
    string it = "Foo!"

Or globals:

    (sdb) eval Environment.CommandLine
    string it = "/home/alexrp/Projects/tests/cs/test.exe"

To continue execution, do:

    (sdb) cont
    Inferior process '5234' ('test.exe') resumed
    Inferior process '5234' ('test.exe') exited
    (sdb)

We can then exit SDB:

    (sdb) q
    Bye

For more commands, consult `help` in SDB.

## Options

SDB has a few command line options that are useful for automation. For the full
list, issue `sdb --help`.

The first option is `-c` which can be used to queue up commands to be executed
once the debugger has started. This is useful for quick automated debugging
runs, but can also make regular work easier. For instance:

    $ sdb -c "run test.exe"

Or:

    $ sdb -c "args --foo --bar baz" -c "run test.exe"

This starts SDB and immediately executes `test.exe` with the given arguments.

The second option is `-f`. This option specifies files that SDB should read
commands from. These commands are executed before any commands specified with
`-c`. This option is useful for longer command sequences that are easier to
maintain in a separate file. Example:

    $ cat cmds.txt
    args --foo --bar baz
    run test.exe
    $ sdb -f cmds.txt

The last option is `-b`. This runs SDB in batch mode; that is, it will exit
as soon as all commands have finished and no inferior process is running. This
goes well with `-f` for running programs regularly under SDB.

## Settings

One configuration element that you almost certainly need to alter is the
`RuntimePrefix` string value. It is set to `/usr` by default, regardless of
OS, which is probably not desirable everywhere. For example, on Windows, you
will want to set it to something like `C:\Program Files (x86)\Mono-3.0.10`.
Or if you have Mono in some other directory, you might set it to e.g.
`/opt/mono`.

Another important element is `FirstChanceExceptions`. SDB sets it to `true` by
default, but chances are that anything you debug will throw some harmless
exceptions that you don't care about. So, you might want to set it to `false`
and use explicit catchpoints instead.

You may want to set `DisableColors` to `true` if you don't want the fancy ANSI
color codes that SDB emits.

Finally, three useful settings for debugging SDB itself exist: `DebugLogging`
can be set to `true` to make SDB spew a bunch of diagnostic information.
`LogInternalErrors` can be set to `true` to log any internal errors that are
encountered in the Mono debugging libraries. `LogRuntimeSpew` can be set to
`true` to log all messages from the Mono VM.

## Paths

When configuration elements are changed with `config set`, SDB will store the
configuration data in `~/.sdb.cfg`. The content of the file is the .NET binary
serialization of the `Mono.Debugger.Client.Configuration` class. This file is
read on startup if it exists.

At startup, SDB will scan the `~/.sdb` directory for plugin assemblies. It will
attempt to load all command and type formatter definitions.

Finally, SDB will read `~/.sdb.rc` and execute any commands (one per line) from
it. This is useful if you prefer to change your settings with commands that you
write down manually, rather than storing the data in a binary file.

## Environment

The `SDB_COLORS` variable can be set to `disable` to tell SDB to not use colors
in output. Normally, SDB will not use colors if it detects that `stdout` has
been redirected, that `TERM` is set to `dumb` (or not set at all), or if the
`DisableColors` configuration element is `true`.

The `SDB_PATH` variable can be set to a list of additional directories that SDB
will scan for plugin assemblies in. Each directory should be separated by a
semicolon (Windows) or a colon (POSIX).

`SDB_DEBUG` can be set to `enable` to make SDB print diagnostic information
while debugging. This may be useful to debug SDB itself.

## Plugins

At the moment, SDB has one extension point which is the `Command` class and the
associated `CommandAttribute` class. A class implementing `Command` that is
tagged with `CommandAttribute` will be instantiated at startup time and put
into the root command list.

For SDB to find custom commands, they should be compiled into `.dll` assemblies
and put in `~/.sdb` (or some other directory specified `SDB_PATH`).

## Licensing

Though I am not particularly fond of the GNU licenses, SDB makes use of the
`libreadline` library which is licensed under the GPL v3. Thus, for reasons of
practicality, SDB is licensed under the GPL v3 as well.

For the boring details, see the `COPYING` file.

## Issues

* Ctrl-C does not work at all. This is because `System.Console.CancelKeyPress`
  messes with the `libreadline` history key bindings, and because of a Mono bug
  that causes child processes to be killed on Ctrl-C (BNC #699451). Please note
  that using Ctrl-C today can leave behind suspended Mono inferior processes.
  Ctrl-D works fine for exiting SDB, regardless.
* There is no completion for commands - the default completion instead tries to
  complete file names which is not very useful most of the time.
* Decompilation is not implemented. The ICSharpCode.Decompiler library needs to
  be separated from ILSpy for this to be practical.
* The exit code of inferior processes is not shown. There is apparently no way
  to get it from Mono.Debugging.
* Attach support is not implemented. This requires special support in the
  debugging libraries.
