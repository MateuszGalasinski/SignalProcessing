
open SignalGeneration.SignalGeneration
open RandomNoise

[<EntryPoint>]
let main _ =
    let line = System.Console.ReadLine()
    if line = "noise" then
        generateSignal 10.0 5.0 200.0 0.0 10.0 generateRandomNoise
        |> consolePrint


    System.Console.ReadKey() |> ignore
    0 // return an integer exit code
