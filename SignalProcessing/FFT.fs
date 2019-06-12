namespace SignalProcessing

module FFT =
    let comment = 
        1

    //open MathNet.Numerics.LinearAlgebra
    //open MathNet.Numerics

    //let kernel 

    //let fourierSimple signal = 
    //    let x = signal.points
    //            |> List.map (fun p -> p.y)
    //            |> CreateVector.DenseOfEnumerable

    //    let Wn = new complex(System.Math.E, 0.0) ** (new complex(0.0,  (2.0 * System.Math.PI) / float(signal.points.Length)))
    //    let mutable f = CreateMatrix.DenseIdentity<complex> signal.points.Length
    //    for r in 0..signal.points.Length do
    //        for c in 0..signal.points.Length do
    //            (f.Item (r, c)) <- Wn ** (-1.0 * (float(r) * float(c)))
    //    f
//    open Extreme.Mathematics
//    open Extreme.Mathematics.SignalProcessing

//    let fft signal = 
//        let realFft = Fft<double>.CreateReal(signal.points.Length)
//        realFft.ForwardScaleFactor <- 1.0 / Math.Sqrt(double signal.points.Length)
//        {
//            signal with
//                points = 
//                    (signal.points
//                    |> List.map (fun p -> p.y.r)
//                    |> List.toArray
//                    |> Vector.Create 
//                    |> realFft.ForwardTransform
//                    |> Vector.Abs)
//                        .ToArray()
//                    |> Complex.GetRealPart
//                    |> Array.indexed
//                    |> Array.toList
//                    |> List.map (fun p -> Point(double(fst(p)), Complex(snd(p), 0.0)))
//        }