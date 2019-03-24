namespace SignalProcessing

open MathNet.Numerics.Integration

module StatisticsContinous = 
//TODO: implement complex
    let composite meta signalFunction =
        SimpsonRule.IntegrateComposite((fun x -> signalFunction x), meta.startTime, meta.startTime + meta.duration, 16 )

    let composite2 meta =
        let signalFunction = ((SignalGeneration.resolveGenerator meta.signalType) meta)
        DoubleExponentialTransformation.Integrate((fun x -> signalFunction x), meta.startTime, meta.startTime + meta.duration, 0.1)

    let meanValue meta =
        let generator = (SignalGeneration.resolveGenerator meta.signalType) meta
        Complex(((composite meta generator) / meta.duration), 0.0)

    let meanAbsValue meta =
        let generator = fun x -> abs (((SignalGeneration.resolveGenerator meta.signalType) meta) x)
        Complex(((composite meta generator) / meta.duration), 0.0)

    let meanPower meta =
        let signalFunction = (SignalGeneration.resolveGenerator meta.signalType) meta
        let generator = fun x -> (signalFunction x) ** 2.0
        Complex(((composite meta generator) / meta.duration), 0.0)

    let effectiveValue meta =
        let power = (meanPower meta)
        Complex(sqrt power.r, sqrt power.i)

    let variance meta =
        let meanValue = meanValue meta
        let signalFunction = (SignalGeneration.resolveGenerator meta.signalType) meta
        let generator = fun x -> ((signalFunction x) - meanValue.r) ** 2.0
        Complex((composite meta generator) / meta.duration, 0.0)