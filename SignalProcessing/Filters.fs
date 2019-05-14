namespace SignalProcessing

module Filters =
    open System
    open MathNet.Numerics

    type WindowFunction =
       | FlatTop
       | Hamming
       | Hanning
       | Blackman
       | Square

    let resolveWindow windowType = 
        match windowType with 
        | FlatTop -> Window.FlatTop
        | Hamming -> Window.Hamming
        | Hanning -> Window.Hann
        | Blackman -> Window.Blackman
        | Square -> (fun n -> Array.create n 1.0)
    
    let generateBaseFilter M cutoffFrequency samplingFrequency = 
        let K = cutoffFrequency / samplingFrequency
        [0..M-1]
        |> List.map (fun n -> 
            if (n = (M-1)/2) then
                2.0 / K
            else
                sin ((2.0 * Math.PI * double((n - (M-1) / 2))) / K)
                / Math.PI * double ((n - (M-1) / 2))
            )

    let generateFilter windowType M cutoffFrequency samplingFrequency = 
        let coefficients = resolveWindow windowType M
        List.ofArray coefficients
        |> List.map2 (fun y1 y2 -> y1 * y2) (generateBaseFilter M cutoffFrequency samplingFrequency)
        |> List.map2 (fun y x -> Point(x, Complex(double(y), 0.0))) [0..coefficients.Length-1]
        |> (fun points -> 
        {
            metadata = 
                {
                    samplingFrequency = samplingFrequency;
                    signalType = SignalType.Composed;
                    duration = 0.0;
                    isContinous = false;
                    amplitude = 0.0;
                    startTime = 0.0;
                    dutyCycle = 0.0;
                    signalFrequency = 0.0;
                }
            points = points
        })
    