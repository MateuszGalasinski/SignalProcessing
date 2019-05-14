namespace SignalProcessing

module FFT =
    open Extreme.Mathematics
    open Extreme.Mathematics.SignalProcessing

    let fft signal = 
        let realFft = Fft<double>.CreateReal(signal.points.Length)
        realFft.ForwardScaleFactor <- 1.0 / Math.Sqrt(double signal.points.Length)
        {
            signal with
                points = 
                    (signal.points
                    |> List.map (fun p -> p.y.r)
                    |> List.toArray
                    |> Vector.Create 
                    |> realFft.ForwardTransform
                    |> Vector.Abs)
                        .ToArray()
                    |> Complex.GetRealPart
                    |> Array.indexed
                    |> Array.toList
                    |> List.map (fun p -> Point(double(fst(p)), Complex(snd(p), 0.0)))
        }