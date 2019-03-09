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
