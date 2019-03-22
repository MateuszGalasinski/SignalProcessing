namespace SignalProcessing

module Operations = 
    type OperationType = 
    | Addition = 0
    | Substraction = 1
    | Multiplication = 2
    | Division = 3

    let processPoints operation points1 points2 = 
        let processer = List.map2 (fun (p1:Point) (p2:Point) -> Point(p1.x, operation p1.y p2.y))
        processer points1 points2

    let add signal1 signal2 =
        {signal1 with points = (processPoints (fun a b -> a + b) signal1.points signal2.points)}

    let substract signal1 signal2 =
        {signal1 with points = (processPoints (fun a b -> a - b) signal1.points signal2.points)}
    
    let multiply signal1 signal2 =
        {signal1 with points = (processPoints (fun a b -> Complex(a.r * b.r, 0.0)) signal1.points signal2.points)}

    let divide signal1 signal2 =
        {signal1 with points = (processPoints (fun a b -> Complex(a.r / b.r, 0.0)) signal1.points signal2.points)}

    let operate operation signal1 signal2 =
        match operation with 
        | OperationType.Addition -> add signal1 signal2
        | OperationType.Substraction -> substract signal1 signal2
        | OperationType.Multiplication -> multiply signal1 signal2
        | OperationType.Division -> divide signal1 signal2



