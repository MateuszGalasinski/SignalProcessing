namespace SignalProcessing

module Quantization = 
    open System

    let quantizate (points:List<Point>) bits = 
        let lvlAmount = int (2.0 ** bits)
        let miny,maxy = 
            points |> List.fold (fun (my,My) (p) -> 
                min my p.y.r, max My p.y.r) (Double.MaxValue,Double.MinValue)
