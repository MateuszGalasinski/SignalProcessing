namespace SignalProcessing

module Statistics = 
    let meanValue (points:List<Point>) : double =
        points
        |> List.map (fun a -> a.y)
        |> List.reduce (fun (a:Complex) (b:Complex) -> a + b)
        |> (fun sum -> sum.r / (double points.Length))
        

