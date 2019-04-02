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
