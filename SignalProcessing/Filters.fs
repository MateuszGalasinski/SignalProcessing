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
        | WindowFunction.FlatTop -> Window.FlatTop
        | WindowFunction.Hamming -> Window.Hamming
        | WindowFunction.Hanning -> Window.Hann
        | WindowFunction.Blackman -> Window.Blackman
        | Square -> (fun n -> Array.create n 1.0)

    type FilterType =
       | LowPass = 0
       | BandPass = 1
       | HighPass = 2

    let generateLowPassFilterFilter windowType M K samplingFrequency = 
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

    let filterShifter filter shiftFunction = 
        {
            filter with points = filter.points
                    |> List.map2 (fun i p -> Point(p.x, Complex(shiftFunction(i) * p.y.r, p.y.i))) [0.0..1.0..double(filter.points.Length-1)]
        }
        

    let makeHighPassFilter (filter:Signal) = 
        filterShifter filter (fun n -> (-1.0)**n)

    let makeBandPassFilter (filter:Signal) = 
        filterShifter filter (fun n -> 2.0 * sin(Math.PI * n / 2.0))

    let createFilter filterType windowType M K samplingFrequency =
        let lowPassFilter = generateLowPassFilterFilter windowType M K samplingFrequency
        match filterType with 
        | FilterType.LowPass -> lowPassFilter
        | FilterType.BandPass -> makeBandPassFilter lowPassFilter
        | FilterType.HighPass -> makeHighPassFilter lowPassFilter