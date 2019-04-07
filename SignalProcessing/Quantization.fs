namespace SignalProcessing

open SignalGeneration

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
                |> List.map (fun v -> (abs (v - p.y.r), v, p.x))
                |> List.fold (fun currentMin v ->
                    let (dist, _, _) = v
                    let (currMinDist, _, _) = currentMin
                    if(dist < currMinDist) then
                        v
                    else 
                        currentMin
                    )((Double.MaxValue, Double.MaxValue, Double.MaxValue)))
                |> List.map (fun (_, value, x) -> new Point(x, new Complex(value, 0.0)))


    let extrapolateZeroOrder (points:List<Point>) startTime duration newFrequency = 
        generateXValues duration newFrequency startTime
        |> List.map (fun x -> 
            points 
            |> List.fold (fun (maxX:Point) (p) -> 
                if(maxX.x < p.x && p.x < x) then
                   p
                else
                   maxX
            )(new Point(Double.MinValue, new Complex(0.0, 0.0)))
            |> fun p -> new Point(x, p.y)
            )