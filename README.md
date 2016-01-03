# SDB: Mono Soft Debugger Client

SDB is a command line client for Mono's soft debugger, a cooperative debugger
that is part of the Mono VM. It tries to be similar in command syntax to tools
such as GDB and LLDB.

## Building

Building and using SDB requires a basic POSIX-like environment, a Bash-like
shell, the `libedit` library (or an API/ABI-compatible replacement), and an
installed Mono framework.

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

You can run the SDB test suite with:

    $ make check

It's generally a good idea to do that to ensure that SDB works correctly on
your system before you start using it.

The following variables can be set in your environment or on the Make command
line to affect the build:

* `CAT`: Path to the `cat` POSIX utility.
* `CD`: Path to the `cd` POSIX utility.
* `CHMOD`: Path to the `chmod` POSIX utility.
* `CP`: Path to the `cp` POSIX utility.
* `ECHO`: Path to the `echo` POSIX utility.
* `FSHARPC`: Which F# compiler executable to use.
* `FSHARPC_FLAGS`: Flags to pass to the F# compiler.
* `FSHARPC_TEST_FLAGS`: Flags to pass to the F# compiler for tests.
* `GENDARME`: Which Gendarme executable to use (optional).
* `GENDARME_FLAGS`: Flags to pass to Gendarme.
* `MCS`: Which C# compiler executable to use.
* `MCS_FLAGS`: Flags to pass to the C# compiler.
* `MCS_TEST_FLAGS`: Flags to pass to the C# compiler for tests.
* `MKDIR`: Path to the `mkdir` POSIX utility.
* `PKG_CONFIG`: Path to the `pkg-config` utility.
* `SED`: Path to the `sed` POSIX utility.
* `TAR`: Path to the `tar` POSIX utility.
* `XBUILD`: Which XBuild executable to use.
* `XBUILD_FLAGS`: Flags to pass to XBuild.

Note that the F# tools are only necessary to run the test suite. Gendarme is
also optional and is mostly used by the SDB developers. `tar` is also only used
to package SDB releases.

Additionally, `MODE` can be set to `Debug` (default) or `Release` to indicate
the kind of build desired.

Finally, `MONO_PREFIX` can be set to tell the test runner which Mono executable
should be used. See the description of `RuntimePrefix` further down for more
information.

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

    (sdb) r test.exe
    Inferior process '5234' ('test.exe') started
    Foo!
    Inferior process '5234' ('test.exe') suspended
    #0 [0x00000001] Program.Bar at /home/alexrp/Projects/tests/cs/test.cs:22
            Debugger.Break();

A stack trace can be generated with `bt`:

    (sdb) bt
    #0 [0x00000001] Program.Bar at /home/alexrp/Projects/tests/cs/test.cs:22
            Debugger.Break();
    #1 [0x00000007] Program.Foo at /home/alexrp/Projects/tests/cs/test.cs:17
            Bar();
    #2 [0x00000008] Program.Main at /home/alexrp/Projects/tests/cs/test.cs:10
            Foo(str);

We can select a frame and inspect locals:

    (sdb) f up
    #1 [0x00000007] Program.Foo at /home/alexrp/Projects/tests/cs/test.cs:17
            Bar();
    (sdb) p str
    string it = "Foo!"

Or globals:

    (sdb) p Environment.CommandLine
    string it = "/home/alexrp/Projects/tests/cs/test.exe"

To continue execution, do:

    (sdb) c
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

First of all, all non-option arguments passed to SDB are treated as commands
that SDB will execute at startup. For instance:

    $ sdb "run test.exe"

Or:

    $ sdb "args --foo --bar baz" "run test.exe"

This starts SDB and immediately executes `test.exe` with the given arguments.

The first option is `-f`. This option specifies files that SDB should read
commands from. These commands are executed before any commands specified as
non-option arguments. This option is useful for longer command sequences that
are easier to maintain in a separate file. Example:

    $ cat cmds.txt
    args --foo --bar baz
    run test.exe
    $ sdb -f cmds.txt

The second option is `-b`. This runs SDB in batch mode; that is, it will exit
as soon as all commands have finished and no inferior process is running. This
goes well with `-f` for running programs regularly under SDB.

## Settings

One configuration element that you almost certainly need to alter is the
`RuntimePrefix` string value. It is set to `/usr` by default, regardless of
OS, which is probably not desirable everywhere. For example, on Windows, you
will want to set it to something like `C:\Program Files (x86)\Mono-3.0.10`.
Or if you have Mono in some other directory, you might set it to e.g.
`/opt/mono`.

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
attempt to load all command definitions.

Finally, SDB will read `~/.sdb.rc` and execute any commands (one per line) from
it. This is useful if you prefer to change your settings with commands that you
write down manually, rather than storing the data in a binary file.

## Environment

The `SDB_COLORS` variable can be set to `disable` to tell SDB to not use colors
in output. Normally, SDB will not use colors if it detects that `stdout` has
been redirected, that `TERM` is set to `dumb` (or not set at all), or if the
`DisableColors` configuration element is `true`.

`SDB_CFG` can be set to a specific configuration file to use instead of the
default `~/.sdb.cfg`. If set to the empty string (i.e. `SDB_CFG="" sdb`), SDB
will not load any configuration file at all, and changed configuration values
will not be saved.

The `SDB_PATH` variable can be set to a list of additional directories that SDB
will scan for plugin assemblies in. Each directory should be separated by a
semicolon (Windows) or a colon (POSIX).

`SDB_DEBUG` can be set to `enable` to make SDB print diagnostic information
while debugging. This may be useful to debug SDB itself.

## Plugins

At the moment, SDB has one extension point which is the
`Mono.Debugger.Client.Command` class and the related
`Mono.Debugger.Client.CommandAttribute` class. A class implementing `Command`
that is tagged with `CommandAttribute` will be instantiated at startup time and
put into the root command list.

For SDB to find custom commands, they should be compiled into `.dll` assemblies
and put in `~/.sdb` (or some other directory specified in `SDB_PATH`).

Here's an example of compiling and using a test plugin:

    $ cat test.cs
    using Mono.Debugger.Client;

    [Command]
    public sealed class MyCommand : Command
    {
        public override string[] Names
        {
            get { return new[] { "mycmd" }; }
        }

        public override string Summary
        {
            get { return "Performs magic."; }
        }

        public override string Syntax
        {
            get { return "mycmd"; }
        }

        public override string Help
        {
            get { return "Some sort of detailed help text goes here."; }
        }

        public override void Process(string args)
        {
            Log.Info("Hello! I received: {0}", args);
        }
    }
    $ mcs -debug -t:library test.cs -r:`which sdb`.exe -out:$HOME/.sdb/test.dll
    $ sdb
    Welcome to the Mono soft debugger (sdb 1.0.5061.14716)
    Type 'help' for a list of commands or 'quit' to exit

    (sdb) h mycmd

      mycmd

    Some sort of detailed help text goes here.

    (sdb) mycmd foo bar baz
    Hello! I received: foo bar baz
