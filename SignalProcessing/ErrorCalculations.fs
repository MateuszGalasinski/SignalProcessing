namespace SignalProcessing

module ErrorCalculations = 
    let noiseSquared original processed =
        original
        |> List.map2 (fun (p1:Point) (p2:Point) -> (p1.y.r - p2.y.r)**2.0) processed
        |> List.sum

    let meanSquaredError original processed = 
        (noiseSquared original processed) / (double original.Length)

    let signalToNoiseRatio original processed = 
        10.0 * log10 (
            original
            |> List.sumBy (fun (p:Point) -> p.y.r ** 2.0)) 
            / (noiseSquared original processed)

        
    let peakSignalToNoiseRatio original processed = 
        10.0 * log10 (
            original
            |> List.maxBy (fun (p:Point) -> p.y.r))
            .y.r
            / (meanSquaredError original processed)

    let maxDifference original processed = 
        original
        |> List.map2 (fun (p1:Point) (p2:Point) -> (p1.y.r - p2.y.r)**2.0) processed
        |> List.max