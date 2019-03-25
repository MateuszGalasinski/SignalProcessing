namespace SignalProcessing

[<AutoOpen>]
module Types =
    type Complex(r: double, i : double) =
       member this.r = r
       member this.i = i
       //overloading + operator
       static member (+) (a : Complex, b: Complex) =
          Complex(a.r + b.r, a.i + b.i)

       //overloading - operator
       static member (-) (a : Complex, b: Complex) =
          Complex(a.r - b.r, a.i - b.i)

       // overriding the ToString method
       member this.ToString(format:string) =
          this.r.ToString(format) + " i" + this.i.ToString(format)


    type Point(x: double, y : Complex) =
       member this.x = x
       member this.y = y
       //overloading + operator
       static member (+) (a : Point, b: Point) =
          Point(a.x + b.x, a.y + b.y)

       //overloading - operator
       static member (-) (a : Point, b: Point) =
          Point(a.x - b.x, a.y - b.y)

       // overriding the ToString method
       override this.ToString() =
          this.x.ToString() + " " + this.y.ToString()


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
            signalType : SignalType
            isContinous : bool
            amplitude : double
            startTime : double
            duration : double
            dutyCycle : double
            signalFrequency : double
            samplingFrequency : double
        }

    type Signal = 
        {
            metadata : SignalMetadata 
            points : list<Point>
        }
