namespace SignalProcessing

open System

module SignalGeneration = 
    let genSignal (metadata : SignalMetadata) (points : List<Point>) = 
        {
            points = points
            metadata = metadata
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

    let applyYCalculator calculator =
        fun values -> values |> List.map calculator

    let pointFromXFactory ySource =
        fun v -> Point(v, Complex(ySource(v), 0.0)) 

    let randomNoiseSource meta = 
        let random = System.Random()
        fun _ -> random.NextDouble() * meta.amplitude

    let gaussianNoiseSource meta = 
        let random = System.Random()
        let randomSource = fun _ -> 
            1.0 + (sqrt (-2.0 * Math.Log(random.NextDouble()))) * (sin (2.0 * Math.PI * random.NextDouble()))
            //TODO check super random 
        fun _ -> randomSource() * meta.amplitude
    
    let impulseNoiseResponse (meta : SignalMetadata) =
        let random = System.Random()
        fun _ -> 
            if random.NextDouble() < meta.dutyCycle then
                meta.amplitude
            else
                0.0 

    let impulseResponse (meta : SignalMetadata) =
        fun x -> 
            if x = 0.0 then
                1.0
            else
                0.0

    let trigonometricGenerator (meta : SignalMetadata) trigFunc =
        fun x -> 
            meta.amplitude * trigFunc (2.0 * Math.PI * meta.signalFrequency * (x - meta.startTime))

    let sinGenerator (meta : SignalMetadata) =
        trigonometricGenerator meta sin

    let fullRectifiedSinGenerator (meta : SignalMetadata) =
        trigonometricGenerator meta (fun x -> abs (sin x))

    let halfRectifiedSinGenerator meta = 
         trigonometricGenerator meta (fun x -> 0.5 * ((sin x) + abs (sin x)))


    let stepResponse (meta : SignalMetadata) =
        fun x -> 
            if x > meta.startTime then
                meta.amplitude
            else if x = meta.startTime then
                meta.amplitude / 2.0
            else
                0.0

    let rectangleResponse (meta : SignalMetadata) =
        fun x -> 
            let absTimeToFreqRatio = (x - meta.startTime) / meta.signalFrequency
            if absTimeToFreqRatio - (double (int absTimeToFreqRatio)) < meta.dutyCycle then
                meta.amplitude
            else
                0.0
        
    let rectangleSymmetricResponse (meta : SignalMetadata) =
        fun x -> 
            let absTimeToFreqRatio = (x - meta.startTime) / meta.signalFrequency
            if absTimeToFreqRatio - (double (int absTimeToFreqRatio)) < meta.dutyCycle then
                meta.amplitude
            else
                -meta.amplitude

    let triangleResponse (meta : SignalMetadata) =
        let oneMinusDuty:double = 1.0-meta.dutyCycle
        let ampOverDuty:double = meta.amplitude / meta.dutyCycle
        let period:double = 1.0 / meta.signalFrequency
        fun x ->
            let x = x - meta.startTime
            let relX = x - (floor (x / period) * period)
            let absTimeToPeriodRatio = x / period
            if absTimeToPeriodRatio - floor absTimeToPeriodRatio < meta.dutyCycle then
                 ampOverDuty * (relX) / period
            else
                ((meta.amplitude) / oneMinusDuty) + ((-meta.amplitude / oneMinusDuty) * (relX / period))

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

    let signalGenerator meta =
        genSignal meta ((genPoints meta) (applyYCalculator (pointFromXFactory ((resolveGenerator meta.signalType) meta))))