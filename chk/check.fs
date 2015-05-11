//
// The MIT License (MIT)
//
// Copyright (c) 2013 Alex Rønne Petersen
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
//

open System
open System.Diagnostics
open System.IO
open System.Text.RegularExpressions

(* ------------------------------------------------------------------- *)
(* Initialization Logic                                                *)
(* ------------------------------------------------------------------- *)

let mutable successes = 0
let mutable failures = 0

let mutable testName = ""
let mutable testArgs = ""
let mutable testLogic = fun p -> ()

let recv (proc : Process) =
    let s = proc.StandardOutput.ReadLine()

    Console.WriteLine(s)

    s

let recvErr (proc : Process) =
    let s = proc.StandardError.ReadLine()

    Console.Error.WriteLine(s)

    s

let send (proc : Process) (str : string) =
    proc.StandardInput.WriteLine(str)

    Console.WriteLine("(sdb) {0}", str)

let runTest () =
    let delim = String.replicate 30 "-"

    Console.WriteLine("{0} {1} {0}", delim, testName)
    Console.WriteLine()

    let psi = ProcessStartInfo(Path.Combine("..", "bin", "sdb"),
                               Arguments = testArgs,
                               CreateNoWindow = true,
                               RedirectStandardError = true,
                               RedirectStandardInput = true,
                               RedirectStandardOutput = true,
                               UseShellExecute = false)

    psi.EnvironmentVariables.Add("SDB_CFG", "")
    psi.EnvironmentVariables.Add("SDB_COLORS", "disable")
    psi.EnvironmentVariables.Add("SDB_DEBUG", "disable")

    use proc = Process.Start(psi)

    // Read the logo lines.
    for _ in 1 .. 3 do
        recv proc |> ignore

    // Set the `RuntimePrefix` to `MONO_PREFIX` as
    // specified in the `Makefile`.
    let prefix = Environment.GetEnvironmentVariable("MONO_PREFIX")

    send proc ("config set RuntimePrefix " + prefix)
    recv proc |> ignore

    let mutable ex = null

    try
        testLogic proc
    with
    | e ->
        try
            proc.Kill()
        with
        | :? InvalidOperationException -> ()

        ex <- e

    proc.WaitForExit()

    let sep = String.Format("{0}-{1}-{0}", delim, String.replicate testName.Length "-")

    Console.WriteLine(sep)
    Console.Write("Result: ")

    if proc.ExitCode = 0 && ex = null then
        successes <- successes + 1

        Console.Write("Success")
    else
        failures <- failures + 1

        Console.Write("Failure")

    Console.WriteLine(" ({0})", proc.ExitCode)

    if ex <> null then
        Console.WriteLine()
        Console.WriteLine(ex)

    Console.WriteLine(sep)
    Console.WriteLine()

(* ------------------------------------------------------------------- *)
(* Assertion Functions                                                 *)
(* ------------------------------------------------------------------- *)

let assertRecv (proc : Process) (pat : string) =
    let txt = recv proc

    if not(Regex.IsMatch(txt, pat)) then
        failwith(String.Format("stdout string '{0}' did not match pattern '{1}'", txt, pat))

let assertRecvErr (proc : Process) (pat : string) =
    let txt = recvErr proc

    if not(Regex.IsMatch(txt, pat)) then
        failwith(String.Format("stderr string '{0}' did not match pattern '{1}'", txt, pat))

(* ------------------------------------------------------------------- *)
(* Test Cases                                                          *)
(* ------------------------------------------------------------------- *)

testName <- "simple quit"
testLogic <- fun p ->
    send p "quit"
runTest ()

testName <- "debugger state reset"
testLogic <- fun p ->
    send p "reset"
    assertRecv p "^All debugger state reset$"
    send p "quit"
runTest ()

testName <- "configuration manipulation"
testLogic <- fun p ->
    send p "config get ChunkRawStrings"
    assertRecv p "^'ChunkRawStrings' = 'False'$"
    send p "config set ChunkRawStrings True"
    assertRecv p "^'ChunkRawStrings' = 'True' \(was 'False'\)$"
    send p "quit"
runTest ()

testName <- "catchpoint"
testLogic <- fun p ->
    send p "catchpoint add System.Exception"
    assertRecv p "^Catchpoint for 'System.Exception' added$"
    send p "run cs/throw.exe"
    assertRecv p "^Inferior process '\d*' \('throw.exe'\) started$"
    assertRecv p "^Trapped first-chance exception of type 'System.Exception'$"
    assertRecv p "^#0 \[0x[A-Fa-f0-9]{8}\] Program.Main at .*/chk/cs/throw.cs:9$"
    assertRecv p "^             throw new Exception\(\);$"
    assertRecv p "^System.Exception: Exception of type 'System.Exception' was thrown.$"
    send p "kill"
    assertRecv p "^Inferior process '\d*' \('throw.exe'\) exited with code '0'$"
    send p "quit"
runTest ()

testName <- "source breakpoint"
testLogic <- fun p ->
    send p "breakpoint add location fs/print.fs 3"
    assertRecv p "^Breakpoint '0' added at '.*/chk/fs/print.fs:3'$"
    send p "run fs/print.exe"
    assertRecv p "^Inferior process '\d*' \('print.exe'\) started$"
    assertRecv p "^Hit breakpoint at '.*/chk/fs/print.fs:3'$"
    assertRecv p "^#0 \[0x[A-Fa-f0-9]{8}\] Print.main at .*/chk/fs/print.fs:3$"
    assertRecv p "^    printfn \"Hello World\"$"
    send p "kill"
    assertRecv p "^Inferior process '\d*' \('print.exe'\) exited with code '0'$"
    send p "quit"
runTest ()

testName <- "method breakpoint"
testLogic <- fun p ->
    send p "breakpoint add method Program.Main"
    assertRecv p "^Breakpoint '0' added for method 'Program.Main'$"
    send p "run cs/cwl.exe"
    assertRecv p "^Inferior process '\d*' \('cwl.exe'\) started$"
    assertRecv p "^Hit method breakpoint on 'Program.Main'$"
    assertRecv p "^#0 \[0x[A-Fa-f0-9]{8}\] Program.Main at .*/chk/cs/cwl.cs:6$"
    assertRecv p "^    {$"
    send p "kill"
    assertRecv p "^Inferior process '\d*' \('cwl.exe'\) exited with code '0'$"
    send p "quit"
runTest ()

(* ------------------------------------------------------------------- *)
(* Teardown Logic                                                      *)
(* ------------------------------------------------------------------- *)

if failures = 0 then Console.ForegroundColor <- ConsoleColor.Green
Console.WriteLine("Successes: {0}", successes)
Console.ResetColor()

if failures <> 0 then Console.ForegroundColor <- ConsoleColor.Red
Console.WriteLine("Failures:  {0}", failures)
Console.ResetColor()

exit failures
