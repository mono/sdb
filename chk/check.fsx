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

    // Read the prompt line.
    recv proc |> ignore

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
    psi.EnvironmentVariables.Add("SDB_DEBUG", "enable")

    use proc = Process.Start(psi)

    // Read the logo lines.
    for _ in 1 .. 3 do
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

let assertRecv proc pat =
    let txt = recv proc

    if not(Regex.IsMatch(txt, pat)) then
        failwith(String.Format("stdout string '{0}' did not match pattern '{1}'", txt, pat))

let assertRecvErr proc pat =
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



testName <- "reset state"
testLogic <- fun p ->
    send p "reset"
    assertRecv p "^All debugger state reset$"
    send p "quit"

runTest ()



testName <- "config values"
testLogic <- fun p ->
    send p "config get ChunkRawStrings"
    assertRecv p "^'ChunkRawStrings' = 'False'$"
    send p "config set ChunkRawStrings True"
    assertRecv p "^'ChunkRawStrings' = 'True' \(was 'False'\)$"
    send p "quit"

runTest ()



testName <- "catchpoints"
testLogic <- fun p ->
    send p "catchpoint add System.Exception"
    assertRecv p "^Catchpoint for 'System.Exception' added$"
    send p "quit"

runTest ()

(* ------------------------------------------------------------------- *)
(* Teardown Logic                                                      *)
(* ------------------------------------------------------------------- *)

Console.WriteLine("Successes: {0}", successes)
Console.WriteLine("Failures:  {0}", failures)
