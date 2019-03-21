namespace SignalProcessing

open System

module SignalProcessing = 
    
    let consolePrint (signal : Signal) = 
        signal.points |> List.iter (fun p -> printfn "x %f || y.r %f y.i %f" p.x p.y.r p.y.i)
        printfn "Points amount: %i" signal.points.Length
    
    let genSignal (metadata : SignalMetadata) (points : List<Point>) = 
        {
            points = points
            metadata = metadata
        }

    let genMetadata amplitude duration startTime dutyCycle signalFreq samplingFreq : SignalMetadata = 
        {
            amplitude = amplitude; 
            startTime = startTime;
            duration = duration;
            dutyCycle = dutyCycle;
            signalFrequency = signalFreq;
            samplingFrequency = samplingFreq
        }

    let generateXValues duration samplingFreq startTime =
        let step:double = 1.0 / samplingFreq
        [
            for x in startTime..step..(startTime+duration) do
                yield x 
        ]
    
    let genPoints metadata valueGenerator : List<Point> =
        generateXValues metadata.duration metadata.samplingFrequency metadata.startTime
                    |> (valueGenerator);

    //x independent generators
    let pointFactory ySource =
        fun v -> Point(v, Complex(ySource(), 0.0)) 
    
    let generatePoints amplitude ySource =
        let pointGenerator = pointFactory (fun _ -> ySource() * amplitude )
        fun values -> values |> List.map pointGenerator

    let randomNoiseSource meta = 
        let random = System.Random()
        generatePoints meta.amplitude random.NextDouble

    let gaussianNoiseSource meta = 
        let random = System.Random()
        let randomSource = fun _ -> 
            1.0 + (sqrt (-2.0 * Math.Log(random.NextDouble()))) * (sin (2.0 * Math.PI * random.NextDouble()))
            //TODO check super random 
        generatePoints meta.amplitude randomSource
    
    let impulseNoiseResponse (meta : SignalMetadata) =
        let random = System.Random()
        generatePoints meta.amplitude (fun _ -> 
            if random.NextDouble() < meta.dutyCycle then
                meta.amplitude
            else
                0.0 )

    //x depenedent generators
    let pointFromXFactory ySource =
        fun v -> Point(v, Complex(ySource(v), 0.0)) 

    let trigonometricGenerator (meta : SignalMetadata) trigFunc =
        let sinCalc = pointFromXFactory (fun x -> meta.amplitude * trigFunc (2.0 * Math.PI * meta.signalFrequency * (x - meta.startTime)))
        fun values -> values |> List.map sinCalc

    let sinGenerator (meta : SignalMetadata) =
        trigonometricGenerator meta sin

    let fullRectifiedSinGenerator (meta : SignalMetadata) =
        trigonometricGenerator meta (fun x -> abs (sin x))

    let halfRectifiedSinGenerator meta = 
         trigonometricGenerator meta (fun x -> 0.5 * ((sin x) + abs (sin x)))

    let impulseResponse (meta : SignalMetadata) =
        let pointCalc = pointFromXFactory (fun x -> 
            if x = 0.0 then
                1.0
            else
                0.0 )
        fun values -> values |> List.map pointCalc

    let stepResponse (meta : SignalMetadata) =
        let pointCalc = pointFromXFactory (fun x -> 
            if x > meta.startTime then
                meta.amplitude
            else if x = meta.startTime then
                meta.amplitude / 2.0
            else
                0.0 )
        fun values -> values |> List.map pointCalc

    let rectangleResponse (meta : SignalMetadata) =
        let pointCalc = pointFromXFactory (fun x -> 
            let absTimeToFreqRatio = (x - meta.startTime) / meta.signalFrequency
            if absTimeToFreqRatio - (double (int absTimeToFreqRatio)) < meta.dutyCycle then
                meta.amplitude
            else
                0.0 )
        fun values -> values |> List.map pointCalc
        
    let rectangleSymmetricResponse (meta : SignalMetadata) =
        let pointCalc = pointFromXFactory (fun x -> 
            let absTimeToFreqRatio = (x - meta.startTime) / meta.signalFrequency
            if absTimeToFreqRatio - (double (int absTimeToFreqRatio)) < meta.dutyCycle then
                meta.amplitude
            else
                -meta.amplitude )
        fun values -> values |> List.map pointCalc

    let triangleResponse (meta : SignalMetadata) =
        let oneMinusDuty:double = 1.0-meta.dutyCycle
        let ampOverDuty:double = meta.amplitude / meta.dutyCycle
        let period:double = 1.0 / meta.signalFrequency
        let pointCalc = pointFromXFactory (fun x -> 
            //abs ((x % period) - meta.amplitude)
            let relX = x - (floor (x / period) * period)
            let absTimeToPeriodRatio = (x - meta.startTime) * period
            if absTimeToPeriodRatio - floor absTimeToPeriodRatio < meta.dutyCycle then
                 ampOverDuty * (relX - meta.startTime) / period
            else
                ((meta.amplitude) / oneMinusDuty) + ((-meta.amplitude / oneMinusDuty) * ((relX - meta.startTime) / period))
            )
        fun values -> values |> List.map pointCalc

    let resolveGenerator signalType = 
        match signalType with 
        | SignalType.RandomNoise -> randomNoiseSource
        | SignalType.GaussianNoise -> gaussianNoiseSource
        | SignalType.Sin -> sinGenerator
        | SignalType.SinHalfRectified -> halfRectifiedSinGenerator
        | SignalType.SinFullyRectified -> fullRectifiedSinGenerator
        | SignalType.Rectangle -> rectangleResponse
        | SignalType.RectangleSymmetric -> rectangleSymmetricResponse
        | SignalType.Triangle -> triangleResponse
        | SignalType.StepResponse -> stepResponse
        | SignalType.ImpulseResponse -> impulseResponse
        | SignalType.ImpulseNoise -> impulseNoiseResponse

    let signalGenerator meta signalType = 
        let pointsGen = genPoints meta
        genSignal meta (pointsGen ((resolveGenerator signalType) meta))