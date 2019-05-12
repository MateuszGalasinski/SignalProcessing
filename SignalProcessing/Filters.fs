namespace SignalProcessing

module Filters =
    open MathNet.Numerics

    type WindowFunction =
       | FlatTop
       | Hamming
       | Hanning
       | Blackman

    let resolveWindow windowType = 
        match windowType with 
        | FlatTop -> Window.FlatTop
        | Hamming -> Window.Hamming
        | Hanning -> Window.Hann
        | Blackman -> Window.Blackman

    let generateFilter windowType M K samplingFrequency = 
        let coefficients = resolveWindow windowType M
        List.ofArray coefficients
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
    