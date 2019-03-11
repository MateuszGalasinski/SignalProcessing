namespace SignalProcessing

[<AutoOpen>]
module Types =
    type Complex = 
        {
            r: float
            i: float
        }

    type Point = 
        {
            x : Complex 
            y : Complex
        }

    type SignalType = 
        | RandomNoise = 0
        | GaussianNoise = 1
        | Sin = 2
        | SinHalfRectified = 3
        | SinFullyRectified = 4
        | Rectangle = 5
        | RectangleSymmetric = 6
        | Triangle = 7
        | StepResponse = 8
        | ImpulseResponse = 9 
        | ImpulseNoise = 10

    type SignalMetadata =
        {
            amplitude : float
            startTime : float
            duration : float
            dutyCycle : float
            samplingFrequency : float
        }

    type Signal = 
        {
            points : list<Point>
            metadata : SignalMetadata 
        }
