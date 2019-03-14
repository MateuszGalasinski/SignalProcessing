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
        fun v -> {x = v; y = {r = ySource(v); i = 0.0}}

    let trigonometricGenerator (meta : SignalMetadata) trigFunc : List<Complex> -> List<Point> =
        let sinCalc = pointFromXFactory (fun x -> meta.amplitude * trigFunc (2.0 * Math.PI * meta.samplingFrequency * (x.r - meta.startTime)))
        fun values -> values |> List.map sinCalc

    let sinGenerator (meta : SignalMetadata) =
        trigonometricGenerator meta sin

    let fullRectifiedSinGenerator (meta : SignalMetadata) =
        trigonometricGenerator meta (fun x -> abs (sin x))

    let halfRectifiedSinGenerator meta = 
        let rectifier = fun (v : Point) -> 
            {
                x = v.x;
                y = 
                {
                    r = (1.0/2.0 * v.y.r) + 1.0/2.0 * meta.amplitude * abs (sin (2.0 * Math.PI * meta.samplingFrequency * (v.x.r - meta.startTime)));
                    i = v.y.i
                } 
            }
        fun (points : List<Point>) -> points |> List.map rectifier

    let stepResponse (meta : SignalMetadata) =
        let pointCalc = pointFromXFactory (fun x -> 
            if x.r > meta.startTime then
                meta.amplitude
            else if x.r = meta.startTime then
                meta.amplitude / 2.0
            else
                0.0 )
        fun values -> values |> List.map pointCalc

    let rectangleResponse (meta : SignalMetadata) =
        let pointCalc = pointFromXFactory (fun x -> 
            let absTimeToFreqRatio = (x.r - meta.startTime) / meta.samplingFrequency
            if absTimeToFreqRatio - (float (int absTimeToFreqRatio)) < meta.dutyCycle then
                meta.amplitude
            else
                0.0 )
        fun values -> values |> List.map pointCalc
        
    let rectangleSymmetricResponse (meta : SignalMetadata) =
        let pointCalc = pointFromXFactory (fun x -> 
            let absTimeToFreqRatio = (x.r - meta.startTime) / meta.samplingFrequency
            if absTimeToFreqRatio - (float (int absTimeToFreqRatio)) < meta.dutyCycle then
                meta.amplitude
            else
                - meta.amplitude )
        fun values -> values |> List.map pointCalc

    let testGenerator amplitude = 
        let startTime = 0.0
        let duration = 10.0
        let samplingFreq = 1.0
        let dutyCycle = 0.5
        let meta : SignalMetadata = genMetadata amplitude duration samplingFreq startTime dutyCycle
        let pointsGen = genPoints {meta with samplingFrequency = (60.0 * samplingFreq)}
        genSignal meta (pointsGen (rectangleSymmetricResponse meta))