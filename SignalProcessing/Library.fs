namespace SignalProcessing

open System

module SignalProcessing = 
    
    let consolePrint (signal : Signal) = 
        signal.points |> List.iter (fun p -> printfn "x.r %f x.i %f || y.r %f y.i %f" p.x.r p.x.i p.y.r p.y.i)
        printfn "Points amount: %i" signal.points.Length
    
    let genSignal (metadata : SignalMetadata) (points : List<Point>) = 
        {
            points = points
            metadata = metadata
        }

    let genMetadata amplitude duration samplingFreq startTime dutyCycle : SignalMetadata = 
        {
            amplitude = amplitude; 
            startTime = startTime;
            duration = duration;
            dutyCycle = dutyCycle;
            samplingFrequency = samplingFreq
        }

    let generateXValues duration samplingFreq startTime : List<Complex> =
        let step = 1.0 / samplingFreq
        [
            for x in startTime..step..(startTime+duration) do
                yield { r = x; i = 0.0 }
        ]
    
    let genPoints 
        (metadata : SignalMetadata) 
        (valueGenerator : List<Complex> -> List<Point>) 
        : List<Point> =
        generateXValues metadata.duration metadata.samplingFrequency metadata.startTime
                    |> (valueGenerator);

    //x independent generators
    let pointFactory ySource : Complex -> Point =
        fun v -> {x = v; y = {r = ySource(); i = 0.0}}
    
    let generatePoints amplitude ySource : List<Complex> -> List<Point> =
        let pointGenerator = pointFactory (fun _ -> ySource() * amplitude )
        fun values -> values |> List.map pointGenerator

    let randomNoiseSource amplitude : List<Complex> -> List<Point>= 
        let random = System.Random()
        generatePoints amplitude random.NextDouble

    let gaussianNoiseSource amplitude : List<Complex> -> List<Point> = 
        let random = System.Random()
        let randomSource = fun _ -> 
            1.0 + (sqrt (-2.0 * Math.Log(random.NextDouble()))) * (sin (2.0 * Math.PI * random.NextDouble()))
            //TODO check super random 
        generatePoints amplitude randomSource
    
    //x depenedent generators
    let pointFromXFactory ySource : Complex -> Point =
        fun v -> 
            let point = {x = v; y = {r = ySource(v); i = 0.0}}
            (point)

    let sinGenerator (meta : SignalMetadata) : List<Complex> -> List<Point> =
        let sinCalc = pointFromXFactory (fun x -> meta.amplitude * sin (2.0 * Math.PI * meta.samplingFrequency * (x.r - meta.startTime)))
        fun values -> values |> List.map sinCalc

    let testGenerator = 
        let amplitude = 10.0
        let startTime = 0.0
        let duration = 13.0
        let samplingFreq = 3.0
        let dutyCycle = 10.0
        let meta : SignalMetadata = genMetadata amplitude duration samplingFreq startTime dutyCycle
        let pointsGen = genPoints {meta with samplingFrequency = (16.0 * samplingFreq)}
        genSignal meta (pointsGen (gaussianNoiseSource amplitude))
        //if line = "sin" then 
        //    genSignal meta (pointsGen (sinGenerator meta))
        //    |> consolePrint