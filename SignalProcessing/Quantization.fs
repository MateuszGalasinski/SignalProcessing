namespace SignalProcessing

module Quantization = 
    open System

    let quantizate (points:List<Point>) bits = 
        let lvlAmount = int (2.0 ** bits)
        let miny,maxy = 
            points 
            |> List.fold (fun (miny,maxy) (p) -> 
                min miny p.y.r, max maxy p.y.r) (Double.MaxValue,Double.MinValue)
        let domainWidth = (maxy - miny) / (double lvlAmount)
        let values = 
            [ 
                for v in miny..domainWidth..maxy do
                    yield v
            ]
        points
        |> List.map (fun p -> 
                values 
                |> List.map (fun v -> abs (v - p.y.r))
                |> List.fold (fun currentMin v ->
                    min currentMin v) (Double.MaxValue)
            )

