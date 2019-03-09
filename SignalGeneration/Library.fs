namespace SignalGeneration

open Types

module SignalGeneration = 
    let generateXValues duration samplingFreq startTime =
        let interval = 1.0 / samplingFreq
        [
            for x in startTime..interval..(startTime+duration) do
                yield { r = x; i = 0.0 }
        ]
    
    let consolePrint signal = 
        signal.points |> List.iter (fun p -> printfn "x.r %f x.i %f || y.r %f y.i %f" p.x.r p.x.i p.y.r p.y.i)
        printfn "Points amount: %i" signal.points.Length

    let generateSignal amplitude duration samplingFreq startTime  dutyCycle valueGenerator = 
        {
            points = generateXValues duration samplingFreq startTime
                    |> (valueGenerator amplitude);
            metadata = {amplitude = amplitude; startTime = startTime; duration = duration; dutyCycle = dutyCycle}
        }
        

    module RandomNoise = 
        let randomNoiseGenerator amplitude =
            let generator = System.Random()
            let randomizer = fun _ -> generator.NextDouble() * amplitude
            fun v -> {x = v; y = {r = randomizer(); i = 0.0}}

        let generateRandomNoise amplitude =
            let pointGenerator = randomNoiseGenerator amplitude
            fun values -> values |> List.map pointGenerator
