namespace SignalProcessing

module Convolution = 
    let tryTake (array:list<Point>) index = 
        if index < array.Length && index >= 0 then
            array.[index].y
        else
            Complex(0.0, 0.0)

    let generateX samplingFreq count =
        let step:double = 1.0 / samplingFreq
        [
            for x in 0..count do
                yield double(x) * step 
        ]

    let convolutePoints (first:List<Point>) (second:List<Point>) = 
        let newLength = first.Length + second.Length - 1
        let conv = Array.zeroCreate<double> newLength
        for i = 0 to newLength - 1 do
            for j = 0 to first.Length - 1 do
                conv.[i] <- conv.[i] + first.[j].y.r * (tryTake second (i - j)).r

        let points = Array.zeroCreate<Point> first.Length
        let step = first.[1].x - first.[0].x
        let offset = second.Length - 1
        for i = 0 to first.Length - 1 do
            points.[i] <- Point(double(i+offset)*step, Complex(conv.[i+offset], 0.0))
        List.ofArray points

    let simpleConvolute (first:List<Point>) (second:List<Point>) = 
        let newLength = first.Length + second.Length - 1
        let conv = Array.zeroCreate<double> newLength
        for i = 0 to newLength - 1 do
            for j = 0 to first.Length - 1 do
                conv.[i] <- conv.[i] + first.[j].y.r * (tryTake second (i - j)).r

        let points = Array.zeroCreate<Point> newLength
        for i = 0 to newLength - 1 do
            points.[i] <- Point(double(i), Complex(conv.[i], 0.0))
        List.ofArray points

    let convolute (first:Signal) (second:Signal) = 
        let newLength = first.points.Length + second.points.Length - 1
        let conv = Array.zeroCreate<double> newLength
        for i = 0 to newLength - 1 do
            for j = 0 to first.points.Length - 1 do
                conv.[i] <- conv.[i] + first.points.[j].y.r * (tryTake second.points (i - j)).r

        let points = Array.zeroCreate<Point> newLength
        let step = 1.0 / first.metadata.samplingFrequency
        for i = 0 to newLength - 1 do
            points.[i] <- Point(double(i)*step, Complex(conv.[i], 0.0))
        {
            metadata = { 
                first.metadata with 
                    signalType = SignalType.Composed;
                    startTime = 0.0}
            points = List.ofArray points
        }