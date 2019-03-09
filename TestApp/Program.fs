
open SignalProcessing.Types
open SignalProcessing.SignalProcessing

[<EntryPoint>]
let main _ =

    let amplitude = 10.0
    let startTime = 0.0
    let duration = 13.0
    let samplingFreq = 3.0
    let dutyCycle = 10.0

    let meta = genMetadata amplitude duration samplingFreq startTime dutyCycle
    let pointsGen = genPoints {meta with samplingFrequency = (16.0 * samplingFreq)}
    let line = System.Console.ReadLine()
    if line = "rn" then
        genSignal meta (pointsGen (randomNoiseSource amplitude))
        |> consolePrint
    if line = "gn" then
        genSignal meta (pointsGen (gaussianNoiseSource amplitude))
        |> consolePrint
    if line = "sin" then 
        genSignal meta (pointsGen (sinGenerator meta))
        |> consolePrint

    System.Console.ReadKey() |> ignore
    0 // return an integer exit code
