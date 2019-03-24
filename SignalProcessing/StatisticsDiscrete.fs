namespace SignalProcessing

open MathNet.Numerics.Integration

module StatisticsDiscrete = 
    let mean aggregator (points:List<Point>) =
        points
        |> List.map (fun a -> a.y)
        |> (List.fold (fun acc (a:Complex) -> aggregator acc a) (Complex(0.0, 0.0)))
        |> (fun sum -> Complex(sum.r / (double points.Length), sum.i / (double points.Length)) )

    let meanValue (points:List<Point>) =
        mean (fun a b -> a + b) points

    let abs (complex:Complex) = 
        sqrt (complex.r * complex.r + complex.i * complex.i)

    let meanAbsValue (points:List<Point>)  =
        mean (fun a b -> Complex((abs a) + (abs b), 0.0)) points

    let meanPower (points:List<Point>) =
        mean (fun a b -> Complex(a.r + (b.r**2.0 - b.i**2.0), a.i + 2.0*b.i*b.r)) points

    let effectiveValue (points:List<Point>) =
        let power = (meanPower points)
        Complex(sqrt power.r, sqrt power.i)

    let variance (points:List<Point>) =
        let meanValue = meanValue points
        let diff = fun a b -> (a - b) ** 2.0
        mean (fun a b -> Complex(a.r + diff b.r meanValue.r, a.i + diff b.i meanValue.i)) points

    let composite meta = 
        let signalFunction = ((SignalGeneration.resolveGenerator meta.signalType) meta)
        SimpsonRule.IntegrateComposite((fun x -> signalFunction x), meta.startTime, meta.startTime + meta.duration, 16 )

    let composite2 meta =
        let signalFunction = ((SignalGeneration.resolveGenerator meta.signalType) meta)
        DoubleExponentialTransformation.Integrate((fun x -> signalFunction x), meta.startTime, meta.startTime + meta.duration, 0.1)
